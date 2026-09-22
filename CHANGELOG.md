# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0] - 2026-09-22

### 💥 Breaking Changes

- **BC-001: Native AOT & Trimming Revocation on `EricksonLopez.Mapper.Mapster`**
  - **Component**: `EricksonLopez.Mapper.Mapster` (`EricksonLopez.Mapper.Mapster.csproj`, `MapsterConverter.cs`, `MapsterMapperExtensions.cs`)
  - **Previous State**: The package inherited `<IsAotCompatible>true</IsAotCompatible>` and `<IsTrimmable>true</IsTrimmable>` from `Directory.Build.props`. `MapsterConverter` constructors and `MapsterMapperExtensions.UseConverter` had no trimming or AOT warning attributes.
  - **Current State**: Package declares `<IsAotCompatible>false</IsAotCompatible>` and `<IsTrimmable>false</IsTrimmable>`. Decorated public constructors and `UseConverter` extension with `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`.
  - **Impact**: Consuming projects compiling with `<PublishAot>true</PublishAot>` or `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` will fail compilation with analyzer errors `IL2026` and `IL3050`. Projects requiring end-to-end Native AOT compliance cannot consume this package.
  - **Migration**: For Native AOT scenarios, replace `MapsterConverter` with source-generated mappers using `[Mapper]` and compile-time generated `IConverter<TSource, TDestination>`. If Mapster dynamic mapping is required in non-AOT projects with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`, suppress warnings `IL2026` and `IL3050` at the registration call site.

- **BC-002: Built-in `DateOnly → DateTimeOffset` Mapping Forces UTC `DateTimeKind.Utc`**
  - **Component**: `EricksonLopez.Mapper.Generator` (`ConversionStrategyFactory.cs`)
  - **Previous State**: The built-in conversion emitted `new DateTimeOffset(source.Date.ToDateTime(TimeOnly.MinValue))`. Because `DateTimeKind` was `Unspecified`, `DateTimeOffset` resolved the offset using the host machine's local timezone offset (e.g. `-04:00`, `+02:00`).
  - **Current State**: Emits `new DateTimeOffset(source.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))`. The mapped `DateTimeOffset` is now guaranteed to have offset `+00:00` (UTC).
  - **Impact**: Runtime behavioral breaking change. Applications running in non-UTC environments will observe different offset values and instant representations. Database persistence, JSON serialization (ISO 8601 UTC representation), and equality checks asserting local offsets will produce different results.
  - **Migration**: If local timezone representation was expected, implement a custom converter via `IConverter<DateOnly, DateTimeOffset>` or use `[MapValue]` to explicitly specify timezone conversion using `TimeZoneInfo`.

- **BC-003: `ELM017` Compile-Time Error for Unresolved `[MapFactory]` Methods**
  - **Component**: `EricksonLopez.Mapper.Generator` (`Diagnostics.cs`, `MapperGenerator.cs`)
  - **Previous State**: If `[MapFactory("MethodName")]` referenced a method name that did not exist on the destination type or lacked matching accessibility/parameters, the generator silently ignored the attribute and fell back to public constructors.
  - **Current State**: The generator emits compile-time error `ELM017` (`MapFactoryMethodNotFound`) and halts mapping code generation for that method.
  - **Impact**: Compile-time breaking change. Codebases that had invalid, mistyped, or non-static `[MapFactory]` attribute arguments that previously compiled via fallback constructors will now fail compilation.
  - **Migration**: Ensure the factory method specified in `[MapFactory]` is a `public static` method on the destination type returning the destination type. Use `nameof(DestinationType.FactoryMethod)` to ensure compile-time symbol accuracy. If constructor instantiation was intended, remove the `[MapFactory]` attribute.

- **BC-004: `ELM012` Compiler Error Extended to Non-Partial `[Mapper]` Interfaces**
  - **Component**: `EricksonLopez.Mapper.Analyzers` (`MapperAnalyzer.cs`)
  - **Previous State**: `MapperAnalyzer` only inspected `ClassDeclarationSyntax` for the `partial` modifier. Interfaces decorated with `[Mapper]` without `partial` were ignored by the analyzer.
  - **Current State**: `MapperAnalyzer` now inspects `InterfaceDeclarationSyntax` and reports compile-time error `ELM012` (`MustBePartial`) if the interface lacks the `partial` modifier.
  - **Impact**: Compile-time breaking change. Any project declaring `[Mapper] public interface IMyMapper` without the `partial` keyword will fail compilation with error `ELM012`.
  - **Migration**: Add the `partial` modifier to all interface declarations decorated with `[Mapper]`, e.g., `[Mapper] public partial interface IMyMapper`.

- **BC-005: Source Generator HintName File Naming Includes Namespace (`GEN-003`)**
  - **Component**: `EricksonLopez.Mapper.Generator` (`MapperGenerator.cs`)
  - **Previous State**: The generator registered source outputs using simple class names: `ClassName.g.cs`.
  - **Current State**: The generator prefixes the hint name with the sanitized namespace: `{Namespace}_{ClassName}.g.cs` (or `{ClassName}.g.cs` only when declared in the global namespace).
  - **Impact**: Integration and configuration breaking change. Projects using `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` with build targets, CI analyzers, snapshot verification, or gitignore rules targeting specific `ClassName.g.cs` paths will break because the output file path on disk has changed.
  - **Migration**: Update MSBuild scripts, CI pipelines, snapshot test paths, and custom build rules to reference `{Namespace}_{ClassName}.g.cs` instead of `{ClassName}.g.cs`.

- **BC-006: `CycleDetector` Cycle Enforcement Extended to Polymorphic `[MapDerivedType]` Mappings (`ELM010`)**
  - **Component**: `EricksonLopez.Mapper.Generator` (`CycleDetector.cs`)
  - **Previous State**: `CycleDetector` only inspected constructor parameters and property member strategies. Polymorphic dispatch methods defined in `[MapDerivedType]` were not checked in the dependency graph.
  - **Current State**: `CycleDetector` now builds dependency edges for all polymorphic target methods referenced in `[MapDerivedType]`. If a circular dependency chain exists across derived type mappings, `ELM010` (`CircularMappingDependency`) is emitted as a compile error.
  - **Impact**: Compile-time breaking change. Any polymorphic mapping containing circular graph dependencies that previously compiled will now fail compilation with error `ELM010`.
  - **Migration**: Refactor cyclic polymorphic hierarchies into unidirectional mapping flows or resolve cycle nodes using custom `IConverter<TSource, TDestination>` implementations.

- **BC-007: `ELM018` Warning for Duplicate `[MapProperty]` Destination Targets**
  - **Component**: `EricksonLopez.Mapper.Generator` (`Diagnostics.cs`, `MapperGenerator.cs`)
  - **Previous State**: Multiple `[MapProperty]` attributes targeting the same destination property were silently overwritten, with the last attribute taking precedence without notice.
  - **Current State**: The generator emits compile-time warning `ELM018` (`DuplicateMapPropertyDestination`) when a destination member is targeted multiple times.
  - **Impact**: Compile-time breaking in projects configured with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`. The duplicate attribute was previously shadowed silently; now it breaks builds under strict warning policies.
  - **Migration**: Remove redundant or shadowed `[MapProperty]` attributes targeting the same destination property so that only a single unambiguous mapping declaration remains.

- **BC-008: Nullable Target Collection Initialization Returns `null` Instead of Empty Instance**
  - **Component**: `EricksonLopez.Mapper.Generator` (`CodeEmitter.cs`, `Models/ConversionStrategy.cs`)
  - **Previous State**: For array (`T[]?`) and immutable collection (`ImmutableArray<T>?`, `ImmutableList<T>?`) destination properties declared as nullable, generated mapping code unconditionally initialized the target variable to `Array.Empty<T>()` or `ImmutableArray.Empty`.
  - **Current State**: When the target collection property is nullable (`IsTargetNullable = true`), generated code initializes the variable to `null`. Similarly, value object mapping wraps/unwraps with ternary null checks when both source and destination are nullable.
  - **Impact**: Runtime behavioral breaking change. Consumers who relied on nullable collection properties defaulting to non-null empty collections will now observe `null` values when source collections are null/unmapped, potentially causing `NullReferenceException` in un-guarded consumer code.
  - **Migration**: Ensure consumer code accessing nullable collections performs appropriate null-coalescing or null-checking (`mapped.Items ?? []`), or change destination property declarations to non-nullable collections (`T[]` / `List<T>`).

### ✨ Added
- `ELM017` (Error): Factory method specified in `[MapFactory]` was not found on destination type.
- `ELM018` (Warning): Duplicate `[MapProperty]` targeting the same destination member.
- Support for unsigned and widening numeric conversions: `uint → ulong`, `byte → ushort/uint/ulong`, `ushort → uint/ulong`.
- Support for `enum → decimal`, `enum → double`, and `enum → float` conversions.
- Support for mapper types declared in the global (empty) namespace without emitting invalid C# syntax.

### 🐛 Bug Fixes
- **di:** escape C# keywords and support global namespace mappers in `DependencyInjectionEmitter` (`services.AddSingleton(...)`).
- **generator:** prevent hint name collision in source generator by qualifying emitted file names with namespace (`GEN-003`).
- **analyzers:** enforce `partial` modifier on `[Mapper]` interface declarations (`ELM012`).
- **generator:** prevent silent constructor fallback when `[MapFactory]` method does not exist (`ELM017`).
- **generator:** prevent silent overwrite of duplicate `[MapProperty]` attributes by warning developer (`ELM018`).

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
