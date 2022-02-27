using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Net;

namespace CosmosRepository
{
    /// <inheritdoc/>
    public class DefaultRepository<TItem> : IRepository<TItem> where TItem : IItem
    {
        readonly ILogger<DefaultRepository<TItem>> _logger;
        readonly IRepositoryExpressionProvider _repositoryExpressionProvider;
        readonly IQueryableProcessor _cosmosQueryableProcessor;

        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;

        private readonly string _endpointUri;
        private readonly string _primaryKey;

        public DefaultRepository(
            IRepositoryExpressionProvider repositoryExpressionProvider,
            IQueryableProcessor cosmosQueryableProcessor,
            ILogger<DefaultRepository<TItem>> logger, IOptions<CosmosDBSettings> options)
        {
            _logger = logger;

            _endpointUri = options.Value.EndpointUri;
            _primaryKey = options.Value.PrimaryKey;
            _repositoryExpressionProvider = repositoryExpressionProvider;
            _cosmosQueryableProcessor = cosmosQueryableProcessor;

            _cosmosClient = new CosmosClient(_endpointUri, _primaryKey, new CosmosClientOptions()
            { ApplicationName = "CosmosDBDotnetQuickstart", ConnectionMode = ConnectionMode.Gateway });

            _database = _cosmosClient.GetDatabase(options.Value.DatabaseId);
        }

        /// <inheritdoc/>
        public ValueTask<TItem> GetAsync(string id, string containerId, string partitionKeyValue = null, CancellationToken cancellationToken = default) =>
            GetAsync(id, containerId, new PartitionKey(partitionKeyValue ?? id), cancellationToken);


        /// <inheritdoc/>
        public async ValueTask<TItem> GetAsync(string id, string containerId, PartitionKey partitionKey, CancellationToken cancellationToken = default)
        {
            if (partitionKey == default)
            {
                partitionKey = new PartitionKey(id);
            }

            var container = _database.GetContainer(containerId);

            var response = await container
                .ReadItemAsync<TItem>(id, partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false);

            return response.Resource;
        }

        /// <inheritdoc/>
        public async ValueTask<IEnumerable<TItem>> GetAsync(string containerId, Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                var container = _database.GetContainer(containerId);

                IQueryable<TItem> query = container.GetItemLinqQueryable<TItem>()
                    .Where(_repositoryExpressionProvider.Build(predicate));

                return await _cosmosQueryableProcessor.IterateAsync(query, cancellationToken);
            }
            catch (CosmosException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, $"A CosmosException occured. Message: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async ValueTask<TItem> CreateAsync(TItem value, string containerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var container = _database.GetContainer(containerId);

                ItemResponse<TItem> response =
                    await container.CreateItemAsync(value, new PartitionKey(value.PartitionKey),
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, $"A CosmosException occured. Message: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async ValueTask<IEnumerable<TItem>> CreateAsync(string containerId, IEnumerable<TItem> values, CancellationToken cancellationToken = default)
        {
            IEnumerable<Task<TItem>> creationTasks =
                values.Select(value => CreateAsync(value, containerId, cancellationToken).AsTask())
                    .ToList();

            _ = await Task.WhenAll(creationTasks).ConfigureAwait(false);

            return creationTasks.Select(x => x.Result);
        }

        /// <inheritdoc/>
        public async ValueTask<TItem> UpdateAsync(TItem value, string containerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var container = _database.GetContainer(containerId);

                ItemResponse<TItem> response =
                    await container.UpsertItemAsync(value, new PartitionKey(value.PartitionKey), null,
                            cancellationToken)
                        .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
            {
                _logger.LogError(ex, $"A CosmosException occured. Message: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }

        }

        /// <inheritdoc/>
        public async ValueTask<IEnumerable<TItem>> UpdateAsync(string containerId, IEnumerable<TItem> values, CancellationToken cancellationToken = default)
        {
            IEnumerable<Task<TItem>> updateTasks =
                values.Select(value => UpdateAsync(value, containerId, cancellationToken).AsTask())
                    .ToList();

            await Task.WhenAll(updateTasks).ConfigureAwait(false);

            return updateTasks.Select(x => x.Result);
        }

        /// <inheritdoc/>
        public ValueTask DeleteAsync(TItem value, string containerId, CancellationToken cancellationToken = default) =>
            DeleteAsync(value.Id.ToString(), containerId, value.PartitionKey, cancellationToken);

        /// <inheritdoc/>
        public ValueTask DeleteAsync(string id, string containerId, string partitionKeyValue = null, CancellationToken cancellationToken = default) =>
            DeleteAsync(id, containerId, new PartitionKey(partitionKeyValue ?? id), cancellationToken);

        /// <inheritdoc/>
        public async ValueTask DeleteAsync(string id, string containerId, PartitionKey partitionKey, CancellationToken cancellationToken = default)
        {
            var container = _database.GetContainer(containerId);

            if (partitionKey == default)
            {
                partitionKey = new PartitionKey(id);
            }

            _ = await container.DeleteItemAsync<TItem>(id, partitionKey, null, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public ValueTask<bool> ExistsAsync(string id, string containerId, string partitionKeyValue = null, CancellationToken cancellationToken = default) =>
            ExistsAsync(id, containerId, new PartitionKey(partitionKeyValue ?? id), cancellationToken);

        /// <inheritdoc/>
        public async ValueTask<bool> ExistsAsync(string id, string containerId, PartitionKey partitionKey, CancellationToken cancellationToken = default)
        {
            var container = _database.GetContainer(containerId);

            if (partitionKey == default)
            {
                partitionKey = new PartitionKey(id);
            }

            try
            {
                _ = await container.ReadItemAsync<TItem>(id, partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound) { return false; }

            return true;
        }

        /// <inheritdoc/>
        public async ValueTask<bool> ExistsAsync(string containerId, Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var container = _database.GetContainer(containerId);

            IQueryable<TItem> query =
                container.GetItemLinqQueryable<TItem>()
                    .Where(_repositoryExpressionProvider.Build(predicate));

            int count = await _cosmosQueryableProcessor.CountAsync(query, cancellationToken);
            return count > 0;
        }

        static async Task<(List<TItem> items, double charge, string continuationToken)> GetAllItemsAsync(IQueryable<TItem> query, int pageSize, CancellationToken cancellationToken = default)
        {
            string continuationToken = null;
            List<TItem> results = new();
            int readItemsCount = 0;
            double charge = 0;
            using FeedIterator<TItem> iterator = query.ToFeedIterator();
            while (readItemsCount < pageSize && iterator.HasMoreResults)
            {
                FeedResponse<TItem> next = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);

                foreach (TItem result in next)
                {
                    if (readItemsCount == pageSize) break;

                    results.Add(result);
                    readItemsCount++;
                }

                charge += next.RequestCharge;
                continuationToken = next.ContinuationToken;
            }
            return (results, charge, continuationToken);
        }
    }
}
