# Contributing to EricksonLopez.Mapper

Thank you for contributing to `EricksonLopez.Mapper`! This guide provides all necessary instructions for setting up your local environment, building the solution, running tests, executing quality gates, and submitting pull requests.

---

## Code of Conduct

All contributors and maintainers are expected to abide by our [Code of Conduct](CODE_OF_CONDUCT.md) (Contributor Covenant v2.1).

---

## Local Development Setup

### Prerequisites

- [**.NET 10.0 SDK**](https://dotnet.microsoft.com/download/dotnet/10.0) (`10.0.x` or later — CI workflows pin to `10.0.x`)

  > **Note:** No `global.json` is present in this repository. Ensure your local SDK matches `10.0.x` or higher to avoid build compatibility issues. The `aot-smoke-test.yml` CI workflow intentionally uses `8.0.x` SDK to validate the `net8.0` TFM baseline but local development requires .NET 10.0.

- An IDE with Roslyn support: [Visual Studio 2022](https://visualstudio.microsoft.com/) (v17.10+), [JetBrains Rider](https://www.jetbrains.com/rider/), or [VS Code](https://code.visualstudio.com/) with C# Dev Kit
- For NativeAOT local validation (Linux/macOS or WSL2): `clang`, `lld`, `zlib1g-dev`

> **Note:** No Docker, databases, or external infrastructure services are required. The entire ecosystem is build-time only.

### Building the Solution

Build the complete solution using `Release` configuration:

```bash
dotnet build EricksonLopez.Mapper.slnx -c Release
```

> **Global Compiler Settings:** `Directory.Build.props` centrally enforces `Nullable=enable`, `TreatWarningsAsErrors=true`, `WarningsAsErrors=true`, `LangVersion=preview`, `IsAotCompatible=true`, and `IsTrimmable=true`.

---

## Running Tests

### Complete Test Suite

Run all test projects across all target frameworks:

```bash
dotnet test EricksonLopez.Mapper.slnx -c Release
```

The solution includes 8 test projects:

| Project | Test Scope | Target Framework(s) |
|---|---|---|
| `EricksonLopez.Mapper.Abstractions.Tests` | Core attribute semantics & converter contracts | `net8.0`, `net9.0`, `net10.0` |
| `EricksonLopez.Mapper.Generator.Tests` | Roslyn incremental generator snapshot verification | `net8.0` |
| `EricksonLopez.Mapper.Analyzers.Tests` | DiagnosticAnalyzers (`ELM008`, `ELM009`, `ELM012`) & CodeFixes | `net8.0` |
| `EricksonLopez.Mapper.IntegrationTests` | End-to-end mapping integration scenarios | `net8.0`, `net9.0`, `net10.0` |
| `EricksonLopez.Mapper.DomainPrimitives.Tests` | Value Object & Strong ID converter integration | `net8.0`, `net9.0`, `net10.0` |
| `EricksonLopez.Mapper.Mapster.Tests` | Mapster adapter bridge verification | `net8.0`, `net9.0`, `net10.0` |
| `EricksonLopez.Mapper.Result.Tests` | Result monad mapping extensions | `net8.0`, `net9.0`, `net10.0` |
| `EricksonLopez.Mapper.AotSmokeTest` | NativeAOT compilation and execution smoke test | `net8.0`, `net9.0`, `net10.0` |

---

## Quality Gates

### 1. NativeAOT Smoke Test

Validate that changes do not introduce reflection or trim warnings under NativeAOT compilation:

```bash
dotnet publish tests/EricksonLopez.Mapper.AotSmokeTest/EricksonLopez.Mapper.AotSmokeTest.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained \
  -p:PublishAot=true \
  -p:TreatWarningsAsErrors=true
```

### 2. Mutation Testing (Stryker.NET)

Stryker measures test effectiveness across the ecosystem. Local tools are managed via `dotnet-tools.json`:

```bash
# Restore local dotnet tools
dotnet tool restore

# Run Stryker for a specific package (config files are at repository root)
dotnet stryker --config-file stryker-generator-config.json   # Generator
dotnet stryker --config-file stryker-config.json              # Core
dotnet stryker --config-file stryker-abstractions-config.json # Abstractions

# Or execute the complete multi-project mutation test suite via PowerShell
pwsh ./run-stryker.ps1
```

**Mutation Thresholds** (configured in each `stryker-*.json` at repository root):
- **High (Target):** ≥100%
- **Low (Warning):** ≥98%
- **Break (CI Gate):** <95% (fails publish pipeline)

### 3. Benchmarks & Performance Verification

If modifying the generator or hot paths, run the benchmark suite to verify zero-overhead performance:

```bash
dotnet run --project benchmarks/EricksonLopez.Mapper.Benchmarks -c Release -- --filter "*" --job short
```

---

## Pull Request Guidelines

1. **Issue First**: Open an issue or discussion to align on intent before submitting large changes.
2. **Branching**: Branch off `develop` (or `main`) using descriptive branch names: `feature/name`, `fix/name`, or `docs/name`.
3. **Conventional Commits**: All commit messages must follow the [Conventional Commits](https://www.conventionalcommits.org/) standard (`feat:`, `fix:`, `docs:`, `perf:`, `refactor:`, `test:`, `chore:`). Breaking changes must use `feat!:` or include a `BREAKING CHANGE:` footer.
4. **Public API Tracking**: If modifying public APIs in `EricksonLopez.Mapper.Abstractions`, ensure `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt` are updated.
5. **Architectural Decisions**: If proposing structural changes, submit an Architecture Decision Record in `docs/adr/`.
6. **PR Checklist**: Review and complete all checkboxes in the [Pull Request Template](.github/PULL_REQUEST_TEMPLATE.md).
