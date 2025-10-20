using MediatR;
using Microsoft.Extensions.Logging;
using Flexischools.Application.Queries;
using Flexischools.Application.DTOs;
using Flexischools.Application.Interfaces;

namespace Flexischools.Application.Handlers;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(
        IOrderService orderService,
        ILogger<GetOrderByIdQueryHandler> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task<OrderResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString();
        
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["OrderId"] = request.OrderId
        });

        _logger.LogInformation("Retrieving order by ID {OrderId}", request.OrderId);

        try
        {
            var order = await _orderService.GetOrderByIdAsync(request.OrderId);
            
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", request.OrderId);
                return null;
            }

            _logger.LogInformation("Order retrieved successfully. Status: {Status}, Total: {TotalAmount:C}", 
                order.Status, order.TotalAmount);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving order {OrderId}", request.OrderId);
            throw;
        }
    }
}
