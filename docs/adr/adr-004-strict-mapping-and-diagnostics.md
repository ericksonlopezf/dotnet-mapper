# ADR-004: Strict Mapping Engine and Compile-Time Diagnostics

## Status
Accepted

## Date
2026-08-13

**Status**: Accepted
**Date**: 2026-08-13
**Deciders**: EricksonLopez.Mapper Architecture Team

## Context

A common source of production bugs in object mapping is "silent failure." When a new property is added to a DTO but forgotten in the domain model (or vice versa), traditional mappers (AutoMapper, Mapster) silently ignore it, producing default values (`null`/`0`) and causing downstream bugs that are hard to trace.

## Decision

Adopt a **Fail-Fast at Compile-Time** philosophy with Strict Mode enabled by default (`StrictMapping = true` on `[Mapper]`).

In Strict Mode, every destination member **must** be either:
1. Automatically mapped by convention (exact or case-insensitive name match).
2. Explicitly remapped via `[MapProperty("source", "dest")]`.
3. Explicitly excluded via `[MapIgnore("dest")]`.

If a destination member is left unhandled, the generator emits **ELM001: UnmappedDestinationMember** — a C# compiler error that blocks the build.

Strict Mode can be disabled per mapper: `[Mapper(StrictMapping = false)]`.

## Alternatives Considered

- **Lenient by default (warning only):** Only warning on unmapped properties. Rejected — contradicts the predictability goal and creates "silent mapping drift" during refactoring.
- **Runtime validation (`AssertConfigurationIsValid()`):** AutoMapper approach. Rejected — failures occur in production, not at build time.

## Consequences

### Positive
- High confidence during refactoring. Evolving a DTO guarantees the mapping is updated.
- Mapping failures are diagnosed at the earliest possible moment (compile time).

### Negative
- More explicit configuration required when developers intentionally want to skip properties.
- `[MapIgnore]` becomes a required part of the API surface.
