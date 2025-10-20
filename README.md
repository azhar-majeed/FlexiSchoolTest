## Architecture Overview

**********************************************************************************************************************************
                o	### ************************* Design/architecture decisions *********************###
**********************************************************************************************************************************

### Domain Layer (`Flexischools.Domain`)
- **Entities**: Enhanced with business methods and new properties
- **Value Objects**: `IdempotencyKey`, `Money` for type safety
- **Enums**: Updated `OrderStatus` with `Confirmed` state
- **Exceptions**: Custom exceptions for each business rule violation
- **Services**: `OrderValidationService` for business rule validation
- **Interfaces**: Repository and unit of work patterns

### Application Layer (`Flexischools.Application`)
- **DTOs**: Request/Response objects for API contracts
- **Services**: `OrderService` orchestrating business logic
- **Interfaces**: Service contracts for dependency injection


##  **Project Structure**

```
Flexischools.Api/
├── Controllers/
│   ├── OrdersController.cs          # Main API endpoints
│   └── TestController.cs            # Testing endpoints
├── Program.cs                       # Application configuration
└── appsettings.json                 # Configuration

Flexischools.Application/
├── Commands/
│   └── CreateOrderCommand.cs        # CQRS command
├── Queries/
│   └── GetOrderByIdQuery.cs         # CQRS query
├── Handlers/
│   ├── CreateOrderCommandHandler.cs # Command handler
│   └── GetOrderByIdQueryHandler.cs  # Query handler
└── DTOs/
    ├── CreateOrderRequest.cs        # Request DTO
    └── OrderResponse.cs             # Response DTO

Flexischools.UnitTests/
├── Domain/Services/
│   └── OrderValidationServiceTests.cs
└── Application/Handlers/
    └── CreateOrderCommandHandlerTests.cs

Flexischools.IntegrationTests/
└── Controllers/
    └── OrdersControllerIntegrationTests.cs
```

### **Clean Architecture Layers**
```
Controllers (API) → MediatR → Application Services → Domain Services → Repositories → Database
```

##  **Non-Functional Requirements**

###  **Error Handling**
- **Structured Error Responses**: `ValidationProblemDetails` with detailed error information
- **Business Rule Exceptions**: Specific exceptions for each validation failure
- **HTTP Status Codes**: Proper status codes (400, 404, 409, 500)
- **Error Context**: Detailed error messages with relevant data

###  **Structured Logging**
- **Serilog Integration**: Console and file logging
- **Correlation IDs**: Request tracking across the application
- **Log Scopes**: Contextual logging with request metadata
- **Log Levels**: Appropriate logging levels (Information, Warning, Error)

###  **Configuration Management**
- **Connection Strings**: Environment-specific database connections
- **JWT Configuration**: Stubbed authentication settings
- **Logging Configuration**: Structured logging with Serilog
- **Health Checks**: Database connectivity monitoring

###  **Security (Lightweight)**
- **JWT Bearer Authentication**: Stubbed for future implementation
- **Authorization**: Ready for parent-student authorization
- **Swagger Security**: API documentation with security definitions
- **Request Validation**: Input validation and sanitization

**********************************************************************************************************************************
o	### ************************* Trade-offs and what you’d do next with more time - Future Enhancements *********************###
**********************************************************************************************************************************


### Potential Improvements
1. **Audit logging**: Track all business rule validations
2. **Metrics**: Monitor validation failure rates
3. **Caching**: Cache frequently accessed data (canteen settings, student allergens)
4. **Async processing**: Move stock updates to background jobs
5. **Event sourcing**: Track all order state changes
6. **Notification system**: Alert parents of order status changes
7. **More unit test**: More unit test could be added to cover more scenarios

**********************************************************************************************************************************
o	            ### ************************* How to run locally (including tests)*********************###
**********************************************************************************************************************************

1. From the Database foelder, find CreateFlexischoolsDatabase.sql script to create the database. README.md file in that folder also placed to explain about the basics of the database.
2. Change the connection string in launchSettings.json file in Flexischools.Api folder.
3. Set Flexischools.Api as your statup project in case it is not marked as startup project by default.
4. Execute it and you are good to test the APIs using swagger.
5. Find the Architecture diagram in folder named "Architecture Sketch"

Note. After running the sql script, you should get few records in each table by default as I included the insert scripts too for tables.
