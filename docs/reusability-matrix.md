# Cross-Project Reusability Matrix

| Component | Target Use Case | Reusability Scope |
|---|---|---|
| `IConverter<TSource, TDestination>` | Custom property translations | Universal across all assemblies |
| `[MapperDefaults]` | Global project conventions | Assembly / Class scope |
| `MapperContractExtensions` | Zero-allocation contract tests | Test project scope |
