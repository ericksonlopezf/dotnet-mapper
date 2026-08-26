# Level 06: Robust Error Handling, Result Integration & Null Fallbacks

## 1. Handling Nullable Source Properties (`[MapNullFallback]`)

When mapping from external systems or legacy schemas, nullable fields often require business fallback defaults:

```csharp
public sealed record ExternalUserPayload(
    string? Nickname,
    string? PreferredTheme,
    int? MaxRetryAttempts);

public sealed record InternalUserSettings(
    string DisplayName,
    string Theme,
    int MaxRetries);

[Mapper]
public static partial class SettingsMapper
{
    [MapNullFallback(nameof(InternalUserSettings.DisplayName), "Anonymous")]
    [MapNullFallback(nameof(InternalUserSettings.Theme), "Dark")]
    [MapNullFallback(nameof(InternalUserSettings.MaxRetries), "3")]
    public static partial InternalUserSettings ToSettings(ExternalUserPayload payload);
}
```

---

## 2. Integration with `Result<T>` (`EricksonLopez.Mapper.Result`)

To safely map domain models into API responses wrapped in `Result<T>` without throwing exceptions:

```csharp
using EricksonLopez.Result;
using EricksonLopez.Mapper.Result;

public sealed class OrderService
{
    public Result<OrderResponse> GetOrder(Guid id)
    {
        Result<Order> domainResult = _repository.FindById(id);

        // Functional mapping over Result<T>
        return domainResult.Map(OrderMapper.ToResponse);
    }
}
```
