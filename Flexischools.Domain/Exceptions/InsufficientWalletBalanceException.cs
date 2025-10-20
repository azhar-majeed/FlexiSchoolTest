namespace Flexischools.Domain.Exceptions;

public class InsufficientWalletBalanceException : Exception
{
    public decimal RequiredAmount { get; }
    public decimal AvailableBalance { get; }
    
    public InsufficientWalletBalanceException(decimal requiredAmount, decimal availableBalance)
        : base($"Insufficient wallet balance. Required: {requiredAmount:C}, Available: {availableBalance:C}")
    {
        RequiredAmount = requiredAmount;
        AvailableBalance = availableBalance;
    }
}
