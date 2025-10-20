using Microsoft.EntityFrameworkCore;
using Flexischools.Domain.Entities;
using Flexischools.Domain.Interfaces;
using Flexischools.Infrastructure.Data;

namespace Flexischools.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(FlexischoolsDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        return await _dbSet
            .Include(o => o.Parent)
            .Include(o => o.Student)
            .Include(o => o.Canteen)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey);
    }

    public async Task<IEnumerable<Order>> GetOrdersByParentAsync(int parentId)
    {
        return await _dbSet
            .Include(o => o.Parent)
            .Include(o => o.Student)
            .Include(o => o.Canteen)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.ParentId == parentId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByStudentAsync(int studentId)
    {
        return await _dbSet
            .Include(o => o.Parent)
            .Include(o => o.Student)
            .Include(o => o.Canteen)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.StudentId == studentId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByCanteenAsync(int canteenId)
    {
        return await _dbSet
            .Include(o => o.Parent)
            .Include(o => o.Student)
            .Include(o => o.Canteen)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.CanteenId == canteenId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByFulfilmentDateAsync(DateTime fulfilmentDate)
    {
        return await _dbSet
            .Include(o => o.Parent)
            .Include(o => o.Student)
            .Include(o => o.Canteen)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.FulfilmentDate.Date == fulfilmentDate.Date)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
    }

    public override async Task<Order?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(o => o.Parent)
            .Include(o => o.Student)
            .Include(o => o.Canteen)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
