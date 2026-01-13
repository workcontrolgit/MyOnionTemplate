namespace MyOnion.Application.Specifications.Employees
{
    public class EmployeesByFiltersSpecification : BaseSpecification<Employee>
    {
        public EmployeesByFiltersSpecification(GetEmployeesQuery request, bool applyPaging = true)
            : base(BuildFilterExpression(request))
        {
            AddInclude(e => e.Position);
            var orderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "LastName" : request.OrderBy;
            ApplyOrderBy(orderBy);

            if (applyPaging && request.PageSize > 0)
            {
                ApplyPaging((request.PageNumber - 1) * request.PageSize, request.PageSize);
            }
        }

        private static Expression<Func<Employee, bool>> BuildFilterExpression(GetEmployeesQuery request)
        {
            var predicate = PredicateBuilder.New<Employee>();

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                var term = request.LastName.ToLower().Trim();
                predicate = predicate.Or(p => p.LastName.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                var term = request.FirstName.ToLower().Trim();
                predicate = predicate.Or(p => p.FirstName.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var term = request.Email.ToLower().Trim();
                predicate = predicate.Or(p => p.Email.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(request.EmployeeNumber))
            {
                var term = request.EmployeeNumber.ToLower().Trim();
                predicate = predicate.Or(p => p.EmployeeNumber.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(request.PositionTitle))
            {
                var term = request.PositionTitle.ToLower().Trim();
                predicate = predicate.Or(p => p.Position.PositionTitle.ToLower().Contains(term));
            }

            return predicate.IsStarted ? predicate : null;
        }
    }

}
