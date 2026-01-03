namespace $safeprojectname$.Features.SalaryRanges.Commands.CreateSalaryRange
{
    /// <summary>
    /// Command to create a salary range.
    /// </summary>
    public class CreateSalaryRangeCommand : IRequest<Result<Guid>>
    {
        public string Name { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }

        public class CreateSalaryRangeCommandHandler : IRequestHandler<CreateSalaryRangeCommand, Result<Guid>>
        {
            private readonly ISalaryRangeRepositoryAsync _repository;
            private readonly IMapper _mapper;

            public CreateSalaryRangeCommandHandler(ISalaryRangeRepositoryAsync repository, IMapper mapper)
            {
                _repository = repository;
                _mapper = mapper;
            }

            public async Task<Result<Guid>> Handle(CreateSalaryRangeCommand request, CancellationToken cancellationToken)
            {
                var salaryRange = _mapper.Map<SalaryRange>(request);
                await _repository.AddAsync(salaryRange);
                return Result<Guid>.Success(salaryRange.Id);
            }
        }
    }
}

