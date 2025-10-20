using System.ComponentModel.DataAnnotations;

namespace Flexischools.Domain.Entities;

public class Student
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public int ParentId { get; set; }
    
    // Optional allergen information (e.g., "nuts,dairy,gluten")
    [MaxLength(200)]
    public string? Allergens { get; set; }
    
    // Navigation properties
    public virtual Parent Parent { get; set; } = null!;
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
