# ADR 008: Testing Metrics and Mutation Score Exclusions

## Status
Accepted

## Date
2026-09-04

## Context
The framework aims for 100% test coverage across Line, Branch, Method, and Mutation metrics. While `EricksonLopez.Mapper.Abstractions` achieves full compliance, the compiler-integrated components (Roslyn Analyzers and Source Generators) present specific practical challenges during automated mutation testing via Stryker.

Specifically:
1. **Analyzers**: Mutation testing environments like `Microsoft.CodeAnalysis.Testing` create in-memory, dynamic compilation contexts. While Stryker correctly mutates the source code, the framework struggles to orchestrate these specific runtime compilation contexts effectively, resulting in the survival of "false positive" mutants.
2. **Generators**: Stryker successfully generates the 1231 mutants for `MapperGenerator`. However, certain mutations applied to the generator's source code provoke non-terminating executions (infinite loops) when the modified generator is processed by Roslyn's `CSharpGeneratorDriver`. Because these tests execute the generator within the compilation pipeline, the process block prevents Stryker from normally completing the mutant's evaluation. The `ObjectDisposedException` and subsequent errors observed during the Test Runner termination are side effects of the teardown process for a heavily blocked session.
3. **Branch Coverage**: `MapperGenerator` achieves 88.2% branch coverage. The remaining unvisited branches are inaccessible compiler-injected logic that Roslyn emits for C# `switch` pattern matching expressions and `SpecialType` definitions. 

## Decision
1. **Mutation Score Exclusions**: We formally exclude `EricksonLopez.Mapper.Analyzers` and `EricksonLopez.Mapper.Generator` from the strict 100% Mutation Score requirement. The Stryker results for the generator must be interpreted by separating the correctly evaluated mutants from those that cause non-terminating executions.
2. **Branch Coverage Exclusions**: We accept 88.2% as the mathematical maximum for branch coverage in `MapperGenerator`, given that the remaining branches are compiler-injected artifacts rather than framework logic.

## Consequences
- **Positive**: We acknowledge the practical limitations of mutation testing on Source Generator architectures without incorrectly attributing the failure to a Roslyn bug or an inevitable Stryker flaw.
- **Negative**: The existing Snapshot Tests (`VerifyXunit`) and the 99.02% code coverage provide strong evidence of the generator's functional correctness and coverage, but they do not substitute mutation testing nor allow us to conclude on their own that all possible mutations would be detected. Any changes in the generator must be carefully reviewed manually.
