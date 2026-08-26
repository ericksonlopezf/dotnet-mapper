# ADR-D06: Automatic Reverse Mapping Rejected

**Status**: Accepted  
**Date**: 2026-08-13

## Decision

Auto reverse mapping (generating `B → A` automatically from a declared `A → B`) is **rejected**.

## Rationale

1. **Semantic asymmetry** — `Domain → DTO` unwraps Value Objects. `DTO → Domain` constructs them. These are not symmetric operations.
2. **Implicit behavior** — A developer reading the mapper class cannot know a reverse mapping exists without checking AutoMapper configuration.
3. **Explicit is better** — Declaring both directions explicitly makes the code self-documenting and maintainable.

## Example of the Asymmetry

```csharp
// Domain → DTO: unwraps VO
source.Email.Value → dto.Email (string)

// DTO → Domain: reconstructs VO with validation
dto.Email (string) → new Email(dto.Email) // may throw if invalid
```
Auto-reversing cannot handle this semantic difference.

## NON_GOALS.md

See `NON_GOALS.md` for this documented as a non-goal.
