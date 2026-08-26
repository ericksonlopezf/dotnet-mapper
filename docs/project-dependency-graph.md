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
        SAMPLE["EricksonLopez.Mapper.Sample\nnet10.0"]
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
| `Microsoft.CodeAnalysis.CSharp` | `4.12.0` | `Generator`, `Analyzers` | Roslyn compiler syntax and semantic model APIs |
| `Microsoft.CodeAnalysis.CSharp.Workspaces` | `4.12.0` | `Analyzers` | Roslyn workspace and CodeFixProvider APIs |
| `Microsoft.CodeAnalysis.PublicApiAnalyzers` | `3.3.4` | `Abstractions` | Enforces `PublicAPI.Shipped.txt` public surface invariant |
| `Microsoft.Extensions.DependencyInjection` | `9.0.2` | `Sample` | ASP.NET Core DI container abstractions |
| `Microsoft.NET.Test.Sdk` | `17.14.1` | Test projects | MSBuild test execution target harness |
| `xunit` | `2.9.3` | Test projects | Primary testing framework |
| `xunit.runner.visualstudio` | `3.0.2` | Test projects | Visual Studio / VSTest test runner adapter |
| `coverlet.collector` | `6.0.4` | Test projects | Cross-platform XPlat code coverage data collector |
| `AwesomeAssertions` | `9.5.0` | Test projects | Fluent assertion library |
| `NSubstitute` | `6.1.0` | `Mapper.Tests` | Dynamic mocking framework for unit testing |
| `AutoFixture` | `4.18.1` | `Mapper.Tests` | Automated anonymous test fixture generation |
| `AutoFixture.Xunit2` | `4.18.1` | `Mapper.Tests` | xUnit 2 data theory attribute integration |
| `FsCheck.Xunit` | `3.3.4` | `Mapper.Tests` | Property-based testing support |
| `Verify.SourceGenerators` | `2.3.0` | `Generator.Tests` | Roslyn source generator snapshot verification |
| `Verify.Xunit` | `26.4.0` | `Generator.Tests` | Snapshot assertion engine for xUnit |
| `Basic.Reference.Assemblies.Net80` | `1.4.5` | `Generator.Tests` | In-memory reference metadata for Roslyn compilation tests |
| `BenchmarkDotNet` | `0.14.0` | `Benchmarks` | Benchmarking framework |
| `AutoMapper` | `13.0.1` | `Benchmarks` | Benchmark competitor baseline |
| `Mapster` | `10.0.11` | `Benchmarks`, `Mapster`, `Mapster.Tests` | Adapter bridge target & competitor baseline |
| `Riok.Mapperly` | `4.3.1` | `Benchmarks` | Source generator competitor baseline |
| `EricksonLopez.DomainPrimitives.Abstractions` | `1.0.0` | `DomainPrimitives` | Target abstractions for domain primitive mapping |
| `EricksonLopez.Result` | `1.0.0` | `Result`, `Result.Tests` | Target monad for functional railway mapping |

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
| `sample/EricksonLopez.Mapper.Sample/EricksonLopez.Mapper.Sample.csproj` | ✅ Yes | `/samples/` | Showcase Application |
| `tests/EricksonLopez.Mapper.Analyzers.Tests/EricksonLopez.Mapper.Analyzers.Tests.csproj` | ✅ Yes | `/tests/` | Analyzer Tests |
| `tests/EricksonLopez.Mapper.AotSmokeTest/EricksonLopez.Mapper.AotSmokeTest.csproj` | ✅ Yes | `/tests/` | NativeAOT Smoke Test |
| `tests/EricksonLopez.Mapper.DomainPrimitives.Tests/EricksonLopez.Mapper.DomainPrimitives.Tests.csproj` | ✅ Yes | `/tests/` | Extension Unit Tests |
| `tests/EricksonLopez.Mapper.Generator.Tests/EricksonLopez.Mapper.Generator.Tests.csproj` | ✅ Yes | `/tests/` | Generator Snapshot Tests |
| `tests/EricksonLopez.Mapper.IntegrationTests/EricksonLopez.Mapper.IntegrationTests.csproj` | ✅ Yes | `/tests/` | Integration Tests |
| `tests/EricksonLopez.Mapper.Mapster.Tests/EricksonLopez.Mapper.Mapster.Tests.csproj` | ✅ Yes | `/tests/` | Extension Unit Tests |
| `tests/EricksonLopez.Mapper.Result.Tests/EricksonLopez.Mapper.Result.Tests.csproj` | ✅ Yes | `/tests/` | Extension Unit Tests |
| `tests/EricksonLopez.Mapper.Abstractions.Tests/EricksonLopez.Mapper.Abstractions.Tests.csproj` | ✅ Yes | `/tests/` | Abstractions Unit Tests |
| `benchmarks/EricksonLopez.Mapper.Benchmarks/EricksonLopez.Mapper.Benchmarks.csproj` | ✅ Yes | `/benchmarks/` | Benchmarking Suite |
