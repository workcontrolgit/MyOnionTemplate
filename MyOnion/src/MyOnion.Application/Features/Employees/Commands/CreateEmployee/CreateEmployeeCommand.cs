namespace MyOnion.Application.Features.Employees.Commands.CreateEmployee
{
    /// <summary>
    /// Command to create a new employee.
    /// </summary>
    public class CreateEmployeeCommand : IRequest<Result<Guid>>
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public Guid PositionId { get; set; }
        public Guid DepartmentId { get; set; }
        public decimal Salary { get; set; }
        public DateTime Birthday { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
        public string EmployeeNumber { get; set; }
        public string Prefix { get; set; }
        public string Phone { get; set; }

        public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<Guid>>
        {
            private readonly IEmployeeRepositoryAsync _repository;
            private readonly IMapper _mapper;

            public CreateEmployeeCommandHandler(IEmployeeRepositoryAsync repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
            {
                var employee = _mapper.Map<Employee>(request);
                await _repository.AddAsync(employee);
                return Result<Guid>.Success(employee.Id);
            }
        }
    }
}
