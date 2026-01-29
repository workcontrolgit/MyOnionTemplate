using Ardalis.Specification;

namespace MyOnion.Application.Specifications.Positions
{
    public class PositionsByFiltersSpecification : Specification<Position>
    {
        public PositionsByFiltersSpecification(GetPositionsQuery request, bool applyPaging = true)
        {
            // OR logic - combine in single Where expression
            var hasPositionNumber = !string.IsNullOrWhiteSpace(request.PositionNumber);
            var hasPositionTitle = !string.IsNullOrWhiteSpace(request.PositionTitle);
            var hasDepartment = !string.IsNullOrWhiteSpace(request.Department);

            if (hasPositionNumber || hasPositionTitle || hasDepartment)
            {
                var positionNumber = request.PositionNumber?.Trim() ?? string.Empty;
                var positionTitle = request.PositionTitle?.Trim() ?? string.Empty;
                var department = request.Department?.Trim() ?? string.Empty;

                Query.Where(p =>
                    (hasPositionNumber && p.PositionNumber.Contains(positionNumber)) ||
                    (hasPositionTitle && p.PositionTitle.Value.Contains(positionTitle)) ||
                    (hasDepartment && p.Department != null && p.Department.Name.Value.Contains(department))
                );
            }

            // Type-safe includes!
            Query.Include(p => p.Department)
                 .Include(p => p.SalaryRange);

            // Expression-based ordering
            Query.OrderBy(p => p.PositionNumber);

            // Pagination
            if (applyPaging && request.PageSize > 0)
            {
                Query.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize);
            }

            // EF Core optimizations
            Query.AsNoTracking()
                 .TagWith("GetPositionsByFilters");
        }
    }
}
