# Packages & Central Package Management

`EricksonLopez.Mapper` is distributed as a suite of 7 specialized NuGet packages. Consumers typically install `EricksonLopez.Mapper` (the umbrella metapackage), which automatically references the core abstractions and registers the Roslyn source generator at compile time.

---

## 1. Produced Packages Overview

| Package ID | Source Project Path | Target Framework(s) | Role & Description |
|---|---|---|---|
| **`EricksonLopez.Mapper`** | `src/EricksonLopez.Mapper/` | `net8.0`, `net9.0`, `net10.0` | **Umbrella Metapackage.** References `Abstractions` (runtime) and `Generator` (as a build-time analyzer). Installing this package is sufficient for most application projects. |
| **`EricksonLopez.Mapper.Abstractions`** | `src/EricksonLopez.Mapper.Abstractions/` | `netstandard2.0`, `net8.0`, `net9.0`, `net10.0` | **Contracts & Attributes.** Contains `[Mapper]`, `[MapProperty]`, `[MapValue]`, `[MapIgnore]`, `[MapFactory]`, `[MapDerivedType]`, `[EnumMappingStrategy]`, and `IConverter<TSource, TDestination>`. |
| **`EricksonLopez.Mapper.Generator`** | `src/EricksonLopez.Mapper.Generator/` | `netstandard2.0` | **Roslyn Incremental Source Generator.** Packaged with `OutputItemType=Analyzer`, `ReferenceOutputAssembly=false`, and `PrivateAssets=all`. Operates purely at build time with zero runtime footprint. |
| **`EricksonLopez.Mapper.Analyzers`** | `src/EricksonLopez.Mapper.Analyzers/` | `netstandard2.0` | **Roslyn Diagnostic Analyzers & Code Fixes.** Enforces `ELM008` (no reflection), `ELM009` (no dynamic), `ELM012` (partial classes), and provides automated code fix providers. |
| **`EricksonLopez.Mapper.DomainPrimitives`** | `src/EricksonLopez.Mapper.DomainPrimitives/` | `net8.0`, `net9.0`, `net10.0` | **Domain Primitives Extension.** Pre-built converters for `IDomainPrimitive<TSelf, TValue>` and `IStrongId<TSelf, TValue>` from `EricksonLopez.DomainPrimitives`. |
| **`EricksonLopez.Mapper.Mapster`** | `src/EricksonLopez.Mapper.Mapster/` | `net8.0`, `net9.0`, `net10.0` | **Mapster Adapter Extension.** Bi-directional adapter bridge between `IConverter` and Mapster `TypeAdapterConfig`. |
| **`EricksonLopez.Mapper.Result`** | `src/EricksonLopez.Mapper.Result/` | `net8.0`, `net9.0`, `net10.0` | **Result Monad Extension.** Functional Railway-Oriented Programming projection extensions (`Map`, `MapAsync`, `MapList`) for `EricksonLopez.Result`. |

---

## 2. Package Dependency Graph

```mermaid
graph TD
    App[Consumer Project] -->|dotnet add package| CORE(EricksonLopez.Mapper)
    App -.->|Optional Extension| DP(EricksonLopez.Mapper.DomainPrimitives)
    App -.->|Optional Extension| MAP(EricksonLopez.Mapper.Mapster)
    App -.->|Optional Extension| RES(EricksonLopez.Mapper.Result)

    CORE -->|ProjectReference (runtime)| ABS[EricksonLopez.Mapper.Abstractions]
    CORE -->|ProjectReference (Analyzer, PrivateAssets=all)| GEN[EricksonLopez.Mapper.Generator]
    CORE -.->|Analyzer Only| ANA[EricksonLopez.Mapper.Analyzers]

    DP --> ABS
    MAP --> ABS
    RES --> ABS
```

- **Runtime Footprint**: Installing `EricksonLopez.Mapper` introduces only `EricksonLopez.Mapper.Abstractions` to the consumer runtime. The generator and analyzers are stripped from output binaries.
- **Zero Reflection & AOT Guarantee**: 4 of the 5 runtime packages (`EricksonLopez.Mapper`, `Abstractions`, `DomainPrimitives`, and `Result`) are compiled with `IsAotCompatible=true` and `IsTrimmable=true`. The `Generator` and `Analyzers` packages set `IsAotCompatible=false` and `IsTrimmable=false` **intentionally** — they target `netstandard2.0` and execute exclusively at build time inside the Roslyn compiler host.
- **Mapster Adapter Boundary (AOT-002)**: The `EricksonLopez.Mapper.Mapster` package explicitly sets `IsAotCompatible=false` and `IsTrimmable=false` because Mapster utilizes reflection and dynamic code internally. Its converters are annotated with `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`.

---

## 3. Central Package Management (CPM)

Dependency versions are declared centrally in `Directory.Packages.props`:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
  <ItemGroup>
    <!-- Core & Integrations -->
    <PackageVersion Include="EricksonLopez.DomainPrimitives.Abstractions" Version="2.0.0" />
    <PackageVersion Include="EricksonLopez.Result" Version="2.0.0" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="10.0.11" />
    <!-- SourceLink -->
    <PackageVersion Include="Microsoft.SourceLink.GitHub" Version="10.0.400" />
    <!-- Analyzers & Generators -->
    <PackageVersion Include="Microsoft.CodeAnalysis.CSharp" Version="5.9.0" />
    <PackageVersion Include="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="5.9.0" />
    <PackageVersion Include="Microsoft.CodeAnalysis.Analyzers" Version="5.9.0" />
    <PackageVersion Include="Microsoft.CodeAnalysis.PublicApiAnalyzers" Version="5.6.0" />
    <PackageVersion Include="Microsoft.CodeAnalysis.CSharp.Analyzer.Testing.XUnit" Version="1.1.2-beta1.22271.1" />
    <PackageVersion Include="Microsoft.CodeAnalysis.CSharp.CodeFix.Testing.XUnit" Version="1.1.2-beta1.22271.1" />
    <!-- Testing & Benchmarks -->
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="18.9.0" />
    <PackageVersion Include="xunit" Version="2.9.3" />
    <PackageVersion Include="xunit.v3" Version="4.0.0" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="4.0.0" />
    <PackageVersion Include="coverlet.MTP" Version="10.0.1" />
    <PackageVersion Include="coverlet.collector" Version="10.0.1" />
    <PackageVersion Include="coverlet.msbuild" Version="10.0.1" />
    <PackageVersion Include="AwesomeAssertions" Version="9.6.0" />
    <PackageVersion Include="NSubstitute" Version="6.2.0" />
    <PackageVersion Include="AutoFixture" Version="4.18.1" />
    <PackageVersion Include="AutoFixture.Xunit2" Version="4.18.1" />
    <PackageVersion Include="FsCheck.Xunit" Version="3.4.0" />
    <PackageVersion Include="Verify.SourceGenerators" Version="2.5.0" />
    <PackageVersion Include="Verify.Xunit" Version="31.12.5" />
    <PackageVersion Include="Basic.Reference.Assemblies.Net80" Version="1.8.11" />
    <!-- Benchmarks -->
    <PackageVersion Include="BenchmarkDotNet" Version="0.15.8" />
    <PackageVersion Include="AutoMapper" Version="16.2.0" />
    <PackageVersion Include="Mapster" Version="10.0.12" />
    <PackageVersion Include="Riok.Mapperly" Version="4.3.1" />
  </ItemGroup>
</Project>
```

---

## 4. Compatibility Matrix

| Feature / Platform | .NET 8.0 | .NET 9.0 | .NET 10.0 | NativeAOT | Trimming |
|---|---|---|---|---|---|
| `EricksonLopez.Mapper` | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| `EricksonLopez.Mapper.Abstractions` | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| `EricksonLopez.Mapper.Generator` | ✅ Yes (Roslyn 4.8+) | ✅ Yes | ✅ Yes | N/A (Build-time) | N/A (Build-time) |
| `EricksonLopez.Mapper.Analyzers` | ✅ Yes (Roslyn 4.8+) | ✅ Yes | ✅ Yes | N/A (Build-time) | N/A (Build-time) |
| `EricksonLopez.Mapper.DomainPrimitives` | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| `EricksonLopez.Mapper.Mapster` | ✅ Yes | ✅ Yes | ✅ Yes | ❌ No (Reflection) | ❌ No (Reflection) |
| `EricksonLopez.Mapper.Result` | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |

---

## 5. Symbol Packages & SourceLink

Every package build produces matching `.snupkg` symbol packages (configured via `<SymbolPackageFormat>snupkg</SymbolPackageFormat>` and `<IncludeSymbols>true</IncludeSymbols>` in `Directory.Build.props`). SourceLink integration (`<PublishRepositoryUrl>true</PublishRepositoryUrl>` and `<EmbedUntrackedSources>true</EmbedUntrackedSources>`) enables step-through debugging from consumer IDEs directly into source code.
