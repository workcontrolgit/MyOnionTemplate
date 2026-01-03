namespace $safeprojectname$.Repositories
{
    public class PositionRepositoryAsync : GenericRepositoryAsync<Position>, IPositionRepositoryAsync
    {
        private readonly DbSet<Position> _repository;
        private readonly IDataShapeHelper<Position> _dataShaper;
        private readonly IMockService _mockData;

        public PositionRepositoryAsync(
            ApplicationDbContext dbContext,
            IDataShapeHelper<Position> dataShaper,
            IMockService mockData) : base(dbContext)
        {
            _repository = dbContext.Set<Position>();
            _dataShaper = dataShaper;
            _mockData = mockData;
        }

        public async Task<bool> IsUniquePositionNumberAsync(string positionNumber)
        {
            return !await _repository.AnyAsync(p => p.PositionNumber == positionNumber);
        }

        public async Task<bool> SeedDataAsync(int rowCount, IEnumerable<Department> departments, IEnumerable<SalaryRange> salaryRanges)
        {
            await BulkInsertAsync(_mockData.GetPositions(rowCount, departments, salaryRanges));
            return true;
        }

        public async Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPositionReponseAsync(GetPositionsQuery requestParameters)
        {
            var recordsTotal = await _repository.CountAsync();

            var filteredSpecification = new PositionsByFiltersSpecification(requestParameters, applyPaging: false);
            var pagedSpecification = new PositionsByFiltersSpecification(requestParameters);

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

