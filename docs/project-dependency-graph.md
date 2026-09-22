# Project Dependency Graph

This document provides a complete visual map of all project dependencies, build configurations, and solution boundaries within the `EricksonLopez.Mapper` repository.

---

## 1. Solution Architecture & Project Graph

```mermaid
graph TD
    subgraph "Consumer Code"
        App["Consumer Project (.csproj)"]
    end

    subgraph "Ecosystem Packages (src/)"
        CORE["EricksonLopez.Mapper\nnet8.0 | net9.0 | net10.0\n(Umbrella Metapackage)"]
        ABS["EricksonLopez.Mapper.Abstractions\nnetstandard2.0 | net8/9/10\n(Attributes & IConverter)"]
        GEN["EricksonLopez.Mapper.Generator\nnetstandard2.0\n(Roslyn Incremental Generator)"]
        ANA["EricksonLopez.Mapper.Analyzers\nnetstandard2.0\n(Roslyn Analyzers & CodeFixes)"]
        DP["EricksonLopez.Mapper.DomainPrimitives\nnet8.0 | net9.0 | net10.0\n(ValueObject / StrongId Converters)"]
        MAP["EricksonLopez.Mapper.Mapster\nnet8.0 | net9.0 | net10.0\n(Mapster Adapter Bridge)"]
        RES["EricksonLopez.Mapper.Result\nnet8.0 | net9.0 | net10.0\n(Result<T> Monad Extensions)"]
    end

    subgraph "Test Suite (tests/)"
        UT["EricksonLopez.Mapper.Abstractions.Tests\nnet8/9/10"]
        GT["EricksonLopez.Mapper.Generator.Tests\nnet8.0"]
        ANAT["EricksonLopez.Mapper.Analyzers.Tests\nnet8.0"]
        IT["EricksonLopez.Mapper.IntegrationTests\nnet8/9/10"]
        DPT["EricksonLopez.Mapper.DomainPrimitives.Tests\nnet8/9/10"]
        MAPT["EricksonLopez.Mapper.Mapster.Tests\nnet8/9/10"]
        REST["EricksonLopez.Mapper.Result.Tests\nnet8/9/10"]
        AOT["EricksonLopez.Mapper.AotSmokeTest\nnet8/9/10 (PublishAot=true)"]
    end

    subgraph "Benchmarks & Samples"
        BENCH["EricksonLopez.Mapper.Benchmarks\nnet10.0"]
        SAMPLE["EricksonLopez.Mapper.Samples\nnet10.0"]
    end

    %% Consumer references
    App -->|PackageReference| CORE
    App -.->|Optional Extension| DP
    App -.->|Optional Extension| MAP
    App -.->|Optional Extension| RES

    %% Internal package references
    CORE -->|ProjectReference (runtime)| ABS
    CORE -->|ProjectReference (Analyzer, PrivateAssets=all)| GEN
    DP -->|ProjectReference| ABS
    MAP -->|ProjectReference| ABS
    RES -->|ProjectReference| ABS

    %% Test project dependencies
    UT --> CORE
    GT --> CORE
    GT --> GEN
    ANAT --> ANA
    IT --> CORE
    IT -->|Analyzer| GEN
    DPT --> DP
    MAPT --> MAP
    REST --> RES
    AOT --> CORE
    AOT -->|Analyzer| GEN

    %% Benchmarks and Sample dependencies
    BENCH --> CORE
    BENCH -->|Analyzer| GEN
    SAMPLE --> CORE
    SAMPLE --> DP
    SAMPLE --> MAP
    SAMPLE --> RES
    SAMPLE -->|Analyzer| GEN
```

---

## 2. Central Package Management (CPM) References

All third-party NuGet package versions are centrally managed in `Directory.Packages.props`:

| Package ID | Central Version | Consumed By | Purpose |
|---|---|---|---|
| `Microsoft.CodeAnalysis.CSharp` | `5.9.0` | `Generator`, `Analyzers` | Roslyn compiler syntax and semantic model APIs |
| `Microsoft.CodeAnalysis.CSharp.Workspaces` | `5.9.0` | `Analyzers` | Roslyn workspace and CodeFixProvider APIs |
| `Microsoft.CodeAnalysis.Analyzers` | `5.9.0` | Build tooling | Roslyn diagnostic analyzer best practices |
| `Microsoft.CodeAnalysis.PublicApiAnalyzers` | `5.6.0` | `Abstractions` | Enforces `PublicAPI.Shipped.txt` public surface invariant |
| `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.XUnit` | `1.1.2-beta1.22271.1` | `Analyzers.Tests` | Roslyn analyzer unit test verification harness |
| `Microsoft.CodeAnalysis.CSharp.CodeFix.Testing.XUnit` | `1.1.2-beta1.22271.1` | `Analyzers.Tests` | Roslyn code fix unit test verification harness |
| `Microsoft.Extensions.DependencyInjection` | `10.0.11` | `Samples` | ASP.NET Core DI container abstractions |
| `Microsoft.SourceLink.GitHub` | `10.0.400` | All `src/` projects | SourceLink deterministic debugging metadata |
| `Microsoft.NET.Test.Sdk` | `18.9.0` | Test projects | MSBuild test execution target harness |
| `xunit` | `2.9.3` | Test projects | Primary testing framework (v2) |
| `xunit.v3` | `4.0.0` | Test projects | Next-generation xUnit v3 runner components |
| `xunit.runner.visualstudio` | `4.0.0` | Test projects | Visual Studio / VSTest test runner adapter |
| `coverlet.MTP` | `10.0.1` | Test projects | Multi-target coverage instrumentation |
| `coverlet.collector` | `10.0.1` | Test projects | Cross-platform XPlat code coverage data collector |
| `coverlet.msbuild` | `10.0.1` | Test projects | MSBuild coverage integration |
| `AwesomeAssertions` | `9.6.0` | Test projects | Fluent assertion library |
| `NSubstitute` | `6.2.0` | `Tests` | Dynamic mocking framework for unit testing |
| `AutoFixture` | `4.18.1` | `Tests` | Automated anonymous test fixture generation |
| `AutoFixture.Xunit2` | `4.18.1` | `Tests` | xUnit 2 data theory attribute integration |
| `FsCheck.Xunit` | `3.4.0` | `Tests` | Property-based testing support |
| `Verify.SourceGenerators` | `2.5.0` | `Generator.Tests` | Roslyn source generator snapshot verification |
| `Verify.Xunit` | `31.12.5` | `Generator.Tests` | Snapshot assertion engine for xUnit |
| `Basic.Reference.Assemblies.Net80` | `1.8.11` | `Generator.Tests` | In-memory reference metadata for Roslyn compilation tests |
| `BenchmarkDotNet` | `0.15.8` | `Benchmarks` | Benchmarking framework |
| `AutoMapper` | `16.2.0` | `Benchmarks` | Benchmark competitor baseline |
| `Mapster` | `10.0.12` | `Benchmarks`, `Mapster`, `Mapster.Tests` | Adapter bridge target & competitor baseline |
| `Riok.Mapperly` | `4.3.1` | `Benchmarks` | Source generator competitor baseline |
| `EricksonLopez.DomainPrimitives.Abstractions` | `2.0.0` | `DomainPrimitives` | Target abstractions for domain primitive mapping |
| `EricksonLopez.Result` | `2.0.0` | `Result`, `Result.Tests` | Target monad for functional railway mapping |

---

## 3. Solution File (`EricksonLopez.Mapper.slnx`) Coverage

All 17 projects in the repository are fully mapped in `EricksonLopez.Mapper.slnx`:

| Project Path | In Solution | Folder | Role |
|---|---|---|---|
| `src/EricksonLopez.Mapper.Abstractions/EricksonLopez.Mapper.Abstractions.csproj` | ✅ Yes | `/src/` | Library Contracts |
| `src/EricksonLopez.Mapper.Analyzers/EricksonLopez.Mapper.Analyzers.csproj` | ✅ Yes | `/src/` | Roslyn Analyzers |
| `src/EricksonLopez.Mapper.DomainPrimitives/EricksonLopez.Mapper.DomainPrimitives.csproj` | ✅ Yes | `/src/` | Extension Package |
| `src/EricksonLopez.Mapper.Generator/EricksonLopez.Mapper.Generator.csproj` | ✅ Yes | `/src/` | Source Generator |
| `src/EricksonLopez.Mapper.Mapster/EricksonLopez.Mapper.Mapster.csproj` | ✅ Yes | `/src/` | Extension Package |
| `src/EricksonLopez.Mapper.Result/EricksonLopez.Mapper.Result.csproj` | ✅ Yes | `/src/` | Extension Package |
| `src/EricksonLopez.Mapper/EricksonLopez.Mapper.csproj` | ✅ Yes | `/src/` | Umbrella Package |
| `samples/EricksonLopez.Mapper.Samples/EricksonLopez.Mapper.Samples.csproj` | ✅ Yes | `/samples/` | Showcase Application |
| `tests/EricksonLopez.Mapper.Analyzers.Tests/EricksonLopez.Mapper.Analyzers.Tests.csproj` | ✅ Yes | `/tests/` | Analyzer Tests |
| `tests/EricksonLopez.Mapper.AotSmokeTest/EricksonLopez.Mapper.AotSmokeTest.csproj` | ✅ Yes | `/tests/` | NativeAOT Smoke Test |
| `tests/EricksonLopez.Mapper.DomainPrimitives.Tests/EricksonLopez.Mapper.DomainPrimitives.Tests.csproj` | ✅ Yes | `/tests/` | Extension Unit Tests |
| `tests/EricksonLopez.Mapper.Generator.Tests/EricksonLopez.Mapper.Generator.Tests.csproj` | ✅ Yes | `/tests/` | Generator Snapshot Tests |
| `tests/EricksonLopez.Mapper.IntegrationTests/EricksonLopez.Mapper.IntegrationTests.csproj` | ✅ Yes | `/tests/` | Integration Tests |
| `tests/EricksonLopez.Mapper.Mapster.Tests/EricksonLopez.Mapper.Mapster.Tests.csproj` | ✅ Yes | `/tests/` | Extension Unit Tests |
| `tests/EricksonLopez.Mapper.Result.Tests/EricksonLopez.Mapper.Result.Tests.csproj` | ✅ Yes | `/tests/` | Extension Unit Tests |
| `tests/EricksonLopez.Mapper.Abstractions.Tests/EricksonLopez.Mapper.Abstractions.Tests.csproj` | ✅ Yes | `/tests/` | Abstractions Unit Tests |
| `benchmarks/EricksonLopez.Mapper.Benchmarks/EricksonLopez.Mapper.Benchmarks.csproj` | ✅ Yes | `/benchmarks/` | Benchmarking Suite |
