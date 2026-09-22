# ADR-D07: Custom Naming Conventions Rejected

## Status
Rejected

## Date
2026-08-13

**Status**: Accepted  
**Date**: 2026-08-13

## Decision

Custom naming conventions (e.g., `ReplaceMemberName("_", "")`) are **rejected**.

## Rationale

1. **Global runtime state** — Naming conventions require a global dictionary evaluated at runtime: AOT-incompatible.
2. **Hidden coupling** — Convention rules are invisible at the mapper declaration site.
3. **"Explicit over magic"** — Convention-based name resolution is the opposite of explicit mapping.

## Alternative

Use `[MapProperty("source_name", "destinationName")]` explicitly for any non-convention mapping.
