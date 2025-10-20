using System.ComponentModel.DataAnnotations;

namespace Flexischools.Domain.Entities;

public class Parent
{
    public int Id { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Wallet balance cannot be negative")]
    public decimal WalletBalance { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
