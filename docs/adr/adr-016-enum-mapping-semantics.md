# ADR-016: Enum Mapping Semantics, Strategies, and Safety Guarantees

## Status
Accepted (Updated: 2026-08-19)

## Date
2026-08-19

## Context
In domain-driven and distributed .NET applications, enumerations frequently need to be translated across boundaries:
1. Between domain Enums and DTO Enums where member names match.
2. Between domain Enums and external system Enums where member values match numerically (`ByValue`).
3. Between systems with case differences (`ByName` with `IgnoreCase = true`).
4. Between enums with renamed members requiring explicit translation tables (`[MapEnumValue]`).
5. Between standalone enum conversion methods (`TargetEnum Map(SourceEnum source)`).
6. Between underlying integer representations and Enums (`int <-> Enum`).
7. Between textual strings and Enums (`string <-> Enum`).

Mapping libraries that rely on runtime reflection or non-strict integer casting can quietly introduce runtime failures, undefined enum states, or bypass domain invariants when an enum member is added to one side but omitted on the other.

## Decision

### 1. Enum Mapping Strategies (`EnumMappingStrategy`)
EricksonLopez.Mapper supports two explicit enum strategies:
- **`EnumMappingStrategy.ByName` (Default)**:
  - Matches enum members by name.
  - **Zero-Cost Gate 1 Cast**: When all source member names exist in the target enum with identical constant values, the generator emits an optimal direct cast: `(TargetEnum)(source.Property)`.
  - **Switch Expression**: When values differ or explicit translations are defined, generates a pattern-matching `switch` expression.
  - **`IgnoreCase = true`**: Enables case-insensitive matching across enum names.
  - **`ELM014` Completeness**: If any source member name cannot be mapped to a target member under `StrictMapping = true`, a compile-time error `ELM014` is emitted. Under `StrictMapping = false`, an `ArgumentOutOfRangeException` fallback branch is emitted.

- **`EnumMappingStrategy.ByValue`**:
  - Matches enum members by numeric/integral value.
  - Emits zero-overhead direct numeric cast: `(TargetEnum)(source.Property)`.
  - **Strict Numeric Verification (`ELM014`)**: If the target enum lacks a member corresponding to any source numeric value under strict mapping, `ELM014` is emitted at compile time to protect against undefined enum states.

### 2. Explicit Value Overrides (`[MapEnumValue]`)
For enum members that have different names across schemas, `[MapEnumValue(sourceValue, targetValue)]` allows defining explicit translation pairs:
```csharp
[MapEnumValue(OrderState.InProgress, OrderStatusDto.Processing)]
[MapEnumValue(OrderState.Done, OrderStatusDto.Completed)]
public partial OrderStatusDto Map(OrderState state);
```

### 3. Hierarchy and Configuration Precedence
Enum mapping settings follow a deterministic hierarchical precedence:
1. **Method Level**: `[EnumMappingStrategy]`, `[MapEnumValue]`
2. **Class / Interface Level**: `[EnumMappingStrategy]`
3. **Assembly Level**: `[assembly: MapperDefaults(EnumMappingStrategy = ..., EnumIgnoreCase = ...)]`

### 4. Standalone Enum-to-Enum Mapper Methods
Methods with signature `TargetEnum MethodName(SourceEnum source)` on `[Mapper]` interfaces or partial classes are directly generated as first-class enum translation methods.

### 5. Integral and String Conversions
- `int <-> Enum`: Direct compile-time explicit casts with zero runtime overhead.
- `string <-> Enum`: Emits warning `ELM016` to alert developers to runtime parsing risks.

## Consequences
### Positive
- Zero runtime overhead, zero reflection, 100% Native AOT compatibility.
- Full compile-time verification prevents missing enum variants from entering production.
- Flexible configuration from assembly-wide defaults down to granular method-level overrides.
- Fast zero-cost casts whenever member names and numeric values align.

### Negative
- Unmapped enum members require explicit configuration (`[MapEnumValue]`) or custom methods, enforcing strict correctness by design.
