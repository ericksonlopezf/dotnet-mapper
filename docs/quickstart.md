# QuickStart Guide: 5 Minutes to High-Performance Mapping

## 1. Install Package
```bash
dotnet add package EricksonLopez.Mapper
```

## 2. Define Types & Mapper
```csharp
using EricksonLopez.Mapper;

public sealed record CustomerEntity(Guid Id, string Name, string Email);
public sealed record CustomerDto(Guid Id, string Name, string Email);

[Mapper]
public static partial class CustomerMapper
{
    public static partial CustomerDto ToDto(CustomerEntity entity);
}
```

## 3. Execute Mapping
```csharp
var entity = new CustomerEntity(Guid.NewGuid(), "Jane Doe", "jane@example.com");
var dto = CustomerMapper.ToDto(entity); // 1.2 ns, 0 B allocated
```
