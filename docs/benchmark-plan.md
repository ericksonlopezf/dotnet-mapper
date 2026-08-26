# Benchmark Strategy & Performance Verification Plan

## 1. Scope & Objectives
The benchmark suite located in `benchmarks/EricksonLopez.Mapper.Benchmarks` measures execution throughput, memory allocations, and hardware performance counters across four industry-standard mapping frameworks:
1. **EricksonLopez.Mapper** (Roslyn Incremental Generator)
2. **Riok.Mapperly** (Source Generator)
3. **Mapster** (Compiled Expression Trees)
4. **AutoMapper** (Reflection & Dynamic IL)

---

## 2. Benchmark Scenarios

### Benchmark 01: Flat Primitive Object Mapping
- **Input:** 10 primitive properties (`Guid`, `string`, `int`, `decimal`, `DateTime`, `bool`).
- **Target:** Verify direct field assignments, register usage, and method inlining.

### Benchmark 02: Deep Nested Object Hierarchy
- **Input:** 3 levels of nested complex types (Order $\rightarrow$ Customer $\rightarrow$ Address $\rightarrow$ GeoCoordinates).
- **Target:** Measure nested mapping overhead and null-propagation branch efficiency.

### Benchmark 03: High-Volume Collection Projections
- **Input:** Collections of 100 and 10,000 items (`List<T>`, `T[]`).
- **Target:** Compare pre-sized loop allocation against LINQ `.Select().ToList()` allocations.

---

## 3. Harness Configuration
- **Runner:** BenchmarkDotNet v0.15.8
- **Job Configuration:** .NET 10.0 Native AOT vs JIT (RyuJIT AVX-512)
- **Diagnosers:** `MemoryDiagnoser`, `DisassemblyDiagnoser`
