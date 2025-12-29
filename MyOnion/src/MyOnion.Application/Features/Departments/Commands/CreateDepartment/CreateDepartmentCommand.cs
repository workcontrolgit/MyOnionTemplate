namespace MyOnion.Application.Features.Departments.Commands.CreateDepartment
{
    /// <summary>
    /// Command to create a new department.
    /// </summary>
    public class CreateDepartmentCommand : IRequest<Result<Guid>>
    {
        /// <summary>
        /// Department name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Handles department creation and returns the identifier.
        /// </summary>
        public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<Guid>>
        {
            private readonly IDepartmentRepositoryAsync _repository;
            private readonly IMapper _mapper;

            public CreateDepartmentCommandHandler(IDepartmentRepositoryAsync repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<Result<Guid>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
            {
                var department = _mapper.Map<Department>(request);
                await _repository.AddAsync(department);
                return Result<Guid>.Success(department.Id);
            }
        }
    }
}
