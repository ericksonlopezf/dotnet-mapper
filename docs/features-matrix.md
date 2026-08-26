# Master Feature Matrix & Architecture Roadmap

## 1. Complete Feature Capabilities

| Category | Capability | Support Status | Target |
|---|---|:---:|---|
| **Core Mapping** | Direct Property-to-Property Mapping | ✅ Fully Supported | v1.0 |
| **Core Mapping** | Constructor Parameter Mapping | ✅ Fully Supported | v1.0 |
| **Core Mapping** | Static Factory Method Instantiation (`[MapFactory]`) | ✅ Fully Supported | v1.0 |
| **Core Mapping** | Member Ignoring (`[MapIgnore]`) | ✅ Fully Supported | v1.0 |
| **Core Mapping** | Custom Member Source Expressions (`[MapProperty]`) | ✅ Fully Supported | v1.0 |
| **Data Types** | Primitive Conversions & Numeric Widening | ✅ Fully Supported | v1.0 |
| **Data Types** | Enum-to-Enum & Enum-to-String Switching (`[MapEnum]`) | ✅ Fully Supported | v1.0 |
| **Data Types** | Nullable Reference Type & Null Fallbacks (`[MapNullFallback]`) | ✅ Fully Supported | v1.0 |
| **Collections** | `List<T>`, `T[]`, `IReadOnlyList<T>`, `HashSet<T>` | ✅ Fully Supported | v1.0 |
| **Polymorphism** | Abstract Base Class & Interface Dispatch (`[MapDerived]`) | ✅ Fully Supported | v1.0 |
| **Extensibility** | Custom Type Converters (`IConverter<TSource, TDestination>`) | ✅ Fully Supported | v1.0 |
| **Integrations** | `EricksonLopez.DomainPrimitives` Strongly-Typed IDs | ✅ Fully Supported | v1.0 |
| **Integrations** | `EricksonLopez.Result` Functional Pipeline Mapping | ✅ Fully Supported | v1.0 |
| **Quality Gates** | Roslyn Analyzers & Code Fixes (`ELM001` - `ELM016`) | ✅ Fully Supported | v1.0 |
| **AOT & Runtime** | 100% Native AOT & Trimming Verified | ✅ Fully Supported | v1.0 |
