using NUnit.Framework;
using FluentAssertions;
using Moq;
using Flexischools.Domain.Entities;
using Flexischools.Domain.Services;
using Flexischools.Domain.Interfaces;
using Flexischools.Domain.Exceptions;

namespace Flexischools.UnitTests.Domain.Services;

[TestFixture]
public class OrderValidationServiceTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private OrderValidationService _validationService;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _validationService = new OrderValidationService(_mockUnitOfWork.Object);
    }

    [Test]
    public void ValidateOrderCutOffAsync_WhenNoCutOffTime_ShouldNotThrow()
    {
        // Arrange
        var canteen = new Canteen { OrderCutOffTime = null };
        var fulfilmentDate = DateTime.Today.AddDays(1);
        var orderTime = DateTime.Now;

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => 
            await _validationService.ValidateOrderCutOffAsync(canteen, fulfilmentDate, orderTime));
    }

    [Test]
    public void ValidateOrderCutOffAsync_WhenOrderTimeBeforeCutOff_ShouldNotThrow()
    {
        // Arrange
        var canteen = new Canteen { OrderCutOffTime = "10:00" };
        var fulfilmentDate = DateTime.Today.AddDays(1);
        var orderTime = DateTime.Today.AddDays(1).Date.AddHours(9); // 9:00 AM

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => 
            await _validationService.ValidateOrderCutOffAsync(canteen, fulfilmentDate, orderTime));
    }

    [Test]
    public void ValidateOrderCutOffAsync_WhenOrderTimeAfterCutOff_ShouldThrowException()
    {
        // Arrange
        var canteen = new Canteen { OrderCutOffTime = "09:30" };
        var fulfilmentDate = DateTime.Today.AddDays(1);
        var orderTime = DateTime.Today.AddDays(1).Date.AddHours(10); // 10:00 AM

        // Act & Assert
        var exception = Assert.ThrowsAsync<OrderCutOffExceededException>(async () => 
            await _validationService.ValidateOrderCutOffAsync(canteen, fulfilmentDate, orderTime));
        
        exception.Should().NotBeNull();
        exception.CutOffTime.Should().Be(fulfilmentDate.Date.AddHours(9).AddMinutes(30));
        exception.RequestedTime.Should().Be(orderTime);
    }

    [Test]
    public void ValidateWalletBalanceAsync_WhenSufficientBalance_ShouldNotThrow()
    {
        // Arrange
        var parent = new Parent { WalletBalance = 100.00m };
        var orderTotal = 50.00m;

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => 
            await _validationService.ValidateWalletBalanceAsync(parent, orderTotal));
    }

    [Test]
    public void ValidateWalletBalanceAsync_WhenInsufficientBalance_ShouldThrowException()
    {
        // Arrange
        var parent = new Parent { WalletBalance = 30.00m };
        var orderTotal = 50.00m;

        // Act & Assert
        var exception = Assert.ThrowsAsync<InsufficientWalletBalanceException>(async () => 
            await _validationService.ValidateWalletBalanceAsync(parent, orderTotal));
        
        exception.Should().NotBeNull();
        exception.RequiredAmount.Should().Be(50.00m);
        exception.AvailableBalance.Should().Be(30.00m);
    }

    [Test]
    public void ValidateAllergenConflictsAsync_WhenNoStudentAllergens_ShouldNotThrow()
    {
        // Arrange
        var student = new Student { Allergens = null };
        var menuItems = new List<MenuItem>
        {
            new() { AllergenTags = "nuts,dairy" }
        };

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => 
            await _validationService.ValidateAllergenConflictsAsync(student, menuItems));
    }

    [Test]
    public void ValidateAllergenConflictsAsync_WhenNoConflicts_ShouldNotThrow()
    {
        // Arrange
        var student = new Student { Allergens = "gluten" };
        var menuItems = new List<MenuItem>
        {
            new() { AllergenTags = "nuts,dairy" }
        };

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => 
            await _validationService.ValidateAllergenConflictsAsync(student, menuItems));
    }

    [Test]
    public void ValidateAllergenConflictsAsync_WhenAllergenConflict_ShouldThrowException()
    {
        // Arrange
        var student = new Student { Name = "John", Allergens = "nuts,dairy" };
        var menuItems = new List<MenuItem>
        {
            new() { Name = "Chocolate Bar", AllergenTags = "nuts,gluten" }
        };

        // Act & Assert
        var exception = Assert.ThrowsAsync<AllergenConflictException>(async () => 
            await _validationService.ValidateAllergenConflictsAsync(student, menuItems));
        
        exception.Should().NotBeNull();
        exception.StudentName.Should().Be("John");
        exception.MenuItemName.Should().Be("Chocolate Bar");
        exception.ConflictingAllergens.Should().Be("nuts");
    }

    [Test]
    public void ValidateAllergenConflictsAsync_WhenCaseInsensitiveConflict_ShouldThrowException()
    {
        // Arrange
        var student = new Student { Name = "Jane", Allergens = "NUTS,DAIRY" };
        var menuItems = new List<MenuItem>
        {
            new() { Name = "Milk Shake", AllergenTags = "nuts,dairy" }
        };

        // Act & Assert
        var exception = Assert.ThrowsAsync<AllergenConflictException>(async () => 
            await _validationService.ValidateAllergenConflictsAsync(student, menuItems));
        
        exception.Should().NotBeNull();
        exception.ConflictingAllergens.Should().Contain("nuts");
        exception.ConflictingAllergens.Should().Contain("dairy");
    }
}
