using System.Linq.Expressions;

namespace MyOnion.Application.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        List<string> IncludeStrings { get; }
        int? Take { get; }
        int? Skip { get; }
        bool IsPagingEnabled { get; }
        string OrderBy { get; }
    }
}
