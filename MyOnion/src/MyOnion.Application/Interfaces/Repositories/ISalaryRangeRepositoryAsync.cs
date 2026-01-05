namespace MyOnion.Application.Interfaces.Repositories
{
    /// <summary>
    /// Represents a repository for performing asynchronous operations on salary ranges.
    /// </summary>
    public interface ISalaryRangeRepositoryAsync : IGenericRepositoryAsync<SalaryRange>
    {
        Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetSalaryRangeResponseAsync(GetSalaryRangesQuery requestParameters);
    }
}
