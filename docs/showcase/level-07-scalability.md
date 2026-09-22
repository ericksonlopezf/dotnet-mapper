# Level 7 — Scalability & Throughput

> **Showcase Source:** [`Level7_Scalability/ScalabilityDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level7_Scalability/ScalabilityDemo.cs)  
> **Complexity Level:** Advanced  
> **API Surface Covered:** Zero-Allocation Model, Throughput Optimization, In-Memory Benchmarks, Horizontal Scaling

---

## 1. Architecture for Horizontal Scalability

Because the library is completely **stateless** and **reflection-free**:
- **Multi-Instance Cloud Scaling:** Multiple Docker containers or Kubernetes pods scale linearly with zero shared locks, cross-instance synchronization, or cluster state.
- **Zero Reflection Overhead:** By eliminating dynamic invocation (`MethodInfo.Invoke`), CPU cycles are reserved entirely for application business logic and I/O.

---

## 2. Showcase In-Memory Throughput Benchmark

In the executable Showcase project, `ScalabilityDemo.cs` executes 1,000,000 continuous mapping iterations to measure raw throughput:

```csharp
[Mapper]
public partial class ScalabilityMapper
{
    public partial MicroDto Map(MicroEntity source);
}

// 1 Million Operations Run:
var sw = Stopwatch.StartNew();
for (int i = 0; i < 1_000_000; i++)
{
    var dto = mapper.Map(entity);
}
sw.Stop();

// Typical result on modern commodity hardware:
// 1,000,000 operations completed in ~30–36 ms (~28–33 million ops/sec).
```

---

## 3. High-Throughput Optimization Best Practices

1. **Use Static Mappers or Singletons:** Avoid instantiating `new Mapper()` inside tight hot-path loops.
2. **Prefer Positional Record Constructors:** Positional primary constructors enable RyuJIT to pass arguments directly via CPU registers (`RCX`, `RDX`, `R8`, `R9`).
3. **Collections with Known Length:** When mapping collections, ensure the source exposes `Count` or `Length` so the generator can emit capacity-pre-allocated target collections.
