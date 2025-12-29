namespace MyOnion.Infrastructure.Persistence.Repositories
{
    // Repository class for handling operations related to the SalaryRange entity asynchronously
    public class SalaryRangeRepositoryAsync : GenericRepositoryAsync<SalaryRange>, ISalaryRangeRepositoryAsync
    {
        // Entity framework set for interacting with the SalaryRange entities in the database
        private readonly DbSet<SalaryRange> _repository;
        private readonly IDataShapeHelper<SalaryRange> _dataShaper;

        // Constructor for the SalaryRangeRepositoryAsync class
        // Takes in the application's database context and passes it to the base class constructor
        public SalaryRangeRepositoryAsync(ApplicationDbContext dbContext, IDataShapeHelper<SalaryRange> dataShaper) : base(dbContext)
        {
            // Initialize the _repository field by associating it with the SalaryRange set in the database context
            _repository = dbContext.Set<SalaryRange>();
            _dataShaper = dataShaper;
        }

        public async Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetSalaryRangeResponseAsync(GetSalaryRangesQuery requestParameters)
        {
            var query = _repository.AsNoTracking();
            var recordsTotal = await query.CountAsync();

            var orderBy = string.IsNullOrEmpty(requestParameters.OrderBy) ? "Name" : requestParameters.OrderBy;
            var orderedQuery = query.OrderBy(orderBy);

            var pagedData = await orderedQuery
                .Skip((requestParameters.PageNumber - 1) * requestParameters.PageSize)
                .Take(requestParameters.PageSize)
                .ToListAsync();

            var shapedData = await _dataShaper.ShapeDataAsync(pagedData, requestParameters.Fields);
            var recordsCount = new RecordsCount
            {
                RecordsFiltered = recordsTotal,
                RecordsTotal = recordsTotal
            };

            return (shapedData, recordsCount);
        }
    }
}
