using NUnit.Framework;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Flexischools.Application.Commands;
using Flexischools.Application.Handlers;
using Flexischools.Application.Interfaces;
using Flexischools.Application.DTOs;
using Flexischools.Domain.Entities;
using Flexischools.Domain.Enums;
using Flexischools.Domain.Exceptions;

namespace Flexischools.UnitTests.Application.Handlers;

[TestFixture]
public class CreateOrderCommandHandlerTests
{
    private Mock<IOrderService> _mockOrderService;
    private Mock<ILogger<CreateOrderCommandHandler>> _mockLogger;
    private CreateOrderCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockOrderService = new Mock<IOrderService>();
        _mockLogger = new Mock<ILogger<CreateOrderCommandHandler>>();
        _handler = new CreateOrderCommandHandler(_mockOrderService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task Handle_WhenValidCommand_ShouldReturnOrderResponse()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            ParentId = 1,
            StudentId = 1,
            CanteenId = 1,
            FulfilmentDate = DateTime.Today.AddDays(1),
            OrderItems = new List<OrderItemRequest>
            {
                new() { MenuItemId = 1, Quantity = 2 }
            },
            IdempotencyKey = "test-key",
            CorrelationId = "correlation-123"
        };

        var expectedResponse = new OrderResponse
        {
            Id = 1,
            ParentId = 1,
            StudentId = 1,
            CanteenId = 1,
            Status = OrderStatus.Confirmed,
            TotalAmount = 13.00m
        };

        _mockOrderService.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Status.Should().Be(OrderStatus.Confirmed);
        result.TotalAmount.Should().Be(13.00m);

        _mockOrderService.Verify(x => x.CreateOrderAsync(It.Is<CreateOrderRequest>(r =>
            r.ParentId == command.ParentId &&
            r.StudentId == command.StudentId &&
            r.CanteenId == command.CanteenId &&
            r.IdempotencyKey == command.IdempotencyKey
        )), Times.Once);
    }

    [Test]
    public async Task Handle_WhenCutOffExceeded_ShouldThrowException()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            ParentId = 1,
            StudentId = 1,
            CanteenId = 1,
            FulfilmentDate = DateTime.Today.AddDays(1),
            OrderItems = new List<OrderItemRequest>()
        };

        _mockOrderService.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
            .ThrowsAsync(new OrderCutOffExceededException(DateTime.Now, DateTime.Now.AddHours(1)));

        // Act & Assert
        var exception = Assert.ThrowsAsync<OrderCutOffExceededException>(async () =>
            await _handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
    }

    [Test]
    public async Task Handle_WhenInsufficientStock_ShouldThrowException()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            ParentId = 1,
            StudentId = 1,
            CanteenId = 1,
            FulfilmentDate = DateTime.Today.AddDays(1),
            OrderItems = new List<OrderItemRequest>()
        };

        _mockOrderService.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
            .ThrowsAsync(new InsufficientStockException(1, "Test Item", 5, 3));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InsufficientStockException>(async () =>
            await _handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.MenuItemId.Should().Be(1);
        exception.MenuItemName.Should().Be("Test Item");
        exception.RequestedQuantity.Should().Be(5);
        exception.AvailableStock.Should().Be(3);
    }

    [Test]
    public async Task Handle_WhenInsufficientWalletBalance_ShouldThrowException()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            ParentId = 1,
            StudentId = 1,
            CanteenId = 1,
            FulfilmentDate = DateTime.Today.AddDays(1),
            OrderItems = new List<OrderItemRequest>()
        };

        _mockOrderService.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
            .ThrowsAsync(new InsufficientWalletBalanceException(100.00m, 50.00m));

        // Act & Assert
        var exception = Assert.ThrowsAsync<InsufficientWalletBalanceException>(async () =>
            await _handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.RequiredAmount.Should().Be(100.00m);
        exception.AvailableBalance.Should().Be(50.00m);
    }

    [Test]
    public async Task Handle_WhenAllergenConflict_ShouldThrowException()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            ParentId = 1,
            StudentId = 1,
            CanteenId = 1,
            FulfilmentDate = DateTime.Today.AddDays(1),
            OrderItems = new List<OrderItemRequest>()
        };

        _mockOrderService.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
            .ThrowsAsync(new AllergenConflictException("John", "Peanut Butter", "nuts"));

        // Act & Assert
        var exception = Assert.ThrowsAsync<AllergenConflictException>(async () =>
            await _handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.StudentName.Should().Be("John");
        exception.MenuItemName.Should().Be("Peanut Butter");
        exception.ConflictingAllergens.Should().Be("nuts");
    }

    [Test]
    public async Task Handle_WhenDuplicateOrder_ShouldThrowException()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            ParentId = 1,
            StudentId = 1,
            CanteenId = 1,
            FulfilmentDate = DateTime.Today.AddDays(1),
            OrderItems = new List<OrderItemRequest>(),
            IdempotencyKey = "duplicate-key"
        };

        _mockOrderService.Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
            .ThrowsAsync(new DuplicateOrderException("duplicate-key", 123));

        // Act & Assert
        var exception = Assert.ThrowsAsync<DuplicateOrderException>(async () =>
            await _handler.Handle(command, CancellationToken.None));

        exception.Should().NotBeNull();
        exception.IdempotencyKey.Should().Be("duplicate-key");
        exception.ExistingOrderId.Should().Be(123);
    }
}
