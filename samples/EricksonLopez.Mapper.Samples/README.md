# EricksonLopez.Mapper — Showcase & Documentation

Welcome to the **Showcase** project of `EricksonLopez.Mapper`.

This project serves simultaneously as:
- ✅ **Official documentation** — every public API has a working, compilable reference example here.
- ✅ **Interactive learning guide** — progressive from beginner (Level 0) to enterprise architect (Level 10).
- ✅ **Integration sample** — demonstrates real-world patterns, Domain Primitives, Result types, and Mapster interop.
- ✅ **Cookbook** — see the `Cookbook/` directory for targeted recipes.
- ✅ **API validation suite** — if it doesn't compile here, the API is broken.

Being a pure **Roslyn Incremental Source Generator**, the library generates Native AOT-compatible, zero-reflection, zero-overhead mapping code at compile time. All mapping errors are caught at build time, not at runtime.

---

## Learning Path Structure

The Showcase is organized into 11 progressive levels. Each level covers a specific dimension of the library.

| Level | Directory | Topic | APIs Demonstrated |
|---|---|---|---|
| **0** | `Level0_Conceptual/` | What is the library? Why use it? | Architectural overview, AOT rationale |
| **1** | `Level1_QuickStart/` | Installation, minimal setup, first mapping | `[Mapper]` (convention-based) |
| **2** | `Level2_Configuration/` | All configuration attributes | `[MapProperty]`, `[MapIgnore]`, `[MapIgnoreSource]`, `[MapperIgnore]`, `[MapNullFallback]`, `[ValueObject]`, `[MapValue]`, `[EnumMappingStrategy]` (ByName+IgnoreCase **and** ByValue), `[MapEnumValue]`, `[assembly: MapperDefaults]`, `static partial class` mapper |
| **3** | `Level3_RealWorld/` | Real-world scenarios | Nested mapping, collections, built-in type conversions (`enum↔string`, `Guid↔string`, `DateTime→DateOnly/Offset`, widening), all collection targets (`T[]`, `ImmutableArray<T>`, `HashSet<T>`, `Dictionary<K,V>`) |
| **4** | `Level4_Advanced/` | Advanced integration patterns | Records with primary constructors, `[MapFactory]`, `[MapDerivedType]` |
| **5** | `Level5_Processing/` | Parallel & batch processing | Thread-safety, statelessness, PLINQ concurrent mapping |
| **6** | `Level6_ErrorHandling/` | Runtime error boundaries | `IConverter<T,T>` exception handling, retry/dead-letter boundary |
| **7** | `Level7_Scalability/` | Throughput & performance | High-throughput in-memory benchmark (30M+ ops/sec) |
| **8** | `Level8_Customization/` | Custom converter implementations | `IConverter<T,T>`, `[UseConverter(Type)]`, `[UseConverter(FieldName)]` (DI field injection) |
| **9** | `Level9_Extensions/` | Official ecosystem extensions & DI | `[GenerateMapperRegistration]`, `AddGeneratedMappers()`, `EricksonLopez.Mapper.DomainPrimitives`, `EricksonLopez.Mapper.Result` (all 4 overloads incl. `ValueTask`), `EricksonLopez.Mapper.Mapster` (both constructors) |
| **10** | `Level10_Architecture/` | Clean Architecture / DDD | `[MapIgnoreSource]` for domain boundaries, DTO projection |

---

## Files in This Showcase

### Level 0 — Conceptual
- [`ConceptualOverview.cs`](Level0_Conceptual/ConceptualOverview.cs) — What is EricksonLopez.Mapper and why use it.

### Level 1 — Quick Start
- [`QuickStartDemo.cs`](Level1_QuickStart/QuickStartDemo.cs) — `[Mapper]` with automatic by-convention mapping.

### Level 2 — Configuration
- [`ConfigurationDemo.cs`](Level2_Configuration/ConfigurationDemo.cs) — `[MapProperty]`, `[MapIgnore]`, `[MapIgnoreSource]`, `[MapperIgnore]`, `StrictMapping = false`.
- [`NullFallbackDemo.cs`](Level2_Configuration/NullFallbackDemo.cs) — `[MapNullFallback]` for nullable value types (`int?`, `decimal?`).
- [`ValueObjectDemo.cs`](Level2_Configuration/ValueObjectDemo.cs) — `[ValueObject]` for DDD Strongly Typed IDs.
- [`ValueAssignmentDemo.cs`](Level2_Configuration/ValueAssignmentDemo.cs) — `[MapValue]` for injecting constant or computed expressions (`System.DateTime.UtcNow`, `"ACTIVE"`).
- [`EnumMappingDemo.cs`](Level2_Configuration/EnumMappingDemo.cs) — `[EnumMappingStrategy]` (`ByName+IgnoreCase` **and** `ByValue`), `[MapEnumValue]`.
- [`AssemblyConfig.cs`](Level2_Configuration/AssemblyConfig.cs) — Assembly-level `[assembly: MapperDefaults]` configuration declaration.
- [`MapperDefaultsDemo.cs`](Level2_Configuration/MapperDefaultsDemo.cs) — `[assembly: MapperDefaults]` effect demonstration: enum case-insensitivity inherited by all mappers without per-class/method attributes.
- [`StaticMapperDemo.cs`](Level2_Configuration/StaticMapperDemo.cs) — `static partial class` mapper pattern: no instance, direct LINQ method group usage.

### Level 3 — Real World
- [`RealWorldDemo.cs`](Level3_RealWorld/RealWorldDemo.cs) — Nested object mapping.
- [`CollectionsDemo.cs`](Level3_RealWorld/CollectionsDemo.cs) — `Dictionary<K,V>` mapping with type conversion.
- [`BuiltinConversionsDemo.cs`](Level3_RealWorld/BuiltinConversionsDemo.cs) — `enum↔string`, `Guid↔string`, `DateTime→DateOnly/Offset`, numeric widening.
- [`CollectionTypesDemo.cs`](Level3_RealWorld/CollectionTypesDemo.cs) — `T[]`, `ImmutableArray<T>`, `HashSet<T>`, `IReadOnlyList<T>`, `IEnumerable<T>`.

### Level 4 — Advanced Integration
- [`AdvancedDemo.cs`](Level4_Advanced/AdvancedDemo.cs) — C# Records with parameterized constructors.
- [`FactoryMethodDemo.cs`](Level4_Advanced/FactoryMethodDemo.cs) — `[MapFactory]` for factory-method-based construction.
- [`PolymorphicDemo.cs`](Level4_Advanced/PolymorphicDemo.cs) — `[MapDerivedType]` for polymorphic dispatch.

### Level 5 — Processing
- [`ProcessingDemo.cs`](Level5_Processing/ProcessingDemo.cs) — Thread-safe PLINQ batch processing.

### Level 6 — Error Handling
- [`ErrorHandlingDemo.cs`](Level6_ErrorHandling/ErrorHandlingDemo.cs) — Runtime exception handling with custom `IConverter<T,T>`.

### Level 7 — Scalability
- [`ScalabilityDemo.cs`](Level7_Scalability/ScalabilityDemo.cs) — 1M iterations throughput benchmark.

### Level 8 — Customization
- [`CustomizationDemo.cs`](Level8_Customization/CustomizationDemo.cs) — `IConverter<TSource, TDestination>` with both `[UseConverter(Type)]` and `[UseConverter(FieldName)]` (DI injection).

### Level 9 — Extensions
- [`ExtensionsDemo.cs`](Level9_Extensions/ExtensionsDemo.cs) — Manual DI container registration.
- [`DependencyInjectionDemo.cs`](Level9_Extensions/DependencyInjectionDemo.cs) — `[GenerateMapperRegistration]` + `AddGeneratedMappers()`.
- [`DomainPrimitivesDemo.cs`](Level9_Extensions/DomainPrimitivesDemo.cs) — `DomainPrimitiveToValueConverter`, `StrongIdToValueConverter`, `ValueToDomainPrimitiveConverter`.
- [`ResultIntegrationDemo.cs`](Level9_Extensions/ResultIntegrationDemo.cs) — All 4 `ResultMappingExtensions` overloads: `Map`, `MapAsync(Task)`, `MapAsync(ValueTask)`, `MapList`.
- [`MapsterBridgeDemo.cs`](Level9_Extensions/MapsterBridgeDemo.cs) — `MapsterConverter<TSource, TDestination>` (both constructors: default and explicit `TypeAdapterConfig`) and `TypeAdapterConfig.UseConverter`.

### Level 10 — Architecture
- [`ArchitectureDemo.cs`](Level10_Architecture/ArchitectureDemo.cs) — Clean Architecture layer separation and `[MapIgnoreSource]`.

### Cookbook
- [`Cookbook/README.md`](Cookbook/README.md) — 27 targeted recipes for common and advanced mapping scenarios.

---

## Running the Showcase

```bash
dotnet run --project sample/EricksonLopez.Mapper.Sample/EricksonLopez.Mapper.Sample.csproj
```

All demos run sequentially and print their structured output to the console.

---

## Public API Coverage

Every element of the public API surface is demonstrated in this Showcase. The table below cross-references each API element to its demo file.

| API Element | Package | Level | Demo File |
|---|---|---|---|
| `[Mapper]` | Abstractions | 1 | `QuickStartDemo.cs` |
| `[Mapper(StrictMapping = false)]` | Abstractions | 2 | `ConfigurationDemo.cs` |
| `[MapProperty]` | Abstractions | 2 | `ConfigurationDemo.cs`, `BuiltinConversionsDemo.cs` |
| `[MapIgnore]` | Abstractions | 2 | `ConfigurationDemo.cs` |
| `[MapIgnoreSource]` | Abstractions | 2,10 | `ConfigurationDemo.cs`, `ArchitectureDemo.cs` |
| `[MapperIgnore]` | Abstractions | 2 | `ConfigurationDemo.cs` |
| `[MapNullFallback]` | Abstractions | 2 | `NullFallbackDemo.cs` |
| `[ValueObject]` | Abstractions | 2 | `ValueObjectDemo.cs` |
| `[MapValue]` | Abstractions | 2 | `ValueAssignmentDemo.cs` |
| `[EnumMappingStrategy] ByName+IgnoreCase` | Abstractions | 2 | `EnumMappingDemo.cs` |
| `[EnumMappingStrategy] ByValue` | Abstractions | 2 | `EnumMappingDemo.cs` |
| `[MapEnumValue]` | Abstractions | 2 | `EnumMappingDemo.cs` |
| `[assembly: MapperDefaults]` | Abstractions | 2 | `AssemblyConfig.cs` + `MapperDefaultsDemo.cs` |
| `static partial class` mapper | Abstractions | 2 | `StaticMapperDemo.cs` |
| `[MapFactory]` | Abstractions | 4 | `FactoryMethodDemo.cs` |
| `[MapDerivedType]` | Abstractions | 4 | `PolymorphicDemo.cs` |
| `IConverter<TSource, TDest>` | Abstractions | 6,8 | `ErrorHandlingDemo.cs`, `CustomizationDemo.cs` |
| `[UseConverter(Type)]` | Abstractions | 8 | `CustomizationDemo.cs` |
| `[UseConverter(FieldName)]` | Abstractions | 8 | `CustomizationDemo.cs` |
| `[assembly: GenerateMapperRegistration]` | Abstractions | 9 | `DependencyInjectionDemo.cs` |
| `AddGeneratedMappers()` | Generator | 9 | `DependencyInjectionDemo.cs` |
| `Result<T>.Map()` | Mapper.Result | 9 | `ResultIntegrationDemo.cs` |
| `Task<Result<T>>.MapAsync()` | Mapper.Result | 9 | `ResultIntegrationDemo.cs` |
| `ValueTask<Result<T>>.MapAsync()` | Mapper.Result | 9 | `ResultIntegrationDemo.cs` |
| `Result<IEnumerable<T>>.MapList()` | Mapper.Result | 9 | `ResultIntegrationDemo.cs` |
| `DomainPrimitiveToValueConverter<T,V>` | Mapper.DomainPrimitives | 9 | `DomainPrimitivesDemo.cs` |
| `StrongIdToValueConverter<T,V>` | Mapper.DomainPrimitives | 9 | `DomainPrimitivesDemo.cs` |
| `ValueToDomainPrimitiveConverter<V,T>` | Mapper.DomainPrimitives | 9 | `DomainPrimitivesDemo.cs` |
| `MapsterConverter<T,D>()` | Mapper.Mapster | 9 | `MapsterBridgeDemo.cs` |
| `MapsterConverter<T,D>(TypeAdapterConfig)` | Mapper.Mapster | 9 | `MapsterBridgeDemo.cs` |
| `TypeAdapterConfig.UseConverter()` | Mapper.Mapster | 9 | `MapsterBridgeDemo.cs` |

---

## Documentation Links

| Document | Location |
|---|---|
| API Reference | [`docs/api-reference.md`](../../docs/api-reference.md) |
| API Inventory | [`docs/api-inventory.md`](../../docs/api-inventory.md) |
| Cookbook | [`docs/cookbook.md`](../../docs/cookbook.md) |
| Architecture Diagrams | [`docs/diagrams.md`](../../docs/diagrams.md) |
| Performance Guide | [`docs/performance-guide.md`](../../docs/performance-guide.md) |
| Migration Guide | [`docs/migration-guide.md`](../../docs/migration-guide.md) |
| Troubleshooting | [`docs/troubleshooting.md`](../../docs/troubleshooting.md) |

---

> **Important:** Any public API not demonstrated in this Showcase is considered undocumented.
> This project is kept synchronized with the library's API surface on every change.
> The Source Generator's compile-time guarantees mean: **if this Showcase compiles, the API works.**
