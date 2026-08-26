# ADR-006: Nested and Collection Mapping Algorithms

**Status**: Accepted
**Date**: 2026-08-13 (Updated 2026-08-15)
**Deciders**: EricksonLopez.Mapper Architecture Team

## Context

Real-world enterprise domain models contain complex nested graphs and diverse collection types (`List<T>`, `T[]`, `ImmutableArray<T>`, `ImmutableList<T>`, `FrozenSet<T>`). The generator must map nested object graphs efficiently without allocating unnecessary iterators, delegate closures, or intermediate LINQ pipelines.

## Decision

### 1. Nested Object Mapping & Null-Propagation
- For complex nested types, the generator invokes declared sibling mapping methods within the same mapper class (e.g., `this.MapAddress(source.Address)`).
- When mapping optional/nullable nested references where the destination is also nullable, the generator emits a null-propagation ternary guard:
  ```csharp
  TargetProp = (source.SourceProp != null ? this.MapChild(source.SourceProp) : null)
  ```
- Circular references across single or multiple hops are caught at compile time via `ELM010` graph cycle detection.

### 2. Collection Mapping Algorithms
The generator emits imperative `for` / `foreach` loops with capacity pre-allocation rather than `.Select().ToList()`, eliminating delegate heap allocations and dynamic array resizing:

| Target Collection Type | Generation Strategy |
| :--- | :--- |
| `T[]` | Direct allocation `new Target[source.Length]` + indexed `for` loop |
| `List<T>` | Pre-sized `new List<Target>(source.Count)` + imperative `foreach` loop |
| `ImmutableArray<T>` | `ImmutableArray.CreateBuilder<Target>(source.Count)` + `.MoveToImmutable()` |
| `ImmutableList<T>` | `ImmutableList.CreateBuilder<Target>()` + `.ToImmutable()` |
| `HashSet<T>` | Pre-sized `new HashSet<Target>(source.Count)` + `.Add()` |
| `FrozenSet<T>` | `new HashSet<Target>(source.Count)` + `.ToFrozenSet()` |
| `Dictionary<TKey, TValue>` | Pre-sized `new Dictionary<TKey, TValue>()` + KVP mapping |
| `FrozenDictionary<TKey, TValue>` | Pre-sized `new Dictionary<TKey, TValue>()` + `.ToFrozenDictionary()` |
| Interface targets (`IList<T>`, `IReadOnlyList<T>`) | Emitted as `List<T>` or `ImmutableArray<T>` |

## Consequences

### Positive
- Zero extra allocations beyond the target collection instantiation.
- First-class support for modern .NET 8+ collections (`FrozenSet`, `FrozenDictionary`, `ImmutableList`).
- Compile-time safety for optional nested object graphs.

### Negative
- Nested mapping methods must be explicitly declared on the mapper if sub-object transformations are required.
