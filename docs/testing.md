# Testing Strategy & Automated Quality Verification

> **Detailed Specifications:**
> - Testing Roadmap & Architecture: [`docs/testing-roadmap.md`](testing-roadmap.md)
> - Technical Testing Audit Report: [`docs/testing-audit-report.md`](testing-audit-report.md)
> - Enterprise Remediation Report: [`docs/remediation-report.md`](remediation-report.md)
> - Mutation Testing Scorecards: [`docs/mutation-score.md`](mutation-score.md)

---

## 1. Multi-Tiered Testing Pyramid

The testing strategy covers 5 specialized test tiers ensuring 100% functional, architectural, and security invariants:

1. **Roslyn Generator Snapshot & Semantic Tests** (`EricksonLopez.Mapper.Generator.Tests`): Automated tests validating emitted source code syntax trees, compilation models, diagnostics, and code fix providers via `Verify.SourceGenerators` snapshot testing.
2. **Roslyn Analyzer Diagnostic Tests** (`EricksonLopez.Mapper.Analyzers.Tests`): Roslyn testing harness test cases verifying compiler warnings, code fix actions, and diagnostic descriptors (`ELM008`, `ELM009`, `ELM012`).
3. **Integration & Polymorphism Tests** (`EricksonLopez.Mapper.IntegrationTests`): Validates runtime mapping correctness across complex polymorphic trees, collections, records, and multi-threaded stress conditions across .NET 8, 9, and 10.
4. **Domain Primitives & Result Tests** (`EricksonLopez.Mapper.DomainPrimitives.Tests`, `EricksonLopez.Mapper.Result.Tests`): Validates Tier-0 ecosystem interoperability and Railway-Oriented Programming contracts.
5. **Native AOT Smoke Tests** (`EricksonLopez.Mapper.AotSmokeTest`): Validates standalone single-file AOT executable compilation and runtime execution without xUnit reflection overhead.

---

## 2. Mutation Testing (Stryker.NET)

Mutation testing is enforced via a parallel matrix in `mutation-testing.yml` (weekly schedule + `workflow_dispatch`):

| Package | Config File | Test Project |
|---|---|---|
| Core (`EricksonLopez.Mapper`) | `stryker-config.json` | `EricksonLopez.Mapper.IntegrationTests` |
| Abstractions | `stryker-abstractions-config.json` | `EricksonLopez.Mapper.Abstractions.Tests` |
| Generator | `stryker-generator-config.json` | `EricksonLopez.Mapper.Generator.Tests` |
| Analyzers | `stryker-analyzers-config.json` | `EricksonLopez.Mapper.Analyzers.Tests` |
| DomainPrimitives | `stryker-domainprimitives-config.json` | `EricksonLopez.Mapper.DomainPrimitives.Tests` |
| Result | `stryker-result-config.json` | `EricksonLopez.Mapper.Result.Tests` |
| Mapster | `stryker-mapster-config.json` | `EricksonLopez.Mapper.Mapster.Tests` |

**Threshold Policy** (configured in each `stryker-*.json` at the repository root):
- **High (Target):** ≥100% mutation score.
- **Low (Warning):** ≥98% — surviving mutants are flagged for review.
- **Break (Hard Gate):** <95% — exits with non-zero code, blocking CI publication.
