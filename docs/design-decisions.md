# Design Decisions Summary

This document provides a consolidated overview of the **34 Architecture Decision Records (ADRs)** governing the `EricksonLopez.Mapper` ecosystem.

For the full detailed decision records, navigate to [docs/adr/](adr/).

---

## 1. Architectural ADRs (`docs/adr/ADR-000` to `ADR-021`)

| ADR | Title | Summary of Architectural Decision |
|---|---|---|
| [ADR-000](adr/ADR-000-non-goals-and-rejections.md) | Non-Goals and Rejections Master Index | Defines the catalog and core criteria for all explicitly rejected features. |
| [ADR-001](adr/ADR-001-source-generation-over-reflection.md) | Source Generation over Runtime Reflection | Mandates 100% compile-time C# code emission via Roslyn Incremental Generators. |
| [ADR-002](adr/ADR-002-incremental-generator-stability.md) | Incremental Generator Stability & Caching | Uses value-equatable record models with `EquatableArray<T>` to guarantee zero Roslyn cache leaks. |
| [ADR-003](adr/ADR-003-attribute-only-configuration.md) | Attribute-Only Declarative Configuration | Configuration expressed exclusively via attributes co-located with mappers (no fluent profiles). |
| [ADR-004](adr/ADR-004-strict-mapping-and-diagnostics.md) | Strict Mapping Engine & Diagnostics | `StrictMapping = true` by default; emits `ELM001` on unmapped destination members. |
| [ADR-005](adr/ADR-005-constructor-and-immutable-types.md) | Constructor and Immutable Types Strategy | Native support for positional records and `init` properties; `[MapFactory]` for disambiguation. |
| [ADR-006](adr/ADR-006-collection-and-nested-mapping.md) | Collection & Nested Mapping Algorithms | Emits explicit `for`/`foreach` loops with capacity pre-sizing; excludes runtime LINQ delegates. |
| [ADR-007](adr/ADR-007-aot-first-zero-tolerance.md) | AOT-First Zero Tolerance Policy | Zero tolerance for dynamic code; `IL2026`/`IL3050` warnings fail CI compilation. |
| [ADR-008](adr/ADR-008-testing-metrics-and-mutation-exclusions.md) | Testing Metrics and Mutation Exclusions | Sets Stryker mutation threshold (break at 95%) and defines method exclusion criteria. |
| [ADR-009](adr/ADR-009-member-resolution-contract.md) | Member Resolution and Mapping Contract | Exact match → case-insensitive fallback → `ELM001`; public properties and constructors only. |
| [ADR-010](adr/ADR-010-di-integration-strategy.md) | Dependency Injection Integration Strategy | Generator emits `AddGeneratedMappers()` extension method without hard runtime DI coupling. |
| [ADR-011](adr/ADR-011-native-aot-strategy.md) | Native AOT Strategy | Enforces `IsAotCompatible=true` and validates compatibility via `aot-smoke-test.yml`. |
| [ADR-012](adr/ADR-012-trimming-strategy.md) | Assembly Trimming Strategy | Enforces `IsTrimmable=true`; eliminates runtime dynamic reflection dependencies. |
| [ADR-013](adr/ADR-013-diagnostic-strategy.md) | Diagnostic Strategy | Stable `ELM001`–`ELM999` diagnostic codes; diagnostic reports replace generator exceptions. |
| [ADR-014](adr/ADR-014-generated-code-strategy.md) | Generated Code Readability & Snapshot Testing | Emits formatted, human-readable C# code; snapshot testing verified via `Verify.SourceGenerators`. |
| [ADR-015](adr/ADR-015-performance-budgets.md) | Build-Time and Runtime Performance Budgets | Zero runtime allocations beyond destination creation; <50ms incremental generator caching. |
| [ADR-016](adr/ADR-016-enum-mapping-semantics.md) | Enum Mapping Semantics | Supports `ByName` and `ByValue` strategies with `[EnumMappingStrategy]` and `[MapEnumValue]`. |
| [ADR-017](adr/ADR-017-temporal-conversions.md) | Temporal Conversions Strategy | Safe zero-overhead bridging across `DateTime`, `DateOnly`, and `DateTimeOffset`. |
| [ADR-018](adr/ADR-018-null-propagation-nested-mappings.md) | Null Propagation in Nested Mappings | Generates safe null ternary chains for deep dot-notation navigation paths. |
| [ADR-019](adr/ADR-019-modern-immutable-collections.md) | Modern Immutable Collections Strategy | Native support for `ImmutableList<T>`, `FrozenSet<T>`, and `FrozenDictionary<K,V>`. |
| [ADR-020](adr/ADR-020-narrowing-numeric-conversions.md) | Narrowing Numeric Conversions Policy | Emits explicit casts accompanied by `ELM015` compile-time precision warnings. |
| [ADR-021](adr/ADR-021-test-naming-convention-and-ide1006.md) | Test Naming Convention & IDE1006 Policy | Adopts Roy Osherove naming pattern (`Method_Scenario_Result`) as living specifications. |

---

## 2. Permanent Non-Goal ADRs (`docs/adr/ADR-D01` to `ADR-D12`)

The following capabilities are **permanently excluded** from `EricksonLopez.Mapper`:

| ADR | Rejected Feature | Rationale Summary |
|---|---|---|
| [ADR-D01](adr/ADR-D01-no-field-mapping.md) | Public Field Mapping | Fields violate standard DTO encapsulation and public API contracts. |
| [ADR-D02](adr/ADR-D02-no-private-member-bypass.md) | Private Member Bypass via Unsafe/Reflection | Bypassing encapsulation breaks domain invariants; requires unsafe reflection. |
| [ADR-D03](adr/ADR-D03-no-automatic-flattening.md) | Automatic Implicit Flattening | String heuristic splitting is refactoring-unsafe; deep paths must use `[MapProperty]`. |
| [ADR-D04](adr/ADR-D04-no-circular-mapping.md) | Circular / Recursive Runtime Graphs | Reference tracking introduces heap allocation overhead and NativeAOT hazards (`ELM010`). |
| [ADR-D05](adr/ADR-D05-no-existing-instance-mapping.md) | Existing-Instance Mutation (`Map(src, dest)`) | In-place mutation breaks immutability, DDD invariants, and concurrent safety. |
| [ADR-D06](adr/ADR-D06-no-reverse-mapping.md) | Automatic Reverse / Bidirectional Mapping | Implicit reverse mapping creates hidden coupling and asymmetric bug propagation. |
| [ADR-D07](adr/ADR-D07-no-naming-conventions.md) | Dynamic Custom Naming Conventions | Explicit attribute annotations are preferred over opaque regex naming conventions. |
| [ADR-D08](adr/ADR-D08-no-global-converter-registry.md) | Global Runtime Converter Registry | Mutable runtime registries break NativeAOT static analysis and tree trimming. |
| [ADR-D09](adr/ADR-D09-no-conditional-mapping.md) | Conditional Mapping via Runtime Delegates | Dynamic predicate evaluation incurs delegate allocations and boxing overhead. |
| [ADR-D10](adr/ADR-D10-no-before-after-hooks.md) | Before / After Mapping Lifecycle Hooks | Interceptors obscure data flow and add runtime delegate dispatch overhead. |
| [ADR-D11](adr/ADR-D11-no-iqueryable-projection.md) | `IQueryable` Projection (`ProjectTo<T>`) | Expression tree rewriting is AOT-unsafe and hides database query costs. |
| [ADR-D12](adr/ADR-D12-no-imapper-generic-interface.md) | Generic Runtime `IMapper` Service Facades | Dynamic method dispatch degrades JIT inlining and static type safety. |
