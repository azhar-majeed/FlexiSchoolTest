namespace Flexischools.Domain.Exceptions;

public class InsufficientStockException : Exception
{
    public int MenuItemId { get; }
    public string MenuItemName { get; }
    public int RequestedQuantity { get; }
    public int AvailableStock { get; }
    
    public InsufficientStockException(int menuItemId, string menuItemName, int requestedQuantity, int availableStock)
        : base($"Insufficient stock for '{menuItemName}'. Requested: {requestedQuantity}, Available: {availableStock}")
    {
        MenuItemId = menuItemId;
        MenuItemName = menuItemName;
        RequestedQuantity = requestedQuantity;
        AvailableStock = availableStock;
    }
}
