# ADR-019: Modern and Immutable Collection Mapping Support

## Status
Accepted

## Context
High-performance DDD enterprise applications frequently use `ImmutableArray<T>`, `ImmutableList<T>`, and .NET 8+ `FrozenSet<T>` and `FrozenDictionary<TKey, TValue>` for immutable value objects and aggregate read models.

Mapping into these collection types requires optimal builder semantics without allocating intermediate discardable arrays or lists whenever possible.

## Decision
Support direct mapping into modern collection types:
1. **`ImmutableArray<T>`**: Emits `ImmutableArray.CreateBuilder<T>(source.Count)` for known-size collections, or `.ToImmutableArray()`.
2. **`ImmutableList<T>`**: Emits `ImmutableList.CreateBuilder<T>()`, populates elements with transformations, and calls `.ToImmutable()`.
3. **`FrozenSet<T>`**: Transforms items to a `HashSet<T>` and emits `.ToFrozenSet()`.
4. **`FrozenDictionary<TKey, TValue>`**: Transforms entries and emits `.ToFrozenDictionary()`.
5. **Standard `List<T>`, `T[]`, `HashSet<T>`, `Dictionary<K, V>`**: Pre-sizes collections using source `.Count` / `.Length` where available to prevent intermediate reallocations.

## Consequences
### Positive
- Direct support for state-of-the-art .NET collection primitives.
- Minimized heap allocations and memory footprint.
- Full Native AOT compatibility.

### Negative
- `FrozenSet<T>` and `FrozenDictionary<TKey, TValue>` are available on .NET 8.0+ targets.
