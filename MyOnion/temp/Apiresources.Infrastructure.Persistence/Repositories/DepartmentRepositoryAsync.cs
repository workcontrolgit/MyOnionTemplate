namespace $safeprojectname$.Repositories
{
    /// <summary>
    /// Represents a repository for asynchronous operations on the Department entity.
    /// </summary>
    public class DepartmentRepositoryAsync : GenericRepositoryAsync<Department>, IDepartmentRepositoryAsync
    {
        private readonly DbSet<Department> _repository;
        private readonly IDataShapeHelper<Department> _dataShaper;

        /// <summary>
        /// Initializes a new instance of the DepartmentRepositoryAsync class with the provided database context.
        /// </summary>
        /// <param name="dbContext">The application's DbContext.</param>
        public DepartmentRepositoryAsync(ApplicationDbContext dbContext, IDataShapeHelper<Department> dataShaper) : base(dbContext)
        {
            _repository = dbContext.Set<Department>();
            _dataShaper = dataShaper;
        }

        public async Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetDepartmentResponseAsync(GetDepartmentsQuery requestParameters)
        {
            var recordsTotal = await _repository.CountAsync();

            var filteredSpecification = new DepartmentsByFiltersSpecification(requestParameters, applyPaging: false);
            var pagedSpecification = new DepartmentsByFiltersSpecification(requestParameters);

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

