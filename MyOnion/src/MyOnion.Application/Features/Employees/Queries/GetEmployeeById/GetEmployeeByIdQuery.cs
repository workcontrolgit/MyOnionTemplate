namespace MyOnion.Application.Features.Employees.Queries.GetEmployeeById
{
    /// <summary>
    /// Query to fetch an employee by id.
    /// </summary>
    public class GetEmployeeByIdQuery : IRequest<Result<Employee>>
    {
        public Guid Id { get; set; }

        public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, Result<Employee>>
        {
            private readonly IEmployeeRepositoryAsync _repository;

            public GetEmployeeByIdQueryHandler(IEmployeeRepositoryAsync repository)
            {
                _repository = repository;
            }

            public async Task<Result<Employee>> Handle(GetEmployeeByIdQuery query, CancellationToken cancellationToken)
            {
                var entity = await _repository.GetByIdAsync(query.Id);
                if (entity == null)
                {
                    throw new ApiException("Employee Not Found.");
                }

                return Result<Employee>.Success(entity);
            }
        }
    }
}
