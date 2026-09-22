# Level 0 — Conceptual Overview

> **Showcase Source:** [`Level0_Conceptual/ConceptualOverview.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level0_Conceptual/ConceptualOverview.cs)  
> **Complexity Level:** Conceptual / Architecture  
> **API Surface Covered:** Mental model, Roslyn Incremental Generation, Native AOT, ADR-D01, ADR-D05

---

## 1. What is the Library?

**EricksonLopez.Mapper** is a state-of-the-art incremental source generator (**Roslyn Incremental Source Generator**) for **.NET 8, .NET 9, and .NET 10**. It generates strongly typed, reflection-free, zero-allocation object mappers at compile time.

The emitted code is structurally identical to handwritten C# code: direct constructor calls and member assignments, without runtime dynamic intermediaries.

---

## 2. What Problem Does It Solve?

In enterprise .NET applications and cloud-native architectures:
1. **CPU and Memory Overhead:** Traditional reflection-based mappers (such as AutoMapper) consume up to 15% of CPU cycles and induce continuous Gen 0 GC pressure from boxing and late-bound instantiation (`Activator.CreateInstance`).
2. **Incompatibility with Native AOT:** Assembly trimming (*IL trimming*) and the absence of runtime JIT compilation cause dynamic mappers to fail fatally under Native AOT.
3. **Delayed Runtime Errors:** Misaligned, renamed, or missing property mappings are only discovered when a specific code path executes in production.

`EricksonLopez.Mapper` solves these three challenges by shifting all type analysis, member resolution, and mapping validation directly to **compile time**.

---

## 3. Why Does It Exist?

It exists to provide a .NET object mapping solution rooted in fundamental software engineering principles:
- **Zero-Allocation**: Allocates zero intermediate memory beyond the target instances requested by the caller.
- **Fail-Fast (Strict Compilation)**: If a destination property cannot be mapped safely, the compiler emits an immediate compilation error (`ELM001`).
- **Complete Transparency**: Developers can inspect, debug with breakpoints, and audit every line of generated C# code (`*.g.cs`).
- **Guaranteed Immutability ([ADR-D05](../adr/adr-d05-no-existing-instance-mapping.md))**: Does not mutate existing in-memory objects; encourages immutable records and parameterized primary constructors.

---

## 4. Advantages and Limitations

### Advantages
- ⚡ **Maximum Performance:** Throughput identical to handwritten code (30+ million ops/sec).
- 🛡️ **Compile-Time Safety:** Roslyn diagnostics (`ELM001`–`ELM018`) eliminate silent mapping bugs.
- 🚀 **100% Native AOT Ready:** Zero usage of `System.Reflection`, `System.Reflection.Emit`, or `Expression.Compile()`.
- 🔍 **Seamless Debuggability:** Supports step-by-step debugging directly inside generated source files.
- 🧩 **Integrated Ecosystem:** First-class support for Domain Primitives, Strongly-Typed IDs, and Railway-Oriented Programming (`Result<T>`).

### Limitations
- ❌ **Requires Compilation:** Cannot generate dynamic mapping profiles defined at runtime (e.g., loaded dynamically from JSON).
- ❌ **No In-Place Mutation (`Map(src, existingDest)`):** Intentionally unsupported ([ADR-D05](../adr/adr-d05-no-existing-instance-mapping.md)) to guarantee functional immutability.
- ❌ **Source Generator Familiarity:** Requires classes to be declared as `partial` and requires Roslyn-aware IDE tooling.

---

## 5. Comparison with Alternatives

| Dimension | AutoMapper (Legacy) | Mapster | Riok.Mapperly | EricksonLopez.Mapper |
|---|---|---|---|---|
| **Mechanism** | Reflection / Dynamic IL | IL Emit / Expression Trees | Roslyn Source Generator | **Roslyn Incremental Generator** |
| **Error Detection Time** | Runtime | Runtime | Compile Time | **Compile Time (`ELM001`–`ELM016`)** |
| **Native AOT Trimmable** | ❌ No | ⚠️ Partial | ✅ Yes | ✅ **100% Native & Verified** |
| **Startup Overhead** | High (type scanning) | Medium | 0 ms | **0 ms (Precompiled)** |
| **Mapping Allocations** | Constant Gen 0 | Low | Zero | **Zero (Zero-Alloc)** |
| **Domain Primitives Support** | Manual | Manual | Manual | ✅ **Native (`IStrongId`, etc.)** |
| **Result Pattern (ROP) Support** | None | None | None | ✅ **Native (`Result<T>`)** |
| **Bridge with Mapster** | None | N/A | None | ✅ **Official (`MapsterConverter`)** |
