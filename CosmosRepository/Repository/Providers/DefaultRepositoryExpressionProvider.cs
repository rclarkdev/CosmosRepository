using Microsoft.Azure.Cosmos.Linq;
using System.Linq.Expressions;

namespace CosmosRepository
{
    /// <inheritdoc/>
    public class DefaultRepositoryExpressionProvider : IRepositoryExpressionProvider
    {
        public Expression<Func<TItem, bool>> Build<TItem>(Expression<Func<TItem, bool>> predicate = null)
            where TItem : IItem =>
            predicate is null
                ? item => !item.Type.IsDefined() || item.Type == typeof(TItem).Name
                : predicate.Compose(item => !item.Type.IsDefined() || item.Type == typeof(TItem).Name,
                    Expression.AndAlso);
    }
}
