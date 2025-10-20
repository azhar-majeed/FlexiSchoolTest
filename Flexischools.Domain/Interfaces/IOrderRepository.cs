using Flexischools.Domain.Entities;

namespace Flexischools.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task<IEnumerable<Order>> GetOrdersByParentAsync(int parentId);
    Task<IEnumerable<Order>> GetOrdersByStudentAsync(int studentId);
    Task<IEnumerable<Order>> GetOrdersByCanteenAsync(int canteenId);
    Task<IEnumerable<Order>> GetOrdersByFulfilmentDateAsync(DateTime fulfilmentDate);
}
