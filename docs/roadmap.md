# Product Roadmap

This document outlines the delivered capabilities, future exploration areas, and permanent non-goals for `EricksonLopez.Mapper`.

> **Note**: This roadmap reflects architectural direction and is subject to change based on community feedback. Only items traceable to existing ADRs or approved design discussions are listed. No item should be assumed committed for a specific release timeline.

---

## Delivered & Production Capabilities (v1.0.0 & v2.0.0)

All core capabilities and ecosystem extensions are 100% implemented, tested, and verified:

### Core Mapping Engine & Conversions
- **Roslyn Incremental Source Generator**: Modular 7-component pipeline with `EquatableArray<T>` models for zero-leak IDE caching.
- **Strict Mapping by Default**: Compile-time `ELM001` errors on unmapped destination members.
- **Deep Nested Path Navigation**: Arbitrary dot-notation paths (`[MapProperty("Customer.Address.City", "City")]`) with safe null navigation.
- **Enum Mapping Engine**: Full `ByName` and `ByValue` strategies (`[EnumMappingStrategy]`), case-insensitivity, explicit member overrides (`[MapEnumValue]`), and `ELM014` diagnostics.
- **Constant & Computed Expressions**: `[MapValue]` for direct C# literal and timestamp injection.
- **Assembly-Wide Defaults**: `[assembly: MapperDefaults]` for project-level conventions.
- **DDD Value Object Support**: Automatic wrap/unwrap for `[ValueObject]` types and single-parameter readonly record structs.
- **Factory Method Construction**: `[MapFactory("MethodName")]` preserving domain invariants.
- **Polymorphic Mapping**: `[MapDerivedType]` emitting compile-time pattern-matching switch expressions.
- **Custom Converters**: `[UseConverter]` supporting static types and DI-injected instance fields.
- **Null Fallbacks**: `[MapNullFallback]` for nullable value types without custom converters.
- **Modern Collections**: Zero-allocation support for `T[]`, `List<T>`, `ImmutableArray<T>`, `ImmutableList<T>`, `HashSet<T>`, `FrozenSet<T>`, `Dictionary<K,V>`, `FrozenDictionary<K,V>`.
- **Automatic DI Registration**: `[assembly: GenerateMapperRegistration]` emitting `AddGeneratedMappers()` extension method.

### Roslyn Analyzers & Diagnostics
- `ELM008`: Prohibits runtime reflection, `Activator`, `Marshal`, and `RuntimeHelpers`.
- `ELM009`: Prohibits `dynamic` keyword usage in mapper classes.
- `ELM012`: Enforces `partial` modifier on `[Mapper]` classes.
- Automated Code Fix Providers for `ELM001` (`[MapIgnore]`) and `ELM012` (`partial` keyword).

### Ecosystem Integrations
- `EricksonLopez.Mapper.DomainPrimitives`: Converters for `IDomainPrimitive` and `IStrongId`.
- `EricksonLopez.Mapper.Mapster`: Bi-directional bridge adapter between `IConverter` and `TypeAdapterConfig`.
- `EricksonLopez.Mapper.Result`: Functional projection extensions for `Result<T>`.

---

## Future Exploration Areas

Items under exploratory design evaluation for future minor releases:

- **Source-level `ReadOnlySpan<T>` optimizations**: Enhanced generator emission for span-based buffers.
- **Expanded Roslyn CodeFixes**: Additional code fixes for ambiguous constructor resolution and enum mismatch suggestions.
- **NativeAOT Benchmarking CI Automation**: Continuous tracking of native binary sizes across releases.

---

## Permanent Non-Goals & Rejections

The following features are **permanently excluded** from this library by explicit architecture decision to protect NativeAOT predictability, zero-reflection invariants, and domain safety:

| Excluded Feature | ADR Reference | Rationale |
| :--- | :--- | :--- |
| **Field Mapping** | [ADR-D01](adr/adr-d01-no-field-mapping.md) | Fields violate standard DTO encapsulation and public API contracts |
| **Private Member Bypass** | [ADR-D02](adr/adr-d02-no-private-member-bypass.md) | Bypassing encapsulation breaks domain invariants; requires unsafe reflection |
| **Automatic Implicit Flattening** | [ADR-D03](adr/adr-d03-no-automatic-flattening.md) | Heuristic string splitting is brittle and refactoring-unsafe; use `[MapProperty("A.B", "C")]` |
| **Circular / Recursive Runtime Graphs** | [ADR-D04](adr/adr-d04-no-circular-mapping.md) | Runtime reference tracking introduces allocation overhead and AOT hazards |
| **Existing-Instance Mutation (`Map(src, dest)`)** | [ADR-D05](adr/adr-d05-no-existing-instance-mapping.md) | In-place mutation breaks immutability, DDD invariants, and concurrent safety |
| **Automatic Bidirectional / Reverse Mapping** | [ADR-D06](adr/adr-d06-no-reverse-mapping.md) | Implicit reverse mapping creates hidden coupling and asymmetric bug propagation |
| **Convention-Based Custom Naming Rules** | [ADR-D07](adr/adr-d07-no-naming-conventions.md) | Explicitness via attributes is preferred over dynamic naming regex conventions |
| **Global Runtime Converter Registry** | [ADR-D08](adr/adr-d08-no-global-converter-registry.md) | Mutable runtime registries break NativeAOT static analysis and tree trimming |
| **Conditional Mapping via Runtime Delegates** | [ADR-D09](adr/adr-d09-no-conditional-mapping.md) | Dynamic predicate evaluation incurs delegate allocations and boxing overhead |
| **Before / After Mapping Lifecycle Hooks** | [ADR-D10](adr/adr-d10-no-before-after-hooks.md) | Interceptors obscure data flow and add delegate dispatch overhead |
| **IQueryable Projection (`ProjectTo<T>`)** | [ADR-D11](adr/adr-d11-no-iqueryable-projection.md) | Expression tree rewriting is AOT-unsafe and hides database query costs |
| **Generic Runtime `IMapper` Service Facades** | [ADR-D12](adr/adr-d12-no-imapper-generic-interface.md) | Dynamic method dispatch degrades JIT inlining and type safety |
