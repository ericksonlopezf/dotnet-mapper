# ADR-D01: Field Mapping Rejected

## Status
Rejected

## Date
2026-08-13

**Status**: Accepted  
**Date**: 2026-08-13

## Decision

EricksonLopez.Mapper does **not** support field mapping (e.g., `public int _count`).

## Rationale

1. **Encapsulation** — Fields are implementation details, not public API. In DDD and Clean Architecture, all observable state must be exposed via properties.
2. **Universality** — 99.9% of real-world mapping use cases involve properties. Field mapping is edge-case territory.
3. **Simplicity** — Supporting fields adds generator complexity for negligible real-world benefit.

## Impact vs Competitors

Mapperly and Mapster support field mapping. For the rare case where field mapping is needed, developers can write a `partial method` implementing the logic manually.

## Reconsideration Criteria

Evidence of real demand in DDD/Clean Architecture codebases. A GitHub issue with 50+ upvotes would trigger review.
