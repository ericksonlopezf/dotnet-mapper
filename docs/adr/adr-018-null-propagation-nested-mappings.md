# ADR-018: Null-Propagation in Optional Nested Object Mappings

## Status
Accepted

## Date
2026-09-04

## Context
When mapping optional nested domain objects or DTOs (e.g., `source.Address?` to `target.Address?`), an unconditional call to a child mapping method `MapAddress(source.Address)` throws a runtime `ArgumentNullException` if the nested source is null, or produces unexpected empty target instances if not properly guarded.

## Decision
1. When mapping a nullable/optional complex object member where the target member is also nullable:
   - If a nested mapping method exists in the mapper definition, emit a ternary null guard:
     ```csharp
     TargetProp = (source.SourceProp != null ? this.MapNested(source.SourceProp) : null)
     ```
2. When the target member is non-nullable:
   - Require `[MapNullFallback]` attribute or emit compile-time error `ELM004` (Nullability mismatch).

## Consequences
### Positive
- Prevents runtime `NullReferenceException` and `ArgumentNullException` on optional nested structures.
- Generates clean, idiomatic ternary C# code with zero reflection overhead.
- Maintains strict compile-time nullability safety invariants.

### Negative
- Nested mapping methods must be explicitly declared on the mapper if nested transformation is desired.
