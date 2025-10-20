using System.ComponentModel.DataAnnotations;

namespace Flexischools.Domain.Entities;

public class Canteen
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    // Opening days - stored as comma-separated values (e.g., "Monday,Tuesday,Wednesday")
    [MaxLength(200)]
    public string? OpeningDays { get; set; }
    
    // Order cut-off time per day (e.g., "09:30")
    [MaxLength(5)]
    public string? OrderCutOffTime { get; set; }
    
    // Navigation properties
    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
