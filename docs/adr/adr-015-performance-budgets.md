# ADR-015: Build-Time and Runtime Performance Budgets

**Status**: Accepted
**Date**: 2026-08-13
**Deciders**: EricksonLopez.Mapper Architecture Team

## Context

Source generators shift computation from runtime to compile time. This benefits runtime performance but can bloat the build process and IDE responsiveness if the generator is not carefully optimized. Both budgets must be explicitly defined and enforced.

## Decision

Two independent performance budgets are established:

### Runtime Budget

| Metric | Target |
|---|---|
| Temporary allocations per mapping | **0** (beyond the destination object itself) |
| Overhead vs. hand-written code | **< 1 ns** (statistically indistinguishable — verified by BenchmarkDotNet) |
| NativeAOT IL2026/IL3050 warnings | **0** |

Enforced by: BenchmarkDotNet benchmarks (`benchmarks.yml`, `weekly-benchmarks.yml`) and the `aot-smoke-test.yml` CI gate.

### Compiler / IDE Budget

| Metric | Target |
|---|---|
| Generator incremental re-run cost | Triggered **only** when semantic meaning of a mapping changes |
| Generator cold-run time | **< 50 ms** per complex mapping graph (incremental warmup) |
| Generated source size | Minimized — sub-methods reused rather than inlined for repeated patterns |

Enforced by: `IIncrementalGenerator` + `EquatableArray<T>` model equality (ADR-002), which prevents unnecessary downstream re-execution.

## Alternatives Considered

- **Ignoring compile-time performance:** Leads to a degraded IDE experience in Visual Studio/Rider when large solutions have many mapper classes. Rejected.
- **Trading runtime performance for generator simplicity (LINQ in emitted code):** Rejected for the runtime budget. See ADR-006.

## Consequences

### Positive
- A lightweight, professional tool that scales to enterprise solutions with hundreds of mapper classes.
- Runtime performance is equivalent to hand-written code — verifiable by any consumer with BenchmarkDotNet.

### Negative
- The Roslyn incremental pipeline requires significant investment to optimize correctly (see ADR-002).
