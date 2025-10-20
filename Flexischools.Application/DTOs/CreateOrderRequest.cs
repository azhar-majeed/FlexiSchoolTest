using System.ComponentModel.DataAnnotations;

namespace Flexischools.Application.DTOs;

public class CreateOrderRequest
{
    [Required]
    public int ParentId { get; set; }
    
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    public int CanteenId { get; set; }
    
    [Required]
    public DateTime FulfilmentDate { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "Order must contain at least one item")]
    public List<OrderItemRequest> OrderItems { get; set; } = new();
    
    public string? IdempotencyKey { get; set; }
}

public class OrderItemRequest
{
    [Required]
    public int MenuItemId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}
