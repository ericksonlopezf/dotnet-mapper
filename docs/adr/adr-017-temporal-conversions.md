# ADR-017: Built-in Temporal Conversions and Interoperability

## Status
Accepted

## Context
.NET modern temporal types (`DateOnly`, `TimeOnly`, `DateTime`, `DateTimeOffset`) frequently interact in modern applications. Legacy databases or external APIs often provide `DateTime` or `DateTimeOffset`, while domain models prefer modern `DateOnly` and `TimeOnly` primitives.

Prior to v1.1, conversions between these types required manual converter classes or custom mapper signatures, adding boilerplate to common enterprise mapping workflows.

## Decision
Support bidirectional, zero-reflection compile-time conversions between modern .NET temporal primitives:
1. **`DateTime` -> `DateOnly`**: Emits `global::System.DateOnly.FromDateTime(source.Property)`.
2. **`DateOnly` -> `DateTime`**: Emits `(source.Property).ToDateTime(global::System.TimeOnly.MinValue)`.
3. **`DateTimeOffset` -> `DateOnly`**: Emits `global::System.DateOnly.FromDateTime((source.Property).DateTime)`.
4. **`DateOnly` -> `DateTimeOffset`**: Emits `new global::System.DateTimeOffset((source.Property).ToDateTime(global::System.TimeOnly.MinValue))`.
5. **`DateTime` -> `DateTimeOffset`**: Emits `new global::System.DateTimeOffset(source.Property)`.
6. **`DateTimeOffset` -> `DateTime`**: Emits `(source.Property).DateTime`.

## Consequences
### Positive
- Built-in ergonomic conversion without manual `IConverter` boilerplate.
- Pure inline C# code emission; zero runtime allocations or reflection.
- 100% Native AOT and trim-compatible.

### Negative
- Assumes `TimeOnly.MinValue` (midnight UTC/local) when expanding `DateOnly` to `DateTime` / `DateTimeOffset`. If a specific time component is required, developers should use a custom `IConverter`.
