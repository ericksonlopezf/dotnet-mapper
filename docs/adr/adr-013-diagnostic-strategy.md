# ADR-013: Diagnostic Strategy and Error Reporting

## Status
Accepted

## Date
2026-08-13

**Status**: Accepted
**Date**: 2026-08-13 (Updated 2026-08-15)
**Deciders**: EricksonLopez.Mapper Architecture Team

## Context

A source generator and compile-time analyzer must communicate failures and warnings clearly, deterministically, and precisely. Silent failures (generator crashes without output) or generic errors prevent developers from identifying the root cause.

## Decision

A formal, stable catalog of diagnostics `ELM001`–`ELM018` is defined. All diagnostics are emitted via Roslyn against the exact `Location` (method, parameter, or attribute syntax node) that caused the issue:

### Complete Diagnostic Catalog

| Diagnostic ID | Title | Severity | Component | Description |
| :--- | :--- | :--- | :--- | :--- |
| `ELM001` | Unmapped Destination Member | Error | Generator | Destination property has no source match and is not marked `[MapIgnore]` (in strict mode). |
| `ELM002` | Missing Constructor or Factory | Error | Generator | Destination type has no accessible public constructors or factory methods. |
| `ELM003` | Unsupported Type Conversion | Error | Generator | Incompatible types between source and destination members with no registered converter. |
| `ELM004` | Nullability Mismatch | Error | Generator | Nullable source assigned to non-nullable destination without `[MapNullFallback]`. |
| `ELM005` | Ambiguous Member Match | Error | Generator | Multiple case-insensitive candidate properties match on the source type. |
| `ELM006` | Missing Supported Constructor | Error | Generator | Destination type has no constructor or public setters available for initialization. |
| `ELM007` | Ambiguous Constructor | Error | Generator | Destination type has multiple parameterized constructors without `[MapFactory]`. |
| `ELM008` | Prohibited Reflection / AOT Violation | Error | Analyzer | Mapper implementation uses `System.Reflection`, `Activator`, `Marshal`, `RuntimeHelpers`, or `FormatterServices`. |
| `ELM009` | Prohibited Dynamic Usage | Error | Analyzer | Mapper implementation uses the C# `dynamic` keyword. |
| `ELM010` | Circular Mapping Dependency | Error | Generator | Recursive object graph cycle detected across one or multiple mapping steps. |
| `ELM011` | Incomplete Polymorphism | Warning | Generator | Abstract target type does not have all derived types registered via `[MapDerivedType]`. |
| `ELM012` | Mapper Type Must Be Partial | Error | Analyzer | Class or interface decorated with `[Mapper]` lacks the required `partial` modifier. |
| `ELM013` | Invalid Converter Type | Error | Generator | Custom converter specified in `[UseConverter]` does not implement `IConverter<TSource, TDestination>`. |
| `ELM014` | Unmapped Enum Member | Error / Warning | Generator | Target enum is missing a member present in the source enum (Error in strict mode, Warning in non-strict). |
| `ELM015` | Narrowing Numeric Conversion | Warning | Generator | Narrowing numeric conversion (e.g. `long -> int`) may result in overflow or precision loss. |
| `ELM016` | String to Enum Conversion Risk | Warning | Generator | String-to-enum mapping carries runtime parsing risk if unconstrained. |
| `ELM017` | MapFactory Method Not Found | Error | Generator | Static factory method specified in `[MapFactory]` was not found on destination type. |
| `ELM018` | Duplicate MapProperty Destination | Warning | Generator | Destination member is targeted by multiple `[MapProperty]` declarations. |

The generator **never throws unhandled exceptions**. If an unrecoverable error occurs, it emits the corresponding diagnostic and suppresses output for that mapping without crashing the Roslyn process.

## Code Fix Providers

The `EricksonLopez.Mapper.Analyzers` package delivers native Roslyn IDE Code Fix Providers:
1. `MakePartialCodeFixProvider`: Fixes `ELM012` by adding the `partial` keyword.
2. `MapIgnoreCodeFixProvider`: Fixes `ELM001` by adding `[MapIgnore("Member")]`.
3. `MapPropertyCodeFixProvider`: Fixes `ELM001` by adding `[MapProperty("Source", "Destination")]`.
4. `MapNullFallbackCodeFixProvider`: Fixes `ELM004` by adding `[MapNullFallback("Member", "default!")]`.
5. `MapFactoryCodeFixProvider`: Fixes `ELM007` by adding `[MapFactory("Create")]`.
6. `AddPartialMappingMethodCodeFixProvider`: Fixes `ELM003` (unsupported type conversion) by adding a typed `partial` mapping method signature to the mapper class, prompting the developer to provide a hand-written implementation for the unsupported type pair.

## Consequences

### Positive
- Actionable compiler errors with exact squiggly spans in Visual Studio, VS Code, JetBrains Rider, and `dotnet build`.
- IDE code fixers automate resolving mapping ambiguities and nullability mismatches with one click.
- 100% stable Roslyn compiler execution with zero unhandled exceptions.
