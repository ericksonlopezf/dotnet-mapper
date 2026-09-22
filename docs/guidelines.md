# Engineering Guidelines & Coding Standards

## 1. Core Guidelines

1. **Prefer Static Partial Classes for Stateless Mappers**: Static partial classes are the optimal pattern for mappers with no DI-injected dependencies. They enable direct `.Select(Mapper.Map)` use without container resolution overhead.
2. **Use Instance Partial Classes with DI for Converter-Dependent Mappers**: When mappers require `[UseConverter(string converterFieldName)]` with DI-injected fields, declare the mapper as a non-static `partial class` and register it as a `Singleton` (all generated mappers are stateless and thread-safe).
3. **Explicit Member Binding Over Assumptions**: When property names differ, always declare `[MapProperty("SourceProp", "DestProp")]`. The generator does not perform automatic heuristic name resolution.
4. **Handle Nullability Explicitly**: Use `[MapNullFallback("DestProp", "fallbackExpression")]` whenever the source member is nullable and the destination is non-nullable.
5. **Use Strong Types for Identifiers**: Map strongly-typed IDs directly using `EricksonLopez.Mapper.DomainPrimitives` converters or `[ValueObject]`-annotated record structs.
6. **Enforce Zero Allocations in Tests**: Use `MapperContractExtensions.AssertZeroAllocations` in regression tests to guard against allocation regressions in generated mapping methods.
