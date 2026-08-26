# Architecture Guide

This guide explains how to position and integrate `EricksonLopez.Mapper` within modern .NET architectural styles: Clean Architecture, CQRS, Vertical Slice Architecture, Minimal APIs, and Domain-Driven Design (DDD).

---

## 1. Clean Architecture Positioning

Mappers are **infrastructure-agnostic transformation utilities**. They belong in the **Application Layer** or **Presentation / API Layer**, never in the Domain Layer.

```
┌────────────────────────────────────────────────────────┐
│                   Presentation / API                   │
│   ┌────────────────────────────────────────────────┐   │
│   │ Controllers / Minimal API Endpoints            │   │
│   │ Uses Mapper: ApiRequestDto → ApplicationCommand │   │
│   └────────────────────────────────────────────────┘   │
├────────────────────────────────────────────────────────┤
│                    Application Layer                   │
│   ┌────────────────────────────────────────────────┐   │
│   │ Use Cases / Command & Query Handlers           │   │
│   │ [Mapper] classes defined here                  │   │
│   │ Uses Mapper: DomainEntity → QueryResponseDto   │   │
│   └────────────────────────────────────────────────┘   │
├────────────────────────────────────────────────────────┤
│                      Domain Layer                      │
│   ┌────────────────────────────────────────────────┐   │
│   │ Entities, Value Objects, Aggregates, Enums     │   │
│   │ Pure C# — Zero references to [Mapper] packages │   │
│   └────────────────────────────────────────────────┘   │
├────────────────────────────────────────────────────────┤
│                  Infrastructure Layer                  │
│   ┌────────────────────────────────────────────────┐   │
│   │ Persistence Models / EF Core / Dapper          │   │
│   │ Uses Mapper: PersistenceModel → DomainEntity   │   │
│   └────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────┘
```

### Invariant Rules:
1. **Domain Isolation**: Domain entities must never reference `EricksonLopez.Mapper` attributes or DTO types.
2. **One-Way Mapping**: Presentation layers map requests to commands; application layers map domain entities to query responses.
3. **No Domain Bypass**: Mappers must not bypass domain entity constructors or business validation rules.

---

## 2. CQRS (Command Query Responsibility Segregation)

In CQRS architectures, mapping concerns are strictly divided:

| Flow Type | Mapping Responsibilities | Recommended Mapper Style |
|---|---|---|
| **Command Ingestion** | `HttpRequest → Command` | Static or instance `[Mapper]` in Presentation Layer |
| **Query Projection** | `DomainEntity → ReadDto` | Static or instance `[Mapper]` in Application Layer |
| **Persistence Mapping** | `DbModel ↔ DomainEntity` | Explicit instance `[Mapper]` in Infrastructure Layer |

```csharp
// Application Layer Query Mapper
[Mapper]
public partial class OrderQueryMapper
{
    [MapProperty("Customer.Address.City", "City")]
    public partial OrderSummaryDto ToSummaryDto(Order order);

    public partial OrderLineItemDto ToItemDto(LineItem item);
}
```

---

## 3. Vertical Slice Architecture (VSA)

In Vertical Slice architectures, each feature slice owns its mapper locally without shared generic mapper dependencies:

```csharp
namespace App.Features.Orders.CreateOrder;

public sealed record CreateOrderRequest(Guid CustomerId, decimal TotalAmount);
public sealed record CreateOrderResponse(Guid OrderId, string Status);

[Mapper]
internal static partial class CreateOrderFeatureMapper
{
    [MapProperty("CustomerId", "Id")]
    public static partial Order ToEntity(CreateOrderRequest request);

    public static partial CreateOrderResponse ToResponse(Order entity);
}
```

**Benefits in VSA:**
- Eliminates giant centralized mapping profiles.
- Feature changes are completely isolated from other vertical slices.
- Static mappers incur zero DI overhead.

---

## 4. ASP.NET Core Minimal API Integration

### Automatic Assembly-Wide DI Registration

Add `[assembly: GenerateMapperRegistration]` to your project:

```csharp
// AssemblyInfo.cs or Program.cs
using EricksonLopez.Mapper;

[assembly: GenerateMapperRegistration]
```

In `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register all generated [Mapper] classes as Singletons
builder.Services.AddGeneratedMappers();

var app = builder.Build();

app.MapPost("/orders", (CreateOrderRequest request, OrderQueryMapper mapper, IOrderRepository repository) =>
{
    var entity = mapper.ToEntity(request);
    repository.Save(entity);
    return Results.Ok(mapper.ToSummaryDto(entity));
});

app.Run();
```

---

## 5. Domain-Driven Design (DDD) Integration

### Value Objects & Strongly Typed IDs

Use `[ValueObject]` or record structs to map strongly-typed identifiers to raw primitives:

```csharp
[ValueObject]
public readonly record struct OrderId(Guid Value);

public sealed class Order
{
    public OrderId Id { get; }
    public string Description { get; }

    public Order(OrderId id, string description)
    {
        Id = id;
        Description = description;
    }
}

public sealed record OrderDto(Guid Id, string Description);

[Mapper]
public partial class OrderMapper
{
    public partial OrderDto ToDto(Order source);   // Unwraps OrderId -> Guid
    public partial Order ToDomain(OrderDto source); // Wraps Guid -> OrderId
}
```

### Factory Method Enforcement (`[MapFactory]`)

Protect private constructor invariants by delegating instantiation to a static factory method:

```csharp
public sealed class CustomerEntity
{
    public Guid Id { get; }
    public string Email { get; }

    private CustomerEntity(Guid id, string email)
    {
        Id = id;
        Email = email;
    }

    public static CustomerEntity Create(Guid id, string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email required", nameof(email));

        return new CustomerEntity(id, email.Trim().ToLowerInvariant());
    }
}

[Mapper]
public partial class CustomerMapper
{
    [MapFactory("Create")]
    public partial CustomerEntity ToEntity(CreateCustomerDto source);
}
```

---

## 6. Dependency Injection Lifetime Strategy

| Lifetime | Usage in EricksonLopez.Mapper | Rationale |
|---|---|---|
| **Singleton** | Default generated via `AddGeneratedMappers()` | Mappers are completely stateless pure functions; thread-safe across concurrent requests. |
| **Static (No DI)** | `[Mapper] public static partial class` | Fastest execution, zero DI resolution overhead, ideal for hot paths and LINQ projections. |
| **Scoped / Transient** | Only if injecting stateful `IConverter` dependencies | If a converter relies on a scoped DbContext or HttpContext, register that mapper instance explicitly. |
