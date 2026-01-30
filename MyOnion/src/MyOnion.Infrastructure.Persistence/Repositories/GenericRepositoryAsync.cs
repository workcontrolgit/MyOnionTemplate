using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

namespace MyOnion.Infrastructure.Persistence.Repository
{
    public class GenericRepositoryAsync<T> : IGenericRepositoryAsync<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public GenericRepositoryAsync(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task BulkInsertAsync(IEnumerable<T> entities)
        {
            await _dbContext.BulkInsertAsync(entities);
        }

        public async Task<IEnumerable<T>> GetPagedReponseAsync(int pageNumber, int pageSize)
        {
            return await _dbSet
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllShapeAsync(string orderBy, string fields)
        {
            return await _dbSet
                .Select<T>("new(" + fields + ")")
                .OrderBy(orderBy)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification)
        {
            // Use Ardalis evaluator - tracking is controlled by specification
            return await SpecificationEvaluator.Default
                .GetQuery(_dbSet.AsQueryable(), specification)
                .ToListAsync();
        }

        public async Task<T> FirstOrDefaultAsync(ISpecification<T> specification)
        {
            // Use Ardalis evaluator - tracking is controlled by specification
            return await SpecificationEvaluator.Default
                .GetQuery(_dbSet.AsQueryable(), specification)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CountAsync(ISpecification<T> specification)
        {
            // Use Ardalis evaluator
            return await SpecificationEvaluator.Default
                .GetQuery(_dbSet.AsQueryable(), specification)
                .CountAsync();
        }
    }
}
