# EricksonLopez.Mapper — Competitive Audit and Market Positioning

> **Classification**: Technical Specification and Competitive Analysis  
> **Date**: 2026-08-13  
> **Status**: Updated to BenchmarkDotNet v0.14.0 results  

---

## 1. Executive Summary

The .NET ecosystem has undergone a structural transformation with **Native AOT** becoming a first-class requirement in cloud-native microservices and high-throughput systems. Legacy runtime-reflection-based mapping libraries (such as AutoMapper) have turned into technical bottlenecks.

**EricksonLopez.Mapper** is positioned directly in the **compile-time source generation** space, competing directly with modern alternatives like *Riok.Mapperly* and the static source generation modes of *Mapster*.

### 1.1 Differentiation Space

Because both EricksonLopez.Mapper and Mapperly generate strongly typed C# code at compile time, **raw performance (latency and memory allocations) is statistically identical** to hand-written mapping code (verified via BenchmarkDotNet). Consequently, our primary architectural differentiation does not rely on claims of being magically faster than bare hardware, but rather on:

1. **Native Domain-Driven Design (DDD) Support**: Automatic, native handling of *Value Objects* and *Strongly Typed IDs* without requiring boilerplate custom converters.
2. **Compile-Time Strictness**: Absolute compile-time verification. Zero silent warnings; if an object mapping cannot be verified safely, compilation is blocked.
3. **Strict Domain Invariant Enforcement**: Complete prohibition of private constructor bypassing (unlike reflection-based mappers using `FormatterServices.GetUninitializedObject`). Explicit use of factories (`[MapFactory]`) is required.

---

## 2. Competitive Landscape Analysis

### 2.1 AutoMapper (v13.x)

*   **Mechanism**: Runtime Reflection + Expression Trees + Dynamic IL Emit
*   **Native AOT Compatibility**: ❌ Incompatible. Heavily relies on dynamic code generation and runtime type scanning.
*   **Trimming**: ❌ Requires extensive trimming descriptors and manual preservation.
*   **Performance vs EricksonLopez.Mapper**: **8.55× slower** (24.90 ns vs 2.80 ns on standard POCO mapping).
*   **Allocations**: High runtime overhead from execution pipelines and boxing.
*   **Conclusion**: Not recommended for Native AOT projects or modern high-throughput architectures. Serves as a legacy migration target.

### 2.2 Mapster (v7.x)

*   **Mechanism**: Dual-mode (Runtime IL Emit by default, optional compile-time Source Generator via MapsterGen).
*   **Native AOT Compatibility**: 🟡 Operates in Source Generator mode, but the runtime Fluent API model complicates strict AOT adoption.
*   **Performance vs EricksonLopez.Mapper**: **2.90× slower** in standard default mode (8.44 ns vs 2.80 ns on POCO mapping).
*   **Conclusion**: Extensive feature set (hooks, conditional mapping), but exhibits an identity tension between its dynamic runtime API and static code generation.

### 2.3 Riok.Mapperly (v4.x)

*   **Mechanism**: Pure Roslyn `IIncrementalGenerator`.
*   **Native AOT Compatibility**: ✅ 100% Native AOT.
*   **Performance vs EricksonLopez.Mapper**: **Statistical Tie** (Both achieve hand-written equivalent latency of ~2.80 ns).
*   **Differentiators for EricksonLopez.Mapper**:
    *   Dedicated semantic awareness for *Value Objects* in DDD. In Mapperly, wrapping/unwrapping a `UserId(Guid Value)` often requires manual converters. In EricksonLopez.Mapper, single-property and implicit operator records unpack automatically.
    *   Diagnostics: Mapperly defaults often emit warnings that permit compilation with partially mapped state (lax defaults), whereas EricksonLopez.Mapper enforces strictness by default.
    *   Explicit first-class support for *Domain Factories* (`[MapFactory]`).

---

## 3. Feature Comparison Matrix

### 3.1 Domain-Driven Design (DDD) Support

| Feature | EricksonLopez.Mapper | Riok.Mapperly | AutoMapper | Architectural Advantage |
|---|:---:|:---:|:---:|---|
| **Value Object Auto-Unwrap/Wrap** | 🟢 Native | 🔴 Manual | 🔴 Manual | **Core Differentiator**. Heuristic detection of value objects (records, implicit operators). |
| **Constructor Privacy Enforcement** | 🟢 Compile Error | 🟡 Warning | 🔴 Silent Bypass | Strict preservation of domain invariants. |
| **Domain Factory `[MapFactory]`** | 🟢 Supported | 🔴 Unsupported | 🟡 Partial | Invokes `Entity.Create(...)` factory methods rather than direct constructors. |

### 3.2 Collection Performance

| Feature | EricksonLopez.Mapper | Riok.Mapperly | AutoMapper | Architectural Advantage |
|---|:---:|:---:|:---:|---|
| **Pure Iterative Loops (No LINQ)** | 🟢 Guaranteed | 🟢 Mostly | 🔴 Frequent LINQ | Zero closure allocations during iteration. |
| **Collection Pre-Sizing** | 🟢 (e.g. `new List<T>(source.Count)`) | 🟢 Supported | 🔴 Infrequent | Prevents multiple internal array reallocations on large collections. |

### 3.3 Strictness and Nullability

| Feature | EricksonLopez.Mapper | Riok.Mapperly | AutoMapper | Architectural Advantage |
|---|:---:|:---:|:---:|---|
| **Mapping `T?` → `T` (without fallback)** | 🔴 Strict Compile Error | 🟡 Permissive Warning | ⚠️ Runtime Failure | Prevents masked null reference exceptions. Mandates explicit `[MapNullFallback]`. |

---

## 4. Competitive Conclusion

EricksonLopez.Mapper does not seek to reproduce dynamic DI-container-heavy workflows for legacy applications. Its mission is clear: **modern DDD architectures engineered for high performance and Native AOT compilation**.

By prioritizing the compile-time developer experience with a focused set of core attributes and eliminating value object boilerplate, the framework achieves the zero-overhead performance of hand-written C# mapping code while enforcing compile-time correctness.
