# Public API Surface Budget & Binary Footprint

## 1. Budget Limits & Allocation Metrics

To prevent framework bloat and preserve sub-millisecond cold starts, `EricksonLopez.Mapper` enforces strict public API surface and binary size limits:

| Assembly / Package | Max Binary Size (Packaged DLL) | Max Types in Public API | Max GC Allocation per Operation |
|---|---|---|---|
| `EricksonLopez.Mapper.Abstractions` | **< 30 KB** | **<= 15 Types** | **0 B** |
| `EricksonLopez.Mapper.Generator` | **< 250 KB** | **<= 5 Internal Types** | **N/A (Compile-time)** |
| `EricksonLopez.Mapper.Analyzers` | **< 100 KB** | **<= 5 Internal Types** | **N/A (Compile-time)** |
| `EricksonLopez.Mapper.DomainPrimitives` | **< 40 KB** | **<= 10 Types** | **0 B** |
| `EricksonLopez.Mapper.Result` | **< 35 KB** | **<= 8 Types** | **0 B** |
| `EricksonLopez.Mapper.Mapster` | **< 50 KB** | **<= 12 Types** | **0 B** |

---

## 2. Dependency Invariance
- `EricksonLopez.Mapper.Abstractions` has **0 external NuGet dependencies**.
- `EricksonLopez.Mapper.Generator` depends solely on `Microsoft.CodeAnalysis.CSharp` (analyzers reference).
- No third-party reflection, expression-compilation, or logging packages are bundled.
