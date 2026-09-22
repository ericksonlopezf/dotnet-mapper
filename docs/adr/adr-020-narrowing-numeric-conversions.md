# ADR-020: Narrowing Numeric Conversions and Diagnostic Policy

## Status
Accepted

## Date
2026-09-04

## Context
In business applications, data schemas frequently have slight numeric discrepancies (e.g., `long` database ID mapped to `int` domain ID, or `double` sensor reading mapped to `float` visualization metric).

Disallowing narrowing conversions entirely with a hard compiler error forces developers to write boilerplate manual converter classes for trivial numeric conversions. Conversely, silently emitting unchecked casts can lead to runtime data truncation or integer overflow without warning.

## Decision
1. **Narrowing Numeric Conversions**:
   - The generator generates explicit casts (e.g. `(int)(source.LongProperty)`) for standard narrowing numeric conversions (`long -> int`, `double -> float`, `int -> short`, etc.).
2. **Compile-Time Diagnostic**:
   - The generator emits compile-time warning `ELM015` (`Narrowing numeric conversion from '{0}' to '{1}' for member '{2}' may result in overflow or precision loss`).
3. **Strict Invariant**:
   - In projects where narrowing is considered dangerous, developers can treat warnings as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` or `<WarningsAsErrors>ELM015</WarningsAsErrors>`) to enforce zero narrowing conversions.

## Consequences
### Positive
- Balance between developer ergonomics and compile-time transparency.
- Developers are immediately notified of potential precision or overflow loss via `ELM015`.
- Configurable per-project strictness using standard MSBuild diagnostic severity levels.

### Negative
- Developers must be aware of potential integer overflow on large values if not checked upstream.
