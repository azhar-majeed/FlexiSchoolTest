namespace Flexischools.Domain.Exceptions;

public class DuplicateOrderException : Exception
{
    public string IdempotencyKey { get; }
    public int ExistingOrderId { get; }
    
    public DuplicateOrderException(string idempotencyKey, int existingOrderId)
        : base($"Order with idempotency key '{idempotencyKey}' already exists (Order ID: {existingOrderId})")
    {
        IdempotencyKey = idempotencyKey;
        ExistingOrderId = existingOrderId;
    }
}
