# ADR-D02: Private Member Bypass Rejected

**Status**: Accepted  
**Date**: 2026-08-13

## Decision

EricksonLopez.Mapper will **never** bypass private constructors or private members to instantiate objects.

## Rationale

1. **Domain invariant protection** — A private constructor signals that the type enforces a creation contract. Bypassing it (as AutoMapper does via `FormatterServices.GetUninitializedObject`) produces objects in an invalid state that never passed through the type's validation logic.
2. **AOT incompatibility** — `FormatterServices.GetUninitializedObject` is not AOT-safe.
3. **Security** — Bypassing constructors can instantiate types with security-sensitive initialization logic.

## Enforced By

When a mapping target has only private constructors and no `[MapFactory]` is provided, the generator emits `ELM002: Missing factory or constructor`.

## Alternative for Developers

Use `[MapFactory(nameof(MyType.Create))]` to point the generator to a public static factory method.

## Impact vs Competitors

AutoMapper and partially Mapster bypass private constructors. Most senior developers consider this an anti-pattern.
