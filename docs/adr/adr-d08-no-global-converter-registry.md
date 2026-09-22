# ADR-D08: Global Converter Registry Rejected

## Status
Rejected

## Date
2026-08-13

**Status**: Accepted  
**Date**: 2026-08-13

## Decision

A global converter registry (e.g., `Register<DateTime, DateOnly>(x => DateOnly.FromDateTime(x))`) is **rejected**.

## Rationale

1. **AOT incompatibility** — A runtime registry requires `Dictionary<(Type, Type), Func<object, object>>` — reflection-based type keys.
2. **Ambiguity** — If two registrations exist for the same type pair, behavior is undefined.
3. **Hidden coupling** — A global registry hides conversion logic from the mapper declaration.

## Alternative

Use `[UseConverter(typeof(MyConverter))]` per mapping method. Built-in conversions (numeric widening, enum↔string, Guid↔string, DateTime→DateOnly) are handled automatically by the generator without any configuration.

## Impact vs Competitors

AutoMapper and Mapster provide global converter registries. The `[UseConverter]` pattern provides the same capability with explicit, local configuration.
