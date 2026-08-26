# ADR-002: Incremental Generator Stability and Modular Architecture

**Status**: Accepted  
**Date**: 2026-08-13 (Updated 2026-08-15)
**Deciders**: EricksonLopez.Mapper Architecture Team

## Context

Roslyn source generators can cause severe IDE performance degradation and memory leaks when they propagate reference types (`ISymbol`, `Compilation`, `SyntaxNode`) across pipeline stages. The incremental generator pipeline uses value equality to determine whether re-execution is needed — if models are not properly equatable, every keystroke triggers full regeneration. Furthermore, monolithic generator files (>800 LOC) violate Single Responsibility Principle and impair maintainability.

## Decision

1. **Strict Value-Equatable Models**:
   - All models produced in the semantic transform phase MUST be immutable records with structural equality.
   - **`EquatableArray<T>`**: All collections in models use `EquatableArray<T>` to provide value equality via `SequenceEqual`. Plain arrays and `ImmutableArray<T>` (which use reference equality for `Equals`) are strictly forbidden across pipeline boundaries.
   - **Zero Roslyn Reference Leakage**: `ISymbol`, `Compilation`, `SyntaxNode`, and `Location` instances are NEVER stored in incremental pipeline models.

2. **Modular 7-Component SRP Architecture**:
   The generator codebase is strictly partitioned into single-responsibility components:
   - `MapperGenerator.cs`: Slim incremental generator orchestrator (<150 LOC) handling pipeline registration and source output.
   - `Models.cs`: Immutable value-equatable records and `EquatableArray<T>`.
   - `MemberResolutionEngine.cs`: Extraction, filtering, and case-insensitive matching of source and destination properties.
   - `ConversionStrategyFactory.cs`: Resolution of type conversion strategies (scalars, enums, dates, collections, converters, nested mappers).
   - `CycleDetector.cs`: Directed graph cycle detection across single and multi-step mapping chains.
   - `CodeEmitter.cs`: Deterministic C# code generation, indentation, null ternary guards, and collection builders.
   - `DependencyInjectionEmitter.cs`: Emits `AddGeneratedMappers` DI extension methods when `[assembly: GenerateMapperRegistration]` is present.

3. **Incremental Pipeline Entry Points**:
   - Class/Interface attribute: `context.SyntaxProvider.ForAttributeWithMetadataName("EricksonLopez.Mapper.MapperAttribute", ...)`
   - Assembly attribute: `context.SyntaxProvider.ForAttributeWithMetadataName("EricksonLopez.Mapper.GenerateMapperRegistrationAttribute", ...)`

## Pipeline Structure

```
ForAttributeWithMetadataName               ← Phase 1: Syntax filter (O(1) per keystroke)
    → GetSemanticTargetForGeneration       ← Phase 2: Semantic transform (ISymbol → Models)
        → TypeMapping (immutable record)   ← Phase 3: Cached model (EquatableArray inside)
            → CodeEmitter.GenerateSource   ← Phase 4: Deterministic code emission
```

## Consequences

### Positive
- Generator models are 100% incremental-safe with zero IDE memory leaks.
- 7 modular SRP components ensure high maintainability and testability.
- Fast keystroke response in Visual Studio, VS Code, and JetBrains Rider.

### Negative
- All diagnostic and metadata state must be serialized into value types before pipeline output.
