using Flexischools.Domain.Entities;

namespace Flexischools.Domain.Services;

public interface IOrderValidationService
{
    Task ValidateOrderCutOffAsync(Canteen canteen, DateTime fulfilmentDate, DateTime orderTime);
    Task ValidateStockAvailabilityAsync(IEnumerable<OrderItem> orderItems);
    Task ValidateWalletBalanceAsync(Parent parent, decimal orderTotal);
    Task ValidateAllergenConflictsAsync(Student student, IEnumerable<MenuItem> menuItems);
    Task ValidateIdempotencyAsync(string? idempotencyKey);
}
