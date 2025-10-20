using System.ComponentModel.DataAnnotations;

namespace Flexischools.Domain.ValueObjects;

public class IdempotencyKey
{
    [Required]
    [MaxLength(100)]
    public string Value { get; private set; } = string.Empty;
    
    public DateTime CreatedAt { get; private set; }
    
    private IdempotencyKey() { } // For EF Core
    
    public IdempotencyKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Idempotency key cannot be null or empty", nameof(value));
            
        Value = value;
        CreatedAt = DateTime.UtcNow;
    }
    
    public bool IsExpired => DateTime.UtcNow > CreatedAt.AddHours(24);
    
    public override bool Equals(object? obj)
    {
        return obj is IdempotencyKey other && Value == other.Value;
    }
    
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    
    public override string ToString()
    {
        return Value;
    }
}
