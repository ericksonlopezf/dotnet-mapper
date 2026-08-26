# Engineering Guidelines & Coding Standards

## 1. Core Guidelines
1. **Declare All Mappers as Static Partial Classes**: Never instantiate mapper classes or create stateful mappers.
2. **Explicit Member Binding Over Assumptions**: When property names differ, always declare `[MapProperty]`.
3. **Handle Nullability Explicitly**: Use `[MapNullFallback]` whenever the source member is nullable and destination is non-nullable.
4. **Use Strong Types for Identifiers**: Map strongly-typed IDs directly using `EricksonLopez.Mapper.DomainPrimitives`.
5. **Enforce Zero Allocations in Unit Tests**: Use `MapperContractExtensions.AssertZeroAllocations` in regression tests.
