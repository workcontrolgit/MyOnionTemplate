using LinqKit;
using MyOnion.Application.Features.Positions.Queries.GetPositions;
using MyOnion.Domain.Entities;
using System.Linq.Expressions;

namespace MyOnion.Application.Specifications.Positions
{
    public class PositionsByFiltersSpecification : BaseSpecification<Position>
    {
        public PositionsByFiltersSpecification(GetPositionsQuery request, bool applyPaging = true)
            : base(BuildFilterExpression(request))
        {
            AddInclude(p => p.Department);
            AddInclude(p => p.SalaryRange);

            var orderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "PositionNumber" : request.OrderBy;
            ApplyOrderBy(orderBy);

            if (applyPaging && request.PageSize > 0)
            {
                ApplyPaging((request.PageNumber - 1) * request.PageSize, request.PageSize);
            }
        }

        private static Expression<Func<Position, bool>> BuildFilterExpression(GetPositionsQuery request)
        {
            var predicate = PredicateBuilder.New<Position>();

            if (!string.IsNullOrWhiteSpace(request.PositionNumber))
            {
                var term = request.PositionNumber.Trim();
                predicate = predicate.Or(p => p.PositionNumber.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(request.PositionTitle))
            {
                var term = request.PositionTitle.Trim();
                predicate = predicate.Or(p => p.PositionTitle.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(request.Department))
            {
                var term = request.Department.Trim();
                predicate = predicate.Or(p => p.Department.Name.Contains(term));
            }

            return predicate.IsStarted ? predicate : null;
        }
    }

    public class PositionsKeywordSpecification : BaseSpecification<Position>
    {
        public PositionsKeywordSpecification(PagedPositionsQuery request, bool applyPaging = true)
            : base(BuildSearchExpression(request.Search?.Value))
        {
            AddInclude(p => p.Department);
            AddInclude(p => p.SalaryRange);

            var orderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "PositionNumber" : request.OrderBy;
            ApplyOrderBy(orderBy);

            if (applyPaging && request.PageSize > 0)
            {
                ApplyPaging((request.PageNumber - 1) * request.PageSize, request.PageSize);
            }
        }

        private static Expression<Func<Position, bool>> BuildSearchExpression(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return null;
            }

            var term = keyword.Trim();
            var predicate = PredicateBuilder.New<Position>();

            predicate = predicate.Or(p => p.PositionNumber.Contains(term));
            predicate = predicate.Or(p => p.PositionTitle.Contains(term));
            predicate = predicate.Or(p => p.Department.Name.Contains(term));

            return predicate;
        }
    }
}
