# Migration Guide

This guide describes how to migrate existing .NET codebases from **AutoMapper** or **Mapster** to `EricksonLopez.Mapper`.

---

## 1. Migrating from AutoMapper

### Step 1: Remove AutoMapper Dependencies

Remove AutoMapper NuGet packages and configuration profile classes from your project:

```bash
dotnet remove package AutoMapper
dotnet remove package AutoMapper.Extensions.Microsoft.DependencyInjection
```

Delete all profile classes inheriting from `Profile`.

### Step 2: Add EricksonLopez.Mapper

Install the umbrella package:

```bash
dotnet add package EricksonLopez.Mapper --version 1.0.0
```

### Step 3: Convert Mapping Profiles to `[Mapper]` Partial Classes

**Before (AutoMapper Profile):**

```csharp
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
            .ForMember(dest => dest.InternalSecret, opt => opt.Ignore());
    }
}
```

**After (EricksonLopez.Mapper):**

```csharp
using EricksonLopez.Mapper;

[Mapper]
public partial class OrderMapper
{
    [MapProperty("Customer.Name", nameof(OrderDto.CustomerName))]
    [MapIgnore(nameof(OrderDto.InternalSecret))]
    public partial OrderDto Map(Order source);
}
```

### Step 4: Convert Call Sites

**Before:**

```csharp
public class OrderService(IMapper mapper)
{
    public OrderDto GetOrder(Order order) => mapper.Map<OrderDto>(order);
}
```

**After:**

```csharp
public class OrderService(OrderMapper mapper)
{
    public OrderDto GetOrder(Order order) => mapper.Map(order);
}
```

### Step 5: Update Dependency Injection Registration

**Before:**

```csharp
builder.Services.AddAutoMapper(typeof(OrderProfile).Assembly);
```

**After:**

```csharp
// 1. Add assembly attribute to AssemblyInfo.cs or Program.cs:
[assembly: GenerateMapperRegistration]

// 2. In Program.cs:
builder.Services.AddGeneratedMappers();
```

---

## 2. Migrating from Mapster

### Step 1: Remove Mapster Dependencies

```bash
dotnet remove package Mapster
dotnet remove package Mapster.DependencyInjection
```

### Step 2: Add EricksonLopez.Mapper

```bash
dotnet add package EricksonLopez.Mapper --version 1.0.0
```

### Step 3: Replace `Adapt<T>()` with Generated Methods

**Before:**

```csharp
var dto = order.Adapt<OrderDto>();
```

**After:**

```csharp
[Mapper]
public static partial class OrderMapper
{
    public static partial OrderDto Map(Order source);
}

// In application code:
var dto = OrderMapper.Map(order);
```

---

## 3. Key Behavioral & Architectural Differences

| Capability | AutoMapper | Mapster | EricksonLopez.Mapper |
|---|---|---|---|
| **Compilation Errors for Missing Mappings** | ❌ No (Runtime assert) | ❌ No | ✅ Compile-time `ELM001` |
| **Native AOT Trimming Safety** | ❌ Trimming issues | ⚠️ Partial | ✅ 100% Zero-warning AOT |
| **Execution Performance** | 24.90 ns (Reflection) | 8.44 ns (Dynamic IL) | **2.80 ns** (Static Pure C#) |
| **Heap Allocations** | Allocates delegates & closures | Modest allocations | **32 B** (Object creation only) |
| **Implicit Flattening** | ✅ Automatic | ✅ Configurable | ❌ Excluded by design ([ADR-D03](adr/ADR-D03-no-automatic-flattening.md)) |
| **In-Place Mutation (`Map(src, dest)`)** | ✅ Supported | ✅ Supported | ❌ Excluded by design ([ADR-D05](adr/ADR-D05-no-existing-instance-mapping.md)) |
| **`IQueryable.ProjectTo<T>()`** | ✅ Supported | ✅ Supported | ❌ Excluded by design ([ADR-D11](adr/ADR-D11-no-iqueryable-projection.md)) |
