# ADR-D10: Before/After Map Hooks Rejected

## Status
Rejected

## Date
2026-08-13

**Status**: Accepted  
**Date**: 2026-08-13

## Decision

Before/after map hooks (AutoMapper's `BeforeMap`, `AfterMap`) are **rejected**.

## Rationale

1. **Hidden side effects** — Hooks are invisible at the call site. A developer calling `mapper.ToDto(order)` cannot know that a hook modifies an audit log or sends a notification.
2. **"Explicit over magic"** — If code must execute before or after mapping, the caller should orchestrate it explicitly.
3. **DI complexity** — Hooks that require DI services complicate the AOT model.

## Alternative

The caller wraps the mapping call with explicit logic:
```csharp
// Caller controls the before/after logic explicitly:
var dto = mapper.ToDto(order);
auditService.RecordMapping(order.Id);
```
