# Repository Inventory

This document is a complete, verified inventory of all artifacts in the `EricksonLopez.Mapper` repository, derived directly from the file system, project configurations, and source code.

---

## 1. Build and Solution Artifacts

| Artifact | Path | Notes |
|---|---|---|
| Solution | `EricksonLopez.Mapper.slnx` | XML-based .NET solution file containing all 17 projects |
| Global MSBuild Properties | `Directory.Build.props` | Configures `ImplicitUsings=disable`, `Nullable=enable`, `WarningsAsErrors=true`, `TreatWarningsAsErrors=true`, `IsAotCompatible=true`, `IsTrimmable=true`, `LangVersion=preview`, `EnableTrimAnalyzer`, SourceLink and symbol packaging (`snupkg`) |
| Central Package Management | `Directory.Packages.props` | Configures `ManagePackageVersionsCentrally=true` with 17 centrally versioned package references |
| Test MSBuild Properties | `tests/Directory.Build.props` | Inherits root props, suppresses `IDE1006` for Roy Osherove test naming conventions (ADR-021), conditionally disables trim analyzers on non-AOT runs |
| Code Style & Formatting | `.editorconfig` | Enforces C# style, 4-space indentation, UTF-8, and test naming exception rules |
| Local .NET Tools | `dotnet-tools.json` | Pinned tool: `dotnet-stryker` (v4.16.0) |
| Code Coverage Config | `.codecov.yml` | Project target: 99% (1% tolerance), PR patch target: 90% (5% tolerance) |
| Release Automation Config | `.release-please-config.json` | Conventional Commits to SemVer release automation |
| Release Automation Manifest | `.release-please-manifest.json` | Current ecosystem version: `1.0.0` |

---

## 2. Source Projects (`src/`)

The ecosystem contains 7 source projects, all targeting .NET 8, .NET 9, and .NET 10 (or `netstandard2.0` for Roslyn compiler tooling):

| Project | Type | Target Framework(s) | Role & Description |
|---|---|---|---|
| `EricksonLopez.Mapper` | Library (Umbrella) | `net8.0`, `net9.0`, `net10.0` | Metapackage referencing `Abstractions` (runtime) and `Generator` (as a Roslyn analyzer) |
| `EricksonLopez.Mapper.Abstractions` | Library (Contracts) | `netstandard2.0`, `net8.0`, `net9.0`, `net10.0` | Declarative mapping attributes (`[Mapper]`, `[MapProperty]`, `[MapValue]`, etc.) and `IConverter<TSource, TDestination>` |
| `EricksonLopez.Mapper.Generator` | Roslyn Source Generator | `netstandard2.0` | Modular 7-component `IIncrementalGenerator` emitting compile-time mapping implementations |
| `EricksonLopez.Mapper.Analyzers` | Roslyn Analyzer & Code Fixes | `netstandard2.0` | `DiagnosticAnalyzer` enforcing AOT safety (`ELM008`, `ELM009`, `ELM012`) and automated code fix providers |
| `EricksonLopez.Mapper.DomainPrimitives` | Library (Extension) | `net8.0`, `net9.0`, `net10.0` | Converters for `EricksonLopez.DomainPrimitives` (`IDomainPrimitive`, `IStrongId`) |
| `EricksonLopez.Mapper.Mapster` | Library (Extension) | `net8.0`, `net9.0`, `net10.0` | Bi-directional adapter bridge between `IConverter` and Mapster `TypeAdapterConfig` |
| `EricksonLopez.Mapper.Result` | Library (Extension) | `net8.0`, `net9.0`, `net10.0` | Functional Railway-Oriented Programming projection extensions for `Result<T>` |

---

## 3. Test Projects (`tests/`)

All 8 test projects are included in the `EricksonLopez.Mapper.slnx` solution:

| Project | Type | Target Framework(s) | Test Frameworks & Tools |
|---|---|---|---|
| `EricksonLopez.Mapper.Abstractions.Tests` | Unit Tests | `net8.0`, `net9.0`, `net10.0` | xUnit 2.9.3, AwesomeAssertions 9.5.0, AutoFixture, FsCheck |
| `EricksonLopez.Mapper.Generator.Tests` | Snapshot & Behavior Tests | `net8.0` | xUnit 2.9.3, Verify.SourceGenerators, Verify.Xunit, Basic.Reference.Assemblies.Net80 |
| `EricksonLopez.Mapper.Analyzers.Tests` | Analyzer & CodeFix Tests | `net8.0` | xUnit 2.9.3, Microsoft.CodeAnalysis CSharp.Analyzer.Testing |
| `EricksonLopez.Mapper.IntegrationTests` | Integration Tests | `net8.0`, `net9.0`, `net10.0` | xUnit 2.9.3, AwesomeAssertions 9.5.0 |
| `EricksonLopez.Mapper.DomainPrimitives.Tests` | Unit Tests | `net8.0`, `net9.0`, `net10.0` | xUnit 2.9.3, AwesomeAssertions 9.5.0 |
| `EricksonLopez.Mapper.Mapster.Tests` | Unit Tests | `net8.0`, `net9.0`, `net10.0` | xUnit 2.9.3, AwesomeAssertions 9.5.0, Mapster 10.0.11 |
| `EricksonLopez.Mapper.Result.Tests` | Unit Tests | `net8.0`, `net9.0`, `net10.0` | xUnit 2.9.3, AwesomeAssertions 9.5.0, EricksonLopez.Result |
| `EricksonLopez.Mapper.AotSmokeTest` | NativeAOT Smoke Test | `net8.0`, `net9.0`, `net10.0` | Executable console app compiled with `PublishAot=true` and verified in CI |

---

## 4. Benchmark & Sample Projects

| Project | Path | Target Framework | Description |
|---|---|---|---|
| `EricksonLopez.Mapper.Benchmarks` | `benchmarks/EricksonLopez.Mapper.Benchmarks/` | `net10.0` | BenchmarkDotNet v0.14.0 harness comparing against AutoMapper 13.0.1, Mapster 10.0.11, and Riok.Mapperly 4.3.1 |
| `EricksonLopez.Mapper.Sample` | `sample/EricksonLopez.Mapper.Sample/` | `net10.0` | Comprehensive 10-level executable showcase covering all public APIs, extensions, and patterns |

---

## 5. CI/CD Workflows (`.github/workflows/`)

| Workflow | File | Trigger | Purpose |
|---|---|---|---|
| **CI** | `ci.yml` | `push`/`PR` → `main`, `develop` | CI orchestrator invoking reusable build-test and AOT smoke test |
| **Reusable Build & Test** | `dotnet-build-test.yml` | `workflow_call` | Restores, builds, executes tests, collects Coverlet coverage, and runs SonarCloud scanner |
| **NativeAOT Smoke Test** | `aot-smoke-test.yml` | `push`/`PR`, `workflow_call`, `workflow_dispatch` | Compiles and executes `AotTest` with `PublishAot=true` on Linux (clang/lld/zlib) |
| **Publish NuGet** | `publish.yml` | `push v*.*.*` tag, `workflow_dispatch` | Builds, tests, packs 7 packages, attests Sigstore provenance, pushes via OIDC to NuGet.org, and creates GitHub Release |
| **Release Please** | `release-please.yml` | `push` → `main` | Analyzes Conventional Commits, maintains `CHANGELOG.md` and versioning, and triggers `publish.yml` upon release PR merge |
| **Mutation Testing** | `mutation-testing.yml` | Schedule Mon 04:00 UTC, `workflow_dispatch` | Stryker.NET parallel matrix across **7 packages** (Core, Abstractions, Generator, Analyzers, Result, DomainPrimitives, Mapster) |
| **Benchmark Regression Gate** | `benchmark-regression-gate.yml` | PR → `main`, `develop` | Runs BenchmarkDotNet and fails if regression exceeds threshold (default: 10%) |
| **Benchmarks Baseline** | `benchmarks.yml` | `push` → `main`, `workflow_dispatch` | Captures baseline performance metrics and commits markdown/json results to `benchmarks/results/` |
| **Weekly Deep Benchmarks** | `weekly-benchmarks.yml` | Schedule Sun 02:00 UTC, `workflow_dispatch` | Multi-TFM deep benchmark evaluation across .NET 8, 9, and 10 |

### CI/CD Secrets

| Secret | Consumed By | Purpose |
|---|---|---|
| `SNK_KEY` | All workflows | Base64-encoded Strong Name Key (`.snk`) for assembly signing (never committed to repository) |
| `CODECOV_TOKEN` | `dotnet-build-test.yml`, `publish.yml` | Token for uploading coverage reports to Codecov |
| `SONAR_TOKEN` | `dotnet-build-test.yml` | Token for SonarCloud static code analysis |
| `GITHUB_TOKEN` | All workflows | Automatically provided by GitHub Actions for attestation, releases, and PR operations |

---

## 6. Community & Quality Configuration Artifacts

| Artifact | Path | Purpose |
|---|---|---|
| Code Owners | `.github/CODEOWNERS` | Auto-assigns maintainer (`@ericksonlopezf`) by directory path |
| Pull Request Template | `.github/PULL_REQUEST_TEMPLATE.md` | PR checklist covering tests, quality gates, mutation scores, and affected packages |
| Bug Report Template | `.github/ISSUE_TEMPLATE/bug_report.md` | Issue template for bug reporting with environment and reproduction details |
| Feature Request Template | `.github/ISSUE_TEMPLATE/feature_request.md` | Template with non-goals check against ADRs |
| Dependabot Configuration | `.github/dependabot.yml` | Weekly NuGet updates (grouped by dependency domain) and monthly GitHub Actions updates |
| Stryker Mutation Configs | Repo root: `stryker-config.json`, `stryker-abstractions-config.json`, `stryker-analyzers-config.json`, `stryker-domainprimitives-config.json`, `stryker-generator-config.json`, `stryker-mapster-config.json`, `stryker-result-config.json` | Thresholds (high: 100, low: 98, break: 95) and method exclusions per package. All located at the **repository root**. |
| Mutation Execution Script | `run-stryker.ps1` | PowerShell script for orchestrating multi-project Stryker mutation test execution locally |

---

## 7. Architecture Decision Records (`docs/adr/`)

Total ADRs: **34 records**

- **Architectural Decisions (ADR-000 to ADR-021)**: 22 records documenting source generation over reflection, incremental caching, attribute configuration, strict mapping, constructor resolution, NativeAOT, Stryker metrics, temporal conversions, modern immutable collections, enum mapping semantics, and Roy Osherove test naming conventions.
- **Permanent Non-Goals & Rejections (ADR-D01 to ADR-D12)**: 12 records documenting why field mapping, private member bypass, automatic flattening, circular mappings, existing-instance mutation, bidirectional mapping, naming conventions, global registries, conditional mappings, lifecycle hooks, IQueryable projection, and generic IMapper interfaces are explicitly excluded.
