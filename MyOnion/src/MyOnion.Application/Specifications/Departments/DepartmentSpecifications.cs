namespace MyOnion.Application.Specifications.Departments
{
    public class DepartmentsByFiltersSpecification : BaseSpecification<Department>
    {
        public DepartmentsByFiltersSpecification(GetDepartmentsQuery request, bool applyPaging = true)
            : base(BuildFilterExpression(request))
        {
            var orderBy = string.IsNullOrWhiteSpace(request.OrderBy) ? "Name" : request.OrderBy;
            ApplyOrderBy(orderBy);

            if (applyPaging && request.PageSize > 0)
            {
                ApplyPaging((request.PageNumber - 1) * request.PageSize, request.PageSize);
            }
        }

        private static Expression<Func<Department, bool>> BuildFilterExpression(GetDepartmentsQuery request)
        {
            var predicate = PredicateBuilder.New<Department>(true);

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var term = request.Name.Trim();
                predicate = predicate.And(d => d.Name.Value.Contains(term));
            }

            return predicate.IsStarted ? predicate : null;
        }
    }
}
