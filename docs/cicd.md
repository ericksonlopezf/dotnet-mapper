# CI/CD Architecture Specification

> **Detailed Specification:** See [`docs/ci-cd.md`](ci-cd.md) for full job topologies, secrets, and environment matrix definitions.

---

## 1. Pipeline Overview

Continuous integration and delivery in **EricksonLopez.Mapper** is automated via GitHub Actions across 10 specialized workflows:

1. **CI Orchestrator (`ci.yml`):** Orchestrates parallel `dotnet-build-test.yml` and `aot-smoke-test.yml` on push/PR to `main` and `develop`.
2. **Reusable Build & Test (`dotnet-build-test.yml`):** Restore → Build → Test → Coverage → SonarCloud analysis.
3. **NativeAOT Smoke Test (`aot-smoke-test.yml`):** Compiles and executes standalone AOT binary under Linux with `PublishAot=true`.
4. **Quality & Governance (`repo-compliance.yml`):** Enforces 8 zero-tolerance governance gates via `./scripts/verify-compliance.ps1`.
5. **Mutation Testing (`mutation-testing.yml`):** Weekly and manual Stryker.NET mutation analysis across 7 packages with parallel matrix.
6. **Benchmark Regression Gate (`benchmark-regression-gate.yml`):** PR gate asserting ≤5% latency regression vs. baseline in `benchmarks/results/`.
7. **Benchmarks Baseline (`benchmarks.yml`):** Captures and commits BenchmarkDotNet results to the branch.
8. **Weekly Deep Benchmarks (`weekly-benchmarks.yml`):** Full non-abbreviated BenchmarkDotNet run every Sunday at 02:00 UTC.
9. **Release Please (`release-please.yml`):** Conventional Commits analysis, automated Release PRs, and publish dispatch.
10. **Publish NuGet (`publish.yml`):** Packs all 7 packages, attests Sigstore provenance, and publishes to NuGet.org via OIDC.

---

## 2. Security & Supply Chain Integrity

- **NuGet Trusted Publishing (OIDC):** Uses GitHub Actions short-lived OIDC tokens instead of static API keys.
- **Sigstore Provenance:** Attests SLSA build provenance for every published NuGet package via `actions/attest-build-provenance@v2`.
- **Strong-Name Signing:** All assemblies are strong-name signed via `Directory.Build.props` when `EricksonLopez.snk` is present.
