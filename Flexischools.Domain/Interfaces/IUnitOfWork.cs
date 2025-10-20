using Flexischools.Domain.Entities;

namespace Flexischools.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Parent> Parents { get; }
    IRepository<Student> Students { get; }
    IRepository<Canteen> Canteens { get; }
    IRepository<MenuItem> MenuItems { get; }
    IOrderRepository Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
