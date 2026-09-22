# Level 3 — Real World Scenarios

> **Showcase Source Files:**
> - [`Level3_RealWorld/RealWorldDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level3_RealWorld/RealWorldDemo.cs)
> - [`Level3_RealWorld/CollectionsDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level3_RealWorld/CollectionsDemo.cs)
> - [`Level3_RealWorld/BuiltinConversionsDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level3_RealWorld/BuiltinConversionsDemo.cs)
> - [`Level3_RealWorld/CollectionTypesDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level3_RealWorld/CollectionTypesDemo.cs)
> - [`Level3_RealWorld/HierarchyFlatteningDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level3_RealWorld/HierarchyFlatteningDemo.cs)  
> **Complexity Level:** Intermediate  
> **API Surface Covered:** Nested objects, automatic sub-method binding, advanced collections, built-in scalar conversions, hierarchy flattening via dot-path `[MapProperty("A.B.C", "Dest")]`

---

## 1. Mapping Nested Object Graphs

When an entity contains complex child objects that also require transformation into DTOs:

```csharp
[Mapper]
public partial class OrderMapper
{
    public partial OrderDto MapOrder(Order source);
    public partial AddressDto MapAddress(Address source);
    public partial OrderItemDto MapItem(OrderItem source);
}
```

The generator recognizes that `Order.ShippingAddress` is of type `Address` and that `OrderDto.ShippingAddress` is of type `AddressDto`. It automatically wires and invokes `MapAddress(source.ShippingAddress)`.

---

## 2. Dictionaries with Type Conversions

Mapping `Dictionary<TKey, TValue>` supports independent transformations of keys and values:

```csharp
[Mapper]
public partial class ConfigurationMapper
{
    // Maps Dictionary<string, int> to Dictionary<string, string>
    public partial AppSettingsDto Map(AppSettings source);
}
```

The emitted code iterates through key-value pairs (`KeyValuePair`) with a pre-sized target dictionary capacity.

---

## 3. Built-In Scalar Type Conversions (Zero Configuration)

The generator applies common scalar conversions automatically without custom converters or attributes:

| Source | Destination | Emitted C# Expression |
|---|---|---|
| `Guid` | `string` | `source.Id.ToString()` |
| `string` | `Guid` | `Guid.Parse(source.Id)` |
| `enum` | `string` | `source.Status.ToString()` |
| `string` | `enum` | `Enum.Parse<TEnum>(source.Status)` |
| `DateTime` | `DateOnly` | `DateOnly.FromDateTime(source.Created)` |
| `DateTime` | `DateTimeOffset` | `new DateTimeOffset(source.Created)` |
| `int` | `long` | Implicit C# widening |

```csharp
[Mapper]
public partial class EventMapper
{
    public partial EventDto Map(EventEntity source);
}
```

---

## 4. Modern Collection Types

The generator emits high-performance loops with pre-allocated capacities to eliminate heap reallocations:

```csharp
[Mapper]
public partial class CollectionTypesMapper
{
    // 1. Direct arrays (indexed for-loop)
    public partial TagDto[] MapTags(List<TagEntity> source);

    // 2. ImmutableArray<T> (CreateBuilder with pre-sizing)
    public partial ImmutableArray<PermissionDto> MapPermissions(IEnumerable<PermissionEntity> source);

    // 3. HashSet<T> (automatic deduplication with capacity)
    public partial HashSet<string> MapUniqueRoles(List<string> source);
}
```

---

## 5. Hierarchy Flattening with Deep Dot-Path Navigation

A deep object graph (`Order.Customer.Address.City`) can be projected into a scalar destination property using dot-separated path notation in `[MapProperty]`. The generator resolves each segment at compile time (via `MemberResolutionEngine.ResolvePropertyPath`) and emits null-safe navigation chains.

```csharp
[Mapper]
public partial class HierarchyFlatteningMapper
{
    [MapProperty("Customer.FullName",              "FullName")]
    [MapProperty("Customer.Address.Street",        "Street")]
    [MapProperty("Customer.Address.City.City",     "CityName")]
    [MapProperty("Customer.Address.City.PostalCode", "PostalCode")]
    [MapProperty("Customer.Address.City.CountryCode", "CountryCode")]
    [MapNullFallback("FullName",    "string.Empty")]
    [MapNullFallback("Street",      "string.Empty")]
    [MapNullFallback("CityName",    "string.Empty")]
    [MapNullFallback("PostalCode",  "string.Empty")]
    [MapNullFallback("CountryCode", "string.Empty")]
    public partial FlatOrderDto Flatten(OrderAggregate source);
}
```

**Emitted C# (simplified):**
```csharp
FullName    = source.Customer?.FullName     ?? string.Empty,
Street      = source.Customer?.Address?.Street ?? string.Empty,
CityName    = source.Customer?.Address?.City?.City ?? string.Empty,
PostalCode  = source.Customer?.Address?.City?.PostalCode ?? string.Empty,
CountryCode = source.Customer?.Address?.City?.CountryCode ?? string.Empty,
```

> **Note:** `[MapNullFallback]` is required alongside each dot-path `[MapProperty]` when the destination member is a non-nullable type, because intermediate reference-type segments make the resolved path nullable. This triggers `ELM004` without a fallback expression.
