using Ardalis.Specification;

namespace MyOnion.Application.Specifications.Departments
{
    public class DepartmentsByFiltersSpecification : Specification<Department>
    {
        public DepartmentsByFiltersSpecification(GetDepartmentsQuery request, bool applyPaging = true)
        {
            // Filtering - type-safe with compile-time validation
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var term = request.Name.Trim();
                Query.Where(d => d.Name.Value.Contains(term));
            }

            // Ordering - expression-based, no string mapping needed!
            Query.OrderBy(d => d.Name.Value);

            // Pagination
            if (applyPaging && request.PageSize > 0)
            {
                Query.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize);
            }

            // EF Core optimizations
            Query.AsNoTracking()
                 .TagWith("GetDepartmentsByFilters");
        }
    }
}
