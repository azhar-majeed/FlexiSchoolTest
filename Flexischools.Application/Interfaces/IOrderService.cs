using Flexischools.Application.DTOs;

namespace Flexischools.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderResponse?> GetOrderByIdAsync(int orderId);
    Task<IEnumerable<OrderResponse>> GetOrdersByParentAsync(int parentId);
    Task<OrderResponse> UpdateOrderStatusAsync(int orderId, int status);
}
