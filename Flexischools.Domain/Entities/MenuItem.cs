using System.ComponentModel.DataAnnotations;

namespace Flexischools.Domain.Entities;

public class MenuItem
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative")]
    public decimal Price { get; set; }
    
    // Optional daily stock count
    public int? DailyStockCount { get; set; }
    
    // Optional allergen tags (e.g., "nuts,dairy,gluten")
    [MaxLength(200)]
    public string? AllergenTags { get; set; }
    
    [Required]
    public int CanteenId { get; set; }
    
    // Navigation properties
    public virtual Canteen Canteen { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
