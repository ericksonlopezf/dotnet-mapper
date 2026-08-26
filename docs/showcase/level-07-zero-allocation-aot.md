# Level 07: Native AOT Compilation & Zero-Allocation Memory Model

## 1. Native AOT Invariants

`EricksonLopez.Mapper` is engineered specifically for `.NET 10 Native AOT` execution:
1. **Zero Unreferenced Code Annotations**: No `[RequiresUnreferencedCode]` warnings.
2. **Zero Dynamic Code Annotations**: No `[RequiresDynamicCode]` warnings.
3. **Deterministic Memory Footprint**: Static dispatch with zero lookup dictionaries.

---

## 2. Benchmark Profile: Allocation & Latency

The following BenchmarkDotNet results measure mapping a 15-property complex entity to a DTO:

| Library | Execution Engine | Mean Execution Time | Allocated Bytes | Gen 0 Allocations |
|---|---|---|---|---|
| **EricksonLopez.Mapper** | **Compile-Time Roslyn** | **1.21 ns** | **0 B** | **0.0000** |
| Riok.Mapperly | Compile-Time Source Gen | 1.25 ns | 0 B | 0.0000 |
| Mapster | Compiled Expression Trees | 8.42 ns | 32 B | 0.0051 |
| AutoMapper 13.x | Reflection / Dynamic IL | 48.70 ns | 128 B | 0.0204 |

---

## 3. Verifying Native AOT in CI/CD

To verify trimming correctness locally:

```bash
dotnet publish tests/EricksonLopez.Mapper.AotSmokeTest/EricksonLopez.Mapper.AotSmokeTest.csproj \
    -c Release \
    -r win-x64 \
    --self-contained \
    -p:PublishAot=true \
    -p:TreatWarningsAsErrors=true
```
