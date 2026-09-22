# QuickStart Guide: 5 Minutes to High-Performance Mapping

Learn how to install, configure, and execute your first strongly typed, zero-allocation, **Native AOT**-compatible object mapping in under 5 minutes.

---

## 1. Install the Core Package

Add `EricksonLopez.Mapper` to your project via the .NET CLI:

```bash
dotnet add package EricksonLopez.Mapper
```

---

## 2. Declare Models and Static Mapper

Create a `static partial` class annotated with `[Mapper]`:

```csharp
using System;
using EricksonLopez.Mapper;

namespace MyApp;

// Models
public sealed record CustomerEntity(Guid Id, string Name, string Email);
public sealed record CustomerDto(Guid Id, string Name, string Email);

// Source-Generated Mapper
[Mapper]
public static partial class CustomerMapper
{
    public static partial CustomerDto ToDto(CustomerEntity entity);
}
```

---

## 3. Execute the Mapping

```csharp
using System;
using MyApp;

var entity = new CustomerEntity(Guid.NewGuid(), "Jane Doe", "jane@example.com");

// Direct invocation without reflection (~1.2 ns, 0 Bytes allocated on heap)
var dto = CustomerMapper.ToDto(entity);

Console.WriteLine($"Generated DTO: {dto.Name} ({dto.Email})");
```

---

## 4. Usage with Dependency Injection (DI)

If you prefer instance-based mappers registered in `IServiceCollection`:

```csharp
// 1. Declare the class as partial (non-static)
[Mapper]
public partial class OrderMapper
{
    public partial OrderDto Map(Order source);
}

// 2. Register in Program.cs
builder.Services.AddSingleton<OrderMapper>();

// Or automatically register all mappers across the assembly:
// [assembly: GenerateMapperRegistration]
// builder.Services.AddGeneratedMappers();
```

---

## 5. Functional Mapping with Result (Railway-Oriented Programming)

If your architecture uses `EricksonLopez.Result` and you wish to map without `if (result.IsSuccess)` branching:

```bash
dotnet add package EricksonLopez.Mapper.Result
```

```csharp
using EricksonLopez.Mapper.Result;
using EricksonLopez.Result;

Result<CustomerEntity> result = repository.FindById(id);

// Projects the successful value; if failed, propagates the error without invoking the mapper
Result<CustomerDto> dtoResult = result.Map(CustomerMapper.ToDto);
```

---

## 6. Next Steps

- Explore the 11-level progressive guides in [`docs/showcase/`](showcase/level-00-conceptual.md).
- Review all 28 recipes in the [`Cookbook`](cookbook.md).
- Consult the complete [`API Reference`](api-reference.md).
