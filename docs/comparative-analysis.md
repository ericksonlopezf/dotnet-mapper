# Comparative Analysis: EricksonLopez.Mapper vs Ecosystem

This document provides a comprehensive technical and architectural comparison of `EricksonLopez.Mapper` against the three primary .NET mapping libraries: **AutoMapper**, **Mapster**, and **Riok.Mapperly**.

---

## 1. Executive Summary

`EricksonLopez.Mapper` uses a Roslyn Incremental Source Generator to synthesize C# mapping implementations during compilation, placing it in the same compile-time category as **Mapperly**, while offering first-class DDD abstractions, explicit strict-mode invariants, and NativeAOT-first guarantees.

| Dimension | EricksonLopez.Mapper | Riok.Mapperly | Mapster | AutoMapper |
|---|---|---|---|---|
| **Architecture** | Roslyn Incremental Generator | Roslyn Incremental Generator | Dynamic Reflection / IL Emit | Dynamic Reflection / IL Emit |
| **Native AOT (`PublishAot=true`)** | ✅ 100% Compatible | ✅ 100% Compatible | ⚠️ Partial (Trimming issues) | ❌ Incompatible |
| **Zero Reflection Guarantee** | ✅ Enforced via `ELM008` | ✅ Compile-time only | ❌ Uses Reflection | ❌ Uses Reflection |
| **Startup Overhead** | ✅ 0 ns (Pure C#) | ✅ 0 ns (Pure C#) | ⚠️ Moderate (Profile build) | ❌ High (Assembly scan) |
| **Compile-Time Safety** | ✅ `ELM001`–`ELM016` | ✅ Roslyn Diagnostics | ❌ Runtime exceptions | ❌ Runtime exceptions |
| **Configuration Model** | Declarative Attributes | Partial methods + Attributes | Fluent API / TypeAdapter | Profile classes |
| **DDD Value Objects & Factories** | ✅ Native `[ValueObject]`, `[MapFactory]` | ⚠️ Custom converters | ⚠️ Custom configuration | ⚠️ Custom value converters |
| **DI Integration** | ✅ `[assembly: GenerateMapperRegistration]` | Manual registration | `AddMapster()` | `AddAutoMapper()` |

---

## 2. Performance Benchmark Comparison

> Data Source: `EricksonLopez.Mapper.Benchmarks` (BenchmarkDotNet v0.14.0, .NET 10.0, RyuJIT AVX-512, Windows 11).

### Simple POCO Mapping Latency

| Method | Mean Latency | Relative Ratio | Heap Allocations | Generation Strategy |
|---|---|---|---|---|
| **Manual Hand-Written C# (Baseline)** | **2.91 ns** | **1.00** | **32 B** | Direct property assignments |
| **EricksonLopez.Mapper** | **2.80 ns** | **0.96** | **32 B** | Roslyn Incremental Generator |
| **Riok.Mapperly** | **2.80 ns** | **0.96** | **32 B** | Roslyn Incremental Generator |
| **Mapster** | **8.44 ns** | **2.90** | **32 B** | Runtime Dynamic IL Emit |
| **AutoMapper** | **24.90 ns** | **8.55** | **32 B** | Runtime Expression Compilation |

**Key Observations:**
- Compile-time source generation achieves the theoretical physical minimum in execution latency.
- AutoMapper incurs ~8.5× latency overhead due to runtime dynamic dispatch and delegates.
- Mapster incurs ~2.9× latency overhead due to dynamic IL emit structures.

---

## 3. Detailed Axis-by-Axis Comparison

### 1. Native AOT & Linker Trimming

- **EricksonLopez.Mapper**: Built from the ground up for Native AOT. Produces zero trim warnings (`IL2026`, `IL3050`). Validated in CI via `aot-smoke-test.yml`.
- **Mapperly**: Also source-generated; fully AOT compatible.
- **Mapster**: Emits dynamic IL at runtime which fails under Ahead-of-Time compilation.
- **AutoMapper**: Relies heavily on `MakeGenericType`, `Activator.CreateInstance`, and reflection scanning; incompatible with NativeAOT.

### 2. Domain-Driven Design (DDD) & Invariant Safety

- **EricksonLopez.Mapper**: First-class support for `[ValueObject]` automatic wrap/unwrap, `[MapFactory]` for private constructors, and immutable collections (`ImmutableArray<T>`, `FrozenSet<T>`).
- **Competitors**: Require writing verbose custom converter boilerplate for every Value Object or domain entity with invariant rules.

### 3. Compile-Time Diagnostics vs. Production Faults

- **EricksonLopez.Mapper**: Missing properties, nullability mismatches, unmapped enum values, and cyclic dependencies fail compilation immediately (`ELM001`–`ELM016`).
- **AutoMapper / Mapster**: Discrepancies fail at runtime in production unless manually caught by startup validation suites.

---

## 4. Decision Matrix

| Choose EricksonLopez.Mapper When | Choose Competitors When |
|---|---|
| Deploying to NativeAOT (AWS Lambda, containers, low-memory environments) | Using an existing AutoMapper codebase where immediate migration is infeasible |
| Strict compile-time safety and zero-reflection invariants are mandatory | Heavy runtime dynamic query projection (`IQueryable.ProjectTo`) is required |
| Domain-Driven Design (Value Objects, Factory methods) is heavily used | Dynamic runtime-discovered mapping rules are required |
| Maximum throughput and zero framework allocations are required | Complex runtime heuristic string flattening is preferred over explicit attributes |
