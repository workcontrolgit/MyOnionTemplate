using Microsoft.EntityFrameworkCore;
using MyOnion.Application.Specifications;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace MyOnion.Infrastructure.Persistence.Specifications
{
    public static class SpecificationEvaluator<T> where T : class
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
        {
            if (specification == null)
            {
                return inputQuery;
            }

            var query = inputQuery;

            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));
            query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

            if (!string.IsNullOrWhiteSpace(specification.OrderBy))
            {
                query = query.OrderBy(specification.OrderBy);
            }

            if (specification.IsPagingEnabled)
            {
                if (specification.Skip.HasValue)
                {
                    query = query.Skip(specification.Skip.Value);
                }

                if (specification.Take.HasValue)
                {
                    query = query.Take(specification.Take.Value);
                }
            }

            return query;
        }
    }
}
