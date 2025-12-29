namespace MyOnion.Application.Specifications.SalaryRanges
{
    public class SalaryRangesByFiltersSpecification : BaseSpecification<SalaryRange>
    {
        public SalaryRangesByFiltersSpecification(GetSalaryRangesQuery request, bool applyPaging = true)
            : base(BuildFilterExpression(request))
        {
            var orderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "Name" : request.OrderBy;
            ApplyOrderBy(orderBy);

            if (applyPaging && request.PageSize > 0)
            {
                ApplyPaging((request.PageNumber - 1) * request.PageSize, request.PageSize);
            }
        }

        private static Expression<Func<SalaryRange, bool>> BuildFilterExpression(GetSalaryRangesQuery request)
        {
            var predicate = PredicateBuilder.New<SalaryRange>(true);

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var term = request.Name.Trim();
                predicate = predicate.And(s => s.Name.Contains(term));
            }

            return predicate.IsStarted ? predicate : null;
        }
    }
}
