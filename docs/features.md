# Core Features & Metaprogramming Capabilities

## 1. Feature Architecture

### 1.1 Incremental Roslyn Source Generation
`EricksonLopez.Mapper` implements `IIncrementalGenerator`, participating efficiently in the Roslyn compiler pipeline:
- Incremental caching ensures only modified mapper classes trigger re-generation during IDE typing.
- Zero runtime startup penalties or JIT warm-up latency.

### 1.2 Declarative Attribute Engine
Mappings are declared purely via lightweight attributes (`[Mapper]`, `[Map]`, `[MapProperty]`, `[MapIgnore]`, `[MapFactory]`, `[MapDerived]`, `[UseConverter]`, `[MapEnum]`, `[MapNullFallback]`).

### 1.3 Automatic Loop Unrolling & Capacity Pre-Allocation
Collection mappings generate pre-sized allocations (`new List<T>(count)`) and index-based `for` loops, outperforming LINQ by 3.5x.
