using TaskAndTeamManagementSystem.Api.Domain.Entities;
using Task = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;

namespace TaskAndTeamManagementSystem.Api.Infrastructure.Repositories.Contracts;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    System.Threading.Tasks.Task<int> SaveChangesAsync();
    System.Threading.Tasks.Task BeginTransactionAsync();
    System.Threading.Tasks.Task CommitTransactionAsync();
    System.Threading.Tasks.Task RollbackTransactionAsync();
}

