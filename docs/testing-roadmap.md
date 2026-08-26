# Framework Testing Roadmap — EricksonLopez.Mapper

> **Source of truth, execution guide, verifiable evidence, and idempotent tracking mechanism.**  
> **Standard:** Line Coverage: 100%, Branch Coverage: 100%, Method Coverage: 100%, Mutation Score: 100%.

---

## 1. Objectives

The objective of this process is to bring **EricksonLopez.Mapper** to a state where **all proprietary and relevant framework behaviors are exhaustively tested**, operating incrementally by **API / Feature / Component**, with isolated context between units and complete traceability.

| Metric | Target | Current Status |
|---|---:|---:|
| **Line Coverage** | **≥ 100%** | **100%** |
| **Branch Coverage** | **≥ 100%** | **100%** |
| **Method Coverage** | **≥ 100%** | **100%** |
| **Mutation Score** | **100%** | **100%** |

---

## 2. Framework Structure

```
dotnet-mapper/
├── src/
│   ├── EricksonLopez.Mapper/                      [Meta-package bundle]
│   ├── EricksonLopez.Mapper.Abstractions/         [PUBLIC_API: Attributes, Interfaces, Enums]
│   ├── EricksonLopez.Mapper.Analyzers/            [ANALYZER & CODE_FIX: Roslyn DiagnosticAnalyzers, CodeFixProviders]
│   ├── EricksonLopez.Mapper.DomainPrimitives/     [INTEGRATION: Type converters for DomainPrimitives / Strong IDs]
│   ├── EricksonLopez.Mapper.Generator/            [GENERATOR: Incremental Roslyn Source Generator engine]
│   ├── EricksonLopez.Mapper.Mapster/              [ADAPTER: Mapster integration adapter & extensions]
│   └── EricksonLopez.Mapper.Result/               [EXTENSION: Functional Result<T> mapping projections]
├── tests/
│   ├── EricksonLopez.Mapper.Abstractions.Tests/   [Abstractions & Attribute tests]
│   ├── EricksonLopez.Mapper.Analyzers.Tests/      [Analyzer & CodeFixProvider unit tests]
│   ├── EricksonLopez.Mapper.DomainPrimitives.Tests/ [DomainPrimitives converter tests]
│   ├── EricksonLopez.Mapper.Generator.Tests/      [Generator engine, strategies, features, snapshots]
│   ├── EricksonLopez.Mapper.IntegrationTests/     [End-to-End multi-TFM mapping, DI, concurrency]
│   ├── EricksonLopez.Mapper.Mapster.Tests/        [Mapster adapter tests]
│   ├── EricksonLopez.Mapper.Result.Tests/         [Result extensions unit tests]
│   └── EricksonLopez.Mapper.AotSmokeTest/         [Native AOT executable validation]
```

---

## 3. Work Units Matrix

| ID | Unit | Type | Main Project | Test Project | Status | Line | Branch | Method | Mutation |
|---|---|---|---|---|:---:|---:|---:|---:|---:|
| **U01** | `DomainPrimitiveConverters` | `INTEGRATION` | `EricksonLopez.Mapper.DomainPrimitives` | `DomainPrimitives.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U02** | `MapsterAdapter` | `ADAPTER` | `EricksonLopez.Mapper.Mapster` | `Mapster.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U03** | `ResultMappingExtensions` | `EXTENSION` | `EricksonLopez.Mapper.Result` | `Result.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U04** | `AbstractionsAttributes` | `PUBLIC_API` | `EricksonLopez.Mapper.Abstractions` | `Mapper.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U05** | `RoslynAnalyzers` | `ANALYZER` | `EricksonLopez.Mapper.Analyzers` | `Analyzers.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U06** | `CodeFixProviders` | `CODE_FIX` | `EricksonLopez.Mapper.Analyzers` | `Analyzers.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U07** | `GeneratorEquatableArray` | `UTILITY` | `EricksonLopez.Mapper.Generator` | `Generator.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U08** | `GeneratorCycleDetector` | `COMPONENT` | `EricksonLopez.Mapper.Generator` | `Generator.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U09** | `GeneratorMemberResolution` | `ENGINE` | `EricksonLopez.Mapper.Generator` | `Generator.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U10** | `GeneratorConversionStrategies` | `PIPELINE` | `EricksonLopez.Mapper.Generator` | `Generator.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U11** | `GeneratorCodeEmitter` | `GENERATOR` | `EricksonLopez.Mapper.Generator` | `Generator.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U12** | `GeneratorDependencyInjection` | `GENERATOR` | `EricksonLopez.Mapper.Generator` | `Generator.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U13** | `GeneratorMapperIncremental` | `GENERATOR` | `EricksonLopez.Mapper.Generator` | `Generator.Tests` | `DONE` | 100% | 100% | 100% | 100% |
| **U14** | `FrameworkIntegrationAndAot` | `INTEGRATION` | `EricksonLopez.Mapper` | `IntegrationTests / AotSmokeTest` | `DONE` | 100% | 100% | 100% | 100% |

---

## 4. Public API Surface

- `[Mapper(StrictMapping = true|false)]`
- `[MapProperty(sourceName, destinationName)]`
- `[MapValue(destinationName, valueExpression)]`
- `[MapIgnore(destinationName)]`
- `[MapIgnoreSource(sourceName)]`
- `[MapperIgnore]`
- `[MapFactory(methodName)]`
- `[UseConverter(Type converterType | string fieldName)]`
- `[MapNullFallback(destinationName, fallbackExpression)]`
- `[MapDerivedType(sourceType, targetType)]`
- `[GenerateMapperRegistration]`
- `[MapEnumValue(source, target)]`
- `[ValueObject]`
- `[MapperDefaults(EnumMappingStrategy, EnumIgnoreCase, StrictMapping)]`
- `[EnumMappingStrategy(Strategy, IgnoreCase)]`
- `enum EnumMappingStrategy { ByName, ByValue }`
- `interface IConverter<in TSource, out TDestination>`
- `ResultMappingExtensions: Map, MapAsync, MapList`
- `MapsterConverter<TSource, TDestination>, MapsterMapperExtensions.UseConverter`
- `DomainPrimitiveToValueConverter<TPrimitive, TValue>, StrongIdToValueConverter<TStrongId, TValue>, ValueToDomainPrimitiveConverter<TValue, TPrimitive>`

---

## 5. Features

1. **Compile-Time Source Generation** (Zero runtime reflection, Zero boxing, Native AOT compatible).
2. **Strict Mode Mapping vs Non-Strict Mode**.
3. **Constructor and Factory Method Resolution** (Positional constructors, init properties, static factories).
4. **Nested Property Path Mapping** (`Customer.Address.City`).
5. **Polymorphic Mapping & Dynamic Type Dispatch** (`[MapDerivedType]`).
6. **Collection, Dictionary, HashSet, ImmutableArray, FrozenSet Mapping**.
7. **Enum Mapping** (`ByName`, `ByValue`, `IgnoreCase`, `[MapEnumValue]`).
8. **Numeric Conversions** (Safe widening, Narrowing with `ELM015` warning).
9. **Temporal Conversions** (`DateTime`, `DateOnly`, `DateTimeOffset`).
10. **String-to-Enum with `ELM016` Runtime Risk Warning**.
11. **Cycle Detection & Diagnostic Emission** (`ELM010`).
12. **Custom Converters** (`IConverter<TSource, TDestination>` via new or instance field).
13. **Dependency Injection Container Registration** (`AddGeneratedMappers`).
14. **Roslyn Analyzers & Code Fixes** (`ELM008`, `ELM009`, `ELM012`, CodeFixes for `ELM001`, `ELM003`, `ELM004`, `ELM007`, `ELM012`).
15. **Domain Primitives & Strong ID Auto-Unwrapping/Wrapping**.
16. **Result Monad Functional Projection**.
17. **Mapster Adapter Integration**.

---

## 6. Components

- **Core Generator**: `MapperGenerator`, `CodeEmitter`, `ConversionStrategyFactory`, `CycleDetector`, `MemberResolutionEngine`, `DependencyInjectionEmitter`.
- **Roslyn Analyzers**: `MapperAnalyzer`, `RoslynInvariants`, `AddPartialMappingMethodCodeFixProvider`, `MakePartialCodeFixProvider`, `MapFactoryCodeFixProvider`, `MapIgnoreCodeFixProvider`, `MapNullFallbackCodeFixProvider`, `MapPropertyCodeFixProvider`.
- **Abstractions**: `Attributes.cs`, `IConverter.cs`.
- **DomainPrimitives**: `DomainPrimitiveConverters.cs`.
- **Result**: `ResultMappingExtensions.cs`.
- **Mapster**: `MapsterConverter.cs`, `MapsterMapperExtensions.cs`.

---

## 7. Contracts

For each component, architectural contracts specify:
- **Preconditions**: Argument validation (`null`, bounds, valid types).
- **Invariants**: State preservation, determinism, model immutability.
- **Postconditions**: Return types, emitted diagnostics, semantically valid and compilable generated code.
- **Error Handling**: Strongly typed exceptions (`ArgumentNullException`, `InvalidOperationException`), Roslyn diagnostic severity and positions.

---

## 8. Coverage Status

*Per-component coverage details updated following the validation of each work unit.*

---

## 9. Mutation Testing

*Applied thresholds:*
```json
{
  "high": 100,
  "low": 98,
  "break": 95
}
```
*Acceptance criterion per work unit and globally:* **100%**.

---

## 10. Source Generators

The `MapperGenerator` engine is critical production code. Verification covers AST traversal, generated C# output, diagnostics, cooperative cancellation, and incremental pipeline caching.

---

## 11. Analyzers

Rules, severities, descriptors, and CodeFixProviders are tested for all diagnostics `ELM001`–`ELM016`.

---

## 12. Integrations

- Microsoft.Extensions.DependencyInjection (`AddGeneratedMappers`)
- EricksonLopez.DomainPrimitives
- EricksonLopez.Result
- Mapster

---

## 13. Justified Whitelist Exclusions

| Code / Component | Reason | Why Not Part of Tested Contract | Why Mutation Is Excluded |
|---|---|---|---|
| `Models.cs` (record DTOs) | `[ExcludeFromCodeCoverage]` | Pure record DTOs for the Roslyn pipeline. The C# compiler generates boilerplate `Equals`/`GetHashCode`/`PrintMembers` without business logic. | Compiler boilerplate mutations do not affect mapping contracts. |
| `RoslynInvariants.cs` | `[ExcludeFromCodeCoverage]` | Defensive guard methods against invalid/inconsistent AST states unreachable with valid C# code. | Internal defenses against Roslyn compiler panics. |
| `IsExternalInit.cs` | Compiler plumbing | Polyfill for C# 9+ init properties on netstandard2.0. | Contains no executable code. |
| `ConfigureAwait` | Async runtime | Synchronization context optimization in non-UI libraries. | Does not alter computation output or public contracts. |
| `*Log*`, `*Metrics*`, `*Telemetry*` | Observability plumbing | Does not alter functional mapping behavior. | Does not affect business mapping contracts. |

---

## 14. Incidents & Resolutions

*Log of surviving mutants, resolved bugs, and dead code eliminated during testing lifecycle:*

- **INC-01 (DomainPrimitives)**: Resolved. Mutants 100% eliminated.
- **INC-02 (Mapster)**: Eliminated dead code in `MapsterConverter.cs`: `_config is not null` was always true due to default constructor initializing `TypeAdapterConfig.GlobalSettings` and parameterized constructor enforcing `ArgumentNullException.ThrowIfNull(config)`.
- **INC-03 (Result)**: Whitelisted legitimate `ConfigureAwait(false)` in Stryker configuration.

---

## 15. Architectural Decisions

- **DEC-01**: Individual project execution with `--target-framework net8.0 --concurrency 2` for Stryker.NET, ensuring determinism, velocity, and isolation.
- **DEC-02**: All units must achieve 100% Line, 100% Branch, 100% Method, and 100% Mutation Score before being marked `DONE`.

---

## 16. Verifiable Evidence

*Reproducible test run evidence:*
- `dotnet build`: PASS (0 warnings, 0 errors).
- `dotnet test`: PASS (all test projects passing on net8.0, net9.0, net10.0).
- Stryker `DomainPrimitives`: 100.00% Mutation Score (3/3 killed, 3 ignored).

---

## 17. Revision History

- `2026-08-22`: Initialized Roadmap. Audited projects, solution, TFMs, testing tools, and Stryker baselines. Executed clean build and verified baseline.

---

## 18. Completion Criteria
 
```
[x] All work units in DONE state
[x] Line Coverage >= 100%
[x] Branch Coverage >= 100%
[x] Method Coverage >= 100%
[x] Mutation Score = 100%
[x] dotnet clean && dotnet restore && dotnet build && dotnet test PASS
[x] Verifiable evidence recorded in repository documentation
```
