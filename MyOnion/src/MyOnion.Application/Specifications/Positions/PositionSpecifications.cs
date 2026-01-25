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
                predicate = predicate.Or(p => p.PositionTitle.Value.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(request.Department))
            {
                var term = request.Department.Trim();
                predicate = predicate.Or(p => p.Department.Name.Value.Contains(term));
            }

            return predicate.IsStarted ? predicate : null;
        }
    }

}
