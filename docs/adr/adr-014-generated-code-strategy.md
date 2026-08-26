# ADR-014: Generated Code Determinism, Readability, and Invariants

**Status**: Accepted
**Date**: 2026-08-13 (Updated 2026-08-15)
**Deciders**: EricksonLopez.Mapper Architecture Team

## Context

Generated code that is minified, non-deterministic, or ambiguous regarding symbol resolution can produce compile errors in large solutions with conflicting namespaces. Furthermore, developers must be able to step into generated `*.g.cs` code during debugging and inspect every property mapping directly.

## Decision

The `CodeEmitter` emits deterministic, transparent, idiomatic C# code:

1. **Deterministic Symbol Qualification**:
   - Types are fully qualified with `global::` to eliminate any symbol shadowing or ambiguous namespace collisions.
2. **Explicit Null Safety**:
   - Generates `#nullable enable` at the top of every generated file.
   - Emits upfront null guards on mapping inputs: `if (source == null) throw new global::System.ArgumentNullException(nameof(source));` (for non-nullable inputs).
   - Emits ternary null-propagation guards for nullable nested structures: `Child = (source.Child != null ? this.MapChild(source.Child) : null)`.
3. **Capacity Pre-Allocation**:
   - Collections are instantiated with exact capacities (`new List<T>(source.Count)`, `ImmutableArray.CreateBuilder<T>(source.Count)`) to eliminate dynamic memory reallocations.
4. **Snapshot Validation**:
   - Generated code formatting and semantics are strictly enforced via `Verify.SourceGenerators` snapshot tests across all mapping combinations.

## Consequences

### Positive
- Zero namespace collision risk in complex enterprise solutions.
- 100% transparent and step-through debuggable with IDE breakpoints.
- Enforced zero-allocation collection emission.

### Negative
- Snapshot tests must be maintained when generator emission logic evolves.
