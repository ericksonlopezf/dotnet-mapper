# Level 02: Advanced Configuration & Member Customization

## 1. Custom Member Mapping (`[MapProperty]`)

When source and destination property names do not align, or when flattening complex nested properties, use `[MapProperty]`:

```csharp
public sealed record OrderLine(string Sku, int Quantity, decimal UnitPrice);
public sealed record Order(Guid Id, string CustomerName, List<OrderLine> Lines);

public sealed record OrderSummaryDto(
    Guid OrderId,
    string Customer,
    int TotalItemCount);

[Mapper]
public static partial class OrderMapper
{
    [MapProperty(nameof(OrderSummaryDto.OrderId), nameof(Order.Id))]
    [MapProperty(nameof(OrderSummaryDto.Customer), nameof(Order.CustomerName))]
    [MapProperty(nameof(OrderSummaryDto.TotalItemCount), "source.Lines.Sum(l => l.Quantity)")]
    public static partial OrderSummaryDto ToSummary(Order source);
}
```

---

## 2. Ignoring Members (`[MapIgnore]`)

To intentionally exclude sensitive or internal destination properties from mapping, decorate the method with `[MapIgnore]`:

```csharp
public sealed record RegisterUserCommand(string Username, string Password, string Email);
public sealed record UserEntity(Guid Id, string Username, string Email, string PasswordHash);

[Mapper]
public static partial class AuthMapper
{
    // Ignore PasswordHash as it will be computed via BCrypt / Argon2 separately
    [MapIgnore(nameof(UserEntity.PasswordHash))]
    [MapProperty(nameof(UserEntity.Id), "Guid.NewGuid()")]
    public static partial UserEntity ToEntity(RegisterUserCommand command);
}
```

---

## 3. Disambiguating Constructors (`[MapConstructor]`)

When a destination model declares multiple parameterized constructors, declare parameter types explicitly:

```csharp
[Mapper]
public static partial class EntityMapper
{
    [MapConstructor(typeof(Guid), typeof(string), typeof(decimal))]
    public static partial Account ToDomain(AccountDto dto);
}
```
