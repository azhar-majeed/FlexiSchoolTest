using MediatR;
using Flexischools.Application.DTOs;

namespace Flexischools.Application.Queries;

public class GetOrderByIdQuery : IRequest<OrderResponse?>
{
    public int OrderId { get; set; }
    public string? CorrelationId { get; set; }
}
