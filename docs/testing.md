# Testing Strategy & Automated Quality Verification

## 1. Multi-Tiered Testing Strategy

1. **Roslyn Generator Snapshot & Semantic Tests** (`EricksonLopez.Mapper.Generator.Tests`): 516 automated tests validating emitted source code syntax trees, compilation models, diagnostics, and code fix providers.
2. **Roslyn Analyzer Diagnostic Tests** (`EricksonLopez.Mapper.Analyzers.Tests`): 79 automated Roslyn testing harness test cases verifying compiler warnings and code fix actions.
3. **Integration & Polymorphism Tests** (`EricksonLopez.Mapper.IntegrationTests`): Validates runtime mapping correctness across complex polymorphic trees, collections, and records.
4. **Domain Primitives & Result Tests** (`EricksonLopez.Mapper.DomainPrimitives.Tests`, `EricksonLopez.Mapper.Result.Tests`): Validates Tier-0 ecosystem interoperability.
5. **Native AOT Smoke Tests** (`EricksonLopez.Mapper.AotSmokeTest`): Validates single-file AOT executable compilation and runtime execution.
