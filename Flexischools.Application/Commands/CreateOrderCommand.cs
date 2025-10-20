using MediatR;
using Flexischools.Application.DTOs;

namespace Flexischools.Application.Commands;

public class CreateOrderCommand : IRequest<OrderResponse>
{
    public int ParentId { get; set; }
    public int StudentId { get; set; }
    public int CanteenId { get; set; }
    public DateTime FulfilmentDate { get; set; }
    public List<OrderItemRequest> OrderItems { get; set; } = new();
    public string? IdempotencyKey { get; set; }
    public string? CorrelationId { get; set; }
}
