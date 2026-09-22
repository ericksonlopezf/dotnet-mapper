# ADR-D03: Automatic Flattening Convention Rejected (Explicit Deep Path Navigation Supported)

## Status
Rejected (Feature Permanently Excluded)

## Date
2026-08-13 (Updated: 2026-08-19)

## Decision

EricksonLopez.Mapper does **not** support automatic implicit flattening by naming heuristics (e.g., automatically guessing that `Order.Customer.Address.City` should map to `CustomerAddressCity` or `City`).

However, EricksonLopez.Mapper **fully supports explicit deep nested path mapping** via the `[MapProperty(sourceMember, destinationMember)]` attribute across object initializers, parameterized constructors, and factory methods.

## Rationale

1. **"Explicit over magic"** — Convention-based implicit flattening creates invisible coupling between source and destination types. If two entities have overlapping property sub-paths, heuristic matchers silently guess wrong.
2. **Refactoring fragility** — Renaming an intermediate navigation property (e.g., `Customer` to `Buyer`) silently breaks heuristic mapping without deterministic compile-time safety.
3. **Readability & Predictability** — Code reviewers and security audits can immediately trace exact data flow without memorizing complex prefix/suffix stripping heuristics.

## Supported Capabilities: Explicit Path Navigation

Explicit path navigation via `[MapProperty]` supports arbitrary dotted chains and emits null-safe navigation (`?.` for reference/nullable types, `.` for non-nullable structs):

```csharp
[Mapper]
public partial class OrderMapper
{
    [MapProperty("Customer.Address.City", "City")]
    [MapProperty("Customer.Address.ZipCode", "ZipCode")]
    [MapNullFallback("City", "\"Unknown\"")]
    public partial OrderSummaryDto ToDto(Order source);
}
```

Generated code:
```csharp
public partial OrderSummaryDto ToDto(Order source)
{
    if (source == null) throw new ArgumentNullException(nameof(source));
    return new OrderSummaryDto
    {
        City = source.Customer?.Address?.City ?? "Unknown",
        ZipCode = source.Customer?.Address?.ZipCode
    };
}
```

### Safety Rules:
- **Compile-Time Validation (`ELM001`)**: Every segment of the dotted path is validated against the Roslyn symbol tree at compile time. Typos immediately produce compilation errors.
- **Nullability Mismatch (`ELM004`)**: If intermediate reference/nullable navigation properties can evaluate to `null` and the target member is non-nullable, the compiler emits `ELM004` unless resolved with `[MapNullFallback]`.
- **Target Formats**: Works seamlessly with direct property assignments, constructor parameters, record primary constructors, and factory methods.

## Impact vs Competitors

AutoMapper and Mapster rely heavily on unvalidated runtime reflection strings or heuristic name flattening that fails silently at runtime. EricksonLopez.Mapper achieves zero-reflection, 100% Native AOT safety with explicit compile-time code generation.
