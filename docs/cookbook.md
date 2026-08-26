# Cookbook & Practical Recipes

> **Source of truth:** Official cookbook derived from the verified public API surface and real showcase scenarios.
> Every recipe in this document compiles against `EricksonLopez.Mapper` v1.0.0.

---

## Quick Recipe Index

| # | Recipe Scenario | Attributes & Concepts Used |
|---|---|---|
| 1 | Simple by-convention mapping | `[Mapper]` |
| 2 | Property name remapping | `[MapProperty]` |
| 3 | Ignoring destination properties | `[MapIgnore]` |
| 4 | Non-strict loose mapping | `[Mapper(StrictMapping = false)]` |
| 5 | Nested object mapping | `[Mapper]` + companion partial methods |
| 6 | Immutable record mapping | Positional records with primary constructors |
| 7 | Factory method construction | `[MapFactory]` |
| 8 | Polymorphic mapping | `[MapDerivedType]` |
| 9 | Nullable value fallback | `[MapNullFallback]` |
| 10 | Custom conversion logic | `[UseConverter]` + `IConverter<T,T>` |
| 11 | Value Objects & Strongly Typed IDs | `[ValueObject]` / `IStrongId` / `IDomainPrimitive` |
| 12 | Automatic DI registration | `[assembly: GenerateMapperRegistration]` |
| 13 | Modern immutable collections | `ImmutableArray<T>`, `FrozenSet<T>`, `FrozenDictionary<K,V>` |
| 14 | Built-in type conversions | Enums, Guids, temporal bridging (`DateOnly`, `DateTimeOffset`) |
| 15 | Static stateless mappers | `[Mapper] public static partial class` |
| 16 | Deep nested dot-notation paths | `[MapProperty("Customer.Address.City", "City")]` |
| 17 | Enum mapping strategies (`ByName`, `ByValue`, `IgnoreCase`) | `[EnumMappingStrategy]` |
| 18 | Explicit enum value remapping | `[MapEnumValue]` |
| 19 | Constant and computed expressions | `[MapValue]` |
| 20 | Member-level exclusion on models | `[MapperIgnore]` |
| 21 | Assembly-wide defaults | `[assembly: MapperDefaults]` |
| 22 | Custom logic via partial methods | Hand-written partial method implementations |
| 23 | Recursive tree / hierarchy mapping | Self-referencing tree models |
| 24 | Entity Framework Core projection pattern | Pure C# static mapper in LINQ `.Select()` |
| 25 | CRUD update without in-place mutation (ADR-D05) | Explicit replacement via entity re-creation |
| 26 | Domain Primitives converters | `EricksonLopez.Mapper.DomainPrimitives` |
| 27 | Functional `Result<T>` mapping (ROP) | `EricksonLopez.Mapper.Result` |
| 28 | Mapster interoperability bridge | `EricksonLopez.Mapper.Mapster` |

---

## Recipe 1: Simple By-Convention Mapping

**Scenario:** Map between two types sharing matching property names.

```csharp
public class User
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

[Mapper]
public partial class UserMapper
{
    public partial UserDto Map(User source);
}

// Usage:
var mapper = new UserMapper();
UserDto dto = mapper.Map(new User { Name = "Jane", Email = "jane@example.com" });
```

---

## Recipe 2: Remap Different Property Names

**Scenario:** Source property name differs from destination property name.

```csharp
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

---

## Recipe 3: Ignore Destination Properties

**Scenario:** Destination contains fields not present on source. Suppress `ELM001` in strict mode.

```csharp
[Mapper]
public partial class SecureMapper
{
    [MapIgnore(nameof(UserDto.PasswordHash))]
    [MapIgnore(nameof(UserDto.SecurityStamp))]
    public partial UserDto Map(UserEntity source);
}
```

---

## Recipe 4: Non-Strict Mode

**Scenario:** Mapping legacy models with numerous unmapped fields where strict mode is undesired.

```csharp
[Mapper(StrictMapping = false)]
public partial class LooseMapper
{
    public partial PartialDto Map(RichEntity source);
}
```

---

## Recipe 5: Nested Object Mapping

**Scenario:** Source contains sub-objects that require transformation into child DTOs.

```csharp
[Mapper]
public partial class OrderMapper
{
    public partial OrderDto MapOrder(Order source);
    public partial AddressDto MapAddress(Address source);  // auto-linked
    public partial LineItemDto MapLineItem(LineItem source);  // auto-linked
}
```

---

## Recipe 6: Immutable Record Mapping

**Scenario:** Destination is a C# record with primary constructor.

```csharp
public record ProductDto(Guid Id, string Name, decimal Price);

[Mapper]
public partial class ProductMapper
{
    public partial ProductDto Map(ProductEntity source);
}
```

---

## Recipe 7: Factory Method Construction

**Scenario:** Destination enforces domain validation through a static factory method.

```csharp
public class Money
{
    public decimal Value { get; }
    public string Currency { get; }

    private Money(decimal value, string currency)
    {
        Value = value;
        Currency = currency;
    }

    public static Money Create(decimal value, string currency)
    {
        if (value < 0) throw new ArgumentException("Negative amount");
        return new Money(value, currency.ToUpperInvariant());
    }
}

[Mapper]
public partial class PaymentMapper
{
    [MapFactory("Create")]
    public partial Money MapMoney(MoneyDto source);
}
```

---

## Recipe 8: Polymorphic Mapping

**Scenario:** Determine destination DTO based on runtime subtype.

```csharp
[Mapper]
public partial class NotificationMapper
{
    [MapDerivedType(typeof(EmailNotification), typeof(EmailNotificationDto))]
    [MapDerivedType(typeof(SmsNotification), typeof(SmsNotificationDto))]
    public partial NotificationDto Map(Notification source);

    public partial EmailNotificationDto MapEmail(EmailNotification source);
    public partial SmsNotificationDto MapSms(SmsNotification source);
}
```

---

## Recipe 9: Nullable Value Fallback

**Scenario:** Map nullable source values (`int?`, `decimal?`) to non-nullable target properties without manual converters.

```csharp
[Mapper]
public partial class InventoryMapper
{
    [MapNullFallback("StockQuantity", "0")]
    [MapNullFallback("Price", "0.0m")]
    public partial InventoryDto Map(InventoryEntity source);
}
```

---

## Recipe 10: Custom Conversion with IConverter

**Scenario:** Complex custom transformation logic encapsulated in an `IConverter` implementation.

```csharp
public class AddressFormatter : IConverter<Address, string>
{
    public string Convert(Address source)
        => $"{source.Street}, {source.City}, {source.Country}";
}

[Mapper]
public partial class ShipmentMapper
{
    [UseConverter(typeof(AddressFormatter))]
    public partial string FormatAddress(Address source);
}
```

---

## Recipe 11: Value Objects & Strongly Typed IDs

**Scenario:** Map strongly-typed IDs to primitives without manual configuration.

```csharp
[ValueObject]
public readonly record struct CustomerId(Guid Value);

[Mapper]
public partial class OrderMapper
{
    public partial OrderDto MapToDto(Order source);   // CustomerId → Guid (.Value)
    public partial Order MapToEntity(OrderDto source); // Guid → CustomerId (new CustomerId)
}
```

---

## Recipe 12: Automatic DI Registration

**Scenario:** Register all mappers in an assembly into the ASP.NET Core DI container.

```csharp
// AssemblyInfo.cs or Program.cs
[assembly: GenerateMapperRegistration]

// In Program.cs:
builder.Services.AddGeneratedMappers();
```

---

## Recipe 13: Modern Immutable Collections

**Scenario:** Map collections directly into modern immutable targets with capacity pre-sizing.

```csharp
[Mapper]
public partial class CatalogMapper
{
    public partial ImmutableArray<ProductDto> MapArray(List<Product> products);
    public partial FrozenSet<string> MapTags(List<string> tags);
    public partial FrozenDictionary<string, decimal> MapPrices(Dictionary<string, decimal> prices);
}
```

---

## Recipe 14: Built-in Type Conversions

Supported out of the box without attributes:
- `SourceEnum` ↔ `TargetEnum` (by matching member names, with `ELM014` in strict mode)
- `enum` ↔ `string` (via `.ToString()` / `Enum.Parse<T>`)
- `Guid` ↔ `string` (via `.ToString()` / `Guid.Parse`)
- `DateTime` ↔ `DateOnly` (`DateOnly.FromDateTime`)
- `DateTime` ↔ `DateTimeOffset` (`new DateTimeOffset`)

---

## Recipe 15: Static Stateless Mappers

**Scenario:** High-performance mapper called without DI or heap allocation.

```csharp
[Mapper]
public static partial class FastOrderMapper
{
    public static partial OrderDto Map(Order source);
}

// Usage in parallel pipelines:
var dtos = orders.AsParallel().Select(FastOrderMapper.Map).ToList();
```

---

## Recipe 16: Deep Nested Property Paths

**Scenario:** Flatten nested hierarchies using dot-notation.

```csharp
[Mapper]
public partial class OrderMapper
{
    [MapProperty("Customer.Address.City", "City")]
    [MapProperty("Customer.Address.PostalCode", "ZipCode")]
    [MapNullFallback("City", "\"Unknown\"")]
    public partial OrderFlatDto Map(Order source);
}
```

---

## Recipe 17: Enum Mapping Strategies

```csharp
[Mapper]
public partial class PriorityMapper
{
    [EnumMappingStrategy(EnumMappingStrategy.ByName, IgnoreCase = true)]
    public partial PriorityDto MapByName(Priority source);

    [EnumMappingStrategy(EnumMappingStrategy.ByValue)]
    public partial ExternalPriority MapByValue(Priority source);
}
```

---

## Recipe 18: Explicit Enum Member Overrides

```csharp
[Mapper]
public partial class StateMapper
{
    [MapEnumValue(OrderState.Created, OrderStatusDto.New)]
    [MapEnumValue(OrderState.InProgress, OrderStatusDto.Processing)]
    [MapEnumValue(OrderState.Shipped, OrderStatusDto.Dispatched)]
    public partial OrderStatusDto Map(OrderState source);
}
```

---

## Recipe 19: Constant & Computed Expressions (`[MapValue]`)

```csharp
[Mapper]
public partial class AuditMapper
{
    [MapValue("CreatedAt", "DateTime.UtcNow")]
    [MapValue("Environment", "\"Production\"")]
    public partial AuditRecordDto Map(Entity source);
}
```

---

## Recipe 20: Member-Level Exclusion on Models (`[MapperIgnore]`)

```csharp
public class UserEntity
{
    public Guid Id { get; set; }

    [MapperIgnore]
    public string PasswordHash { get; set; }
}
```

---

## Recipe 21: Assembly-Wide Defaults

```csharp
using EricksonLopez.Mapper;

[assembly: MapperDefaults(
    StrictMapping = true,
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumIgnoreCase = true
)]
```

---

## Recipe 22: Custom Logic via Partial Methods

```csharp
[Mapper]
public partial class OrderMapper
{
    public partial OrderDto Map(Order source);

    // Custom method implemented in companion file
    public decimal CalculateDiscount(Order order) => order.TotalAmount > 1000m ? 0.15m : 0.05m;
}
```

---

## Recipe 23: Recursive Tree Mapping

```csharp
public class Category
{
    public string Name { get; set; } = string.Empty;
    public List<Category> SubCategories { get; set; } = new();
}

public class CategoryDto
{
    public string Name { get; set; } = string.Empty;
    public List<CategoryDto> SubCategories { get; set; } = new();
}

[Mapper]
public partial class CategoryMapper
{
    public partial CategoryDto Map(Category source);
}
```

---

## Recipe 24: Entity Framework Core Projection Pattern

Because mappers emit pure C# static expressions, project directly inside LINQ `.Select()`:

```csharp
[Mapper]
public static partial class CustomerProjectionMapper
{
    public static partial CustomerSummaryDto Project(Customer customer);
}

// In EF Core query:
var summaries = await dbContext.Customers
    .AsNoTracking()
    .Select(c => new CustomerSummaryDto
    {
        Id = c.Id,
        FullName = c.FirstName + " " + c.LastName,
        City = c.Address.City
    })
    .ToListAsync();
```

---

## Recipe 25: CRUD Update Without Existing-Target (ADR-D05)

**Pattern:** Replace in-place mutation with immutable recreation:

```csharp
[Mapper]
public partial class ProductMapper
{
    public partial ProductDto ToDto(Product source);
    public partial Product ToDomain(ProductUpdateDto dto);
}

public async Task UpdateProductAsync(Guid id, ProductUpdateDto dto, MyDbContext db)
{
    var existing = await db.Products.FindAsync(id) ?? throw new NotFoundException(id);
    var updated = _mapper.ToDomain(dto);
    db.Entry(existing).CurrentValues.SetValues(updated);
    await db.SaveChangesAsync();
}
```

---

## Recipe 26: Domain Primitives Integration (`EricksonLopez.Mapper.DomainPrimitives`)

```csharp
using EricksonLopez.Mapper.DomainPrimitives;

var strongIdConverter = new StrongIdToValueConverter<CustomerId, Guid>();
Guid rawGuid = strongIdConverter.Convert(customerId);

// ValueToDomainPrimitiveConverter requires .NET 7+ (uses static abstract interface members).
// Available on all supported target frameworks: net8.0, net9.0, net10.0.
var valueToPrimitive = new ValueToDomainPrimitiveConverter<string, EmailAddress>();
EmailAddress domainEmail = valueToPrimitive.Convert("user@example.com");
```

---

## Recipe 27: Functional Result<T> Mapping (`EricksonLopez.Mapper.Result`)

```csharp
using EricksonLopez.Mapper.Result;
using EricksonLopez.Result;

Result<UserEntity> entityResult = repository.GetById(id);
Result<UserDto> dtoResult = entityResult.Map(userMapper.Map);

Task<Result<UserEntity>> entityTask = repository.GetByIdAsync(id);
Result<UserDto> asyncDtoResult = await entityTask.MapAsync(userMapper.Map);
```

---

## Recipe 28: Mapster Interoperability Bridge (`EricksonLopez.Mapper.Mapster`)

```csharp
using EricksonLopez.Mapper.Mapster;
using Mapster;

IConverter<SourceDto, TargetDto> converter = new MapsterConverter<SourceDto, TargetDto>();
TargetDto target = converter.Convert(source);

var config = new TypeAdapterConfig();
config.UseConverter(new CustomConverterImplementation());
```
