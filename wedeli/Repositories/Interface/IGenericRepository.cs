using System.Linq.Expressions;

namespace wedeli.Repositories.Interface
{
    /// <summary>
    /// Generic Repository Interface - Base CRUD operations for all entities
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        // ============================================
        // READ OPERATIONS
        // ============================================

        /// <summary>
        /// Get entity by ID
        /// </summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Get entity by ID with related entities
        /// </summary>
        Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Get all entities
        /// </summary>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Get all entities with includes
        /// </summary>
        Task<List<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Find entities by condition
        /// </summary>
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Find entities by condition with includes
        /// </summary>
        Task<List<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Get first entity matching condition
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get first entity matching condition with includes
        /// </summary>
        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Check if any entity matches condition
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Count entities matching condition
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get total count of entities
        /// </summary>
        Task<int> CountAsync();

        // ============================================
        // PAGINATION
        // ============================================

        /// <summary>
        /// Get paginated entities
        /// </summary>
        Task<List<T>> GetPagedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Get paginated entities with condition
        /// </summary>
        Task<List<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get paginated entities with ordering
        /// </summary>
        Task<List<T>> GetPagedAsync<TKey>(
            int pageNumber,
            int pageSize,
            Expression<Func<T, TKey>> orderBy,
            bool ascending = true);

        // ============================================
        // CREATE OPERATIONS
        // ============================================

        /// <summary>
        /// Add single entity
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Add multiple entities
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities);

        // ============================================
        // UPDATE OPERATIONS
        // ============================================

        /// <summary>
        /// Update entity
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Update multiple entities
        /// </summary>
        void UpdateRange(IEnumerable<T> entities);

        // ============================================
        // DELETE OPERATIONS
        // ============================================

        /// <summary>
        /// Delete entity
        /// </summary>
        void Delete(T entity);

        /// <summary>
        /// Delete entity by ID
        /// </summary>
        Task<bool> DeleteByIdAsync(int id);

        /// <summary>
        /// Delete multiple entities
        /// </summary>
        void DeleteRange(IEnumerable<T> entities);

        /// <summary>
        /// Soft delete (if entity has IsDeleted property)
        /// </summary>
        Task<bool> SoftDeleteAsync(int id);

        // ============================================
        // SAVE OPERATIONS
        // ============================================

        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}