using Ardalis.Specification;

namespace MyOnion.Application.Specifications.Employees
{
    public class EmployeesByFiltersSpecification : Specification<Employee>
    {
        public EmployeesByFiltersSpecification(GetEmployeesQuery request, bool applyPaging = true)
        {
            // OR logic across multiple fields
            var hasLastName = !string.IsNullOrWhiteSpace(request.LastName);
            var hasFirstName = !string.IsNullOrWhiteSpace(request.FirstName);
            var hasEmail = !string.IsNullOrWhiteSpace(request.Email);
            var hasEmployeeNumber = !string.IsNullOrWhiteSpace(request.EmployeeNumber);
            var hasPositionTitle = !string.IsNullOrWhiteSpace(request.PositionTitle);

            if (hasLastName || hasFirstName || hasEmail || hasEmployeeNumber || hasPositionTitle)
            {
                var lastName = request.LastName?.ToLower().Trim() ?? string.Empty;
                var firstName = request.FirstName?.ToLower().Trim() ?? string.Empty;
                var email = request.Email?.ToLower().Trim() ?? string.Empty;
                var employeeNumber = request.EmployeeNumber?.ToLower().Trim() ?? string.Empty;
                var positionTitle = request.PositionTitle?.ToLower().Trim() ?? string.Empty;

                Query.Where(e =>
                    (hasLastName && e.Name.LastName.ToLower().Contains(lastName)) ||
                    (hasFirstName && e.Name.FirstName.ToLower().Contains(firstName)) ||
                    (hasEmail && e.Email.ToLower().Contains(email)) ||
                    (hasEmployeeNumber && e.EmployeeNumber.ToLower().Contains(employeeNumber)) ||
                    (hasPositionTitle && e.Position.PositionTitle.Value.ToLower().Contains(positionTitle))
                );
            }

            // Type-safe includes
            Query.Include(e => e.Position);

            // Expression-based ordering
            Query.OrderBy(e => e.Name.LastName);

            // Pagination
            if (applyPaging && request.PageSize > 0)
            {
                Query.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize);
            }

            // EF Core optimizations
            Query.AsNoTracking()
                 .TagWith("GetEmployeesByFilters");
        }
    }

    public class EmployeeByIdWithPositionSpecification : SingleResultSpecification<Employee>
    {
        public EmployeeByIdWithPositionSpecification(Guid id)
        {
            Query.Where(e => e.Id == id)
                 .Include(e => e.Position)
                    .ThenInclude(p => p.Department)
                 .Include(e => e.Department)
                 .AsNoTracking()
                 .TagWith($"GetEmployeeById:{id}");
        }
    }
}
