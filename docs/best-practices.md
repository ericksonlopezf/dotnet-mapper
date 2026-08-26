# Best Practices Guide

This guide details recommended engineering practices, design patterns, and anti-patterns when building and maintaining applications using `EricksonLopez.Mapper`.

---

## 1. Always Keep StrictMapping Enabled in Production

`StrictMapping = true` (the default) causes the compiler to fail with **`ELM001`** whenever a destination property has no source mapping.

```csharp
// ✅ RECOMMENDED: Strict mapping prevents silent data loss
[Mapper(StrictMapping = true)]
public partial class OrderMapper
{
    public partial OrderDto Map(Order source);
}
```

*Only disable strict mapping for third-party or legacy models where models are out of your control.*

---

## 2. Use `nameof()` for Attribute Arguments

Avoid magic string literals that break during refactoring:

```csharp
// ❌ AVOID: Magic strings
[MapProperty("CustomerName", "ClientName")]

// ✅ RECOMMENDED: Refactoring-safe identifiers
[MapProperty(nameof(Order.CustomerName), nameof(OrderDto.ClientName))]
[MapIgnore(nameof(OrderDto.InternalTrackingCode))]
```

---

## 3. Prefer `[MapNullFallback]` over Custom Converters for Defaults

For nullable primitive values with known fallbacks, `[MapNullFallback]` generates minimal inlined null-coalescing expressions:

```csharp
// ✅ RECOMMENDED: Minimal, inlined code
[MapNullFallback(nameof(ProductDto.Price), "0m")]
public partial ProductDto Map(Product source);

// ❌ AVOID: Heavy IConverter class for a simple default
public class DecimalFallbackConverter : IConverter<decimal?, decimal>
{
    public decimal Convert(decimal? source) => source ?? 0m;
}
```

---

## 4. Co-Locate Nested Mappers in the Same Class

When mapping complex hierarchies, define child mapping methods in the same partial mapper class:

```csharp
// ✅ RECOMMENDED: Generator automatically links MapAddress and MapLineItem
[Mapper]
public partial class OrderMapper
{
    public partial OrderDto MapOrder(Order source);
    public partial AddressDto MapAddress(Address source);
    public partial LineItemDto MapLineItem(LineItem source);
}
```

---

## 5. Explicitly Cover All Derived Types in Polymorphic Mappers

Ensure every concrete subtype is registered with `[MapDerivedType]` to prevent runtime fallback exceptions:

```csharp
[Mapper]
public partial class PaymentMapper
{
    [MapDerivedType(typeof(CreditCardPayment), typeof(CreditCardPaymentDto))]
    [MapDerivedType(typeof(PayPalPayment), typeof(PayPalPaymentDto))]
    [MapDerivedType(typeof(CryptoPayment), typeof(CryptoPaymentDto))]
    public partial PaymentDto Map(Payment source);

    public partial CreditCardPaymentDto MapCredit(CreditCardPayment source);
    public partial PayPalPaymentDto MapPayPal(PayPalPayment source);
    public partial CryptoPaymentDto MapCrypto(CryptoPayment source);
}
```

---

## 6. Use Static Mappers for High-Throughput & LINQ Pipelines

Static mappers eliminate instantiation overhead and enable method group delegate caching:

```csharp
[Mapper]
public static partial class FastMetricMapper
{
    public static partial MetricDto Map(Metric source);
}

// Zero-allocation parallel pipeline:
var dtos = metrics.AsParallel().Select(FastMetricMapper.Map).ToList();
```

---

## 7. Keep `IConverter<TSource, TDestination>` Pure & Stateless

Converters must be idempotent, thread-safe, and free of I/O side effects:

```csharp
// ✅ RECOMMENDED: Pure functional transformation
public class HexStringToBytesConverter : IConverter<string, byte[]>
{
    public byte[] Convert(string source) => Convert.FromHexString(source);
}
```

---

## 8. Mark DDD Value Objects Explicitly with `[ValueObject]`

Explicitly marking value objects documents intent and ensures predictable wrap/unwrap code synthesis:

```csharp
[ValueObject]
public readonly record struct OrderId(Guid Value);
```

---

## 9. Maintain Strict Architectural Layering

| Application Layer | Allowed Mapping Direction | Prohibited Mapping Direction |
|---|---|---|
| **Presentation / API** | `HttpRequestDto → Command` | `DomainEntity → HttpResponseDto` (bypass) |
| **Application** | `DomainEntity → ResponseDto` | Domain entities referencing DTO types |
| **Infrastructure** | `PersistenceModel ↔ DomainEntity` | DTOs referencing database entities directly |

---

## 10. Split Large Mappers by Bounded Context / Feature

Avoid massive god-mappers containing hundreds of unrelated methods. Group mapper classes per domain slice or aggregate.

---

## 11. Run Continuous Performance & Mutation Regression Gates

- Run `pwsh ./run-stryker.ps1` before submitting pull requests to ensure assertion quality.
- Verify benchmarks locally with `dotnet run -c Release --project benchmarks/EricksonLopez.Mapper.Benchmarks` whenever modifying generator logic.
