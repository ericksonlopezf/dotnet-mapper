# Technical Debt Inventory & Quality Metrics

> **Classification:** Architectural Governance & Quality Audit  
> **Repository:** `ericksonlopez.dev/dotnet-mapper`  
> **Status:** Low Risk / Actively Managed

---

## 1. Quality Gate Metrics

| Dimension | Measured Value | Threshold | Status | Enforcing Mechanism |
|---|---|---|---|---|
| **Compiler Warnings** | 0 warnings | 0 (`TreatWarningsAsErrors=true`) | ✅ Zero Debt | Roslyn Compiler / MSBuild |
| **Obsolete API Usages** | 0 usages | 0 (`[Obsolete]` prohibited in `src/`) | ✅ Zero Debt | `verify-compliance.ps1` Gate 2 |
| **Canonical Copyright Headers** | 100% compliant | 100% required | ✅ Zero Debt | `verify-compliance.ps1` Gate 3 |
| **One Type Per File** | 100% compliant | 100% required | ✅ Zero Debt | `verify-compliance.ps1` Gate 4 |
| **Prohibited NoWarn Suppressions** | 0 violations | 0 violations allowed | ✅ Zero Debt | `verify-compliance.ps1` Gate 7 |
| **Native AOT Trimming Warnings** | 0 warnings | 0 warnings | ✅ Zero Debt | `EricksonLopez.Mapper.AotSmokeTest` |
| **Test Suite Results** | 480+ tests passing | 100% passing | ✅ Zero Debt | `dotnet test -c Release` across net8.0, net9.0, net10.0 |
| **Stryker Mutation Score** | Per-package (see `StrykerOutput/`) | High ≥100% / Low ≥98% / Break ≥95% | Tracked per CI run | Stryker.NET / `mutation-testing.yml` |

---

## 2. Identified Architectural Trade-Offs & Tracked Technical Debt

### TD-01: Strong-Name Key Filename Divergence in CI Script vs Props
- **Category:** Build & CI Consistency
- **Severity:** Low
- **Description:** `Directory.Build.props` searches for `EricksonLopez.snk` in the solution root, while `.github/workflows/aot-smoke-test.yml` (line 55) decodes the secret into `EricksonLopez.Mapper.snk`. Both files exist in the repository root for compatibility, ensuring builds succeed in all environments.
- **Remediation Plan:** Consolidate to a single canonical key filename (`EricksonLopez.snk`) across all CI workflows and MSBuild configuration.

### TD-02: Mapster Extension Bridge AOT Boundary
- **Category:** Architecture & Trimming Compatibility
- **Severity:** Low (Documented Architectural Constraint)
- **Description:** `EricksonLopez.Mapper.Mapster` integrates with Mapster v10, which inherently relies on runtime reflection, dynamic IL emission, and unreferenced code. Consequently, `EricksonLopez.Mapper.Mapster` sets `<IsAotCompatible>false</IsAotCompatible>` and `<IsTrimmable>false</IsTrimmable>` with `[RequiresUnreferencedCode]`.
- **Remediation Plan:** Maintain clear boundary documentation. The package is intended as an incremental migration bridge for legacy systems; pure Native AOT applications must use core `EricksonLopez.Mapper` and compile-time converters.

### TD-03: Stryker Mutation Execution Latency Across 7 Packages
- **Category:** CI/CD & Developer Feedback Loop
- **Severity:** Medium
- **Description:** Running full Stryker mutation analysis across all packable packages (`Abstractions`, `Generator`, `Analyzers`, `DomainPrimitives`, `Result`, `Mapster`, and Metapackage) requires significant execution time.
- **Remediation Plan:** Stryker execution is optimized with concurrency 2 and fast AST filters (`Category=FastAst`). Dedicated nightly or release-gate mutation triggers ensure pull requests maintain rapid feedback cycles without sacrificing coverage verification.

### TD-04: Documentation Stub Accuracy Gaps
- **Category:** Documentation Hygiene
- **Severity:** Low
- **Description:** Historical stub documents (`docs/cicd.md`, `docs/performance.md`, `docs/ci-cd-pipelines.md`, `docs/testing.md`) contained stale or inaccurate information: incorrect workflow file names, wrong benchmark regression thresholds, and incorrect mutation testing trigger descriptions.
- **Remediation Plan:** Corrected in v1.0.0 documentation audit. All stubs now accurately reflect the 10 actual workflow files, the 5% benchmark regression threshold, and the correct mutation testing schedule trigger.

### TD-05: Strong-Name Key Filename Divergence — Extended Scope
- **Category:** Build & CI Consistency
- **Severity:** Low
- **Description:** Extends TD-01. Not only `aot-smoke-test.yml` but also `benchmarks.yml` and `weekly-benchmarks.yml` decode the SNK secret into `EricksonLopez.Mapper.snk`, while `benchmark-regression-gate.yml` and `Directory.Build.props` correctly use `EricksonLopez.snk`. This creates two coexisting SNK file names in CI environments.
- **Remediation Plan:** Consolidate all 3 affected CI workflows to use the canonical filename `EricksonLopez.snk` in a future patch. The divergence is functional (both files exist in the root) but introduces maintenance risk.

