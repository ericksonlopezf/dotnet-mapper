# API Public Inventory — EricksonLopez.Mapper Ecosystem

**Sources of truth:**
- `PublicAPI.Shipped.txt` & `PublicAPI.Unshipped.txt` from `EricksonLopez.Mapper.Abstractions`
- `EricksonLopez.Mapper.DomainPrimitives`
- `EricksonLopez.Mapper.Result`
- `EricksonLopez.Mapper.Mapster`

This inventory is the canonical reference for all public APIs in the `EricksonLopez.Mapper` library and its official extension packages. Every code example in this repository uses only APIs present in this document.

---

## 1. Attributes (`EricksonLopez.Mapper.Abstractions`)

### `MapperAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.MapperAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Instructs the Source Generator to generate implementations of `partial` methods in the annotated class or interface. |
| **Dependencies** | None at runtime. Consumed by `MapperGenerator` at compile time. |
| **Properties** | `bool StrictMapping { get; set; }` — default `true`. |
| **Use Cases** | All general-purpose mappers: Entity→DTO, DTO→Entity, Command→Entity. |
| **Complexity** | Basic |
| **Sample** | ✅ `Level1_QuickStart/QuickStartDemo.cs`, `Level2_Configuration/ConfigurationDemo.cs` |

### `MapPropertyAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.MapPropertyAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Explicitly remaps a source property to a destination property with a different name. |
| **Dependencies** | None. Combined with `[Mapper]`. |
| **Parameters** | `string sourceName`, `string destinationName` |
| **Use Cases** | DTOs with different naming conventions than entities. Deep dot-path hierarchy flattening (`"Customer.Address.City"` → `"CityName"`). |
| **Complexity** | Basic (rename) / Intermediate (dot-path) |
| **Sample** | ✅ `Level2_Configuration/ConfigurationDemo.cs`, `Level3_RealWorld/BuiltinConversionsDemo.cs`, `Level3_RealWorld/HierarchyFlatteningDemo.cs` (dot-path) |

### `MapIgnoreAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.MapIgnoreAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Explicitly excludes a destination property from mapping, suppressing ELM001 in strict mode. |
| **Dependencies** | None. Combined with `[Mapper]`. |
| **Parameters** | `string destinationName` |
| **Use Cases** | Computed properties, audit properties not exposed externally. |
| **Complexity** | Basic |
| **Sample** | ✅ `Level2_Configuration/ConfigurationDemo.cs` |

### `MapIgnoreSourceAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.MapIgnoreSourceAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Explicitly excludes a source property from participation in mapping resolution. |
| **Dependencies** | None. Combined with `[Mapper]`. |
| **Parameters** | `string sourceName` |
| **Use Cases** | Sensitive security fields on source entities (`PasswordHash`, `SecurityStamp`) not mapped to DTO. |
| **Complexity** | Basic |
| **Sample** | ✅ `Level2_Configuration/ConfigurationDemo.cs`, `Level10_Architecture/ArchitectureDemo.cs` |

### `MapperIgnoreAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.MapperIgnoreAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Excludes a property or field from mapping when declared directly on the model member. |
| **Dependencies** | None. |
| **Target** | `AttributeTargets.Property \| AttributeTargets.Field` |
| **Use Cases** | Internal cache fields, diagnostic tokens, or ORM shadow properties. |
| **Complexity** | Basic |
| **Sample** | ✅ `Level2_Configuration/ConfigurationDemo.cs` |

### `MapValueAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.MapValueAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Assigns a constant or computed C# expression directly to a destination property or constructor parameter. |
| **Dependencies** | None. |
| **Parameters** | `string destinationName`, `string valueExpression` |
| **Use Cases** | Emitting fixed flags (`"ACTIVE"`), audit timestamps (`System.DateTime.UtcNow`), or environment tags. |
| **Complexity** | Intermediate |
| **Sample** | ✅ `Level2_Configuration/ValueAssignmentDemo.cs` |

### `EnumMappingStrategyAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.EnumMappingStrategyAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Configures enum mapping strategy (`ByName` vs `ByValue`) and case-sensitivity for a class or method. |
| **Dependencies** | None. |
| **Parameters** | `EnumMappingStrategy strategy`, `bool IgnoreCase { get; set; }` |
| **Use Cases** | Case-insensitive enum matching (`pending` -> `Pending`), numeric enum cast (`ByValue`). |
| **Complexity** | Intermediate |
| **Sample** | ✅ `Level2_Configuration/EnumMappingDemo.cs` — Scenario A: `ByName+IgnoreCase`, Scenario B: `ByValue` |

### `MapEnumValueAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.MapEnumValueAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Explicitly maps an individual source enum member to a destination enum member when names differ. |
| **Dependencies** | None. |
| **Parameters** | `object source`, `object target` |
| **Use Cases** | Heterogeneous enums (e.g. `PaymentStatus.Captured` -> `OrderStatus.Completed`). |
| **Complexity** | Intermediate |
| **Sample** | ✅ `Level2_Configuration/EnumMappingDemo.cs` |

### `MapperDefaultsAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.MapperDefaultsAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Defines assembly-wide defaults for `StrictMapping`, `EnumMappingStrategy`, and `EnumIgnoreCase`. |
| **Dependencies** | None. |
| **Target** | `AttributeTargets.Assembly` |
| **Properties** | `bool StrictMapping { get; set; } = true`, `EnumMappingStrategy EnumMappingStrategy { get; set; } = ByName`, `bool EnumIgnoreCase { get; set; } = false` |
| **Use Cases** | Project-wide convention configuration without repetitive class annotations. |
| **Complexity** | Intermediate |
| **Sample** | ✅ `Level2_Configuration/AssemblyConfig.cs` (declaration) + `Level2_Configuration/MapperDefaultsDemo.cs` (behavior demo) |

### `MapFactoryAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.MapFactoryAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Instructs the generator to use a static factory method instead of `new DestType()` or constructors. |
| **Dependencies** | The factory method must be `public static` and return the destination type. |
| **Parameters** | `string methodName` — name of the factory method on the destination type. |
| **Use Cases** | DDD: objects with domain invariants in their factory. Types with private constructors. |
| **Complexity** | Advanced |
| **Sample** | ✅ `Level4_Advanced/FactoryMethodDemo.cs` |

### `MapDerivedTypeAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.MapDerivedTypeAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Configures polymorphic dispatch via a compile-time generated pattern matching switch. |
| **Dependencies** | Requires additional partial methods for each derived type in the same class. |
| **Parameters** | `Type sourceType`, `Type targetType` |
| **Use Cases** | Inheritance hierarchies (Vehicle→Car/Truck), CQRS commands, polymorphic serialization. |
| **Complexity** | Advanced |
| **Sample** | ✅ `Level4_Advanced/PolymorphicDemo.cs` |

### `UseConverterAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.UseConverterAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Delegates mapping to an `IConverter<TSource, TDestination>` type or an injected instance field. |
| **Constructors** | `UseConverterAttribute(Type converterType)`, `UseConverterAttribute(string converterFieldName)` |
| **Properties** | `Type ConverterType { get; }`, `string? ConverterFieldName { get; }` |
| **Use Cases** | Complex transformation logic, DI-injected converters, legacy integrations. |
| **Complexity** | Intermediate |
| **Sample** | ✅ `Level6_ErrorHandling/ErrorHandlingDemo.cs`, `Level8_Customization/CustomizationDemo.cs` |

### `MapNullFallbackAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.MapNullFallbackAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Embeds a literal C# expression as a fallback when the source property is null and the destination is non-nullable. |
| **Dependencies** | None. Resolves ELM004 without requiring an `IConverter`. |
| **Parameters** | `string destinationName`, `string fallbackExpression` |
| **Use Cases** | Nullable value types (`int?`, `decimal?`) mapping to non-nullable. Legacy data with empty fields. |
| **Complexity** | Intermediate |
| **Sample** | ✅ `Level2_Configuration/NullFallbackDemo.cs` |

### `GenerateMapperRegistrationAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.GenerateMapperRegistrationAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Triggers generation of `AddGeneratedMappers(this IServiceCollection)` for automatic DI registration. |
| **Dependencies** | `Microsoft.Extensions.DependencyInjection` in the consuming project. |
| **Usage** | Assembly attribute: `[assembly: GenerateMapperRegistration]` |
| **Use Cases** | Applications with multiple mappers following the DI pattern. |
| **Complexity** | Basic |
| **Sample** | ✅ `Level9_Extensions/DependencyInjectionDemo.cs` |

### `ValueObjectAttribute`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.ValueObjectAttribute` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Marks a type as a Value Object to enable automatic wrap/unwrap in mappings. |
| **Dependencies** | The type must have a single `Value` property of a primitive type, or be a `readonly record struct` with a single constructor parameter. |
| **Use Cases** | DDD: Strongly Typed IDs (CustomerId, OrderId), Money, Email, etc. |
| **Complexity** | Intermediate |
| **Sample** | ✅ `Level2_Configuration/ValueObjectDemo.cs` |

---

## 2. Enums (`EricksonLopez.Mapper.Abstractions`)

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

## 3. Public Interfaces (`EricksonLopez.Mapper.Abstractions`)

### `IConverter<TSource, TDestination>`

| Field | Description |
|---|---|
| **Name** | `EricksonLopez.Mapper.IConverter<TSource, TDestination>` |
| **Namespace** | `EricksonLopez.Mapper` |
| **Responsibility** | Contract for custom conversion logic implementations. |
| **Method** | `TDestination Convert(TSource source)` |
| **Dependencies** | None. Implemented by user or provided by extension packages. |
| **Use Cases** | Complex transformations, mappings with business logic, legacy integrations. |
| **Complexity** | Intermediate |
| **Sample** | ✅ `Level8_Customization/CustomizationDemo.cs`, `Level6_ErrorHandling/ErrorHandlingDemo.cs` |

---

## 4. Official Extension Packages

### Package: `EricksonLopez.Mapper.DomainPrimitives`

Provides pre-built, high-performance converters for Domain Primitives and Strongly Typed IDs:

| Type | Contract | Responsibility | Sample |
|---|---|---|---|
| `DomainPrimitiveToValueConverter<TPrimitive, TValue>` | `IConverter<TPrimitive, TValue>` | Extracts `.Value` from `IDomainPrimitive<TSelf, TValue>` | ✅ `Level9_Extensions/DomainPrimitivesDemo.cs` |
| `StrongIdToValueConverter<TStrongId, TValue>` | `IConverter<TStrongId, TValue>` | Extracts `.Value` from `IStrongId<TSelf, TValue>` | ✅ `Level9_Extensions/DomainPrimitivesDemo.cs` |
| `ValueToDomainPrimitiveConverter<TValue, TPrimitive>` | `IConverter<TValue, TPrimitive>` | Constructs `IDomainPrimitive` via `.Create(value)` — **requires .NET 7+** (uses `static abstract` interface members; available on all supported target frameworks: `net8.0`, `net9.0`, `net10.0`) | ✅ `Level9_Extensions/DomainPrimitivesDemo.cs` |

### Package: `EricksonLopez.Mapper.Result`

Provides functional Railway-Oriented Programming projection methods:

| Method | Signature | Responsibility | Sample |
|---|---|---|---|
| `ResultMappingExtensions.Map` | `Result<TDest> Map<TSource, TDest>(this Result<TSource>, Func<TSource, TDest>)` | Synchronous functional projection; propagates failure without mapping invocation | ✅ `Level9_Extensions/ResultIntegrationDemo.cs` |
| `ResultMappingExtensions.MapAsync` | `Task<Result<TDest>> MapAsync<TSource, TDest>(this Task<Result<TSource>>, Func<TSource, TDest>)` | Asynchronous Task functional projection | ✅ `Level9_Extensions/ResultIntegrationDemo.cs` |
| `ResultMappingExtensions.MapAsync` | `ValueTask<Result<TDest>> MapAsync<TSource, TDest>(this ValueTask<Result<TSource>>, Func<TSource, TDest>)` | Asynchronous ValueTask functional projection | ✅ `Level9_Extensions/ResultIntegrationDemo.cs` |
| `ResultMappingExtensions.MapList` | `Result<IReadOnlyList<TDest>> MapList<TSource, TDest>(this Result<IEnumerable<TSource>>, Func<TSource, TDest>)` | Collection projection to read-only list | ✅ `Level9_Extensions/ResultIntegrationDemo.cs` |

### Package: `EricksonLopez.Mapper.Mapster`

Provides bi-directional integration with Mapster:

| Type / Method | Responsibility | Sample |
|---|---|---|
| `MapsterConverter<TSource, TDestination>()` | Implements `IConverter<TSource, TDest>` using `TypeAdapterConfig.GlobalSettings` | ✅ `Level9_Extensions/MapsterBridgeDemo.cs` |
| `MapsterConverter<TSource, TDestination>(TypeAdapterConfig config)` | Implements `IConverter<TSource, TDest>` using an explicitly provided Mapster configuration (isolated, test-friendly) | ✅ `Level9_Extensions/MapsterBridgeDemo.cs` |
| `MapsterMapperExtensions.UseConverter` | Configures Mapster's `TypeAdapterConfig` with an `IConverter` implementation | ✅ `Level9_Extensions/MapsterBridgeDemo.cs` |

---

## 5. Generator-Emitted APIs

| Element | Description | Sample |
|---|---|---|
| `MapperServiceCollectionExtensions.AddGeneratedMappers()` | Extension method emitted by the generator when `[GenerateMapperRegistration]` is present on the assembly. Registers all non-static `[Mapper]` classes as Singletons. | ✅ `Level9_Extensions/DependencyInjectionDemo.cs` |

---

## 6. Diagnostic Codes (Compiler Messages & Analyzers)

| Code | Severity | Description | How Resolved |
|---|---|---|---|
| **ELM001** | ❌ Error | Destination property unmapped in strict mode | Add `[MapProperty]`, `[MapIgnore]`, or matching source property |
| **ELM002** | ❌ Error | No public constructor or factory for destination type | Add public constructor or `[MapFactory]` |
| **ELM003** | ❌ Error | Unsupported type conversion | Use matching types or `[UseConverter]` |
| **ELM004** | ❌ Error | Nullability mismatch | Use `[MapNullFallback]` or nullable destination |
| **ELM005** | ❌ Error | Ambiguous property match | Disambiguate with explicit `[MapProperty]` |
| **ELM006** | ❌ Error | Missing constructor mapping | Add accessible constructor |
| **ELM007** | ❌ Error | Ambiguous constructor | Use `[MapFactory]` or explicit constructor |
| **ELM008** | ❌ Error | Mapper uses reflection (Analyzer) | Remove reflection; use AOT-compatible code |
| **ELM009** | ❌ Error | Mapper uses dynamic (Analyzer) | Remove dynamic types |
| **ELM010** | ❌ Error | Circular reference detected | Break cycle in DTO hierarchy |
| **ELM011** | ⚠️ Warning | Abstract destination type with potentially uncovered derived types | Map concrete destination types |
| **ELM012** | ❌ Error | `[Mapper]` class not declared as `partial` (Analyzer) | Add `partial` modifier |
| **ELM013** | ❌ Error | Invalid converter type referenced in `[UseConverter]` | Ensure converter implements `IConverter<TSrc, TDst>` |
| **ELM014** | ❌ Error | Enum member has no destination equivalent in strict mode | Use `[MapEnumValue]` or non-strict enum mapping |
| **ELM015** | ⚠️ Warning | Narrowing numeric conversion potential data loss | Use `[UseConverter]` or matching numeric width |
| **ELM016** | ⚠️ Warning | String to enum parsing runtime exception risk | Use typed enums or validate string inputs |

---

## 7. Built-in Conversion Strategies (Zero User Configuration)

| From | To | Generated Expression |
|---|---|---|
| `enum` | `string` | `source.Prop.ToString()` |
| `string` | `enum` | `Enum.Parse<TEnum>(source.Prop)` |
| `Guid` | `string` | `source.Prop.ToString()` |
| `string` | `Guid` | `Guid.Parse(source.Prop)` |
| `DateTime` | `DateOnly` | `DateOnly.FromDateTime(source.Prop)` |
| `DateTime` | `DateTimeOffset` | `new DateTimeOffset(source.Prop)` |
| Numeric widening | Wider numeric | Implicit C# cast |
| Same-type primitive | Same | Direct assignment |
| ValueObject | Inner primitive | `source.Prop.Value` |
| Primitive | ValueObject | `new ValueObject(source.Prop)` |

---

## 8. Supported Collection Target Types

| Target | Strategy | Pre-Sizing |
|---|---|---|
| `T[]` | for loop + indexer | `.Length` of source |
| `List<T>` | `new List<T>(count)` + foreach | `.Count` if available |
| `IList<T>` | → emitted as `List<T>` | ✅ |
| `ICollection<T>` | → emitted as `List<T>` | ✅ |
| `IEnumerable<T>` | → emitted as `List<T>` | `TryGetNonEnumeratedCount` |
| `IReadOnlyList<T>` | → emitted as `List<T>` | ✅ |
| `IReadOnlyCollection<T>` | → emitted as `List<T>` | ✅ |
| `ImmutableArray<T>` | `ImmutableArray.CreateBuilder<T>(count).ToImmutable()` | ✅ |
| `HashSet<T>` | `new HashSet<T>(count)` + foreach `.Add()` | ✅ |
| `Dictionary<K,V>` | `new Dictionary<K,V>()` + foreach KVP | ✅ |
| `IDictionary<K,V>` | → emitted as `Dictionary<K,V>` | ✅ |
| `IReadOnlyDictionary<K,V>` | → emitted as `Dictionary<K,V>` | ✅ |

---

## 9. Architectural Gaps & Non-Existent APIs

| Scenario | Status | Recommendation |
|---|---|---|
| Mutation of existing objects (`Map(src, existingDest)`) | ❌ Intentionally not supported (ADR-D05) | Use explicit re-creation or `with` expressions for immutability and AOT safety. |
| Runtime reflection expression trees | ❌ Intentionally not supported (ADR-D01) | Use compile-time Source Generators. |
| Global runtime profiles | ❌ Not supported | Use compile-time `[assembly: MapperDefaults]` or DI registration. |
