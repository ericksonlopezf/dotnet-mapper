# ADR-009: Member Resolution and Mapping Contract

## Status
Accepted

## Date
2026-08-13

**Status**: Accepted
**Date**: 2026-08-13
**Deciders**: EricksonLopez.Mapper Architecture Team

## Context

The generator must define an explicit, deterministic contract for how source members are matched to destination members. Ambiguity in case-sensitivity, field vs. property resolution, or precedence rules leads to unpredictable behavior.

## Decision

The mapping contract is strictly limited to **public properties** and **public constructor parameters**. Public fields are excluded to enforce standard DTO design practices.

Resolution rules (evaluated in order):

1. **Explicit override wins**: `[MapProperty("sourceName", "destName")]` always takes precedence.
2. **Exact name match**: Case-sensitive equality between source and destination property names.
3. **Case-insensitive match**: PascalCase vs. camelCase normalization (e.g., `name` → `Name`).
4. **Ambiguous match**: If two source properties match case-insensitively → **ELM005**.
5. **No match + StrictMapping = true**: **ELM001**.
6. **No match + StrictMapping = false**: Silently skipped.

**Nullability rule**: Mapping `string?` → `string` without an explicit fallback emits **ELM004**. The generator never invents default values. Use `[MapNullFallback("dest", "\"\"")]` to resolve.

**Type compatibility**: The generator only maps semantically-equivalent types. It will not auto-map `Guid` → `UserId` (strongly typed ID) unless an explicit `[UseConverter]` is provided.

## Alternatives Considered

- **Include public fields:** Rejected for MVP to limit API surface and avoid resolution ambiguities.
- **Convention-based structural wrappers (Guid → UserId):** Rejected — implicit domain leakage violates the Explicitness principle (see ADR-D07).

## Consequences

### Positive
- Deterministic, predictable mapping. Developers always know why a property was or wasn't mapped.
- Explicit configuration surfaces intent in the code.

### Negative
- Extra `[UseConverter]` declarations required for strongly typed IDs unless `[ValueObject]` is used.
