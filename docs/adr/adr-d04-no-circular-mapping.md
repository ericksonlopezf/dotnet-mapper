# ADR-D04: Circular/Recursive Mapping Rejected

**Status**: Accepted  
**Date**: 2026-08-13

## Decision

Circular mapping (e.g., `A → B` where `B` references `A`) is **rejected**. The generator emits `ELM010: Circular mapping reference` at compile time.

## Rationale

1. **Reference tracking allocations** — Supporting cycles requires a `Dictionary<object, object>` reference tracker → allocations → AOT risk.
2. **DTO design smell** — A DTO with circular references is a design error. DTOs should represent acyclic projections of domain data.
3. **Stack overflow risk** — Without reference tracking, circular mapping causes infinite recursion. With tracking, it adds overhead.

## Enforced By

`ELM010` is emitted when the generator detects a cycle in the mapping dependency graph (DFS analysis over `MethodMapping` references).

## Impact vs Competitors

AutoMapper supports cycles via reference tracking. We treat this as an intentional design constraint that encourages better DTO design.
