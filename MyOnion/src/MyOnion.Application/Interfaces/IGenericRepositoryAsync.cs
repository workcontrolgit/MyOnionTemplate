namespace MyOnion.Application.Interfaces
{
    /// <summary>
    /// Interface for a generic repository that can handle CRUD operations and more on any entity of type T.
    /// </summary>
    public interface IGenericRepositoryAsync<T> where T : class
    {
        Task<T> GetByIdAsync(Guid id);

        Task<IEnumerable<T>> GetAllAsync();

        Task<T> AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task BulkInsertAsync(IEnumerable<T> entities);

        Task<IEnumerable<T>> GetPagedReponseAsync(int pageNumber, int pageSize);

        Task<IEnumerable<T>> GetPagedAdvancedReponseAsync(int pageNumber, int pageSize, string orderBy, string fields, ExpressionStarter<T> predicate);

        Task<IEnumerable<T>> GetAllShapeAsync(string orderBy, string fields);

        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification);

        Task<T> FirstOrDefaultAsync(ISpecification<T> specification);

        Task<int> CountAsync(ISpecification<T> specification);
    }
}
