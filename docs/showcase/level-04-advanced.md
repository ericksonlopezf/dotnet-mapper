# Level 4 — Advanced Integration Patterns

> **Showcase Source Files:**
> - [`Level4_Advanced/AdvancedDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level4_Advanced/AdvancedDemo.cs)
> - [`Level4_Advanced/FactoryMethodDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level4_Advanced/FactoryMethodDemo.cs)
> - [`Level4_Advanced/PolymorphicDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level4_Advanced/PolymorphicDemo.cs)  
> **Complexity Level:** Advanced  
> **API Surface Covered:** Immutable Records, Constructor Binding, `[MapFactory]`, `[MapDerivedType]`

---

## 1. Mapping to Immutable Records (Parameterized Constructors)

When the destination model is a C# `record` with a primary constructor and no property setters:

```csharp
public sealed record ProductDto(Guid Id, string Name, decimal Price);

[Mapper]
public partial class ProductMapper
{
    public partial ProductDto Map(ProductEntity source);
}
```

The generator inspects constructor parameter names and binds matching source properties, emitting:
```csharp
return new ProductDto(source.Id, source.Name, source.Price);
```

---

## 2. Instantiation via Static Factory Methods (`[MapFactory]`)

In domain models that encapsulate object creation to enforce business invariants:

```csharp
public class PaymentTransactionDto
{
    public Guid TransactionId { get; }
    public decimal Amount { get; }
    public string Currency { get; }

    public static PaymentTransactionDto Create(Guid transactionId, decimal amount, string currency)
    {
        return new PaymentTransactionDto(transactionId, amount, currency.ToUpperInvariant());
    }
}

[Mapper]
public partial class PaymentMapper
{
    [MapFactory(nameof(PaymentTransactionDto.Create))]
    public partial PaymentTransactionDto Map(PaymentRequest source);
}
```

The compiler emits an invocation to `PaymentTransactionDto.Create(...)` passing resolved properties from the source object.

---

## 3. Compile-Time Polymorphic Dispatch (`[MapDerivedType]`)

To map inheritance hierarchies (base classes or interfaces) to DTO hierarchies without runtime reflection:

```csharp
[Mapper]
public partial class VehicleMapper
{
    [MapDerivedType(typeof(CarEntity), typeof(CarDto))]
    [MapDerivedType(typeof(TruckEntity), typeof(TruckDto))]
    public partial VehicleDto Map(VehicleEntity source);

    public partial CarDto MapCar(CarEntity source);
    public partial TruckDto MapTruck(TruckEntity source);
}
```

### Emitted C# Source Code:
```csharp
public partial VehicleDto Map(VehicleEntity source)
{
    if (source == null) throw new ArgumentNullException(nameof(source));

    return source switch
    {
        CarEntity car => MapCar(car),
        TruckEntity truck => MapTruck(truck),
        _ => throw new InvalidOperationException($"Unsupported vehicle type: {source.GetType()}")
    };
}
```
Dispatch is executed via native C# pattern matching expressions, guaranteeing peak execution speed and Native AOT safety.
