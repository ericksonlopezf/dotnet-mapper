# Performance Invariants & Benchmark Metrics

## 1. Zero Overhead Principle
`EricksonLopez.Mapper` produces assembly code structurally identical to handwritten C# property assignments.

## 2. Inlining & Register Allocation
Generated mapping methods are small and linear, allowing RyuJIT to inline mapping calls at the call site and allocate struct and primitive parameters directly into CPU registers (`RCX`, `RDX`, `R8`, `R9`).
