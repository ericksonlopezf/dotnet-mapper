# Official Recipe Cookbook: EricksonLopez.Mapper

> **Showcase Reference:** This document forms an integral part of the executable Showcase project.
> All recipes contained herein correspond to real, compilable, and verified scenarios across the **EricksonLopez.Mapper** ecosystem.

---

## Recipe Index

| # | Recipe | Demonstrated Public API | Level |
|---|---|---|---|
| 1 | Simple Mapping by Name Convention | `[Mapper]` | Basic |
| 2 | Properties with Different Names | `[MapProperty]` | Basic |
| 3 | Ignoring Destination Properties in Strict Mode | `[MapIgnore]` | Basic |
| 4 | Non-Strict Mapping for Legacy Models | `[Mapper(StrictMapping = false)]` | Basic |
| 5 | Mapping Nested Objects and Subgraphs | `[Mapper]` (linked sub-methods) | Intermediate |
| 6 | Mapping to Immutable Records with Positional Constructors | Constructor binding | Intermediate |
| 7 | Instantiation with Static Factory Methods | `[MapFactory]` | Advanced |
| 8 | Compile-Time Polymorphic Dispatch | `[MapDerivedType]` | Advanced |
| 9 | Default Fallback Values for Nullable Types | `[MapNullFallback]` | Intermediate |
| 10 | Complex Logic Converters with `IConverter` | `[UseConverter(Type)]`, `IConverter<TSource, TDestination>` | Intermediate |
| 11 | Value Objects and Strongly Typed IDs | `[ValueObject]` | Intermediate |
| 12 | Automated Dependency Injection Registration | `[assembly: GenerateMapperRegistration]` | Basic |
| 13 | Mapping to Modern Immutable Collections | `T[]`, `ImmutableArray<T>`, `HashSet<T>` | Intermediate |
| 14 | Built-In Scalar Type Conversions | `enum↔string`, `Guid↔string`, `DateTime→DateOnly` | Basic |
| 15 | Stateless Static Mappers for LINQ and Concurrency | `static partial class` | Basic |
| 16 | Hierarchy Flattening with Deep Dot Paths | `[MapProperty("A.B.C", "C")]` | Intermediate |
| 17 | Enum Mapping Strategies (Name vs Value) | `[EnumMappingStrategy]` | Intermediate |
| 18 | Explicit Remapping of Disparate Enum Members | `[MapEnumValue]` | Intermediate |
| 19 | Constant and Computed Expression Injection | `[MapValue]` | Intermediate |
| 20 | Global Exclusion in Data Models | `[MapperIgnore]` | Basic |
| 21 | Assembly-Wide Global Configuration | `[assembly: MapperDefaults]` | Intermediate |
| 22 | Converter Injection with IoC Dependencies | `[UseConverter(fieldName)]` | Advanced |
| 23 | Sensitive Source Data Exclusion | `[MapIgnoreSource]` | Intermediate |
| 24 | Typed Dictionary Transformation | `Dictionary<TKey, TValue>` | Intermediate |
| 25 | Clean Architecture Domain Primitives Integration | `EricksonLopez.Mapper.DomainPrimitives` | Advanced |
| 26 | Railway-Oriented Programming (ROP) with Result | `EricksonLopez.Mapper.Result` | Advanced |
| 27 | Interoperability Bridge with Mapster | `EricksonLopez.Mapper.Mapster` | Advanced |
| 28 | CRUD Update without In-Place Mutation (ADR-D05) | Immutability / Pure Projection | Advanced |

---

## Recipe 1: Simple Mapping by Name Convention

### Problem
You need to transform a source entity into a destination DTO when both classes share identical property names and types, without incurring runtime reflection costs or repetitive manual mapping code.

### Solution
Declare a `partial` class annotated with `[Mapper]` and define a `partial` method with the desired mapping signature. The generator automatically wires matching properties (case-insensitive).

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

[Mapper]
public partial class UserMapper
{
    public partial UserDto Map(User source);
}

public class Example
{
    public static void Run()
    {
        var mapper = new UserMapper();
        var user = new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@example.com" };
        UserDto dto = mapper.Map(user);
        Console.WriteLine($"User: {dto.Name} <{dto.Email}>");
    }
}
```

### Explanation
The Roslyn Incremental Generator detects `[Mapper]` at compile time, inspects public readable properties of `User` and writable properties of `UserDto`, and emits the partial method implementation using direct member assignments (`target.Name = source.Name;`).

### Best Practices
- Keep `StrictMapping = true` (default behavior) so the compiler alerts you immediately if a property is added to `UserDto` without a matching source property in `User`.
- Use semantically homogeneous property names across architectural tiers.

### Common Mistakes
- Omitting the `partial` keyword on the mapper class or mapping method, causing analyzer error `ELM012`.
- Defining properties without public accessors (`get; set;` or `init;`).

---

## Recipe 2: Properties with Different Names

### Problem
The domain model and public API contract use different naming conventions or terms for the same data (e.g., `InternalId` vs `CustomerId`).

### Solution
Decorate the mapping method with `[MapProperty(sourceName, destinationName)]`.

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

public class Customer
{
    public Guid InternalId { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public class CustomerDto
{
    public Guid CustomerId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

[Mapper]
public partial class CustomerMapper
{
    [MapProperty(nameof(Customer.InternalId), nameof(CustomerDto.CustomerId))]
    [MapProperty(nameof(Customer.FullName), nameof(CustomerDto.DisplayName))]
    public partial CustomerDto Map(Customer source);
}
```

### Explanation
`[MapProperty]` overrides convention matching. The generator emits `target.CustomerId = source.InternalId;` and `target.DisplayName = source.FullName;`.

### Best Practices
- Use the `nameof(...)` operator to reference property names, ensuring safe IDE refactoring without stale string literals.

### Common Mistakes
- Inverting argument order (`destinationName, sourceName`). The signature strictly requires `sourceName` first, then `destinationName`.

---

## Recipe 3: Ignoring Destination Properties in Strict Mode

### Problem
The destination DTO contains computed properties, internal flags, or identifiers that do not exist on the source model, triggering compilation error `ELM001` in strict mode.

### Solution
Apply `[MapIgnore(destinationName)]` to the mapping method to instruct the generator to deliberately exclude that destination property.

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

public class OrderEntity
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public string FormattedTotal { get; set; } = string.Empty; // Calculated later
}

[Mapper]
public partial class OrderMapper
{
    [MapIgnore(nameof(OrderDto.FormattedTotal))]
    public partial OrderDto Map(OrderEntity source);
}
```

### Explanation
The generator skips initializing `FormattedTotal`, satisfying strict mode checks without compilation warnings or errors.

### Best Practices
- Document in code comments why a destination property is ignored (e.g., "calculated by middleware or downstream client").

### Common Mistakes
- Passing a source property name instead of a destination property name to `[MapIgnore]`. To exclude sensitive source properties, use `[MapIgnoreSource]`.

---

## Recipe 4: Non-Strict Mapping for Legacy Models

### Problem
Integrating legacy models or external database tables with dozens of columns where you only want to project a small subset of properties, making individual `[MapIgnore]` annotations tedious.

### Solution
Disable strict validation by configuring `[Mapper(StrictMapping = false)]`.

### Full Code
```csharp
using EricksonLopez.Mapper;

public class LegacyComplexEntity
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Field1 { get; set; } = string.Empty;
    public string Field2 { get; set; } = string.Empty;
    public string Field3 { get; set; } = string.Empty;
}

public class SlimDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ExtraNote { get; set; } = string.Empty; // Does not exist in source; silently ignored
}

[Mapper(StrictMapping = false)]
public partial class LooseMapper
{
    public partial SlimDto Map(LegacyComplexEntity source);
}
```

### Explanation
When `StrictMapping = false`, the generator only maps properties that match by name or explicit directive; unmatched members are silently ignored without raising `ELM001`.

### Best Practices
- Confine `StrictMapping = false` to external integration boundaries or legacy DTOs. Keep `StrictMapping = true` across core domain boundaries.

### Common Mistakes
- Assuming `StrictMapping = false` suppresses type incompatibility errors; if property names match but types cannot be converted, `ELM003` will still be emitted.

---

## Recipe 5: Mapping Nested Objects and Subgraphs

### Problem
The source entity contains nested entities or value objects (e.g., an `Order` containing an `Address` and a list of `OrderItem` objects).

### Solution
Declare `partial` sub-mapping methods within the same mapper class. The generator detects type dependencies and links them automatically.

### Full Code
```csharp
using System;
using System.Collections.Generic;
using EricksonLopez.Mapper;

public class Order
{
    public Guid Id { get; set; }
    public Address ShippingAddress { get; set; } = new();
    public List<OrderItem> Items { get; set; } = new();
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class OrderItem
{
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public AddressDto ShippingAddress { get; set; } = new();
    public List<OrderItemDto> Items { get; set; } = new();
}

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class OrderItemDto
{
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

[Mapper]
public partial class OrderHierarchyMapper
{
    public partial OrderDto MapOrder(Order source);
    public partial AddressDto MapAddress(Address source);
    public partial OrderItemDto MapItem(OrderItem source);
}
```

### Explanation
The generator inspects the syntax tree. Upon identifying that `Order.ShippingAddress` must be converted to `AddressDto`, it verifies the existence of `MapAddress(Address)` and emits a direct call without dynamic dispatch.

### Best Practices
- Group all mapping methods for an aggregate root within the same `partial` mapper class.

### Common Mistakes
- Defining direct circular references between types (e.g., `Order` references `OrderItem` and `OrderItem` references `Order`), which triggers diagnostic `ELM010` (cycle detected).

---

## Recipe 6: Mapping to Immutable Records with Positional Constructors

### Problem
You need to map to immutable C# types (`record`, `record struct`) that expose no public property setters and require instantiation exclusively via primary constructors.

### Solution
Declare the mapping method normally. The generator inspects constructor parameter names and binds matching source properties.

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

public class ProductEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public sealed record ProductDto(Guid Id, string Name, decimal Price);

[Mapper]
public partial class ProductRecordMapper
{
    public partial ProductDto Map(ProductEntity source);
}
```

### Explanation
The generator determines that `ProductDto` has no parameterless constructor but exposes a public constructor with `(Guid id, string name, decimal price)`. It emits:
```csharp
return new ProductDto(source.Id, source.Name, source.Price);
```

### Best Practices
- Prefer positional records for all output DTOs; they guarantee immutability, value equality, and Native AOT safety.

### Common Mistakes
- Naming a constructor parameter incompatibly with the source property (e.g., parameter `unitPrice` vs property `Price`), causing `ELM001`.

---

## Recipe 7: Instantiation with Static Factory Methods (`[MapFactory]`)

### Problem
The target type encapsulates object creation via a static factory method (`Create(...)`) to enforce domain invariants, and its constructors are private.

### Solution
Decorate the mapping method with `[MapFactory(methodName)]`.

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

public class MoneyEntity
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Negative amount.");
        return new Money(amount, currency.ToUpperInvariant());
    }
}

[Mapper]
public partial class MoneyMapper
{
    [MapFactory(nameof(Money.Create))]
    public partial Money Map(MoneyEntity source);
}
```

### Explanation
The generator binds parameters of the static `Create` method with properties from `MoneyEntity` and emits `return Money.Create(source.Amount, source.Currency);`.

### Best Practices
- Use `[MapFactory]` in Domain-Driven Design (DDD) to construct rich domain entities and Value Objects that enforce strict invariants.

### Common Mistakes
- Declaring the factory method as private or instance-scoped, producing diagnostic `ELM002`.

---

## Recipe 8: Compile-Time Polymorphic Dispatch (`[MapDerivedType]`)

### Problem
You manage an inheritance hierarchy (e.g., `Vehicle` with derived subtypes `Car` and `Truck`) and need to dispatch to the correct target DTO based on runtime concrete type without reflection (`GetType()`).

### Solution
Decorate the base mapping method with one or more `[MapDerivedType(typeof(SourceSubtype), typeof(TargetSubtype))]` attributes and provide specialized mapping methods for each subtype.

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

public abstract class Vehicle { public string Vin { get; set; } = string.Empty; }
public class Car : Vehicle { public int Doors { get; set; } }
public class Truck : Vehicle { public decimal PayloadCapacity { get; set; } }

public abstract class VehicleDto { public string Vin { get; set; } = string.Empty; }
public class CarDto : VehicleDto { public int Doors { get; set; } }
public class TruckDto : VehicleDto { public decimal PayloadCapacity { get; set; } }

[Mapper]
public partial class PolymorphicVehicleMapper
{
    [MapDerivedType(typeof(Car), typeof(CarDto))]
    [MapDerivedType(typeof(Truck), typeof(TruckDto))]
    public partial VehicleDto Map(Vehicle source);

    public partial CarDto MapCar(Car source);
    public partial TruckDto MapTruck(Truck source);
}
```

### Explanation
The generator emits a C# pattern matching `switch` expression:
```csharp
return source switch
{
    Car car => MapCar(car),
    Truck truck => MapTruck(truck),
    _ => throw new InvalidOperationException($"Unsupported type: {source.GetType()}")
};
```

### Best Practices
- Explicitly cover all possible derived types to avoid runtime unsupported type exceptions.

### Common Mistakes
- Omitting the concrete mapping methods (`MapCar`, `MapTruck`) in the same mapper class.

---

## Recipe 9: Default Fallback Values for Nullable Types (`[MapNullFallback]`)

### Problem
The source property is a nullable value type (`int?`, `decimal?`, `DateTime?`), while the destination requires a non-nullable value type, producing `ELM004`.

### Solution
Decorate the method with `[MapNullFallback(destinationName, fallbackExpression)]` to emit a C# literal fallback when null.

### Full Code
```csharp
using EricksonLopez.Mapper;

public class InventoryRecord
{
    public int? Stock { get; set; }
    public decimal? Price { get; set; }
}

public class InventoryDto
{
    public int Stock { get; set; }
    public decimal Price { get; set; }
}

[Mapper]
public partial class InventoryMapper
{
    [MapNullFallback(nameof(InventoryDto.Stock), "0")]
    [MapNullFallback(nameof(InventoryDto.Price), "0.0m")]
    public partial InventoryDto Map(InventoryRecord source);
}
```

### Explanation
The generator emits null-coalescing expressions: `target.Stock = source.Stock ?? 0;` and `target.Price = source.Price ?? 0.0m;`.

### Best Practices
- Use exact C# literal suffixes in fallback expressions (`0.0m` for decimal, `0L` for long).

### Common Mistakes
- Supplying invalid C# syntax in the `fallbackExpression` argument.

---

## Recipe 10: Complex Logic Converters with `IConverter`

### Problem
Transformation between two types involves custom parsing, mathematical operations, or localized formatting beyond declarative convention matching.

### Solution
Implement `IConverter<TSource, TDestination>` and decorate the mapping method with `[UseConverter(typeof(MyConverter))]`.

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

public class Coordinates { public double Lat { get; set; } public double Lon { get; set; } }
public class GeoDto { public string LocationString { get; set; } = string.Empty; }

public class CoordinatesToStringConverter : IConverter<Coordinates, string>
{
    public string Convert(Coordinates source) => $"{source.Lat:F4},{source.Lon:F4}";
}

[Mapper]
public partial class GeoMapper
{
    [UseConverter(typeof(CoordinatesToStringConverter))]
    [MapProperty(nameof(Coordinates), nameof(GeoDto.LocationString))]
    public partial string FormatLocation(Coordinates source);
}
```

### Explanation
The generator instantiates `new CoordinatesToStringConverter()` and delegates the call to its `Convert(source)` method.

### Best Practices
- Keep converters pure, idempotent, and free of global side effects.

### Common Mistakes
- Specifying a converter whose generic arguments do not match the source and destination types of the method (`ELM013`).

---

## Recipe 11: Value Objects and Strongly Typed IDs (`[ValueObject]`)

### Problem
In DDD architectures, strongly typed IDs (`CustomerId(Guid)`) prevent primitive obsession, and you need to wrap and unwrap inner primitive values without boilerplate converters.

### Solution
Annotate the Value Object with `[ValueObject]`.

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

[ValueObject]
public readonly record struct CustomerId(Guid Value);

public class CustomerEntity
{
    public CustomerId Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CustomerApiDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

[Mapper]
public partial class CustomerIdMapper
{
    public partial CustomerApiDto ToDto(CustomerEntity source); // CustomerId -> Guid (.Value)
    public partial CustomerEntity ToEntity(CustomerApiDto source); // Guid -> CustomerId (new CustomerId)
}
```

### Explanation
Upon encountering `[ValueObject]`, the generator automatically unpacks `.Value` in the entity $\rightarrow$ DTO direction, and invokes `new CustomerId(source.Id)` in the DTO $\rightarrow$ entity direction.

### Best Practices
- Use `readonly record struct` for strongly typed IDs to ensure zero heap allocations.

### Common Mistakes
- Failing to expose a `.Value` property or lacking a single-parameter constructor matching the primitive type.

---

## Recipe 12: Automated Dependency Injection Registration

### Problem
A solution with dozens of mappers requires registration in `IServiceCollection`, and manual registration is error-prone.

### Solution
Add assembly-level attribute `[assembly: GenerateMapperRegistration]`.

### Full Code
```csharp
using EricksonLopez.Mapper;
using Microsoft.Extensions.DependencyInjection;

// Declared at assembly level (AssemblyConfig.cs or Program.cs)
[assembly: GenerateMapperRegistration]

public class StartupExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Generator-emitted extension method
        services.AddGeneratedMappers();
    }
}
```

### Explanation
The generator emits a static `MapperServiceCollectionExtensions` class in the `Microsoft.Extensions.DependencyInjection` namespace registering every non-static `[Mapper]` class as a `Singleton`.

### Best Practices
- Declare the attribute once per assembly in a designated file such as `Properties/AssemblyConfig.cs`.

### Common Mistakes
- Expecting `static partial class` mappers to be registered; static mappers require no DI container instances.

---

## Recipe 13: Mapping to Modern Immutable Collections

### Problem
You need to map collections to modern high-performance immutable types (`ImmutableArray<T>`, `HashSet<T>`, arrays `T[]`) without LINQ enumerator heap allocations.

### Solution
Declare the desired collection types on the destination DTO; the generator emits pre-allocated capacity loops.

### Full Code
```csharp
using System.Collections.Generic;
using System.Collections.Immutable;
using EricksonLopez.Mapper;

public class Catalog
{
    public List<Item> Items { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

public class Item { public string Sku { get; set; } = string.Empty; }
public class ItemDto { public string Sku { get; set; } = string.Empty; }

public class CatalogDto
{
    public ImmutableArray<ItemDto> Items { get; set; }
    public HashSet<string> Tags { get; set; } = new();
}

[Mapper]
public partial class CatalogMapper
{
    public partial CatalogDto Map(Catalog source);
    public partial ItemDto MapItem(Item source);
}
```

### Explanation
For `ImmutableArray<T>`, the generator emits `ImmutableArray.CreateBuilder<ItemDto>(source.Items.Count)` with pre-sized capacity; for `HashSet<T>`, it emits `new HashSet<string>(source.Tags.Count)`.

### Best Practices
- Prefer `ImmutableArray<T>` over `IReadOnlyList<T>` for read-only collections in high-throughput pipelines.

### Common Mistakes
- Mapping unsupported custom generic collection interfaces; stick to standard .NET collection abstractions (`IList<T>`, `IReadOnlyList<T>`, `IEnumerable<T>`).

---

## Recipe 14: Built-In Scalar Type Conversions

### Problem
Converting between common scalar types such as `Guid ↔ string`, `enum ↔ string`, and `DateTime ↔ DateOnly` without writing manual converters.

### Solution
Rely on the generator's built-in scalar conversion templates.

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

public enum OrderState { Submitted, Shipped, Delivered }

public class SourcePayload
{
    public Guid TrackingId { get; set; }
    public OrderState State { get; set; }
    public DateTime EventDate { get; set; }
}

public class DestContract
{
    public string TrackingId { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
}

[Mapper]
public partial class ScalarMapper
{
    public partial DestContract Map(SourcePayload source);
}
```

### Explanation
The generator automatically emits `source.TrackingId.ToString()`, `source.State.ToString()`, and `DateOnly.FromDateTime(source.EventDate)`.

### Best Practices
- Keep member names aligned to trigger automatic conversion heuristics.

### Common Mistakes
- Attempting narrowing numeric conversions (e.g., `long` to `int`) without validating value bounds; the analyzer will emit warning `ELM015`.

---

## Recipe 15: Stateless Static Mappers for LINQ and Concurrency

### Problem
Executing LINQ queries or PLINQ parallel projections where allocating mapper instances degrades throughput.

### Solution
Declare the mapper class as `static partial`.

### Full Code
```csharp
using System.Collections.Generic;
using System.Linq;
using EricksonLopez.Mapper;

public class Item { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
public class ItemDto { public int Id { get; set; } public string Name { get; set; } = string.Empty; }

[Mapper]
public static partial class StaticItemMapper
{
    public static partial ItemDto Map(Item source);
}

public class LinqDemo
{
    public static List<ItemDto> ProjectList(List<Item> items)
    {
        return items.AsParallel().Select(StaticItemMapper.Map).ToList();
    }
}
```

### Explanation
The generator emits `public static partial ItemDto Map(Item source)` without references to `this`, enabling direct C# method group delegates in LINQ pipelines.

### Best Practices
- Use static mappers whenever mapping logic requires no runtime injectable dependencies.

### Common Mistakes
- Trying to register a static mapper in the IoC container (`services.AddSingleton(typeof(StaticMapper))`), which is invalid C#.

---

## Recipe 16: Hierarchy Flattening with Deep Dot Paths

### Problem
A deep object hierarchy (`Order.Customer.Address.City`) must be flattened into a scalar destination property (`FlatOrderDto.City`).

### Solution
Use dot notation in `[MapProperty("Customer.Address.City", "City")]`.

### Full Code
```csharp
using EricksonLopez.Mapper;

public class Order { public CustomerInfo Customer { get; set; } = new(); }
public class CustomerInfo { public AddressInfo Address { get; set; } = new(); }
public class AddressInfo { public string City { get; set; } = string.Empty; }

public class FlatOrderDto { public string City { get; set; } = string.Empty; }

[Mapper]
public partial class FlatOrderMapper
{
    [MapProperty("Customer.Address.City", nameof(FlatOrderDto.City))]
    public partial FlatOrderDto Map(Order source);
}
```

### Explanation
The generator emits null-safe navigation code (`source.Customer?.Address?.City ?? string.Empty`).

### Best Practices
- Pair with `[MapNullFallback]` if the destination property is a non-nullable value type.

### Common Mistakes
- Typographical errors in intermediate path segments, which produce error `ELM001`.

---

## Recipe 17: Enum Mapping Strategies (Name vs Value)

### Problem
You need to map enums by their integral value (`ByValue`) or match names case-insensitively (`ByName` with `IgnoreCase = true`).

### Solution
Apply `[EnumMappingStrategy(EnumMappingStrategy, IgnoreCase = bool)]` at the class or method level.

### Full Code
```csharp
using EricksonLopez.Mapper;

public enum InternalStatus { Active = 1, Suspended = 2 }
public enum ExternalStatus { active = 1, suspended = 2 }

[Mapper]
[EnumMappingStrategy(EnumMappingStrategy.ByName, IgnoreCase = true)]
public partial class EnumStrategyMapper
{
    public partial ExternalStatus Map(InternalStatus source);
}
```

### Explanation
With `ByName` and `IgnoreCase = true`, the generator emits a `switch` comparing normalized enum names without string heap allocations. With `ByValue`, it emits an explicit cast `(ExternalStatus)(int)source`.

### Best Practices
- Use `ByName` with `IgnoreCase = true` for external JSON APIs with varied casing conventions.

### Common Mistakes
- Using `ByValue` when integer values represent distinct meanings between different enums.

---

## Recipe 18: Explicit Remapping of Disparate Enum Members

### Problem
Two enums have differently named members representing the same business concept (e.g., `PaymentStatus.Captured` $\leftrightarrow$ `OrderStatus.Completed`).

### Solution
Decorate the mapping method with `[MapEnumValue(SourceMember, TargetMember)]`.

### Full Code
```csharp
using EricksonLopez.Mapper;

public enum PaymentStatus { Pending, Captured, Cancelled }
public enum OrderStatus { Queued, Completed, Aborted }

[Mapper]
public partial class EnumMemberMapper
{
    [MapEnumValue(PaymentStatus.Pending, OrderStatus.Queued)]
    [MapEnumValue(PaymentStatus.Captured, OrderStatus.Completed)]
    [MapEnumValue(PaymentStatus.Cancelled, OrderStatus.Aborted)]
    public partial OrderStatus MapStatus(PaymentStatus source);
}
```

### Explanation
The generator embeds explicit remappings into the generated `switch` expression.

### Best Practices
- Cover all possible source enum members to prevent runtime unsupported value exceptions.

### Common Mistakes
- Passing raw integer values instead of strongly typed enum members in attribute arguments.

---

## Recipe 19: Constant and Computed Expression Injection (`[MapValue]`)

### Problem
The destination DTO requires audit timestamps, environment markers, or constants not present in the source object.

### Solution
Use `[MapValue(destinationName, valueExpression)]`.

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

public class RawInput { public string Name { get; set; } = string.Empty; }
public class AuditedDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public string Environment { get; set; } = string.Empty;
}

[Mapper]
public partial class AuditedMapper
{
    [MapValue(nameof(AuditedDto.ProcessedAt), "System.DateTime.UtcNow")]
    [MapValue(nameof(AuditedDto.Environment), "\"Production\"")]
    public partial AuditedDto Map(RawInput source);
}
```

### Explanation
The generator writes the expression directly into the assignment: `target.ProcessedAt = System.DateTime.UtcNow;`.

### Best Practices
- Properly escape quotation marks for string literals (`"\"Text\""`).

### Common Mistakes
- Specifying expressions that reference non-existent types or inaccessible members in the generated method scope.

---

## Recipe 20: Global Exclusion in Data Models (`[MapperIgnore]`)

### Problem
A domain entity contains internal infrastructure fields or cache tokens that must never be exposed in any mapping across the entire solution.

### Solution
Decorate the property directly on the data model with `[MapperIgnore]`.

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

public class Account
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;

    [MapperIgnore]
    public string InternalMemoryCacheToken { get; set; } = string.Empty;
}

public class AccountDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
}

[Mapper]
public partial class AccountMapper
{
    public partial AccountDto Map(Account source);
}
```

### Explanation
The generator excludes the property across all mappers involving `Account`, satisfying strict mode checks.

### Best Practices
- Use `[MapperIgnore]` for internal ORM change tokens or entity state flags.

### Common Mistakes
- Applying `[MapperIgnore]` to methods rather than properties or fields.

---

## Recipe 21: Assembly-Wide Global Configuration (`[assembly: MapperDefaults]`)

### Problem
You want to standardize `StrictMapping = true`, `EnumMappingStrategy = ByName`, and `EnumIgnoreCase = true` across the entire project without repeating attributes on every mapper.

### Solution
Declare `[assembly: MapperDefaults]` in `AssemblyConfig.cs`.

### Full Code
```csharp
using EricksonLopez.Mapper;

[assembly: MapperDefaults(
    StrictMapping = true,
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumIgnoreCase = true
)]
```

### Explanation
The generator reads assembly-level metadata and applies defaults to any `[Mapper]` class that does not explicitly override them.

### Best Practices
- Place the declaration in a dedicated file such as `Properties/AssemblyConfig.cs`.

### Common Mistakes
- Duplicating the attribute across multiple files within the same compilation.

---

## Recipe 22: Converter Injection with IoC Dependencies

### Problem
A custom converter requires dependencies from ASP.NET Core's IoC container (e.g., encryption providers or locale-aware formatters).

### Solution
Use the `[UseConverter(string fieldName)]` constructor, passing the name of the private field injected via constructor.

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

public interface IEncryptionService { string Decrypt(string cipherText); }

public class SecretPayload { public string EncryptedData { get; set; } = string.Empty; }
public class PlainDto { public string Data { get; set; } = string.Empty; }

public class SecretConverter : IConverter<string, string>
{
    private readonly IEncryptionService _encryptionService;
    public SecretConverter(IEncryptionService encryptionService) => _encryptionService = encryptionService;
    public string Convert(string source) => _encryptionService.Decrypt(source);
}

[Mapper]
public partial class SecureDataMapper
{
    private readonly IConverter<string, string> _converter;

    public SecureDataMapper(IConverter<string, string> converter)
    {
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
    }

    [UseConverter(nameof(_converter))]
    [MapProperty(nameof(SecretPayload.EncryptedData), nameof(PlainDto.Data))]
    public partial PlainDto Map(SecretPayload source);
}
```

### Explanation
The generator emits `this._converter.Convert(source.EncryptedData)`, integrating seamlessly with dependency injection.

### Best Practices
- Use `nameof(_fieldName)` to ensure safe refactoring.

### Common Mistakes
- Declaring the backing field as static or leaving it uninitialized.

---

## Recipe 23: Sensitive Source Data Exclusion (`[MapIgnoreSource]`)

### Problem
The source entity contains sensitive fields (`PasswordHash`, `SecurityStamp`) and you need compile-time certainty that they will never match public destination properties.

### Solution
Decorate the mapping method with `[MapIgnoreSource(sourceName)]`.

### Full Code
```csharp
using EricksonLopez.Mapper;

public class UserEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}

public class PublicUserDto
{
    public string Email { get; set; } = string.Empty;
}

[Mapper]
public partial class SafeUserMapper
{
    [MapIgnoreSource(nameof(UserEntity.PasswordHash))]
    public partial PublicUserDto Map(UserEntity source);
}
```

### Explanation
The generator removes `PasswordHash` from source candidate members before evaluating property bindings.

### Best Practices
- Apply systematically when mapping security entities to public contracts.

### Common Mistakes
- Confusing `[MapIgnoreSource]` (filters source) with `[MapIgnore]` (filters destination).

---

## Recipe 24: Typed Dictionary Transformation

### Problem
Transforming dictionaries `Dictionary<TKeySource, TValueSource>` to `Dictionary<TKeyDest, TValueDest>`.

### Solution
Declare the signature on the mapper; the generator iterates over key-value pairs, converting keys and values.

### Full Code
```csharp
using System.Collections.Generic;
using EricksonLopez.Mapper;

public class SettingsEntity { public Dictionary<string, int> Flags { get; set; } = new(); }
public class SettingsDto { public Dictionary<string, string> Flags { get; set; } = new(); }

[Mapper]
public partial class SettingsMapper
{
    public partial SettingsDto Map(SettingsEntity source);
}
```

### Explanation
The generator allocates a new `Dictionary<string, string>(source.Flags.Count)` and populates key-value pairs, converting values automatically.

### Best Practices
- Use typed dictionaries for dynamic configuration models.

### Common Mistakes
- Attempting to map dictionaries without accessible property initializers or getters.

---

## Recipe 25: Clean Architecture Domain Primitives Integration

### Problem
Using `EricksonLopez.DomainPrimitives` (`IDomainPrimitive` and `IStrongId`) and needing maximum throughput between the database and domain layers.

### Solution
Use built-in converters from `EricksonLopez.Mapper.DomainPrimitives`.

### Full Code
```csharp
using System;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.Mapper.DomainPrimitives;

public readonly record struct OrderId(Guid Value) : IStrongId<OrderId, Guid>;
public sealed record EmailAddress(string Value) : IDomainPrimitive<EmailAddress, string>
{
    public static EmailAddress Create(string value) => new(value.ToLowerInvariant());
}

public class DomainPrimitivesExample
{
    public static void Run()
    {
        var id = new OrderId(Guid.NewGuid());
        var strongIdConverter = new StrongIdToValueConverter<OrderId, Guid>();
        Guid rawGuid = strongIdConverter.Convert(id);

        var valueToPrimitiveConverter = new ValueToDomainPrimitiveConverter<string, EmailAddress>();
        EmailAddress email = valueToPrimitiveConverter.Convert("TEST@EXAMPLE.COM");
        Console.WriteLine($"Guid: {rawGuid}, Email: {email.Value}");
    }
}
```

### Explanation
These converters implement `IConverter<TSource, TDestination>` optimized for zero allocations and full Native AOT compatibility.

### Best Practices
- Use `ValueToDomainPrimitiveConverter` when hydrating domain entities from Dapper or EF Core queries.

### Common Mistakes
- Omitting the static factory method `Create` on the domain primitive when using `ValueToDomainPrimitiveConverter`.

---

## Recipe 26: Railway-Oriented Programming (ROP) with Result

### Problem
Use cases return `Result<T>` and you want to transform success values without manual `if (result.IsFailure)` branching.

### Solution
Install `EricksonLopez.Mapper.Result` and chain calls with `result.Map(mapper.Map)` or `resultTask.MapAsync(mapper.Map)`.

### Full Code
```csharp
using System;
using System.Threading.Tasks;
using EricksonLopez.Mapper.Result;
using EricksonLopez.Result;

public class Entity { public int Id { get; set; } }
public class Dto { public int Id { get; set; } }

public class RopExample
{
    public static async Task Run()
    {
        Func<Entity, Dto> mapFunc = e => new Dto { Id = e.Id };

        // 1. Synchronous
        Result<Entity> success = Result<Entity>.Success(new Entity { Id = 42 });
        Result<Dto> mappedSuccess = success.Map(mapFunc);

        // 2. Asynchronous Task
        Task<Result<Entity>> taskResult = Task.FromResult(success);
        Result<Dto> mappedAsync = await taskResult.MapAsync(mapFunc);

        // 3. Failure: propagates error without executing mapFunc
        Result<Entity> failure = Result<Entity>.Failure(new Error("Err", "Database error"));
        Result<Dto> mappedFailure = failure.Map(mapFunc);
        Console.WriteLine($"Failed?: {mappedFailure.IsFailure}");
    }
}
```

### Explanation
`ResultMappingExtensions` evaluates `IsFailure`; if true, it immediately returns `Result<TDest>.Failure(result.Error)` without invoking the mapping delegate, saving CPU cycles.

### Best Practices
- Use `ValueTask<Result<T>>.MapAsync` in high-throughput read paths to eliminate `Task` heap allocation.

### Common Mistakes
- Passing null delegates, triggering `ArgumentNullException`.

---

## Recipe 27: Interoperability Bridge with Mapster

### Problem
An existing codebase uses Mapster and you want to migrate incrementally to `EricksonLopez.Mapper` without rewriting all configurations at once.

### Solution
Use `EricksonLopez.Mapper.Mapster` via `MapsterConverter` or `TypeAdapterConfig.UseConverter`.

### Full Code
```csharp
using EricksonLopez.Mapper;
using EricksonLopez.Mapper.Mapster;
using Mapster;

public class LegacySource { public string Name { get; set; } = string.Empty; }
public class LegacyTarget { public string Name { get; set; } = string.Empty; }

public class MapsterBridgeExample
{
    public static void Run()
    {
        // 1. Wrap Mapster as an IConverter
        IConverter<LegacySource, LegacyTarget> converter = new MapsterConverter<LegacySource, LegacyTarget>();
        LegacyTarget target = converter.Convert(new LegacySource { Name = "Test" });

        // 2. Connect IConverter into TypeAdapterConfig
        var config = new TypeAdapterConfig();
        config.UseConverter(converter);
    }
}
```

### Explanation
The adapter encapsulates `Mapster.Adapt` behind the uniform `IConverter<TSource, TDestination>` interface.

### Best Practices
- Pass isolated `TypeAdapterConfig` instances to `MapsterConverter` in unit test fixtures.

### Common Mistakes
- Relying on Mapster in Native AOT production paths; remember Mapster relies on dynamic compilation and should be migrated toward `[Mapper]`.

---

## Recipe 28: CRUD Update without In-Place Mutation (ADR-D05)

### Problem
In entity update scenarios (HTTP PUT / PATCH), developers accustomed to AutoMapper expect `mapper.Map(dto, existingEntity)` to mutate in-place, which is prohibited by design in `EricksonLopez.Mapper` ([ADR-D05](../../../docs/adr/adr-d05-no-existing-instance-mapping.md)).

### Solution
Map the update command into a clean domain representation and update persistence declaratively.

### Full Code
```csharp
using System;
using EricksonLopez.Mapper;

public class UpdateUserCommand
{
    public string NewName { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
}

public class UserEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

[Mapper]
public partial class UserUpdateMapper
{
    [MapProperty(nameof(UpdateUserCommand.NewName), nameof(UserEntity.Name))]
    [MapProperty(nameof(UpdateUserCommand.NewEmail), nameof(UserEntity.Email))]
    [MapIgnore(nameof(UserEntity.Id))] // ID is preserved from persistence
    public partial UserEntity ToEntity(UpdateUserCommand command);
}

public class UpdateHandler
{
    public static void ApplyUpdate(UserEntity existing, UpdateUserCommand command, UserUpdateMapper mapper)
    {
        // Obtain newly validated projection
        UserEntity updated = mapper.ToEntity(command);
        
        // Apply values to entity in EF Core / Dapper
        existing.Name = updated.Name;
        existing.Email = updated.Email;
    }
}
```

### Explanation
[ADR-D05](../../../docs/adr/adr-d05-no-existing-instance-mapping.md) prohibits in-place mutation to protect domain invariants from reflection bypass and to enable immutable records (`init;`).

### Best Practices
- Treat entities as immutable and invoke explicit business methods on domain entities (`existing.UpdateProfile(updated.Name, updated.Email)`).

### Common Mistakes
- Looking for a non-existent `Map(source, target)` method; its omission is an intentional architectural invariant.
