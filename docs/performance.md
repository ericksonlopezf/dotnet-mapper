# Performance Invariants & Benchmark Metrics

> **Detailed Specifications:**
> - Performance Architecture: [`docs/performance-guide.md`](performance-guide.md)
> - Benchmark Evidence & Matrices: [`docs/benchmark-results.md`](benchmark-results.md)

---

## 1. Zero-Overhead Principle

`EricksonLopez.Mapper` generates pure C# code directly at compile time via Roslyn incremental generation. The emitted assembly code is structurally identical to handwritten property-by-property assignments, operating at the theoretical hardware speed limit of RyuJIT.

## 2. Inlining & Register Allocation

Generated mapping methods are linear and free of complex control-flow branches, allowing RyuJIT to inline mapping invocations directly at the call site. Positional constructor parameters and primitive values are allocated directly into CPU registers (`RCX`, `RDX`, `R8`, `R9`), eliminating stack frame overhead.

## 3. Benchmark Verification Gate

All performance claims are continuously asserted via BenchmarkDotNet in CI:
- **Zero Heap Allocations:** 0 Bytes allocated on standard POCO projections.
- **Latency Tie with Handwritten Code:** ~2.80 ns per object mapping.
- **5% Regression Threshold:** Monitored via `.github/workflows/benchmark-regression-gate.yml` and enforced by `scripts/verify-benchmark-gate.ps1`.
