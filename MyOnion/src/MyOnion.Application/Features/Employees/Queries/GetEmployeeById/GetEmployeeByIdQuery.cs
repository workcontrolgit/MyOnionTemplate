using MyOnion.Application.Specifications.Employees;

namespace MyOnion.Application.Features.Employees.Queries.GetEmployeeById
{
    /// <summary>
    /// Query to fetch an employee by id.
    /// </summary>
    public class GetEmployeeByIdQuery : IRequest<Result<EmployeeDetailsDto>>
    {
        public Guid Id { get; set; }

        public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDetailsDto>>
        {
            private readonly IEmployeeRepositoryAsync _repository;

            public GetEmployeeByIdQueryHandler(IEmployeeRepositoryAsync repository)
            {
                _repository = repository;
            }

            public async Task<Result<EmployeeDetailsDto>> Handle(GetEmployeeByIdQuery query, CancellationToken cancellationToken)
            {
                var spec = new EmployeeByIdWithPositionSpecification(query.Id);
                var entity = await _repository.FirstOrDefaultAsync(spec);
                if (entity == null)
                {
                    throw new ApiException("Employee Not Found.");
                }

                var dto = new EmployeeDetailsDto
                {
                    Id = entity.Id,
                    FirstName = entity.FirstName,
                    MiddleName = entity.MiddleName,
                    LastName = entity.LastName,
                    Birthday = entity.Birthday,
                    Email = entity.Email,
                    Gender = entity.Gender,
                    EmployeeNumber = entity.EmployeeNumber,
                    Prefix = entity.Prefix,
                    Phone = entity.Phone,
                    Salary = entity.Salary,
                    PositionId = entity.PositionId,
                    PositionTitle = entity.Position?.PositionTitle,
                    DepartmentId = entity.DepartmentId,
                    DepartmentName = entity.Department?.Name
                };

                return Result<EmployeeDetailsDto>.Success(dto);
            }
        }
    }
}
