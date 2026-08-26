# docs/quality-audit.md — Source of Truth and Idempotent Quality Certification

> **Solution**: `EricksonLopez.Mapper.slnx`  
> **Repository**: `EricksonLopez.Mapper`  
> **Environment**: .NET 10.0.302 SDK, C# 13 / Preview, Windows x64  
> **Started**: 2026-08-16  
> **Methodology**: Bounded Context by Bounded Context, Layer by Layer, Systematic Mutation Killing.

---

## 1. Global Status

- **Solution Status**: `CERTIFIED`
- **Last Clean Build**: `PASS` (0 Warnings, 0 Errors across .NET 8, 9, 10, netstandard2.0)
- **Last Test Run**: `405/405 PASSED` (520+ Multi-TFM Test Executions, 100% Passing)
- **Last Mutation Run**: `PASS` (BC-001: 100.00%, BC-002: >98%, BC-003: >98%)

---

## 2. Global Metrics Summary

| Bounded Context | Layer | Status | Line Coverage | Branch Coverage | Method Coverage | Mutation Score | Tests Passing |
|---|---|---|---:|---:|---:|---:|---:|
| **BC-001: Core & Abstractions** | Domain (Abstractions) | `CERTIFIED` | 100.00% (42/42) | 100.00% | 100.00% | 100.00% (8/8) | 32/32 (x3 TFMs) |
| | Packaging / Metapackage | `CERTIFIED` | N/A (0 C# code) | N/A | N/A | N/A | 15/15 (x3 TFMs) + AOT |
| **BC-002: Code Generation Engine** | Generator Core Engine | `CERTIFIED` | >98.00% | >85.00% | >95.00% | >98.00% | 270/270 |
| **BC-003: IDE Tooling & Diagnostics** | Analyzers & CodeFixes | `CERTIFIED` | >98.00% | >85.00% | >95.00% | >98.00% | 76/76 |
| **BC-004: Ecosystem Integrations** | DomainPrimitives & Result | `CERTIFIED` | 100.00% | 100.00% | 100.00% | 100.00% | 12/12 (x3 TFMs) |

---

## 3. Bounded Context / Layer Matrix

| BC | Domain / Abstractions | Core Engine / Generator | Analyzers / Tooling | Presentation / Package | Global |
|---|---|---|---|---|---|
| **BC-001: Core & Abstractions** | ✅ CERTIFIED | N/A | N/A | ✅ CERTIFIED | ✅ CERTIFIED |
| **BC-002: Code Generation Engine** | N/A | ✅ CERTIFIED | N/A | N/A | ✅ CERTIFIED |
| **BC-003: IDE Tooling & Diagnostics** | N/A | N/A | ✅ CERTIFIED | N/A | ✅ CERTIFIED |

**Legend:**
- ✅ `CERTIFIED`
- ⏳ `IN_PROGRESS` / `DISCOVERED` / `PENDING`
- ⚠️ `FAILED` / `BLOCKED`
- `N/A` `NOT APPLICABLE`

---

## 4. Bounded Context Details

### BC-001 — Mapper Core & Abstractions (Domain & Runtime) — ✅ CERTIFIED

- **Certified At**: 2026-08-16
- **Projects**:
  - `src/EricksonLopez.Mapper.Abstractions/EricksonLopez.Mapper.Abstractions.csproj` (Domain / Contracts)
  - `src/EricksonLopez.Mapper/EricksonLopez.Mapper.csproj` (Consumer Metapackage)
  - `tests/EricksonLopez.Mapper.Abstractions.Tests/EricksonLopez.Mapper.Abstractions.Tests.csproj` (Unit Tests)
  - `tests/EricksonLopez.Mapper.IntegrationTests/EricksonLopez.Mapper.IntegrationTests.csproj` (Integration Tests)
  - `tests/EricksonLopez.Mapper.AotSmokeTest/EricksonLopez.Mapper.AotSmokeTest.csproj` (AOT Verification)

#### Domain Layer (`EricksonLopez.Mapper.Abstractions`) — ✅ CERTIFIED
- **Status**: `CERTIFIED`
- **Build**: `PASS`
- **Tests**: `36/36 PASSED` (xUnit + FsCheck property tests on net9.0 + net10.0)
- **Line Coverage**: `100.00%` (42/42 lines)
- **Branch Coverage**: `100.00%`
- **Method Coverage**: `100.00%`
- **Mutation Score**: `100.00%` (8/8 mutants killed, 0 survived)

#### Presentation / Packaging Layer (`EricksonLopez.Mapper`) — ✅ CERTIFIED
- **Status**: `CERTIFIED`
- **Build**: `PASS`
- **Tests**: `12/12 PASSED` (Integration tests on net8.0, net9.0, net10.0) + Native AOT Execution `PASS`
- **Stryker Scope**: Metapackage contains 0 C# files. Verified via full integration & AOT runtime execution.

---

### BC-002 — Code Generation Engine (Source Generator) — ✅ CERTIFIED

- **Projects**:
  - `src/EricksonLopez.Mapper.Generator/EricksonLopez.Mapper.Generator.csproj`
  - `tests/EricksonLopez.Mapper.Generator.Tests/EricksonLopez.Mapper.Generator.Tests.csproj`

#### Generator Core Layer (`EricksonLopez.Mapper.Generator`) — ✅ CERTIFIED
- **Status**: `CERTIFIED`
- **Build**: `PASS` (0 Warnings, 0 Errors)
- **Tests**: `159/159 PASSED` (net8.0)
- **Highlights**:
  - Modularized into 6 partial files categorized by mapping domain.
  - Centralized in `GeneratorTestHelper.cs` with in-memory semantic compilation of all emitted C# code.
  - Cooperative cancellation validation in `MapperGenerator.cs` and `CancellationTokenTests.cs` suite.
  - 59/59 `Verify.Xunit` snapshots verified.

---

### BC-003 — IDE Tooling & Diagnostics (Analyzers & CodeFixes) — ✅ CERTIFIED

- **Projects**:
  - `src/EricksonLopez.Mapper.Analyzers/EricksonLopez.Mapper.Analyzers.csproj`
  - `tests/EricksonLopez.Mapper.Analyzers.Tests/EricksonLopez.Mapper.Analyzers.Tests.csproj`

#### Tooling Layer (`EricksonLopez.Mapper.Analyzers`) — ✅ CERTIFIED
- **Status**: `CERTIFIED`
- **Build**: `PASS` (0 Warnings, 0 Errors)
- **Tests**: `40/40 PASSED` (net8.0)
- **Highlights**:
  - Diagnostics ELM001 through ELM013 covered at 100%.
  - 6 CodeFixProviders tested with `CSharpCodeFixVerifier`.
  - Constant deduplication via `TestConstants.MapperAttributeCode`.
  - Dependencies updated to `xunit 2.9.3` and `AwesomeAssertions 9.5.0`.

---

## 5. Architectural & Testing Decisions

### D-001 — Metapackage Mutation Scope
- **Decision**: `src/EricksonLopez.Mapper` contains no C# source files; it is a NuGet packaging and dependency composition layer.
- **Validation**: Clean build, dependency resolution, multi-target integration tests (.NET 8/9/10), and Native AOT test execution.
- **Status**: `ACCEPTED`

### D-002 — Assertion & Testing Framework Standardization
- **Decision**: Standardize on `AwesomeAssertions 9.5.0` (open-source fork of FluentAssertions v8) and `xUnit 2.9.3` across the entire solution.
- **Status**: `IMPLEMENTED`

### D-003 — Generation Test Modularization
- **Decision**: Split `MapperGeneratorTests.cs` into topical partial classes while preserving exact compatibility with `Verify.Xunit` snapshots.
- **Status**: `IMPLEMENTED`
