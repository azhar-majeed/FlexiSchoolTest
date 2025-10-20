using MediatR;
using Microsoft.Extensions.Logging;
using Flexischools.Application.Commands;
using Flexischools.Application.DTOs;
using Flexischools.Application.Interfaces;
using Flexischools.Domain.Exceptions;

namespace Flexischools.Application.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderService orderService,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString();
        
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["ParentId"] = request.ParentId,
            ["StudentId"] = request.StudentId,
            ["CanteenId"] = request.CanteenId,
            ["FulfilmentDate"] = request.FulfilmentDate,
            ["IdempotencyKey"] = request.IdempotencyKey ?? "none"
        });

        _logger.LogInformation("Starting order creation process");

        try
        {
            // Convert command to request DTO
            var createOrderRequest = new CreateOrderRequest
            {
                ParentId = request.ParentId,
                StudentId = request.StudentId,
                CanteenId = request.CanteenId,
                FulfilmentDate = request.FulfilmentDate,
                OrderItems = request.OrderItems,
                IdempotencyKey = request.IdempotencyKey
            };

            _logger.LogInformation("Order request created with {ItemCount} items", request.OrderItems.Count);

            var order = await _orderService.CreateOrderAsync(createOrderRequest);

            _logger.LogInformation("Order created successfully with ID {OrderId} and total amount {TotalAmount:C}", 
                order.Id, order.TotalAmount);

            return order;
        }
        catch (OrderCutOffExceededException ex)
        {
            _logger.LogWarning("Order creation failed due to cut-off time exceeded. Cut-off: {CutOffTime}, Requested: {RequestedTime}", 
                ex.CutOffTime, ex.RequestedTime);
            throw;
        }
        catch (InsufficientStockException ex)
        {
            _logger.LogWarning("Order creation failed due to insufficient stock. MenuItem: {MenuItemName}, Requested: {Requested}, Available: {Available}", 
                ex.MenuItemName, ex.RequestedQuantity, ex.AvailableStock);
            throw;
        }
        catch (InsufficientWalletBalanceException ex)
        {
            _logger.LogWarning("Order creation failed due to insufficient wallet balance. Required: {Required:C}, Available: {Available:C}", 
                ex.RequiredAmount, ex.AvailableBalance);
            throw;
        }
        catch (AllergenConflictException ex)
        {
            _logger.LogWarning("Order creation failed due to allergen conflict. Student: {StudentName}, MenuItem: {MenuItemName}, Allergens: {Allergens}", 
                ex.StudentName, ex.MenuItemName, ex.ConflictingAllergens);
            throw;
        }
        catch (DuplicateOrderException ex)
        {
            _logger.LogInformation("Duplicate order detected with idempotency key {IdempotencyKey}, returning existing order {OrderId}", 
                ex.IdempotencyKey, ex.ExistingOrderId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during order creation");
            throw;
        }
    }
}
