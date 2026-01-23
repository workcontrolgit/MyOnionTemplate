using MyOnion.Application.Events;

namespace MyOnion.Application.Features.Employees.Commands.UpdateEmployee
{
    /// <summary>
    /// Command to update an employee record.
    /// </summary>
    public class UpdateEmployeeCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }
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

        public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result<Guid>>
        {
            private readonly IEmployeeRepositoryAsync _repository;
            private readonly IEventDispatcher _eventDispatcher;

            public UpdateEmployeeCommandHandler(
                IEmployeeRepositoryAsync repository,
                IEventDispatcher eventDispatcher)
            {
                _repository = repository;
                _eventDispatcher = eventDispatcher;
            }

            public async Task<Result<Guid>> Handle(UpdateEmployeeCommand command, CancellationToken cancellationToken)
            {
                var employee = await _repository.GetByIdAsync(command.Id);
                if (employee == null)
                {
                    throw new ApiException("Employee Not Found.");
                }

                employee.FirstName = command.FirstName;
                employee.MiddleName = command.MiddleName;
                employee.LastName = command.LastName;
                employee.PositionId = command.PositionId;
                employee.DepartmentId = command.DepartmentId;
                employee.Salary = command.Salary;
                employee.Birthday = command.Birthday;
                employee.Email = command.Email;
                employee.Gender = command.Gender;
                employee.EmployeeNumber = command.EmployeeNumber;
                employee.Prefix = command.Prefix;
                employee.Phone = command.Phone;

                await _repository.UpdateAsync(employee);
                await _eventDispatcher.PublishAsync(new EmployeeChangedEvent(employee.Id), cancellationToken);

                return Result<Guid>.Success(employee.Id);
            }
        }
    }
}
