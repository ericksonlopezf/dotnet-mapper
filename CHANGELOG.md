# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.1](https://github.com/ericksonlopezf/dotnet-mapper/compare/v1.0.0...v1.0.1) (2026-08-26)


### 🐛 Bug Fixes

* **ci:** correct AOT smoke test paths, snapshot slashes, and gate triggers ([fb4f7d5](https://github.com/ericksonlopezf/dotnet-mapper/commit/fb4f7d59e1d3749469e2d8bd64093aa97368d452))

## [Unreleased]

---

## [1.0.0] - 2026-08-26

### Added

#### Core Library & Attributes (`EricksonLopez.Mapper.Abstractions`)
- `[Mapper]` — attribute enabling compile-time source generation for `partial` classes and interfaces (`StrictMapping = true` by default).
- `[MapProperty(string sourceName, string destinationName)]` — explicit property name remapping and deep nested path navigation (e.g. `"Customer.Address.City"`).
- `[MapIgnore(string destinationName)]` — excludes a destination property from mapping, resolving `ELM001`.
- `[MapIgnoreSource(string sourceName)]` — excludes a source property from participating in member resolution.
- `[MapperIgnore]` — model member attribute to exclude properties/fields across all mappers.
- `[MapValue(string destinationName, string valueExpression)]` — injects constant or computed C# expressions into destination targets.
- `[EnumMappingStrategy(EnumMappingStrategy strategy)]` — configures `ByName` (default) or `ByValue` enum matching with optional `IgnoreCase = true`.
- `[MapEnumValue(object source, object target)]` — explicit member translation between heterogeneous or asymmetric enums.
- `[MapperDefaults]` — assembly-level attribute defining project-wide defaults for strict mapping and enum strategies.
- `[MapFactory(string methodName)]` — delegates instantiation to a static factory method to preserve domain invariants.
- `[MapDerivedType(Type sourceType, Type targetType)]` — polymorphic dispatch via compile-time pattern matching switch expressions with deterministic ordering.
- `[UseConverter(Type converterType)]` and `[UseConverter(string converterFieldName)]` — custom conversion via `IConverter<TSource, TDestination>` instances or injected fields.
- `[MapNullFallback(string destinationName, string fallbackExpression)]` — inline null fallback expressions for nullable source values.
- `[GenerateMapperRegistration]` — triggers compile-time emission of `services.AddGeneratedMappers()` for automatic DI registration.
- `[ValueObject]` — marks types as DDD Value Objects for automatic wrap/unwrap generation.
- `IConverter<TSource, TDestination>` — foundational interface for custom mapping logic.

#### Generator Engine (`EricksonLopez.Mapper.Generator`)
- Roslyn `IIncrementalGenerator` implementation partitioned into 7 modular components:
  - `MapperGenerator` — slim orchestrator pipeline using `ForAttributeWithMetadataName`.
  - `Models` — immutable value-equatable models and `EquatableArray<T>` guaranteeing Roslyn caching efficiency.
  - `MemberResolutionEngine` — property discovery, attribute filtering, case-insensitive matching, and nested path navigation.
  - `ConversionStrategyFactory` — resolution of scalar, enum, temporal, collection, dictionary, converter, and nested object strategies.
  - `CycleDetector` — directed graph cycle detection across mapping chains (`ELM010`).
  - `CodeEmitter` — deterministic C# code generation with null ternary safety and pre-sized builders.
  - `DependencyInjectionEmitter` — emits `AddGeneratedMappers()` DI extension methods.
- Built-in conversions:
  - `enum ↔ string` and `enum ↔ enum` (by name or value with case-insensitivity support).
  - `Guid ↔ string`.
  - `DateTime ↔ DateOnly`, `DateTime ↔ DateTimeOffset`, `DateTimeOffset ↔ DateOnly`.
  - Numeric widening (automatic) and narrowing (explicit cast with `ELM015` warning).
- Supported collection targets: `T[]`, `List<T>`, `IList<T>`, `ICollection<T>`, `IEnumerable<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `ImmutableArray<T>`, `ImmutableList<T>`, `HashSet<T>`, `FrozenSet<T>`, `Dictionary<K,V>`, `FrozenDictionary<K,V>`.

#### Roslyn Analyzers & Code Fix Providers (`EricksonLopez.Mapper.Analyzers`)
- `MapperAnalyzer` enforcing zero-reflection and NativeAOT invariants:
  - `ELM008`: prohibits `System.Reflection`, `System.Activator`, `Marshal`, `RuntimeHelpers`, and `FormatterServices` inside mapper classes.
  - `ELM009`: prohibits `dynamic` keyword inside mapper classes.
  - `ELM012`: requires classes annotated with `[Mapper]` to be declared `partial`.
- Code Fix Providers (6 total):
  - `MakePartialCodeFixProvider`: automated quick fix for `ELM012` (adds `partial` keyword).
  - `MapIgnoreCodeFixProvider`: automated quick fix for `ELM001` (adds `[MapIgnore("Member")]`).
  - `MapPropertyCodeFixProvider`: automated quick fix for `ELM001` (adds `[MapProperty("Source", "Destination")]`).
  - `MapNullFallbackCodeFixProvider`: automated quick fix for `ELM004` (adds `[MapNullFallback("Member", "default!")]`).
  - `MapFactoryCodeFixProvider`: automated quick fix for `ELM007` (adds `[MapFactory("Create")]`).
  - `AddPartialMappingMethodCodeFixProvider`: automated quick fix for `ELM003` (adds a typed `partial` mapping method stub).

#### Ecosystem Extensions
- `EricksonLopez.Mapper.DomainPrimitives`: pre-built converters for `IDomainPrimitive<TSelf, TValue>` and `IStrongId<TSelf, TValue>`.
- `EricksonLopez.Mapper.Mapster`: bi-directional bridge adapter between `IConverter` and Mapster `TypeAdapterConfig`.
- `EricksonLopez.Mapper.Result`: functional Railway-Oriented Programming projection extensions (`Map`, `MapAsync`, `MapList`).

#### Diagnostics Catalog
- `ELM001` (Error): Unmapped destination member (StrictMapping violation).
- `ELM002` (Error): Destination type has no accessible constructors or factory methods.
- `ELM003` (Error): Unsupported type conversion between source and destination members.
- `ELM004` (Error): Nullability mismatch (nullable source assigned to non-nullable destination).
- `ELM005` (Error): Ambiguous member match (case-insensitive collision).
- `ELM006` (Error): Missing constructor for mapping.
- `ELM007` (Error): Ambiguous constructor (requires `[MapFactory]`).
- `ELM008` (Error): Forbidden reflection or runtime introspection API used in mapper class.
- `ELM009` (Error): Forbidden `dynamic` keyword used in mapper class.
- `ELM010` (Error): Circular mapping dependency detected.
- `ELM011` (Warning): Incomplete polymorphism warning for abstract base destination types.
- `ELM012` (Error): Mapper class missing `partial` modifier.
- `ELM013` (Error): Invalid converter type referenced in `[UseConverter]`.
- `ELM014` (Error): Unmapped enum member in strict mode.
- `ELM015` (Warning): Narrowing numeric conversion warning (potential precision/overflow loss).
- `ELM016` (Warning): String to enum conversion warning (unconstrained runtime parsing risk).

### Technical & Quality Infrastructure
- Multi-targeting: `net8.0`, `net9.0`, `net10.0` for runtime libraries; `netstandard2.0` for Roslyn analyzers and source generators.
- 100% Native AOT compatibility (`PublishAot=true`) validated via `aot-smoke-test.yml` CI gate.
- Zero runtime reflection, zero IL emit, zero allocation overhead beyond destination object creation.
- 9 GitHub Actions CI/CD workflows covering continuous integration, NativeAOT smoke testing, multi-project Stryker mutation matrix, benchmark regression checks, and Sigstore/OIDC package publishing.

---

## Migration Guide

See [docs/migration-guide.md](docs/migration-guide.md) for upgrade and usage instructions.

## API Reference

See [docs/api-reference.md](docs/api-reference.md) for the complete public API documentation.
