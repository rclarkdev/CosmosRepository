using System.Linq.Expressions;

namespace CosmosRepository
{
    public interface IRepositoryExpressionProvider
    {
        /// <summary>
        /// Returns an Expression for a given predicate.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Expression<Func<TItem, bool>> Build<TItem>(Expression<Func<TItem, bool>> predicate) where TItem : IItem;
    }
}