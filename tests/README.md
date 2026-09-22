# EricksonLopez.Mapper — Testing Architecture and Execution Guide

This directory contains the comprehensive automated test suite for the **EricksonLopez.Mapper** library.

---

## 1. Test Project Structure

```text
tests/
├── EricksonLopez.Mapper.Abstractions.Tests/  # Unit and property-based tests (Attributes, IConverter)
├── EricksonLopez.Mapper.Generator.Tests/     # Roslyn code generation, AST, algorithms, and snapshot tests
│   ├── Core/                                 # Base types (EquatableArray, Diagnostics, CancellationToken, Converters)
│   ├── Engine/                               # Engine tests (CycleDetector, CodeEmitter, MemberResolutionEngine, AttributeResolution)
│   ├── Features/                             # Feature tests (Collections, Constructors, Polymorphism, Nullability, MapValue, Models)
│   ├── Infrastructure/                       # Fixtures, TestDataBuilders, Normalizers, TestSnippets
│   ├── Strategies/                           # Conversion strategy tests (Exhaustive, Enums, Primitives, Collections)
│   └── Snapshots/                            # Module snapshots (Core, Collections, Constructors, Diagnostics, Polymorphism, SpecialTypes)
├── EricksonLopez.Mapper.Analyzers.Tests/     # DiagnosticAnalyzer and 6 Roslyn CodeFixProvider tests
├── EricksonLopez.Mapper.IntegrationTests/    # End-to-end multi-target (.NET 8/9/10) and concurrency integration tests
│   ├── Fixtures/                             # Domain fixture modules (Orders, Products/Invoices, Animals, Customer/Branches)
│   ├── ConcurrentExecutionIntegrationTests.cs# Multi-threaded stress and thread-safety tests
│   ├── DependencyInjectionIntegrationTests.cs# ServiceCollection mapper resolution and lifecycle tests
│   ├── DomainMappingIntegrationTests.cs      # Complex domain mapping workflows
│   ├── EnumAndCollectionIntegrationTests.cs  # Enum and collection mapping workflows
│   └── PolymorphicMappingIntegrationTests.cs # Runtime polymorphic dispatch tests
├── EricksonLopez.Mapper.DomainPrimitives.Tests/ # Domain primitive value and strong ID converter tests
├── EricksonLopez.Mapper.Mapster.Tests/       # Mapster interoperability adapter tests
├── EricksonLopez.Mapper.Result.Tests/        # Result<T> functional mapping extension tests
└── EricksonLopez.Mapper.AotSmokeTest/        # Standalone Native AOT compilation smoke test executable
```

---

## 2. Test Execution

### Running the Entire Solution Suite
```bash
dotnet test --nologo
```

### Running a Specific Project
```bash
dotnet test tests/EricksonLopez.Mapper.Generator.Tests/EricksonLopez.Mapper.Generator.Tests.csproj
```

### Fast TDD Execution (< 2s Feedback Loop)
Tests are categorized with `[Trait("Category", "FastAst")]` and `[Trait("Category", "Snapshot")]`:
```bash
# Run only fast unit tests for AST, algorithms, and models (instant feedback):
dotnet test --filter "Category=FastAst"

# Run all tests excluding heavy end-to-end Roslyn compilation snapshots:
dotnet test --filter "Category!=Snapshot"
```

### Running by Name Pattern Filter
```bash
dotnet test --filter "FullyQualifiedName~CycleDetector"
```

### Concurrency and Parallelism Configuration (`xunit.runner.json`)
The suite uses a unified configuration in `tests/xunit.runner.json` (propagated to all projects via `tests/Directory.Build.props`):
- `parallelizeAssembly: true`: Enables concurrent execution across multiple test assemblies.
- `parallelizeTestCollections: true`: Executes test collections in parallel to maximize CPU throughput.
- `maxParallelThreads: -1`: Scales dynamically according to available CPU cores on local machines and CI/CD runners.

#### Stress Test Calibration (`ConcurrentExecutionIntegrationTests`)
Multi-threaded concurrency tests feature a deterministic 15-second timeout per test (`[Fact(Timeout = 15000)]`) and support the `MAPPER_CONCURRENCY_ITERATIONS` environment variable (default: `500` iterations). In resource-constrained CI or containerized environments (1 vCPU), calibrate dynamically:
```bash
export MAPPER_CONCURRENCY_ITERATIONS=100
dotnet test tests/EricksonLopez.Mapper.IntegrationTests/
```

---

## 3. Snapshot Testing with Verify.Xunit

The code generator uses **[Verify.Xunit](https://github.com/VerifyTests/Verify)** to guarantee that emitted C# source code and diagnostics are 100% deterministic and free of syntactic regressions.

- Snapshots are stored in `tests/EricksonLopez.Mapper.Generator.Tests/Snapshots/**/*.verified.txt`.
- When code emission logic is intentionally modified, tests fail displaying a diff between received output (`*.received.txt`) and expected baseline (`*.verified.txt`).

### Accepting Snapshot Updates
When generated code changes are intentional and verified:
1. Using the Verify CLI tool:
   ```bash
   dotnet tool install -g verify.terminal
   verify
   ```
2. Or copying `.received.txt` contents over to the corresponding `.verified.txt` files.

---

## 4. Mutation Testing with Stryker.NET

To verify assertion effectiveness and prevent surviving mutants:

### Unified Execution Script (`scripts/run-stryker.ps1`)
The repository includes `scripts/run-stryker.ps1` to orchestrate Stryker execution across all components using declarative thresholds:
```powershell
# Full local execution across all packages:
pwsh ./scripts/run-stryker.ps1

# Execution with custom configuration file or mutation level:
pwsh ./scripts/run-stryker.ps1 -Config "stryker-config.json" -MutationLevel "Standard"
```

### Component Quality Thresholds

| Component | Break | Low | High | Rationale |
| :--- | :--- | :--- | :--- | :--- |
| **Abstractions** | **90%** | 95% | 100% | Public attributes and interfaces; 100% mutation kill target |
| **Result** | **90%** | 95% | 100% | Functional extensions with 100% active mutant kills |
| **DomainPrimitives** | **90%** | 95% | 100% | Primitive converters with 100% active mutant kills |
| **Mapster** | **90%** | 95% | 100% | Interoperability adapter with 100% active mutant kills |
| **Analyzers** | **75%** | 80% | 90% | DiagnosticAnalyzer and 6 CodeFixProviders verified via Roslyn testing harness |
| **Generator** | **75%** | 80% | 90% | Baseline ~78% documented in ADR-008 due to Roslyn AST syntax traversal branches |

---

## 5. Target Framework Architecture (TFMs)

- **`EricksonLopez.Mapper.Abstractions`**: Multi-TFM (`netstandard2.0;net8.0;net9.0;net10.0`).
- **`EricksonLopez.Mapper`**: Multi-TFM (`net8.0;net9.0;net10.0`) supporting consumers on all modern .NET releases.
- **`EricksonLopez.Mapper.Abstractions.Tests` and `IntegrationTests`**: Compile and execute across all 3 TFMs (`net8.0`, `net9.0`, `net10.0`) to guarantee binary and runtime compatibility.
- **`EricksonLopez.Mapper.Generator` and `EricksonLopez.Mapper.Analyzers`**: Target `netstandard2.0` (Roslyn compiler host compatibility requirement). Test projects target `net8.0`.

---

## 6. Testing Conventions and Living Specifications (ADR-021)

1. **Roy Osherove Naming Pattern**: In accordance with **[ADR-021](../docs/adr/adr-021-test-naming-convention-and-ide1006.md)**, all test methods follow the three-part pattern:
   $$\textbf{UnitOfWork\_StateUnderTest\_ExpectedBehavior}$$
   *(e.g., `Generate_WhenSimpleProperties_ShouldCreateCorrectMapping`, `DetectCycles_WhenDirectSelfCycle_ShouldEmitDiagnostic`, `UseConverter_WhenConverterIsValid_ShouldApplyConverter`)*.
2. **Living Specifications and `.editorconfig` Naming Rules**: Tests act as human-readable executable specifications in CI/CD runners. `.editorconfig` defines custom naming rules for test methods to enforce consistency without suppressing analyzers globally.
3. **Assertions**: Standardized on `AwesomeAssertions` (`sut.Should().Be(...)`) across all test suites.
4. **AAA Structure**: Every test is structured into distinct `Arrange`, `Act`, and `Assert` phases.
5. **Semantic Verification**: Generator tests compile emitted code in memory (`verifyEmittedCodeCompiles = true`) to confirm zero C# compiler errors and warnings.
6. **Snapshot Determinism**: Strict newline normalization (`\r\n` / `\n`) and deterministic member sorting prevent cross-platform discrepancies between Windows, Linux, and macOS runners.

---

## 7. Native AOT Validation (`EricksonLopez.Mapper.AotSmokeTest`)

To validate zero-reflection compatibility with `PublishAot=true` and IL trimming (`PublishTrimmed=true`) without test runner reflection interference, `tests/EricksonLopez.Mapper.AotSmokeTest` executes as a standalone native binary in CI. See **[ADR-011](../docs/adr/adr-011-native-aot-strategy.md)**.
