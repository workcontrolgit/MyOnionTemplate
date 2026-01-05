namespace MyOnion.Application.Features.Departments.Queries.GetDepartmentById
{
    /// <summary>
    /// Query to fetch a department by id.
    /// </summary>
    public class GetDepartmentByIdQuery : IRequest<Result<Department>>
    {
        public Guid Id { get; set; }

        public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, Result<Department>>
        {
            private readonly IDepartmentRepositoryAsync _repository;

            public GetDepartmentByIdQueryHandler(IDepartmentRepositoryAsync repository)
            {
                _repository = repository;
            }

            public async Task<Result<Department>> Handle(GetDepartmentByIdQuery query, CancellationToken cancellationToken)
            {
                var entity = await _repository.GetByIdAsync(query.Id);
                if (entity == null)
                {
                    throw new ApiException("Department Not Found.");
                }

                return Result<Department>.Success(entity);
            }
        }
    }
}
