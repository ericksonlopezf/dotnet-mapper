# Public API Surface Specification

> **Canonical Public Surface Reference:** Derived strictly from `PublicAPI.Shipped.txt` (`EricksonLopez.Mapper.Abstractions`), `EricksonLopez.Mapper.DomainPrimitives`, `EricksonLopez.Mapper.Result`, `EricksonLopez.Mapper.Mapster`, and Roslyn emitted types.  
> **Guarantees:** Zero unverified APIs, zero phantom attributes, 100% NativeAOT and trimming compliant across all runtime libraries (with explicit reflection boundaries noted for Mapster).

---

## 1. Core Abstractions & Attributes (`EricksonLopez.Mapper.Abstractions`)

Namespace: `EricksonLopez.Mapper`

### Attributes

| Attribute | Target | Signature / Parameters | Purpose |
|---|---|---|---|
| `[Mapper]` | `Class`, `Interface` | `MapperAttribute()`<br/>`bool StrictMapping { get; set; } = true` | Marks a `partial` class or interface for compile-time mapping synthesis. |
| `[MapProperty]` | `Method` | `MapPropertyAttribute(string sourceName, string destinationName)` | Explicitly remaps member names and navigates dotted paths (e.g. `"Customer.Address.City"`). |
| `[MapIgnore]` | `Method` | `MapIgnoreAttribute(string destinationName)` | Excludes a destination property from mapping under strict mode, suppressing `ELM001`. |
| `[MapIgnoreSource]` | `Method` | `MapIgnoreSourceAttribute(string sourceName)` | Excludes a source property from participating in member resolution. |
| `[MapperIgnore]` | `Property`, `Field` | `MapperIgnoreAttribute()` | Excludes a model property/field across all mappers in the compilation. |
| `[MapValue]` | `Method` | `MapValueAttribute(string destinationName, string valueExpression)` | Injects a constant literal or computed C# expression into the destination member. |
| `[MapNullFallback]` | `Method` | `MapNullFallbackAttribute(string destinationName, string fallbackExpression)` | Inlines a null-coalescing fallback expression for nullable source values. |
| `[MapFactory]` | `Method` | `MapFactoryAttribute(string methodName)` | Directs object construction to a static factory method on the destination type. |
| `[MapDerivedType]` | `Method` | `MapDerivedTypeAttribute(Type sourceType, Type targetType)` | Enables polymorphic dispatch via compile-time pattern-matching switch expressions. |
| `[UseConverter]` | `Method` | `UseConverterAttribute(Type converterType)`<br/>`UseConverterAttribute(string converterFieldName)` | Directs conversion of specific member types to an `IConverter<S, D>` implementation or injected field. |
| `[EnumMappingStrategy]` | `Class`, `Interface`, `Method` | `EnumMappingStrategyAttribute(EnumMappingStrategy strategy)`<br/>`bool IgnoreCase { get; set; }` | Configures enum translation strategy (`ByName` or `ByValue`) with optional case-insensitivity. |
| `[MapEnumValue]` | `Method` | `MapEnumValueAttribute(object source, object target)` | Explicitly pairs enum members when names or values differ across heterogeneous enums. |
| `[MapperDefaults]` | `Assembly` | `MapperDefaultsAttribute()`<br/>`bool StrictMapping { get; set; }`<br/>`EnumMappingStrategy EnumMappingStrategy { get; set; }`<br/>`bool EnumIgnoreCase { get; set; }` | Defines assembly-wide defaults for strict mapping and enum strategies. |
| `[GenerateMapperRegistration]` | `Assembly` | `GenerateMapperRegistrationAttribute()` | Triggers compile-time emission of `services.AddGeneratedMappers()` extension method. |
| `[ValueObject]` | `Struct`, `Class` | `ValueObjectAttribute()` | Marks DDD Value Objects for automatic scalar wrap/unwrap code synthesis. |

### Enums & Interfaces

| Type | Kind | Definition |
|---|---|---|
| `EnumMappingStrategy` | `enum` | `ByName = 0`, `ByValue = 1` |
| `IConverter<TSource, TDestination>` | `interface` | `TDestination Convert(TSource source);` |

---

## 2. Extension: Domain Primitives (`EricksonLopez.Mapper.DomainPrimitives`)

Namespace: `EricksonLopez.Mapper.DomainPrimitives`

| Type | Signature | Description |
|---|---|---|
| `DomainPrimitiveToValueConverter<TPrimitive, TValue>` | `class : IConverter<TPrimitive, TValue>`<br/>`where TPrimitive : IDomainPrimitive<TPrimitive, TValue>` | Unwraps domain primitive into underlying primitive scalar value. |
| `StrongIdToValueConverter<TStrongId, TValue>` | `class : IConverter<TStrongId, TValue>`<br/>`where TStrongId : IStrongId<TStrongId, TValue>` | Unwraps strongly typed ID into its primitive scalar key. |
| `ValueToDomainPrimitiveConverter<TValue, TPrimitive>` | `class : IConverter<TValue, TPrimitive>`<br/>`where TPrimitive : IDomainPrimitive<TPrimitive, TValue>` | Wraps raw primitive value into validated domain primitive instance. |

---

## 3. Extension: Functional Result Monad (`EricksonLopez.Mapper.Result`)

Namespace: `EricksonLopez.Mapper.Result`

| Extension Method | Target | Description |
|---|---|---|
| `Map<TSource, TDestination>` | `Result<TSource>` | Projects `Result<TSource>` to `Result<TDestination>` via mapper delegate. |
| `MapAsync<TSource, TDestination>` | `Task<Result<TSource>>` | Asynchronously projects `Task<Result<TSource>>` preserving railway-oriented failure states. |
| `MapAsync<TSource, TDestination>` | `ValueTask<Result<TSource>>` | Low-allocation asynchronous projection for `ValueTask<Result<TSource>>`. |
| `MapList<TSource, TDestination>` | `Result<IEnumerable<TSource>>` | Projects collection inside `Result<IEnumerable<TSource>>` to `Result<IReadOnlyList<TDestination>>`. |

---

## 4. Extension: Mapster Bridge Adapter (`EricksonLopez.Mapper.Mapster`)

Namespace: `EricksonLopez.Mapper.Mapster`

> ⚠️ **Notice**: Mapster uses runtime reflection. This package is explicitly configured with `IsAotCompatible=false` and `IsTrimmable=false`.

| Type / Method | Signature | Description |
|---|---|---|
| `MapsterConverter<TSource, TDestination>` | `class : IConverter<TSource, TDestination>` | Bridges Mapster's runtime adapter engine to the unified `IConverter` contract. |
| `MapsterMapperExtensions.UseConverter` | `TypeAdapterConfig.UseConverter(IConverter<S, D>)` | Registers an `EricksonLopez.Mapper` converter into Mapster's global configuration. |

---

## 5. Emitted Public APIs (`EricksonLopez.Mapper.Generator`)

When `[assembly: GenerateMapperRegistration]` is present in the compilation:

```csharp
namespace EricksonLopez.Mapper
{
    public static class MapperServiceCollectionExtensions
    {
        public static IServiceCollection AddGeneratedMappers(this IServiceCollection services);
    }
}
```

---

## 6. Public Compiler Diagnostics Catalog (`ELM001`–`ELM018`)

| Diagnostic ID | Default Severity | Category | Description |
|---|---|---|---|
| **`ELM001`** | Error | Generator | Strict Mapping: Destination property has no source match. |
| **`ELM002`** | Error | Generator | Destination type lacks an accessible constructor or static factory method. |
| **`ELM003`** | Error | Generator | Unsupported type conversion between source and destination members. |
| **`ELM004`** | Error | Generator | Nullability mismatch: Nullable source assigned to non-nullable destination. |
| **`ELM005`** | Error | Generator | Ambiguous case-insensitive member match. |
| **`ELM006`** | Error | Generator | Destination type has no constructor matching mapping requirements. |
| **`ELM007`** | Error | Generator | Ambiguous constructors: Multiple accessible constructors require `[MapFactory]`. |
| **`ELM008`** | Error | Analyzer | Prohibited reflection API (`System.Reflection`, `Activator`, `Marshal`, `RuntimeHelpers`). |
| **`ELM009`** | Error | Analyzer | Prohibited `dynamic` keyword in mapper class. |
| **`ELM010`** | Error | Generator | Circular mapping dependency detected across object graphs. |
| **`ELM011`** | Warning | Generator | Incomplete polymorphism: Abstract base destination type has unmapped derived types. |
| **`ELM012`** | Error | Analyzer | Mapper class or interface annotated with `[Mapper]` is missing the `partial` modifier. |
| **`ELM013`** | Error | Generator | Converter specified in `[UseConverter]` does not implement `IConverter<S, D>`. |
| **`ELM014`** | Error / Warning | Generator | Unmapped enum member in strict mode. |
| **`ELM015`** | Warning | Generator | Narrowing numeric conversion potential data loss (e.g. `long` to `int`). |
| **`ELM016`** | Warning | Generator | String-to-enum conversion runtime parsing risk. |
| **`ELM017`** | Error | Generator | Factory method specified in `[MapFactory]` was not found on destination type. |
| **`ELM018`** | Warning | Generator | Destination member is targeted by duplicate `[MapProperty]` declarations. |
