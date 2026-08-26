# Cookbook: EricksonLopez.Mapper

Collection of real-world recipes for solving mapping scenarios using the public API of **EricksonLopez.Mapper**.
Each recipe is based on actual compiler and generator behavior, ensuring complete type-safety and Native AOT compatibility.

---

## Recipe 1: Simple Mapping (Name Convention)

**Problem:** You need to map between two types with matching property names without additional configuration.

**Solution:** Declare a `[Mapper]` attribute on a `partial class` with a `partial` method. The generator matches properties case-insensitively.

```csharp
[Mapper]
public partial class UserMapper
{
    public partial UserDto Map(User source);
}
```

**Generated Code Equivalent:**
```csharp
public partial UserDto Map(User source)
{
    if (source == null) throw new ArgumentNullException(nameof(source));
    var target = new UserDto()
    {
        Name = source.Name,
        Email = source.Email,
    };
    return target;
}
```

**Best Practices:**
- Keep `StrictMapping = true` (default) to catch unmapped destination properties at compile time.
- Use consistent property names across architectural layers.

---

## Recipe 2: Mapping Properties with Different Names

**Problem:** Source and destination types use different property names.

**Solution:** Use `[MapProperty("SourceName", "DestinationName")]`.

```csharp
[Mapper]
public partial class CustomerMapper
{
    [MapProperty(nameof(Customer.InternalId), nameof(CustomerDto.CustomerId))]
    [MapProperty(nameof(Customer.FullName), nameof(CustomerDto.DisplayName))]
    public partial CustomerDto Map(Customer source);
}
```

---

## Recipe 3: Ignoring Destination Properties

**Problem:** The destination contains properties that have no source equivalent and trigger `ELM001`.

**Solution:** Use `[MapIgnore("DestinationPropertyName")]` on the mapping method.

```csharp
[Mapper]
public partial class SecureUserMapper
{
    [MapIgnore(nameof(UserDto.PasswordHash))]
    [MapIgnore(nameof(UserDto.ComputedScore))]
    public partial UserDto Map(User source);
}
```

---

## Recipe 4: Disabling StrictMapping

**Problem:** The models have numerous unmapped properties and decorating each with `[MapIgnore]` is tedious.

**Solution:** Set `StrictMapping = false` on `[Mapper]`.

```csharp
[Mapper(StrictMapping = false)]
public partial class LooseMapper
{
    public partial PartialDto Map(RichEntity source);
}
```

---

## Recipe 5: Nested Mapping (Complex Object Graphs)

**Problem:** Source contains nested complex types that also require mapping.

**Solution:** Declare sub-mapping methods within the same mapper class. The generator links them automatically.

```csharp
[Mapper]
public partial class OrderMapper
{
    public partial OrderDto MapOrder(Order source);
    public partial AddressDto MapAddress(Address source);
    public partial OrderItemDto MapItem(OrderItem source);
}
```

---

## Recipe 6: Mapping to Immutable Records (Parameterized Constructors)

**Problem:** Target is an immutable C# record with a primary constructor.

**Solution:** No configuration required. The generator detects the constructor and matches arguments by parameter name.

```csharp
public record ProductDto(Guid Id, string Name, decimal Price);

[Mapper]
public partial class ProductMapper
{
    public partial ProductDto Map(ProductEntity source);
}
```

---

## Recipe 7: Using Factory Methods

**Problem:** The target type enforces encapsulation and domain invariants through a static factory method instead of a public constructor.

**Solution:** Use `[MapFactory("FactoryMethodName")]`.

```csharp
public class MonetaryAmount
{
    public decimal Value { get; init; }
    public string Currency { get; init; }

    public static MonetaryAmount Create(decimal value, string currency)
    {
        if (value < 0) throw new ArgumentException("Amount cannot be negative");
        return new MonetaryAmount { Value = value, Currency = currency.ToUpperInvariant() };
    }
}

[Mapper]
public partial class PaymentMapper
{
    [MapFactory("Create")]
    public partial MonetaryAmount Map(PaymentRequest source);
}
```

---

## Recipe 8: Polymorphic Mapping with [MapDerivedType]

**Problem:** You have an inheritance hierarchy and need to dispatch to specific DTO derived types based on runtime instance types.

**Solution:** Use `[MapDerivedType(typeof(SourceDerived), typeof(DestDerived))]`.

```csharp
[Mapper]
public partial class NotificationMapper
{
    [MapDerivedType(typeof(EmailNotification), typeof(EmailNotificationDto))]
    [MapDerivedType(typeof(SmsNotification), typeof(SmsNotificationDto))]
    [MapDerivedType(typeof(PushNotification), typeof(PushNotificationDto))]
    public partial NotificationDto Map(Notification source);

    public partial EmailNotificationDto MapEmail(EmailNotification source);
    public partial SmsNotificationDto MapSms(SmsNotification source);
    public partial PushNotificationDto MapPush(PushNotification source);
}
```

---

## Recipe 9: Null Fallback for Value Types

**Problem:** Source contains nullable value types (`int?`, `decimal?`) while destination requires non-nullable types (`int`, `decimal`).

**Solution:** Use `[MapNullFallback("PropertyName", "fallbackLiteral")]`.

```csharp
[Mapper]
public partial class InventoryMapper
{
    [MapNullFallback("StockQuantity", "0")]
    [MapNullFallback("ReorderPoint", "10")]
    [MapNullFallback("Price", "0m")]
    public partial InventoryDto Map(InventoryEntity source);
}
```

---

## Recipe 10: Custom Converters via IConverter<TSource, TDestination>

**Problem:** Complex transformation logic exceeds automated mapping heuristics.

**Solution:** Implement `IConverter<TSource, TDestination>` and decorate the mapping method with `[UseConverter(typeof(TConverter))]`.

```csharp
public class AddressToStringConverter : IConverter<Address, string>
{
    public string Convert(Address source)
        => $"{source.Street}, {source.City} {source.PostalCode}, {source.Country}";
}

[Mapper]
public partial class ShipmentMapper
{
    [UseConverter(typeof(AddressToStringConverter))]
    public partial string MapAddressToLabel(Address source);
}
```

---

## Recipe 11: Value Objects and Strongly Typed IDs

**Problem:** Domain types use strongly typed IDs (`CustomerId(Guid)`) and you need automatic wrapping and unwrapping.

**Solution:** Annotate value objects with `[ValueObject]` or rely on single-parameter constructor heuristics.

```csharp
[ValueObject]
public readonly record struct CustomerId(Guid Value);

[Mapper]
public partial class OrderMapper
{
    public partial OrderDto MapToDto(OrderEntity source);
    public partial OrderEntity MapToEntity(CreateOrderCommand source);
}
```

---

## Recipe 12: Automatic DI Registration with [assembly: GenerateMapperRegistration]

**Problem:** Registering all mappers individually in `IServiceCollection` is error-prone.

**Solution:** Add `[assembly: GenerateMapperRegistration]` to the project.

```csharp
[assembly: EricksonLopez.Mapper.GenerateMapperRegistration]

// In service configuration:
services.AddGeneratedMappers();
```

---

## Recipe 13: Collection and Immutable Mapping

**Problem:** Transforming between different collection types (`List<T>`, `T[]`, `ImmutableArray<T>`, `HashSet<T>`).

**Solution:** The generator recognizes collection targets and emits optimized builders with size pre-allocation.

```csharp
public class CatalogDto
{
    public ProductDto[] Products { get; set; }
    public ImmutableArray<TagDto> Tags { get; set; }
    public HashSet<string> Keywords { get; set; }
}

[Mapper]
public partial class CatalogMapper
{
    public partial CatalogDto Map(CatalogEntity source);
    public partial ProductDto MapProduct(ProductEntity source);
    public partial TagDto MapTag(TagEntity source);
}
```

---

## Recipe 14: Built-in Scalar Conversions

**Problem:** Converting between common scalar types (`enum ↔ string`, `Guid ↔ string`, `DateTime ↔ DateOnly`).

**Solution:** The generator automatically applies built-in conversion templates without manual converter declarations.

```csharp
public class ApiRequest
{
    public string Status { get; set; }
    public string OrderId { get; set; }
}

public class OrderCommand
{
    public OrderStatus Status { get; set; }
    public Guid OrderId { get; set; }
}

[Mapper]
public partial class RequestMapper
{
    public partial OrderCommand Map(ApiRequest source);
}
```

---

## Recipe 15: Stateless Static Mappers

**Problem:** Pure mapping routines that do not require object instantiation.

**Solution:** Declare the mapper class as `static partial`.

```csharp
[Mapper]
public static partial class OrderMapper
{
    public static partial OrderDto Map(Order source);
}
```

---

## Recipe 16: Thread-Safe Parallel Execution (PLINQ)

**Problem:** High-throughput batch mapping across multiple CPU threads.

**Solution:** Generated mappers are stateless and inherently thread-safe.

```csharp
var mapper = new ProductMapper();
var dtos = products.AsParallel().Select(mapper.Map).ToList();
```

---

## Recipe 17: Constant and Literal Injection with [MapValue]

**Problem:** Destination requires constant metadata or environment variables not present in source.

**Solution:** Use `[MapValue("PropertyName", "literalExpression")]`.

```csharp
[Mapper]
public partial class AccountMapper
{
    [MapValue("Status", "\"ACTIVE\"")]
    [MapValue("CreatedAt", "System.DateTime.UtcNow")]
    public partial AccountEntity Map(RegistrationRequest source);
}
```

---

## Recipe 18: Ignoring Source Properties with [MapIgnoreSource]

**Problem:** Exclude sensitive source properties from mapping evaluation.

**Solution:** Use `[MapIgnoreSource("PropertyName")]`.

```csharp
[Mapper]
public partial class UserProfileMapper
{
    [MapIgnoreSource("PasswordHash")]
    public partial UserDto Map(UserEntity source);
}
```

---

## Recipe 19: Global Member Exclusion with [MapperIgnore]

**Problem:** An entity property should never be mapped across any mapper in the codebase.

**Solution:** Decorate the member directly with `[MapperIgnore]`.

```csharp
public class UserEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    [MapperIgnore]
    public string InternalDiagnosticsToken { get; set; }
}
```

---

## Recipe 20: Enum Mapping Strategies with [EnumMappingStrategy]

**Problem:** Enums differ by case or require numeric integer mapping (`ByValue`).

**Solution:** Use `[EnumMappingStrategy(EnumMappingStrategy.ByName, IgnoreCase = true)]` or `[EnumMappingStrategy(EnumMappingStrategy.ByValue)]`.

```csharp
[Mapper]
[EnumMappingStrategy(EnumMappingStrategy.ByName, IgnoreCase = true)]
public partial class OrderMapper
{
    public partial OrderDto Map(OrderEntity source);
}
```

---

## Recipe 21: Explicit Enum Value Mapping with [MapEnumValue]

**Problem:** Enum members have differing identifiers across domain and DTO representations.

**Solution:** Use `[MapEnumValue(SourceEnum.Member, TargetEnum.Member)]`.

```csharp
[Mapper]
public partial class PaymentMapper
{
    [MapEnumValue(PaymentStatus.Pending, OrderStatus.Queued)]
    [MapEnumValue(PaymentStatus.Captured, OrderStatus.Completed)]
    public partial OrderDto Map(PaymentEntity source);
}
```

---

## Recipe 22: Assembly Defaults with [assembly: MapperDefaults]

**Problem:** Standardize mapping configurations across the entire assembly.

**Solution:** Declare `[assembly: MapperDefaults]` in assembly attributes.

```csharp
using EricksonLopez.Mapper;

[assembly: MapperDefaults(
    StrictMapping = true,
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumIgnoreCase = true
)]
```

---

## Recipe 23: Instance Converter Injection with [UseConverter("fieldName")]

**Problem:** Converters require constructor-injected runtime dependencies (e.g., formatters or encryption services).

**Solution:** Pass the converter field name to `[UseConverter]`.

```csharp
[Mapper]
public partial class LocalizedOrderMapper
{
    private readonly IConverter<Money, FormattedMoneyDto> _moneyFormatter;

    public LocalizedOrderMapper(IConverter<Money, FormattedMoneyDto> moneyFormatter)
    {
        _moneyFormatter = moneyFormatter;
    }

    [UseConverter(nameof(_moneyFormatter))]
    public partial FormattedMoneyDto MapMoney(Money source);
}
```

---

## Recipe 24: Integration with EricksonLopez.DomainPrimitives

**Problem:** Clean Architecture domains using `IDomainPrimitive<TSelf, TValue>` and `IStrongId<TSelf, TValue>`.

**Solution:** Use `EricksonLopez.Mapper.DomainPrimitives`:
- `DomainPrimitiveToValueConverter<TPrimitive, TValue>`
- `StrongIdToValueConverter<TStrongId, TValue>`
- `ValueToDomainPrimitiveConverter<TValue, TPrimitive>`

```csharp
using EricksonLopez.Mapper.DomainPrimitives;

var idConverter = new StrongIdToValueConverter<CustomerId, Guid>();
Guid rawGuid = idConverter.Convert(customerId);
```

---

## Recipe 25: Functional ROP Integration with EricksonLopez.Result

**Problem:** Railway-Oriented Programming using `Result<T>` without manual unwrapping or error checks.

**Solution:** Use `EricksonLopez.Mapper.Result`:

```csharp
using EricksonLopez.Mapper.Result;
using EricksonLopez.Result;

Result<UserEntity> entityResult = GetUser();
Result<UserDto> dtoResult = entityResult.Map(userMapper.Map);

Task<Result<UserEntity>> entityTask = GetUserAsync();
Result<UserDto> asyncDtoResult = await entityTask.MapAsync(userMapper.Map);
```

---

## Recipe 26: Interoperability with Mapster

**Problem:** Gradual migration from Mapster to `EricksonLopez.Mapper`.

**Solution:** Use `EricksonLopez.Mapper.Mapster`:

```csharp
using EricksonLopez.Mapper.Mapster;
using Mapster;

// 1. Wrap Mapster as an IConverter:
IConverter<LegacyUser, ModernUser> converter = new MapsterConverter<LegacyUser, ModernUser>();
ModernUser user = converter.Convert(legacyUser);

// 2. Register IConverter inside Mapster TypeAdapterConfig:
var config = new TypeAdapterConfig();
config.UseConverter(new CustomUserConverter());
```


