namespace MyOnion.Application.Features.Employees.Commands.DeleteEmployeeById
{
    /// <summary>
    /// Command to delete an employee by id.
    /// </summary>
    public class DeleteEmployeeByIdCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }

        public class DeleteEmployeeByIdCommandHandler : IRequestHandler<DeleteEmployeeByIdCommand, Result<Guid>>
        {
            private readonly IEmployeeRepositoryAsync _repository;
            private readonly ICacheInvalidationService _cacheInvalidationService;

            public DeleteEmployeeByIdCommandHandler(
                IEmployeeRepositoryAsync repository,
                ICacheInvalidationService cacheInvalidationService)
            {
                _repository = repository;
                _cacheInvalidationService = cacheInvalidationService;
            }

            public async Task<Result<Guid>> Handle(DeleteEmployeeByIdCommand command, CancellationToken cancellationToken)
            {
                var entity = await _repository.GetByIdAsync(command.Id);
                if (entity == null)
                {
                    throw new ApiException("Employee Not Found.");
                }

                await _repository.DeleteAsync(entity);
                await _cacheInvalidationService.InvalidatePrefixAsync(CacheKeyPrefixes.EmployeesAll, cancellationToken);
                return Result<Guid>.Success(entity.Id);
            }
        }
    }
}
