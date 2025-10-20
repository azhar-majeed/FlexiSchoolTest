using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.Extensions.Logging;
using Flexischools.Application.Commands;
using Flexischools.Application.Queries;
using Flexischools.Application.DTOs;
using Flexischools.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Flexischools.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new order with business rule validation
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <returns>Created order details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var correlationId = HttpContext.TraceIdentifier;
        
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Endpoint"] = "CreateOrder"
        });

        _logger.LogInformation("Received order creation request");

        try
        {
            // Extract Idempotency-Key from headers
            var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault();
            
            var command = new CreateOrderCommand
            {
                ParentId = request.ParentId,
                StudentId = request.StudentId,
                CanteenId = request.CanteenId,
                FulfilmentDate = request.FulfilmentDate,
                OrderItems = request.OrderItems,
                IdempotencyKey = idempotencyKey,
                CorrelationId = correlationId
            };

            var order = await _mediator.Send(command);

            _logger.LogInformation("Order created successfully with ID {OrderId}", order.Id);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (OrderCutOffExceededException ex)
        {
            _logger.LogWarning("Order creation failed: Cut-off time exceeded");
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Order Cut-off Time Exceeded",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Extensions = { ["cutOffTime"] = ex.CutOffTime, ["requestedTime"] = ex.RequestedTime }
            });
        }
        catch (InsufficientStockException ex)
        {
            _logger.LogWarning("Order creation failed: Insufficient stock for {MenuItemName}", ex.MenuItemName);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Insufficient Stock",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Extensions = { 
                    ["menuItemId"] = ex.MenuItemId,
                    ["menuItemName"] = ex.MenuItemName,
                    ["requestedQuantity"] = ex.RequestedQuantity,
                    ["availableStock"] = ex.AvailableStock
                }
            });
        }
        catch (InsufficientWalletBalanceException ex)
        {
            _logger.LogWarning("Order creation failed: Insufficient wallet balance");
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Insufficient Wallet Balance",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Extensions = { 
                    ["requiredAmount"] = ex.RequiredAmount,
                    ["availableBalance"] = ex.AvailableBalance
                }
            });
        }
        catch (AllergenConflictException ex)
        {
            _logger.LogWarning("Order creation failed: Allergen conflict for {StudentName}", ex.StudentName);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Allergen Conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Extensions = { 
                    ["studentName"] = ex.StudentName,
                    ["menuItemName"] = ex.MenuItemName,
                    ["conflictingAllergens"] = ex.ConflictingAllergens
                }
            });
        }
        catch (DuplicateOrderException ex)
        {
            _logger.LogInformation("Duplicate order detected, returning existing order");
            return Conflict(new ValidationProblemDetails
            {
                Title = "Duplicate Order",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict,
                Extensions = { 
                    ["idempotencyKey"] = ex.IdempotencyKey,
                    ["existingOrderId"] = ex.ExistingOrderId
                }
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Order creation failed: Validation error - {Message}", ex.Message);
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during order creation");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Retrieves order details by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponse>> GetOrder(int id)
    {
        var correlationId = HttpContext.TraceIdentifier;
        
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Endpoint"] = "GetOrder",
            ["OrderId"] = id
        });

        _logger.LogInformation("Retrieving order with ID {OrderId}", id);

        try
        {
            var query = new GetOrderByIdQuery
            {
                OrderId = id,
                CorrelationId = correlationId
            };

            var order = await _mediator.Send(query);

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", id);
                return NotFound(new ProblemDetails
                {
                    Title = "Order Not Found",
                    Detail = $"Order with ID {id} was not found",
                    Status = StatusCodes.Status404NotFound
                });
            }

            _logger.LogInformation("Order retrieved successfully");
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while retrieving the order",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}
