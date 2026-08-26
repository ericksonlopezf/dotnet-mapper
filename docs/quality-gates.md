# Quality Gates & Code Analysis Methodology

`EricksonLopez.Mapper` implements multiple defensive layers of automated quality gates to guarantee zero-defect compilation, preserve 100% NativeAOT compatibility, and protect architectural invariants.

---

## 1. Mutation Testing: The Primary Verification Gate

While traditional line and branch coverage measure which instructions executed, **Mutation Testing** (via Stryker.NET) validates whether assertions verify correctness under intentional source code mutations.

### Stryker.NET Configuration Matrix

Mutation testing is executed across **7** core and extension packages in a parallel matrix:

| Target Package | Test Project | Config File (repo root) | Mutation Scope |
|---|---|---|---|
| `EricksonLopez.Mapper` (Core) | `EricksonLopez.Mapper.IntegrationTests` | `stryker-config.json` | Core mapping behavior |
| `EricksonLopez.Mapper.Abstractions` | `EricksonLopez.Mapper.Abstractions.Tests` | `stryker-abstractions-config.json` | Core attribute semantics |
| `EricksonLopez.Mapper.Generator` | `EricksonLopez.Mapper.Generator.Tests` | `stryker-generator-config.json` | Roslyn code synthesis pipeline |
| `EricksonLopez.Mapper.Analyzers` | `EricksonLopez.Mapper.Analyzers.Tests` | `stryker-analyzers-config.json` | Diagnostic rules & code fixes |
| `EricksonLopez.Mapper.Result` | `EricksonLopez.Mapper.Result.Tests` | `stryker-result-config.json` | Result monad projections |
| `EricksonLopez.Mapper.DomainPrimitives` | `EricksonLopez.Mapper.DomainPrimitives.Tests` | `stryker-domainprimitives-config.json` | Value object converters |
| `EricksonLopez.Mapper.Mapster` | `EricksonLopez.Mapper.Mapster.Tests` | `stryker-mapster-config.json` | Adapter bridge mappings |

### Threshold Policies

| Level | Threshold | Action |
|---|---|---|
| **High (Target)** | **≥100%** | Green build — complete mutant elimination. |
| **Low (Warning)** | **≥98%** | Yellow alert — investigate surviving mutants for assertion gaps. |
| **Break (Hard Gate)** | **<95%** | Red build — CI gate fails and blocks publish pipeline. |

### Method & Symbol Exclusions

Configured via `ignore-methods` in each `stryker-*.json` config at the repository root:
- `ConfigureAwait` — side-effect with no functional assertion value.
- `Dispose` — cleanup method with no return value assertion.

---

## 2. Roslyn Diagnostic Verification

Diagnostics are partitioned across two layers:

### Layer 1: Generator Compilation Gate (Errors & Warnings)

Emitted during compilation by `EricksonLopez.Mapper.Generator`:

| Diagnostic Code | Severity | Invariant Enforced |
|---|---|---|
| **`ELM001`** | Error | Strict Mapping: Destination property has no source match |
| **`ELM002`** | Error | Constructor Safety: Destination type lacks accessible constructor or factory |
| **`ELM003`** | Error | Type Safety: Unsupported type conversion |
| **`ELM004`** | Error | Null Safety: Nullable source assigned to non-nullable target without fallback |
| **`ELM005`** | Error | Ambiguity Safety: Multiple source properties match destination name |
| **`ELM006`** | Error | Construction Safety: Missing supported constructor |
| **`ELM007`** | Error | Disambiguation: Multiple constructors require explicit `[MapFactory]` |
| **`ELM010`** | Error | Recursion Safety: Circular mapping dependency detected |
| **`ELM011`** | Warning | Polymorphism Safety: Uncovered derived types on abstract base target |
| **`ELM013`** | Error | Converter Contract: Converter does not implement `IConverter<S, D>` |
| **`ELM014`** | Error / Warn | Enum Completeness: Unmapped enum member in strict mode |
| **`ELM015`** | Warning | Precision Safety: Narrowing numeric conversion potential data loss |
| **`ELM016`** | Warning | Runtime Parse Safety: String to enum mapping parsing risk |

### Layer 2: Roslyn Analyzer Rules (IDE & Build Invariants)

Emitted by `EricksonLopez.Mapper.Analyzers`:

| Rule Code | Severity | Description | Code Fix Provider |
|---|---|---|---|
| **`ELM008`** | Error | Prohibits `System.Reflection`, `Activator`, `Marshal`, `RuntimeHelpers` | Manual remediation |
| **`ELM009`** | Error | Prohibits C# `dynamic` keyword | Manual remediation |
| **`ELM012`** | Error | Requires `[Mapper]` classes to be declared `partial` | `MakePartialCodeFixProvider` |

---

## 3. Code Coverage Quality Gates

Collected via `coverlet.collector` (XPlat Code Coverage) across all test runs:

- **Config File**: `.codecov.yml`
- **Project Target**: `99%` (1% tolerance)
- **Patch Target**: `90%` (5% tolerance for new PR code)
- **Exclusions**: Test projects (`tests/**/*`), benchmarks (`benchmarks/**/*`), samples (`sample/**/*`), source generators, and `*.g.cs` files.

---

## 4. NativeAOT Compatibility Hard Gate

Enforced by `aot-smoke-test.yml` on every PR and commit to `main`/`develop`:

```bash
dotnet publish tests/EricksonLopez.Mapper.AotTest/EricksonLopez.Mapper.AotTest.csproj \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained \
  -p:PublishAot=true \
  -p:TreatWarningsAsErrors=true \
  -p:WarningLevel=5
```

- **Zero-Warning Enforcement**: `DOTNET_EnableAotCompilationWarningsAsErrors=true` turns any `IL2026` (RequiresUnreferencedCode) or `IL3050` (RequiresDynamicCode) trim warning into a fatal build error.
- **Physical Binary Execution**: The published native binary is executed on Linux (`ubuntu-latest`) asserting zero exit code.

---

## 5. Performance Regression Gate

Enforced by `benchmark-regression-gate.yml`:
- Compares PR BenchmarkDotNet results against baseline data in `benchmarks/results/`.
- Fails CI if any benchmark regresses by more than **10%** in execution latency or memory allocations.
