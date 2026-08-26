# Non-Goals

`EricksonLopez.Mapper` is strictly designed as an **AOT-First, Zero-Reflection, Zero-Overhead** source generator mapper. To maintain compile-time correctness and extreme performance, the following features are explicitly designated as **NON-GOALS**:

## 1. Deep Path Flattening
- **Description:** Automatically flattening complex paths (e.g. `source.Address.City` -> `CityName`).
- **Rationale:** Implicit flattening violates the "Explicitness > Magic" rule. It creates brittle mappings that break silently when source models are refactored. Users must explicitly map deep paths using `[MapProperty("Address.City", "CityName")]` or custom methods.

## 2. Structural `Result<T>` / `PagedResult<T>` Integration
- **Description:** Native structural awareness and unwrapping of `Result<T>`, `Option<T>`, or `PagedResult<T>` wrappers.
- **Rationale:** The mapper should not couple itself to specific third-party functional programming or pagination libraries. Users should map the inner types (e.g. `TSource` to `TDestination`) and handle wrapper construction in their own application logic.

## 3. Reverse Mapping (`ReverseMap()`)
- **Description:** Generating bidirectional mappings automatically.
- **Rationale:** Bidirectional mapping in complex domains often leads to asymmetric data loss. DTO-to-Domain and Domain-to-DTO are distinct operations that require explicit, separate definitions.

## 4. `IQueryable` / `Expression` Projections
- **Description:** Emitting LINQ `Expression<Func<TSource, TDest>>` trees for Entity Framework Core.
- **Rationale:** ORM projection generation is out of scope for an object-to-object memory mapper.

## 5. Runtime Reflection Fallbacks
- **Description:** Falling back to `Activator.CreateInstance` or `System.Reflection` when AOT fails.
- **Rationale:** If it doesn't map at compile time, it's a compiler error. Zero runtime reflection is our primary guarantee.
