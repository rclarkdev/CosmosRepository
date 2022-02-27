using Microsoft.Azure.Cosmos;

namespace CosmosRepository
{
    public interface IQueryableProcessor
    {
        ValueTask<IEnumerable<TItem>> IterateAsync<TItem>(IQueryable<TItem> queryable, CancellationToken cancellationToken = default) where TItem : IItem;

        ValueTask<int> CountAsync<TItem>(IQueryable<TItem> queryable, CancellationToken cancellationToken = default) where TItem : IItem;

        ValueTask<IEnumerable<TItem>> IterateAsync<TItem>(Container container, QueryDefinition queryDefinition,
            CancellationToken cancellationToken = default) where TItem : IItem;
    }
}