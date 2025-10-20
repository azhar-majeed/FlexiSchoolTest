using System.ComponentModel.DataAnnotations;
using Flexischools.Domain.Enums;

namespace Flexischools.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    
    [Required]
    public int ParentId { get; set; }
    
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    public int CanteenId { get; set; }
    
    [Required]
    public DateTime FulfilmentDate { get; set; }
    
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Idempotency key for duplicate request prevention
    [MaxLength(100)]
    public string? IdempotencyKey { get; set; }
    
    // Navigation properties
    public virtual Parent Parent { get; set; } = null!;
    public virtual Student Student { get; set; } = null!;
    public virtual Canteen Canteen { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    
    // Calculated property for total order amount
    public decimal TotalAmount => OrderItems.Sum(item => item.Quantity * item.MenuItem.Price);
    
    // Business methods
    public void Confirm()
    {
        if (Status != OrderStatus.Placed)
            throw new InvalidOperationException("Only placed orders can be confirmed");
            
        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Cancel()
    {
        if (Status == OrderStatus.Fulfilled)
            throw new InvalidOperationException("Cannot cancel fulfilled orders");
            
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Fulfill()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be fulfilled");
            
        Status = OrderStatus.Fulfilled;
        UpdatedAt = DateTime.UtcNow;
    }
}
