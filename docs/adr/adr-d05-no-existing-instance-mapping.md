# ADR-D05: Existing Instance Mapping Rejected

**Status**: Accepted  
**Date**: 2026-08-13

## Decision

The `Map(source, destination)` pattern — mapping into an existing instance — is **rejected**.

## Rationale

1. **Mutable shared state** — Mapping into an existing instance implies shared mutable state, violating immutability principles.
2. **DDD invariant violation** — Domain objects should be reconstructed through their factories/constructors, not mutated externally.
3. **Ambiguous semantics** — What happens to properties not present in the source? Reset to default? Preserve existing value? The semantics are undefined without additional configuration.
4. **Patch semantics belong to the application layer** — Partial update logic is a business concern, not a mapper concern.

## Alternative

Generate a new instance with `Map(source)`. For partial update scenarios, write explicit application-layer code.

## Impact vs Competitors

Mapperly, Mapster, and AutoMapper all support this pattern. The rejection forces explicit reconstruction, which is correct for DDD.
