using Flexischools.Application.DTOs;
using Flexischools.Application.Interfaces;
using Flexischools.Domain.Entities;
using Flexischools.Domain.Enums;
using Flexischools.Domain.Exceptions;
using Flexischools.Domain.Interfaces;
using Flexischools.Domain.Services;

namespace Flexischools.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderValidationService _validationService;

    public OrderService(IUnitOfWork unitOfWork, IOrderValidationService validationService)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Check for duplicate order (idempotency)
            if (!string.IsNullOrEmpty(request.IdempotencyKey))
            {
                var existingOrder = await _unitOfWork.Orders.GetByIdempotencyKeyAsync(request.IdempotencyKey);
                if (existingOrder != null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return MapToOrderResponse(existingOrder);
                }
            }

            // Load required entities
            var parent = await _unitOfWork.Parents.GetByIdAsync(request.ParentId);
            if (parent == null)
                throw new ArgumentException($"Parent with ID {request.ParentId} not found");

            var student = await _unitOfWork.Students.GetByIdAsync(request.StudentId);
            if (student == null)
                throw new ArgumentException($"Student with ID {request.StudentId} not found");

            var canteen = await _unitOfWork.Canteens.GetByIdAsync(request.CanteenId);
            if (canteen == null)
                throw new ArgumentException($"Canteen with ID {request.CanteenId} not found");

            // Load menu items
            var menuItemIds = request.OrderItems.Select(oi => oi.MenuItemId).ToList();
            var menuItems = new List<MenuItem>();
            foreach (var menuItemId in menuItemIds)
            {
                var menuItem = await _unitOfWork.MenuItems.GetByIdAsync(menuItemId);
                if (menuItem == null)
                    throw new ArgumentException($"MenuItem with ID {menuItemId} not found");
                menuItems.Add(menuItem);
            }

            // Create order items
            var orderItems = new List<OrderItem>();
            foreach (var itemRequest in request.OrderItems)
            {
                var menuItem = menuItems.First(mi => mi.Id == itemRequest.MenuItemId);
                var orderItem = new OrderItem
                {
                    MenuItemId = itemRequest.MenuItemId,
                    Quantity = itemRequest.Quantity
                };
                orderItems.Add(orderItem);
            }

            // Business rule validations
            await _validationService.ValidateOrderCutOffAsync(canteen, request.FulfilmentDate, DateTime.UtcNow);
            await _validationService.ValidateStockAvailabilityAsync(orderItems);
            await _validationService.ValidateWalletBalanceAsync(parent, CalculateOrderTotal(orderItems, menuItems));
            await _validationService.ValidateAllergenConflictsAsync(student, menuItems);

            // Create the order
            var order = new Order
            {
                ParentId = request.ParentId,
                StudentId = request.StudentId,
                CanteenId = request.CanteenId,
                FulfilmentDate = request.FulfilmentDate,
                IdempotencyKey = request.IdempotencyKey,
                Status = OrderStatus.Placed
            };

            // Add order items
            foreach (var orderItem in orderItems)
            {
                orderItem.OrderId = order.Id; // Will be set after order is saved
                order.OrderItems.Add(orderItem);
            }

            // Save order
            var savedOrder = await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Update order items with correct order ID
            foreach (var orderItem in savedOrder.OrderItems)
            {
                orderItem.OrderId = savedOrder.Id;
                await _unitOfWork.OrderItems.UpdateAsync(orderItem);
            }

            // Confirm order and apply side effects
            savedOrder.Confirm();
            
            // Decrement stock
            await DecrementStockAsync(orderItems, menuItems);
            
            // Debit parent's wallet
            await DebitParentWalletAsync(parent, CalculateOrderTotal(orderItems, menuItems));

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return MapToOrderResponse(savedOrder);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(int orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        return order != null ? MapToOrderResponse(order) : null;
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersByParentAsync(int parentId)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByParentAsync(parentId);
        return orders.Select(MapToOrderResponse);
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(int orderId, int status)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        var newStatus = (OrderStatus)status;
        
        switch (newStatus)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Fulfilled:
                order.Fulfill();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
            default:
                throw new ArgumentException($"Invalid order status: {status}");
        }

        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();

        return MapToOrderResponse(order);
    }

    private decimal CalculateOrderTotal(List<OrderItem> orderItems, List<MenuItem> menuItems)
    {
        return orderItems.Sum(oi => 
        {
            var menuItem = menuItems.First(mi => mi.Id == oi.MenuItemId);
            return oi.Quantity * menuItem.Price;
        });
    }

    private async Task DecrementStockAsync(List<OrderItem> orderItems, List<MenuItem> menuItems)
    {
        foreach (var orderItem in orderItems)
        {
            var menuItem = menuItems.First(mi => mi.Id == orderItem.MenuItemId);
            if (menuItem.DailyStockCount.HasValue)
            {
                menuItem.DailyStockCount -= orderItem.Quantity;
                await _unitOfWork.MenuItems.UpdateAsync(menuItem);
            }
        }
    }

    private async Task DebitParentWalletAsync(Parent parent, decimal amount)
    {
        parent.WalletBalance -= amount;
        await _unitOfWork.Parents.UpdateAsync(parent);
    }

    private OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            ParentId = order.ParentId,
            ParentName = order.Parent?.Name ?? string.Empty,
            StudentId = order.StudentId,
            StudentName = order.Student?.Name ?? string.Empty,
            CanteenId = order.CanteenId,
            CanteenName = order.Canteen?.Name ?? string.Empty,
            FulfilmentDate = order.FulfilmentDate,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            IdempotencyKey = order.IdempotencyKey,
            TotalAmount = order.TotalAmount,
            OrderItems = order.OrderItems.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                MenuItemId = oi.MenuItemId,
                MenuItemName = oi.MenuItem?.Name ?? string.Empty,
                MenuItemPrice = oi.MenuItem?.Price ?? 0,
                Quantity = oi.Quantity,
                LineTotal = oi.LineTotal
            }).ToList()
        };
    }
}
