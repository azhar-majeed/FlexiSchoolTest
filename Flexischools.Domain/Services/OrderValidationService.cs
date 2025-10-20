using Flexischools.Domain.Entities;
using Flexischools.Domain.Exceptions;
using Flexischools.Domain.Interfaces;

namespace Flexischools.Domain.Services;

public class OrderValidationService : IOrderValidationService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderValidationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task ValidateOrderCutOffAsync(Canteen canteen, DateTime fulfilmentDate, DateTime orderTime)
    {
        if (string.IsNullOrEmpty(canteen.OrderCutOffTime))
            return Task.CompletedTask; // No cut-off time set, allow order

        // Parse cut-off time (format: "HH:mm")
        if (!TimeSpan.TryParse(canteen.OrderCutOffTime, out var cutOffTime))
            return Task.CompletedTask; // Invalid format, allow order

        // Create cut-off datetime for the fulfilment date
        var cutOffDateTime = fulfilmentDate.Date.Add(cutOffTime);

        if (orderTime > cutOffDateTime)
        {
            throw new OrderCutOffExceededException(cutOffDateTime, orderTime);
        }
        
        return Task.CompletedTask;
    }

    public async Task ValidateStockAvailabilityAsync(IEnumerable<OrderItem> orderItems)
    {
        foreach (var orderItem in orderItems)
        {
            var menuItem = await _unitOfWork.MenuItems.GetByIdAsync(orderItem.MenuItemId);
            if (menuItem == null)
                continue; // Skip validation if menu item not found

            // If no stock count is set, assume unlimited stock
            if (menuItem.DailyStockCount == null)
                continue;

            var availableStock = menuItem.DailyStockCount.Value;
            if (orderItem.Quantity > availableStock)
            {
                throw new InsufficientStockException(
                    menuItem.Id, 
                    menuItem.Name, 
                    orderItem.Quantity, 
                    availableStock);
            }
        }
    }

    public Task ValidateWalletBalanceAsync(Parent parent, decimal orderTotal)
    {
        if (parent.WalletBalance < orderTotal)
        {
            throw new InsufficientWalletBalanceException(orderTotal, parent.WalletBalance);
        }
        
        return Task.CompletedTask;
    }

    public Task ValidateAllergenConflictsAsync(Student student, IEnumerable<MenuItem> menuItems)
    {
        if (string.IsNullOrEmpty(student.Allergens))
            return Task.CompletedTask; // No allergens recorded for student

        var studentAllergens = student.Allergens.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(a => a.Trim().ToLowerInvariant())
            .ToHashSet();

        foreach (var menuItem in menuItems)
        {
            if (string.IsNullOrEmpty(menuItem.AllergenTags))
                continue; // No allergens in menu item

            var menuItemAllergens = menuItem.AllergenTags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim().ToLowerInvariant())
                .ToHashSet();

            var conflictingAllergens = studentAllergens.Intersect(menuItemAllergens).ToList();
            if (conflictingAllergens.Any())
            {
                throw new AllergenConflictException(
                    student.Name,
                    menuItem.Name,
                    string.Join(", ", conflictingAllergens));
            }
        }
        
        return Task.CompletedTask;
    }

    public Task ValidateIdempotencyAsync(string? idempotencyKey)
    {
        if (string.IsNullOrEmpty(idempotencyKey))
            return Task.CompletedTask; // No idempotency key provided

        // Note: This validation should be done in the application service where we have async context
        // For now, we'll just return completed task
        return Task.CompletedTask;
    }
}
