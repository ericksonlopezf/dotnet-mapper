# Performance Tuning Guide

This guide details the technical mechanisms enabling `EricksonLopez.Mapper` to achieve physical minimum latency, zero framework allocations, and maximum NativeAOT execution speed.

---

## 1. Why Generated Code Achieves Zero Overhead

`EricksonLopez.Mapper` emits plain, deterministic C# code during compilation:

```csharp
// Generated implementation for: public partial UserDto Map(User source)
public partial UserDto Map(User source)
{
    if (source == null) throw new ArgumentNullException(nameof(source));
    return new UserDto
    {
        Name = source.Name,
        Email = source.Email,
        Age = source.Age
    };
}
```

Because the emitted code contains no reflection, dynamic proxies, or lambda delegates:
- **JIT Inlining**: The .NET JIT compiler can inline small mapping methods directly into the caller's stack frame.
- **Hardware Acceleration**: The compiler can vectorize sequential memory copies using AVX-512/AVX2 registers.
- **Null Safety Branch Prediction**: Simple null ternary checks are optimized via Profile-Guided Optimization (PGO).

---

## 2. Collection Pre-Sizing Optimization

When mapping collections, resizing buffers dynamically causes repeated heap reallocations. `EricksonLopez.Mapper` automatically pre-sizes collections whenever the source count is knowable:

```csharp
// 1. For List<T> targets:
var result = new List<UserDto>(source.Count);
foreach (var item in source)
{
    result.Add(this.Map(item));
}
return result;

// 2. For ImmutableArray<T> targets:
var builder = ImmutableArray.CreateBuilder<UserDto>(source.Count);
foreach (var item in source)
{
    builder.Add(this.Map(item));
}
return builder.MoveToImmutable();
```

> **Zero LINQ Policy**: The generator never emits `.Select().ToList()` in generated mapping code, completely eliminating `IEnumerator<T>` state machine allocations and closure captures.

---

## 3. Value Object Unwrapping Performance

For Domain-Driven Design Value Objects marked with `[ValueObject]` or single-value record structs:

```csharp
// Direct scalar unwrap (0 heap allocations)
dto.CustomerId = source.CustomerId.Value;

// Direct scalar wrap (0 heap allocations for readonly record struct)
entity.CustomerId = new CustomerId(dto.CustomerId);
```

No boxing, unboxing, or intermediate mapping objects are allocated.

---

## 4. Static Mappers for Hot Paths

When mappers are declared `static partial`, method calls bypass virtual dispatch tables and DI service lookups:

```csharp
[Mapper]
public static partial class FastOrderMapper
{
    public static partial OrderDto Map(Order source);
}

// In high-throughput serialization pipelines:
var dto = FastOrderMapper.Map(order);
```

**Recommended Use Cases for Static Mappers:**
- High-frequency event ingestion loops
- Database row-to-entity transformation
- LINQ `.Select(FastOrderMapper.Map)` pipelines

---

## 5. NativeAOT Memory Footprint

Because `EricksonLopez.Mapper` generates statically discoverable C# code:
- The .NET IL Linker aggressively trims unused framework assemblies.
- Data structures are laid out contiguously in native memory.
- Startup RSS (Resident Set Size) memory is minimized for container and serverless deployments.
