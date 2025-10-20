using System.ComponentModel.DataAnnotations;

namespace Flexischools.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    
    [Required]
    public int OrderId { get; set; }
    
    [Required]
    public int MenuItemId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual MenuItem MenuItem { get; set; } = null!;
    
    // Calculated property for line total
    public decimal LineTotal => Quantity * MenuItem.Price;
}
