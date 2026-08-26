# ADR-D11: IQueryable Projection Rejected

**Status**: Accepted  
**Date**: 2026-08-13

## Decision

`IQueryable<T>.ProjectTo<TDto>()` and `Expression<Func<T, TDto>>` generation are **rejected**.

## Rationale

1. **AOT incompatibility** — Expression Trees compiled at runtime (`Expression.Compile()`) are not AOT-safe.
2. **ORM coupling** — Projection requires EF Core-specific LINQ provider knowledge. This violates Single Responsibility.
3. **Version coupling** — EF Core changes its expression tree interpretation across versions.

## Scope Boundary

EricksonLopez.Mapper transforms **objects in memory**. It does not generate SQL or LINQ expressions. ORM projection is the responsibility of the DAL/repository layer.

## NON_GOALS.md

See `NON_GOALS.md` for this documented as a non-goal.

## Impact vs Competitors

AutoMapper's `ProjectTo<T>()` is its most popular feature for EF Core users. We accept this as a competitive disadvantage for EF Core-heavy projects, as it's fundamentally incompatible with our AOT-first philosophy.
