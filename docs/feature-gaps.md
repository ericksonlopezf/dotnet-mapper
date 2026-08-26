# Feature Gaps & Systematic Rejections (ADR Discards)

## 1. Intentional Feature Discards & Architectural Boundaries

To preserve zero-allocation invariants, simplicity, and Native AOT safety, the following capabilities have been systematically evaluated and discarded:

| Discarded Feature | Evaluation Rationale | Recommended Architectural Pattern |
|---|---|---|
| **Runtime Assembly Scanning** | Breaches AOT trimming invariants and degrades application startup time. | Declare static partial mappers processed by Roslyn at compile time. |
| **Dynamic String Property Paths** | e.g., `"Customer.Address.City"`. Runtime path resolution allocates strings and relies on reflection. | Use compile-time `[MapProperty(nameof(Target), nameof(Source.Path))]`. |
| **Runtime Custom Type Converters via `Func<object, object>`** | Boxing value types to `object` creates continuous GC Gen 0 pressure. | Implement strongly-typed `IConverter<TSource, TDestination>`. |
| **Bidirectional Cyclic Graph Auto-Tracking** | Requires maintaining a runtime `ReferenceHandler` identity dictionary per mapping call, adding 64 B - 256 B allocation per request. | Map flat DTOs or break cyclic loops via `[MapIgnore]`. |
