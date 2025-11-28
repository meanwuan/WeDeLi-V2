using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo
{
    /// <summary>
    /// Generic Repository Implementation
    /// </summary>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<GenericRepository<T>> _logger;

        public GenericRepository(AppDbContext context, ILogger<GenericRepository<T>> logger)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _logger = logger;
        }

        // ============================================
        // READ OPERATIONS
        // ============================================

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting {typeof(T).Name} by ID: {id}");
                throw;
            }
        }

        public virtual async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            try
            {
                IQueryable<T> query = _dbSet;

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                return await query.FirstOrDefaultAsync(GetPrimaryKeyPredicate(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting {typeof(T).Name} by ID with includes: {id}");
                throw;
            }
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            try
            {
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting all {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<List<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            try
            {
                IQueryable<T> query = _dbSet;

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting all {typeof(T).Name} with includes");
                throw;
            }
        }

        public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<List<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            try
            {
                IQueryable<T> query = _dbSet.Where(predicate);

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error finding {typeof(T).Name} with includes");
                throw;
            }
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting first {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            try
            {
                IQueryable<T> query = _dbSet;

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                return await query.FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting first {typeof(T).Name} with includes");
                throw;
            }
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking existence of {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.CountAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error counting {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<int> CountAsync()
        {
            try
            {
                return await _dbSet.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error counting all {typeof(T).Name}");
                throw;
            }
        }

        // ============================================
        // PAGINATION
        // ============================================

        public virtual async Task<List<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                return await _dbSet
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting paged {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<List<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet
                    .Where(predicate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting filtered paged {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<List<T>> GetPagedAsync<TKey>(
            int pageNumber,
            int pageSize,
            Expression<Func<T, TKey>> orderBy,
            bool ascending = true)
        {
            try
            {
                IQueryable<T> query = _dbSet;

                query = ascending
                    ? query.OrderBy(orderBy)
                    : query.OrderByDescending(orderBy);

                return await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting ordered paged {typeof(T).Name}");
                throw;
            }
        }

        // ============================================
        // CREATE OPERATIONS
        // ============================================

        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);
                _logger.LogInformation($"{typeof(T).Name} added successfully");
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                await _dbSet.AddRangeAsync(entities);
                _logger.LogInformation($"Multiple {typeof(T).Name} added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding multiple {typeof(T).Name}");
                throw;
            }
        }

        // ============================================
        // UPDATE OPERATIONS
        // ============================================

        public virtual void Update(T entity)
        {
            try
            {
                _dbSet.Update(entity);
                _logger.LogInformation($"{typeof(T).Name} updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating {typeof(T).Name}");
                throw;
            }
        }

        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            try
            {
                _dbSet.UpdateRange(entities);
                _logger.LogInformation($"Multiple {typeof(T).Name} updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating multiple {typeof(T).Name}");
                throw;
            }
        }

        // ============================================
        // DELETE OPERATIONS
        // ============================================

        public virtual void Delete(T entity)
        {
            try
            {
                _dbSet.Remove(entity);
                _logger.LogInformation($"{typeof(T).Name} deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<bool> DeleteByIdAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning($"{typeof(T).Name} with ID {id} not found for deletion");
                    return false;
                }

                _dbSet.Remove(entity);
                _logger.LogInformation($"{typeof(T).Name} with ID {id} deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {typeof(T).Name} by ID: {id}");
                throw;
            }
        }

        public virtual void DeleteRange(IEnumerable<T> entities)
        {
            try
            {
                _dbSet.RemoveRange(entities);
                _logger.LogInformation($"Multiple {typeof(T).Name} deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting multiple {typeof(T).Name}");
                throw;
            }
        }

        public virtual async Task<bool> SoftDeleteAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity == null)
                {
                    return false;
                }

                // Check if entity has IsDeleted property
                var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
                if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
                {
                    isDeletedProperty.SetValue(entity, true);
                    Update(entity);
                    _logger.LogInformation($"{typeof(T).Name} with ID {id} soft deleted");
                    return true;
                }

                _logger.LogWarning($"{typeof(T).Name} does not support soft delete");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error soft deleting {typeof(T).Name} with ID: {id}");
                throw;
            }
        }

        // ============================================
        // SAVE OPERATIONS
        // ============================================

        public virtual async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database");
                throw;
            }
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        /// <summary>
        /// Get primary key predicate for entity
        /// </summary>
        private Expression<Func<T, bool>> GetPrimaryKeyPredicate(int id)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, GetPrimaryKeyName());
            var constant = Expression.Constant(id);
            var equality = Expression.Equal(property, constant);

            return Expression.Lambda<Func<T, bool>>(equality, parameter);
        }

        /// <summary>
        /// Get primary key property name
        /// </summary>
        private string GetPrimaryKeyName()
        {
            var entityType = _context.Model.FindEntityType(typeof(T));
            var primaryKey = entityType?.FindPrimaryKey();
            return primaryKey?.Properties.First().Name ?? "Id";
        }
    }
}