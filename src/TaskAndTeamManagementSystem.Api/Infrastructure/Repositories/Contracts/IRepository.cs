using System.Linq.Expressions;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using Task = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;

namespace TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;

public interface IRepository<T> where T : BaseEntity
{
    System.Threading.Tasks.Task<T?> GetByIdAsync(Guid id);
    System.Threading.Tasks.Task<IEnumerable<T>> GetAllAsync();
    System.Threading.Tasks.Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    System.Threading.Tasks.Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    System.Threading.Tasks.Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    System.Threading.Tasks.Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    System.Threading.Tasks.Task<T> AddAsync(T entity);
    System.Threading.Tasks.Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    IQueryable<T> Query();
}

