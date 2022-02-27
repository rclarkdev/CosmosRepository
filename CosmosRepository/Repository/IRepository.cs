using Microsoft.Azure.Cosmos;
using System.Linq.Expressions;

namespace CosmosRepository
{
    public interface IRepository<TItem> where TItem : IItem
    {
        /// <summary>
        /// Gets the <see cref="IItem"/> implementation class instance as a <typeparamref name="TItem"/> that corresponds to the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="containerId"></param>
        /// <param name="partitionKeyValue"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<TItem> GetAsync(string id, string containerId, string partitionKeyValue = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the <see cref="IItem"/> implementation class instance as a <typeparamref name="TItem"/> that corresponds to the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="containerId"></param>
        /// <param name="partitionKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<TItem> GetAsync(string id, string containerId, PartitionKey partitionKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an <see cref="IEnumerable{TItem}"/> collection of <see cref="IItem"/>
        /// implementation classes that match the given <paramref name="predicate"/>
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<IEnumerable<TItem>> GetAsync(string containerId, Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a cosmos item representing the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="containerId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<TItem> CreateAsync(TItem value, string containerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates one or more cosmos item(s) representing the given <paramref name="values"/>.
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="values"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<IEnumerable<TItem>> CreateAsync(string containerId, IEnumerable<TItem> values, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the cosmos object that corresponds to the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="containerId"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="ignoreEtag"></param>
        /// <returns></returns>
        ValueTask<TItem> UpdateAsync(TItem value, string containerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates one or more cosmos item(s) representing the given <paramref name="values"/>.
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="values"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="ignoreEtag"></param>
        /// <returns></returns>
        ValueTask<IEnumerable<TItem>> UpdateAsync(string containerId, IEnumerable<TItem> values, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the cosmos object that corresponds to the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="containerId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask DeleteAsync(TItem value, string containerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the cosmos object that corresponds to the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="containerId"></param>
        /// <param name="partitionKeyValue"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask DeleteAsync(string id, string containerId, string partitionKeyValue = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the cosmos object that corresponds to the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="containerId"></param>
        /// <param name="partitionKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask DeleteAsync(string id, string containerId, PartitionKey partitionKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries cosmos DB to see if an item exists.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="containerId"></param>
        /// <param name="partitionKeyValue"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<bool> ExistsAsync(string id, string containerId, string partitionKeyValue = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries cosmos DB to see if an item exists.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="containerId"></param>
        /// <param name="partitionKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<bool> ExistsAsync(string id, string containerId, PartitionKey partitionKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries cosmos DB to see if an item exists.
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<bool> ExistsAsync(string containerId, Expression<Func<TItem, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
