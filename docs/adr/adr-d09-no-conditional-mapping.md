# ADR-D09: Conditional Mapping Rejected

**Status**: Accepted  
**Date**: 2026-08-13

## Decision

Conditional mapping via attributes (e.g., `Condition(src => src.Email != null)`) is **rejected**.

## Rationale

1. **Attribute limitation** — Complex conditional logic cannot be cleanly expressed as attribute metadata.
2. **Generator complexity** — Evaluating arbitrary conditions at generate-time is not feasible.
3. **Responsibility separation** — The mapper should not be a business rules engine. Conditional logic belongs in the application layer.

## Alternative

Use a `partial method` to implement custom mapping logic that includes conditions:
```csharp
[Mapper]
public partial class OrderMapper
{
    public partial OrderDto ToDto(Order source);
    
    // This partial method is NOT generated — you implement the logic:
    private string MapStatus(Order source) =>
        source.IsActive ? "ACTIVE" : "INACTIVE";
}
```
