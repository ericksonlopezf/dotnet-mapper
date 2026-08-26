# API Reference: EricksonLopez.Mapper Ecosystem

> **Source of Truth:** Canonical reference generated from the public API surface defined across `EricksonLopez.Mapper.Abstractions`, `EricksonLopez.Mapper.DomainPrimitives`, `EricksonLopez.Mapper.Result`, and `EricksonLopez.Mapper.Mapster`.
> Every code snippet and signature in this document is verified against current source contracts and `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`.

---

## 1. Core Attributes (`EricksonLopez.Mapper.Abstractions`)

### `MapperAttribute`

Instructs the Source Generator to implement the `partial` mapping methods defined in the annotated class or interface.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public sealed class MapperAttribute : Attribute
{
    public bool StrictMapping { get; set; } = true;
}
```

- `StrictMapping` (default `true`): When enabled, all destination properties must be matched or explicitly excluded. Unmapped destination properties produce `ELM001` at compile time.

```csharp
[Mapper(StrictMapping = true)]
public partial class UserMapper
{
    public partial UserDto Map(User source);
}
```

---

### `MapPropertyAttribute`

Explicitly maps a source member or deep path to a destination member when names differ.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapPropertyAttribute : Attribute
{
    public string SourceName { get; }
    public string DestinationName { get; }

    public MapPropertyAttribute(string sourceName, string destinationName);
}
```

- Supports deep dot-notation navigation (e.g. `"Customer.Address.City"`, `"City"`) with safe null navigation (`?.`).

```csharp
[Mapper]
public partial class OrderMapper
{
    [MapProperty("Customer.Address.City", "City")]
    [MapProperty("TotalAmount", "Amount")]
    public partial OrderSummaryDto Map(Order source);
}
```

---

### `MapIgnoreAttribute`

Explicitly excludes a **destination** property from mapping, suppressing `ELM001` in strict mode.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapIgnoreAttribute : Attribute
{
    public string DestinationName { get; }

    public MapIgnoreAttribute(string destinationName);
}
```

```csharp
[Mapper]
public partial class UserMapper
{
    [MapIgnore("InternalToken")]
    public partial UserDto Map(User source);
}
```

---

### `MapIgnoreSourceAttribute`

Explicitly excludes a **source** property from participating in member resolution.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapIgnoreSourceAttribute : Attribute
{
    public string SourceName { get; }

    public MapIgnoreSourceAttribute(string sourceName);
}
```

```csharp
[Mapper]
public partial class SecurityMapper
{
    [MapIgnoreSource("PasswordHash")]
    public partial UserDto Map(UserEntity source);
}
```

---

### `MapperIgnoreAttribute`

Excludes a property or field from all mapping operations when placed directly on the model member.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class MapperIgnoreAttribute : Attribute;
```

```csharp
public class UserEntity
{
    public Guid Id { get; set; }

    [MapperIgnore]
    public string InternalCacheKey { get; set; }
}
```

---

### `MapValueAttribute`

Assigns a constant or computed C# expression directly to a destination property or constructor parameter.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapValueAttribute : Attribute
{
    public string DestinationName { get; }
    public string ValueExpression { get; }

    public MapValueAttribute(string destinationName, string valueExpression);
}
```

```csharp
[Mapper]
public partial class AuditMapper
{
    [MapValue("CreatedAt", "System.DateTime.UtcNow")]
    [MapValue("Environment", "\"Production\"")]
    public partial AuditRecordDto Map(Entity source);
}
```

---

### `EnumMappingStrategyAttribute`

Configures enum member matching strategy (`ByName` vs `ByValue`) and case-sensitivity for a class or method.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class EnumMappingStrategyAttribute : Attribute
{
    public EnumMappingStrategy Strategy { get; }
    public bool IgnoreCase { get; set; }

    public EnumMappingStrategyAttribute(EnumMappingStrategy strategy);
}
```

```csharp
[Mapper]
[EnumMappingStrategy(EnumMappingStrategy.ByName, IgnoreCase = true)]
public partial class StatusMapper
{
    public partial OrderStatusDto Map(OrderState source);
}
```

---

### `MapEnumValueAttribute`

Explicitly maps a source enum member to a destination enum member when names differ.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapEnumValueAttribute : Attribute
{
    public object Source { get; }
    public object Target { get; }

    public MapEnumValueAttribute(object source, object target);
}
```

```csharp
[Mapper]
public partial class OrderMapper
{
    [MapEnumValue(PaymentStatus.Pending, OrderStatus.Queued)]
    [MapEnumValue(PaymentStatus.Captured, OrderStatus.Completed)]
    public partial OrderStatus Map(PaymentStatus source);
}
```

---

### `MapperDefaultsAttribute`

Specifies assembly-wide defaults for `StrictMapping`, `EnumMappingStrategy`, and `EnumIgnoreCase`.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public sealed class MapperDefaultsAttribute : Attribute
{
    public EnumMappingStrategy EnumMappingStrategy { get; set; } = EnumMappingStrategy.ByName;
    public bool EnumIgnoreCase { get; set; } = false;
    public bool StrictMapping { get; set; } = true;
}
```

```csharp
[assembly: MapperDefaults(StrictMapping = true, EnumMappingStrategy = EnumMappingStrategy.ByName, EnumIgnoreCase = true)]
```

---

### `MapFactoryAttribute`

Instructs the generator to instantiate the destination type by calling a static factory method.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class MapFactoryAttribute : Attribute
{
    public string MethodName { get; }

    public MapFactoryAttribute(string methodName);
}
```

```csharp
[Mapper]
public partial class OrderMapper
{
    [MapFactory("Create")]
    public partial OrderEntity Map(CreateOrderRequest source);
}
```

---

### `MapDerivedTypeAttribute`

Registers a derived source type and its target type for compile-time polymorphic dispatch (pattern-matching switch).

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapDerivedTypeAttribute : Attribute
{
    public Type SourceType { get; }
    public Type TargetType { get; }

    public MapDerivedTypeAttribute(Type sourceType, Type targetType);
}
```

```csharp
[Mapper]
public partial class VehicleMapper
{
    [MapDerivedType(typeof(CarEntity), typeof(CarDto))]
    [MapDerivedType(typeof(TruckEntity), typeof(TruckDto))]
    public partial VehicleDto Map(VehicleEntity source);

    public partial CarDto MapCar(CarEntity source);
    public partial TruckDto MapTruck(TruckEntity source);
}
```

---

### `UseConverterAttribute`

Specifies a custom `IConverter<TSource, TDestination>` type or an instance field holding a converter.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class UseConverterAttribute : Attribute
{
    public Type ConverterType { get; }
    public string? ConverterFieldName { get; }

    public UseConverterAttribute(Type converterType);
    public UseConverterAttribute(string converterFieldName);
}
```

```csharp
[Mapper]
public partial class ShipmentMapper
{
    [UseConverter(typeof(CustomAddressConverter))]
    public partial AddressDto MapAddress(Address source);
}
```

---

### `MapNullFallbackAttribute`

Emits a literal C# expression as fallback when a nullable source member is null and destination is non-nullable.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapNullFallbackAttribute : Attribute
{
    public string DestinationName { get; }
    public string FallbackExpression { get; }

    public MapNullFallbackAttribute(string destinationName, string fallbackExpression);
}
```

```csharp
[Mapper]
public partial class ProductMapper
{
    [MapNullFallback("Price", "0m")]
    public partial ProductDto Map(ProductEntity source);
}
```

---

### `GenerateMapperRegistrationAttribute`

Emits an `AddGeneratedMappers(this IServiceCollection)` extension method for automated DI registration.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public sealed class GenerateMapperRegistrationAttribute : Attribute;
```

```csharp
[assembly: GenerateMapperRegistration]
```

**DI Lifetime Behavior:**
- All non-static `[Mapper]` classes → registered as `Singleton` (stateless, thread-safe).
- Custom converters referenced via `[UseConverter(typeof(MyConverter))]` → registered as `Transient`.
- Static `partial class` mappers → omitted (no instantiation required).

---

### `ValueObjectAttribute`

Marks a class or struct as a Value Object to enable automatic wrap/unwrap.

```csharp
namespace EricksonLopez.Mapper;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class ValueObjectAttribute : Attribute;
```

```csharp
[ValueObject]
public readonly record struct CustomerId(Guid Value);
```

---

## 2. Core Interfaces & Enums (`EricksonLopez.Mapper.Abstractions`)

### `IConverter<TSource, TDestination>`

Contract for custom type-to-type conversion.

```csharp
namespace EricksonLopez.Mapper;

public interface IConverter<TSource, TDestination>
{
    TDestination Convert(TSource source);
}
```

### `EnumMappingStrategy`

```csharp
namespace EricksonLopez.Mapper;

public enum EnumMappingStrategy
{
    ByName = 0,
    ByValue = 1
}
```

---

## 3. Extension Packages

### `EricksonLopez.Mapper.DomainPrimitives`

Converters for `EricksonLopez.DomainPrimitives`:

```csharp
namespace EricksonLopez.Mapper.DomainPrimitives;

public sealed class DomainPrimitiveToValueConverter<TPrimitive, TValue> : IConverter<TPrimitive, TValue>
    where TPrimitive : IDomainPrimitive<TPrimitive, TValue>
    where TValue : notnull, IComparable<TValue>, IEquatable<TValue>;

public sealed class StrongIdToValueConverter<TStrongId, TValue> : IConverter<TStrongId, TValue>
    where TStrongId : IStrongId<TStrongId, TValue>
    where TValue : notnull, IComparable<TValue>, IEquatable<TValue>;

// Available on .NET 7+ (static abstract interface members). All supported target frameworks satisfy this requirement.
#if NET7_0_OR_GREATER
public sealed class ValueToDomainPrimitiveConverter<TValue, TPrimitive> : IConverter<TValue, TPrimitive>
    where TPrimitive : IDomainPrimitive<TPrimitive, TValue>
    where TValue : notnull, IComparable<TValue>, IEquatable<TValue>;
#endif
```

### `EricksonLopez.Mapper.Result`

Functional mapping extensions for `Result<T>`:

```csharp
namespace EricksonLopez.Mapper.Result;

public static class ResultMappingExtensions
{
    public static Result<TDest> Map<TSource, TDest>(
        this Result<TSource> result, Func<TSource, TDest> mapFunc);

    public static Task<Result<TDest>> MapAsync<TSource, TDest>(
        this Task<Result<TSource>> resultTask, Func<TSource, TDest> mapFunc);

    public static ValueTask<Result<TDest>> MapAsync<TSource, TDest>(
        this ValueTask<Result<TSource>> resultTask, Func<TSource, TDest> mapFunc);

    public static Result<IReadOnlyList<TDest>> MapList<TSource, TDest>(
        this Result<IEnumerable<TSource>> result, Func<TSource, TDest> mapFunc);
}
```

### `EricksonLopez.Mapper.Mapster`

Bi-directional integration with Mapster:

```csharp
namespace EricksonLopez.Mapper.Mapster;

public sealed class MapsterConverter<TSource, TDestination> : IConverter<TSource, TDestination>
{
    public MapsterConverter();
    public MapsterConverter(TypeAdapterConfig config);
    public TDestination Convert(TSource source);
}

public static class MapsterMapperExtensions
{
    public static TypeAdapterConfig UseConverter<TSource, TDestination>(
        this TypeAdapterConfig config, IConverter<TSource, TDestination> converter);
}
```

---

## 4. Diagnostics Index (ELM001–ELM016)

| Code | Severity | Description | Fix |
|---|---|---|---|
| **ELM001** | Error | Unmapped destination member in strict mode | Add `[MapProperty]`, `[MapIgnore]`, `[MapValue]`, or matching source property |
| **ELM002** | Error | Destination type has no accessible public constructors or factories | Add accessible constructor or `[MapFactory]` |
| **ELM003** | Error | Unsupported type conversion | Use matching types or `[UseConverter]` |
| **ELM004** | Error | Nullability mismatch | Use `[MapNullFallback]` |
| **ELM005** | Error | Ambiguous property match | Disambiguate with `[MapProperty]` |
| **ELM006** | Error | Destination type lacks supported constructor or settable properties | Add accessible constructor or setters |
| **ELM007** | Error | Ambiguous constructor | Use `[MapFactory]` |
| **ELM008** | Error | Mapper uses reflection or prohibited AOT APIs | Remove reflection |
| **ELM009** | Error | Mapper uses dynamic keyword | Remove dynamic keyword |
| **ELM010** | Error | Circular mapping dependency detected | Break recursion cycle in DTO models |
| **ELM011** | Warning | Abstract base type with potentially uncovered derived types | Map all derived types with `[MapDerivedType]` |
| **ELM012** | Error | `[Mapper]` class is not declared `partial` | Add `partial` keyword (`MakePartialCodeFixProvider`) |
| **ELM013** | Error | Type does not implement `IConverter<TSrc, TDst>` | Implement `IConverter` contract |
| **ELM014** | Error / Warn | Enum member has no destination equivalent in strict mode | Use `[MapEnumValue]` |
| **ELM015** | Warning | Narrowing numeric conversion potential data loss | Use `[UseConverter]` or explicit cast |
| **ELM016** | Warning | String to enum parsing runtime exception risk | Validate strings or use typed enums |
