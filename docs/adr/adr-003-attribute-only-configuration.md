# ADR-003: Attribute-Only Configuration (No Fluent API)

**Status**: Accepted  
**Date**: 2026-08-13

## Context

Three configuration models were evaluated:
- (A) **Fluent API** — Centralized configuration object: `CreateMap<User, UserDto>().ForMember(...)` (AutoMapper approach)
- (B) **Attribute-only** — Configuration lives on the mapper class and its methods: `[MapProperty("Email", "EmailAddress")]`
- (C) **Hybrid** — Attributes for simple cases, fluent for complex ones (Mapster approach)

## Decision

**Attribute-only configuration**. All mapper configuration is expressed via attributes placed directly on the mapper class and its partial methods.

The available attributes are:
| Attribute | Target | Purpose |
|-----------|--------|---------|
| `[Mapper]` | Class | Marks the class as a mapper |
| `[MapProperty]` | Method | Renames source→destination member |
| `[MapIgnore]` | Method | Excludes a destination member |
| `[MapFactory]` | Method | Specifies factory method for construction |
| `[MapDerivedType]` | Method | Registers a polymorphic type pair |
| `[UseConverter]` | Method | Delegates to a custom `IConverter<T,U>` |
| `[MapNullFallback]` | Method | Provides fallback for nullable→non-nullable |

## Rationale

1. **Source generator simplicity** — `AttributeData` is trivially analyzable. Lambda/expression analysis in a source generator is extremely complex and fragile.
2. **Locality** — Configuration is co-located with the mapping method declaration, not in a separate profile class.
3. **Discoverability** — `[MapProperty]` on a method is immediately visible to reviewers. A fluent profile hidden elsewhere is not.
4. **No runtime state** — Fluent APIs require runtime evaluation of lambdas. Attributes are pure metadata.

## Alternatives Considered

- Fluent API: Rejected. Cannot be analyzed by a source generator without executing the lambda.
- Profile classes (AutoMapper): Rejected. Introduces indirection and hidden coupling.

## Consequences

- Edge cases that don't fit into attributes require `partial methods` as escape hatches (by design)
- API surface is small and learnable in under 30 minutes
- No global configuration; everything is per-mapper and per-method
