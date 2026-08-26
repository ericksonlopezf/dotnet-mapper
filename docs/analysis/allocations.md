# Heap Allocation & GC Pressure Profile Analysis

Comprehensive allocation profiling and BenchmarkDotNet analysis for `EricksonLopez.Mapper`.

---

## 1. Zero Allocation Invariants

| Mapping Scenario | AutoMapper Allocations | Mapster Allocations | EricksonLopez.Mapper Allocations |
|---|---|---|---|
| Struct $\rightarrow$ Struct | 24 B (boxing) | 0 B | **0 B** |
| Class $\rightarrow$ Class | 48 B + instance | 48 B + instance | **Instance only** |
| Collection mapping (Span) | 64 B (IEnumerable) | 32 B | **0 B (Span-based)** |
