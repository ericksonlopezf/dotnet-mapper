# ADR-005: Constructor and Immutable Types Strategy

## Status
Accepted

## Date
2026-08-13

**Status**: Accepted
**Date**: 2026-08-13
**Deciders**: EricksonLopez.Mapper Architecture Team

## Context

Modern C# strongly favors immutable data structures: `record` types, `init`-only properties, and primary constructors. A mapping library must instantiate objects that do not expose a parameterless constructor, without resorting to reflection or `FormatterServices.GetUninitializedObject`.

## Decision

Constructor mapping is fully supported. The generator resolves the construction strategy in priority order:

1. **`[MapFactory("MethodName")]`** — use the specified static factory method.
2. **Parameterless constructor + init/set properties** — emit `new DestType { Prop = val }`.
3. **Single parameterized constructor** — emit `new DestType(param1, param2, ...)`.
4. **Multiple parameterized constructors** — emit **ELM007** (ambiguous constructor) and require `[MapFactory]` to disambiguate.

Constructor parameters are matched to source properties with the same strictness as property mappings.

## Alternatives Considered

- **Require parameterless constructors only:** Breaks modern C# paradigms (`record`, DDD value objects). Rejected.
- **Reflection-based bypass (`FormatterServices.GetUninitializedObject`):** Breaks NativeAOT and nullability guarantees. Rejected.
- **Roslyn expression-tree emit:** Unnecessary overhead; plain `new T(...)` expressions are equivalent. Rejected.

## Consequences

### Positive
- Full support for `record` types and immutable DTOs out of the box.
- Constructor mapping is validated at compile time — parameter name mismatches are ELM001.

### Negative
- Overload resolution for multiple constructors with similar parameter signatures requires explicit disambiguation via `[MapFactory]`.
