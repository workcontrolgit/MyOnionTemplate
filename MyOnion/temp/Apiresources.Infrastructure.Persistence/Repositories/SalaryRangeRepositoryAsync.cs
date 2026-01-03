namespace $safeprojectname$.Repositories
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
            var recordsTotal = await _repository.CountAsync();

            var filteredSpecification = new SalaryRangesByFiltersSpecification(requestParameters, applyPaging: false);
            var pagedSpecification = new SalaryRangesByFiltersSpecification(requestParameters);

            var recordsFiltered = await CountAsync(filteredSpecification);
            var resultData = await ListAsync(pagedSpecification);

            var shapedData = _dataShaper.ShapeData(resultData, requestParameters.Fields);
            var recordsCount = BuildRecordsCount(recordsTotal, recordsFiltered);

            return (shapedData, recordsCount);
        }

        private static RecordsCount BuildRecordsCount(int total, int filtered)
        {
            return new RecordsCount
            {
                RecordsFiltered = filtered,
                RecordsTotal = total
            };
        }
    }
}

