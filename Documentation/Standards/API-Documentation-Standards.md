# API Documentation Standards

## Overview

This document defines the standards for documenting APIs in the Retail Microservices .NET 8 solution. Consistent API documentation improves developer experience and ensures clear service contracts.

## Table of Contents

1. [OpenAPI/Swagger Documentation](#openapiswagger-documentation)
2. [XML Documentation Comments](#xml-documentation-comments)
3. [AsyncAPI Documentation](#asyncapi-documentation)
4. [API Versioning](#api-versioning)
5. [Error Documentation](#error-documentation)
6. [Examples and Best Practices](#examples-and-best-practices)

---

## OpenAPI/Swagger Documentation

### Purpose

OpenAPI/Swagger documentation provides interactive API documentation that:
- Describes all endpoints
- Shows request/response schemas
- Allows testing endpoints
- Generates client SDKs

### Implementation

All REST API services use Swashbuckle.AspNetCore to generate OpenAPI documentation:

```csharp
// Startup.cs
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Retail Customers API",
        Version = "v1",
        Description = "API for managing customer data",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "api-support@example.com"
        }
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

### Accessing Swagger UI

- **Development**: http://localhost:{port}/swagger
- **Production**: Disabled by default (enable only for internal APIs)

---

## XML Documentation Comments

### Required Documentation

All public APIs must have XML documentation comments:

- **Controllers**: Class-level documentation
- **Action Methods**: Method-level documentation
- **DTOs**: Property-level documentation (if complex)
- **Enums**: Enum and value documentation

### Documentation Format

```csharp
/// <summary>
/// Creates a new customer in the system.
/// </summary>
/// <param name="customerDto">The customer data to create. Must include valid email address.</param>
/// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
/// <returns>
/// A result containing the created customer with assigned ID, or an error if creation fails.
/// Returns 201 Created on success, 400 Bad Request on validation failure, or 500 Internal Server Error on system error.
/// </returns>
/// <remarks>
/// The email address must be unique. If a customer with the same email already exists,
/// the operation will fail with a validation error.
/// </remarks>
/// <response code="201">Customer created successfully</response>
/// <response code="400">Invalid customer data</response>
/// <response code="500">Internal server error</response>
[HttpPost]
[ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> CreateCustomerAsync(
    [FromBody] CustomerDto customerDto,
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### XML Comment Tags

- **`<summary>`**: Brief description (required)
- **`<param>`**: Parameter description (required for all parameters)
- **`<returns>`**: Return value description (required)
- **`<remarks>`**: Additional information (optional)
- **`<example>`**: Code example (optional)
- **`<exception>`**: Exception documentation (optional)

### DTO Documentation

```csharp
/// <summary>
/// Represents customer information in the system.
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the customer.
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the customer's full name.
    /// </summary>
    /// <example>John Doe</example>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the customer's email address. Must be unique and valid.
    /// </summary>
    /// <example>john.doe@example.com</example>
    public string Email { get; set; }
}
```

---

## AsyncAPI Documentation

### Purpose

AsyncAPI documentation describes messaging contracts between services:
- Event schemas
- Channel definitions
- Message routing
- Version compatibility

### AsyncAPI Specification

Each service has an AsyncAPI YAML file in `Contracts/AsyncAPI/`:

```yaml
asyncapi: 2.0.0
info:
  title: Retail Customers AsyncAPI
  version: 1.0.0
  description: |
    AsyncAPI specification for Retail Customers service.
    Defines events published and subscribed by this service.
channels:
  inventorydomain.topic.exchange/inventory.updated.event:
    subscribe:
      message:
        $ref: '#/components/messages/InventoryUpdatedEvent'
components:
  messages:
    InventoryUpdatedEvent:
      payload:
        type: object
        required:
          - customerId
          - orderId
        properties:
          customerId:
            type: string
            format: uuid
            description: Unique identifier of the customer
          orderId:
            type: string
            format: uuid
            description: Unique identifier of the order
```

### Documentation Requirements

- **Channel Names**: Descriptive and follow naming convention
- **Message Schemas**: Complete property definitions
- **Descriptions**: Clear descriptions for all fields
- **Examples**: Include example payloads where helpful

See [Contract Specifications](./Contract-Specifications.md) for details.

---

## API Versioning

### Versioning Strategy

APIs use URL versioning:

```
/api/v1/customers
/api/v2/customers
```

### Version Documentation

Document version changes:

```csharp
/// <summary>
/// Creates a new customer (v2).
/// </summary>
/// <remarks>
/// Version 2 adds support for customer preferences and notification settings.
/// </remarks>
[ApiVersion("2.0")]
[HttpPost]
public async Task<IActionResult> CreateCustomerV2Async(...)
```

### Deprecation

Document deprecated endpoints:

```csharp
/// <summary>
/// Creates a new customer (deprecated).
/// </summary>
/// <remarks>
/// This endpoint is deprecated and will be removed in v3.0.
/// Use POST /api/v2/customers instead.
/// </remarks>
[ApiVersion("1.0", Deprecated = true)]
[HttpPost]
public async Task<IActionResult> CreateCustomerAsync(...)
```

---

## Error Documentation

### Error Response Format

All errors follow RFC 7807 ProblemDetails format:

```csharp
/// <summary>
/// Creates a new customer.
/// </summary>
/// <response code="400">Invalid customer data. Returns ProblemDetails with validation errors.</response>
/// <response code="404">Customer not found. Returns ProblemDetails with error details.</response>
/// <response code="500">Internal server error. Returns ProblemDetails with error details.</response>
[HttpPost]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> CreateCustomerAsync(...)
```

### Error Codes

Document all possible error codes:

| HTTP Status | Description | When It Occurs |
|-------------|-------------|----------------|
| 400 | Bad Request | Invalid input data, validation failures |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource does not exist |
| 409 | Conflict | Resource conflict (e.g., duplicate email) |
| 500 | Internal Server Error | Unexpected system error |

### Error Response Example

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-1234567890abcdef1234567890abcdef-0123456789abcdef-01",
  "errors": {
    "Email": [
      "The Email field is required.",
      "The Email field must be a valid email address."
    ]
  }
}
```

---

## Examples and Best Practices

### Complete Controller Example

```csharp
namespace Retail.Customers.API.Controllers
{
    /// <summary>
    /// Provides endpoints for managing customers.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            ICustomerService customerService,
            ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a customer by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the customer.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>
        /// The customer if found, or 404 Not Found if the customer does not exist.
        /// </returns>
        /// <response code="200">Customer retrieved successfully</response>
        /// <response code="404">Customer not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCustomerAsync(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _customerService.GetCustomerAsync(id, cancellationToken);
            
            if (result.IsFailure)
            {
                return Problem(
                    detail: result.Error,
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Creates a new customer in the system.
        /// </summary>
        /// <param name="customerDto">The customer data to create.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>
        /// The created customer with assigned identifier.
        /// </returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/v1/customer
        ///     {
        ///         "name": "John Doe",
        ///         "email": "john.doe@example.com",
        ///         "address": "123 Main St"
        ///     }
        /// </remarks>
        /// <response code="201">Customer created successfully</response>
        /// <response code="400">Invalid customer data</response>
        /// <response code="409">Customer with email already exists</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCustomerAsync(
            [FromBody] CustomerDto customerDto,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _customerService.CreateCustomerAsync(customerDto, cancellationToken);
            
            if (result.IsFailure)
            {
                return Problem(
                    detail: result.Error,
                    statusCode: StatusCodes.Status400BadRequest);
            }

            return CreatedAtAction(
                nameof(GetCustomerAsync),
                new { id = result.Value.Id },
                result.Value);
        }
    }
}
```

### Best Practices

1. **Be Descriptive**: Write clear, concise descriptions
2. **Include Examples**: Provide request/response examples
3. **Document All Responses**: Document success and error responses
4. **Use Proper Types**: Use correct return types in attributes
5. **Version Documentation**: Document version changes
6. **Keep Updated**: Update documentation when APIs change

---

## Documentation Checklist

Before submitting a PR with API changes:

- [ ] All public methods have XML documentation
- [ ] All parameters are documented
- [ ] Return values are documented
- [ ] Error responses are documented
- [ ] Examples are provided for complex endpoints
- [ ] Swagger documentation is generated correctly
- [ ] AsyncAPI contracts are updated (if messaging changed)
- [ ] Version changes are documented

---

## Tools and Resources

- **Swashbuckle**: OpenAPI/Swagger generation
- **AsyncAPI Generator**: Contract code generation
- **DocFX**: Documentation generation (if needed)
- **OpenAPI Specification**: https://swagger.io/specification/
- **AsyncAPI Specification**: https://www.asyncapi.com/

---

## References

- [Coding Guidelines](./Coding-Guidelines/README.md)
- [Contracts Documentation](./Contracts/README.md)
- [Design Documentation](./Design/README.md)

---

**Last Updated**: 2024

