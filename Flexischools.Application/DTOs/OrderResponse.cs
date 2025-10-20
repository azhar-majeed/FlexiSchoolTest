using Flexischools.Domain.Enums;

namespace Flexischools.Application.DTOs;

public class OrderResponse
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CanteenId { get; set; }
    public string CanteenName { get; set; } = string.Empty;
    public DateTime FulfilmentDate { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? IdempotencyKey { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemResponse> OrderItems { get; set; } = new();
}

public class OrderItemResponse
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public decimal MenuItemPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
