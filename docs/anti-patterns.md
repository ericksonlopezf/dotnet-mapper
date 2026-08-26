# Architectural Anti-Patterns & Prohibited Practices

## 1. Overview
This document catalogs prohibited architectural antipatterns within the `EricksonLopez.Mapper` library ecosystem. These rules protect Native AOT performance, memory efficiency, and compile-time determinism.

---

## 2. Prohibited Antipatterns

### AP-01: Runtime Reflection for Object Instantiation
- **Antipattern:** Calling `Activator.CreateInstance()`, `FormatterServices`, or `Type.GetConstructor().Invoke()` in mapping routines.
- **Why Forbidden:** Reflection breaks Native AOT code stripping, disables RyuJIT method inlining, and incurs severe runtime overhead (up to 40x slower than `new T()`).
- **Enforcement:** Roslyn Analyzer rule `ELM008` (Error).

### AP-02: Use of Dynamic Typing (`dynamic` keyword)
- **Antipattern:** Declaring mapper parameters or return types as `dynamic`.
- **Why Forbidden:** The DLR requires runtime IL emission which crashes in AOT published binaries.
- **Enforcement:** Roslyn Analyzer rule `ELM009` (Error).

### AP-03: Unchecked Cyclic Object Graph Mapping
- **Antipattern:** Mapping bidirectional reference graphs (e.g. `Parent.Children[0].Parent`) without explicit depth limits or `[MapIgnore]`.
- **Why Forbidden:** Leads to unresolvable recursion and runtime `StackOverflowException`.
- **Enforcement:** Roslyn Generator cycle detector rule `ELM010` (Error).

### AP-04: Runtime Assembly Scanning for Mapping Profiles
- **Antipattern:** Using `Assembly.GetExecutingAssembly().GetTypes()` to discover mapping configurations at application startup.
- **Why Forbidden:** Causes cold-start startup latency spikes (50ms - 300ms) and fails unpredictably when types are trimmed.
- **Enforcement:** All mappers must be declared as static partial classes processed at compile time.
