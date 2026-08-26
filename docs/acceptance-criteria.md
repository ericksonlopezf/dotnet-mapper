# Acceptance Criteria & Invariant Verification

> **Ecosystem:** `EricksonLopez.Mapper`  
> **Engineering Specification:** Target: .NET 10 / C# 14 · Architecture: Native AOT / Roslyn Generator  
> **Status:** 100% Verified against Automated Integration & Benchmark Test Suites

---

## 1. Functional Mapping Acceptance Criteria

### AC-01: Zero Allocation for Compile-Time Inlined Mappings
- **Requirement:** Direct object-to-object mappings (`User` $\rightarrow$ `UserDto`) must allocate **0 bytes** beyond the single target destination object instantiation itself on the managed GC heap.
- **Verification:** Test suites measuring `GC.GetAllocatedBytesForCurrentThread()` assert zero intermediate allocation delta.

### AC-02: Strict Nullability Invariance (C# NRT Safe)
- **Requirement:** Mapping a nullable source member (`string?`) to a non-nullable target (`string`) without an explicit fallback (`[MapNullFallback]`) must be rejected at compile time via diagnostic `ELM004`.
- **Verification:** Roslyn analyzer verification tests assert emission of `ELM004`.

### AC-03: Complete Polymorphic Branch Coverage
- **Requirement:** Abstract base classes decorated with polymorphic mapping rules must fail compilation with warning `ELM011` if any derived concrete subtype in the compilation lacks a matching `[MapDerived]` branch.
- **Verification:** Unit tests with polymorphic inheritance trees assert diagnostic warning emission.

### AC-04: Non-Allocating Enum Transformations
- **Requirement:** Enum-to-enum and enum-to-string mappings must compile into static C# switch expressions without calling `Enum.ToString()` or `Enum.Parse()`.
- **Verification:** Decompiled output verification and zero-allocation assertions.

---

## 2. Tooling & Quality Gate Acceptance Criteria

### AC-05: 100% Native AOT & Trimming Compatibility
- **Requirement:** The entire library and generated code must publish cleanly under `PublishAot=true` with zero trimming warnings (`IL2026`, `IL3050`).
- **Verification:** `EricksonLopez.Mapper.AotSmokeTest` executes under Native AOT executable runner.

### AC-06: Strict Code Analysis (TreatWarningsAsErrors)
- **Requirement:** 100% build pass rate under `TreatWarningsAsErrors=true` and `AnalysisLevel=latest-recommended`.
- **Verification:** Verified via CI/CD `repo-compliance.yml` and `scripts/verify-compliance.ps1`.
