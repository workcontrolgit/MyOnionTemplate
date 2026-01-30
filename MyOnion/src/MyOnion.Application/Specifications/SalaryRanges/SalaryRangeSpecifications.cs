using Ardalis.Specification;

namespace MyOnion.Application.Specifications.SalaryRanges
{
    public class SalaryRangesByFiltersSpecification : Specification<SalaryRange>
    {
        public SalaryRangesByFiltersSpecification(GetSalaryRangesQuery request, bool applyPaging = true)
        {
            // Filtering - type-safe with compile-time validation
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var term = request.Name.Trim();
                Query.Where(s => s.Name.Contains(term));
            }

            // Ordering - expression-based
            Query.OrderBy(s => s.Name);

            // Pagination
            if (applyPaging && request.PageSize > 0)
            {
                Query.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize);
            }

            // EF Core optimizations
            Query.AsNoTracking()
                 .TagWith("GetSalaryRangesByFilters");
        }
    }
}
