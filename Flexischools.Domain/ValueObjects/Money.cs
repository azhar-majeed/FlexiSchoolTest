namespace Flexischools.Domain.ValueObjects;

public class Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    
    private Money() { } // For EF Core
    
    public Money(decimal amount, string currency = "AUD")
    {
        if (amount < 0)
            throw new ArgumentException("Money amount cannot be negative", nameof(amount));
            
        Amount = amount;
        Currency = currency;
    }
    
    public static Money Zero(string currency = "AUD") => new(0, currency);
    
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
            
        return new Money(left.Amount + right.Amount, left.Currency);
    }
    
    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");
            
        return new Money(left.Amount - right.Amount, left.Currency);
    }
    
    public static bool operator >=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");
            
        return left.Amount >= right.Amount;
    }
    
    public static bool operator <=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");
            
        return left.Amount <= right.Amount;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Money other && Amount == other.Amount && Currency == other.Currency;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }
    
    public override string ToString()
    {
        return $"{Amount:C} {Currency}";
    }
}
