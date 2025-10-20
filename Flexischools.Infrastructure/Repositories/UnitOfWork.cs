using Microsoft.EntityFrameworkCore.Storage;
using Flexischools.Domain.Entities;
using Flexischools.Domain.Interfaces;
using Flexischools.Infrastructure.Data;

namespace Flexischools.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly FlexischoolsDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    private IRepository<Parent>? _parents;
    private IRepository<Student>? _students;
    private IRepository<Canteen>? _canteens;
    private IRepository<MenuItem>? _menuItems;
    private IOrderRepository? _orders;
    private IRepository<OrderItem>? _orderItems;

    public UnitOfWork(FlexischoolsDbContext context)
    {
        _context = context;
    }

    public IRepository<Parent> Parents => _parents ??= new Repository<Parent>(_context);
    public IRepository<Student> Students => _students ??= new Repository<Student>(_context);
    public IRepository<Canteen> Canteens => _canteens ??= new Repository<Canteen>(_context);
    public IRepository<MenuItem> MenuItems => _menuItems ??= new Repository<MenuItem>(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
    public IRepository<OrderItem> OrderItems => _orderItems ??= new Repository<OrderItem>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started");
        }
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction to commit");
        }

        try
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await _transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction to rollback");
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
            _disposed = true;
        }
    }
}
