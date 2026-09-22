# Level 2 — Configuration Deep Dive

> **Showcase Source Files:**
> - [`Level2_Configuration/ConfigurationDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level2_Configuration/ConfigurationDemo.cs)
> - [`Level2_Configuration/NullFallbackDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level2_Configuration/NullFallbackDemo.cs)
> - [`Level2_Configuration/ValueObjectDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level2_Configuration/ValueObjectDemo.cs)
> - [`Level2_Configuration/ValueAssignmentDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level2_Configuration/ValueAssignmentDemo.cs)
> - [`Level2_Configuration/EnumMappingDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level2_Configuration/EnumMappingDemo.cs)
> - [`Level2_Configuration/MapperDefaultsDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level2_Configuration/MapperDefaultsDemo.cs)
> - [`Level2_Configuration/StaticMapperDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level2_Configuration/StaticMapperDemo.cs)
> - [`Level2_Configuration/AssemblyConfig.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level2_Configuration/AssemblyConfig.cs)  
> **Complexity Level:** Intermediate  
> **API Surface Covered:** `[MapProperty]`, `[MapIgnore]`, `[MapIgnoreSource]`, `[MapperIgnore]`, `[MapValue]`, `[MapNullFallback]`, `[ValueObject]`, `[EnumMappingStrategy]`, `[MapEnumValue]`, `[assembly: MapperDefaults]`, `static partial class`

---

## 1. Mapping Properties with Disparate Names (`[MapProperty]`)

When property names differ between source and destination models, or when flattening nested paths:

```csharp
[Mapper]
public partial class ConfiguredUserMapper
{
    [MapProperty(nameof(User.Email), nameof(ConfiguredUserDto.EmailAddress))]
    [MapProperty("Profile.Bio", nameof(ConfiguredUserDto.Biography))]
    public partial ConfiguredUserDto Map(User source);
}
```

---

## 2. Ignoring Destination Properties (`[MapIgnore]`)

In strict mode (`StrictMapping = true`), any destination property without a matching source member produces compilation error `ELM001`. Use `[MapIgnore]` to exclude it explicitly:

```csharp
[Mapper]
public partial class SupportTicketMapper
{
    [MapIgnore(nameof(SupportTicketDto.InternalResolutionNotes))]
    public partial SupportTicketDto Map(SupportTicket source);
}
```

---

## 3. Ignoring Sensitive Source Properties (`[MapIgnoreSource]`)

To ensure confidential or internal entity properties are never accidentally processed:

```csharp
[Mapper]
public partial class SecurityMapper
{
    [MapIgnoreSource(nameof(UserEntity.PasswordHash))]
    [MapIgnoreSource(nameof(UserEntity.SecurityStamp))]
    public partial UserDto Map(UserEntity source);
}
```

---

## 4. Model-Level Global Exclusion (`[MapperIgnore]`)

If a property in a model must never participate in any mapping across the entire application:

```csharp
public class UserEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [MapperIgnore]
    public string InternalMemoryToken { get; set; } = string.Empty;
}
```

---

## 5. Constant and Literal Expressions (`[MapValue]`)

Inject constant expressions, audit timestamps, or environment values directly into the target:

```csharp
[Mapper]
public partial class AccountRegistrationMapper
{
    [MapValue(nameof(AccountDto.Status), "\"ACTIVE\"")]
    [MapValue(nameof(AccountDto.CreatedAt), "System.DateTime.UtcNow")]
    public partial AccountDto Map(RegistrationRequest source);
}
```

---

## 6. Null Handling and Fallbacks (`[MapNullFallback]`)

When the source property is a nullable value type (`int?`, `decimal?`) and the destination requires a non-nullable value type (`int`, `decimal`), use `[MapNullFallback]`:

```csharp
[Mapper]
public partial class ProductCatalogMapper
{
    [MapNullFallback(nameof(ProductCatalogDto.DiscountPercent), "0")]
    [MapNullFallback(nameof(ProductCatalogDto.UnitsInStock), "0")]
    public partial ProductCatalogDto Map(ProductCatalog source);
}
```

---

## 7. Value Objects and Strongly-Typed IDs (`[ValueObject]`)

The `[ValueObject]` attribute instructs the generator to automatically unpack `.Value` into scalar types, or rehydrate via the Value Object's constructor:

```csharp
[ValueObject]
public readonly record struct CustomerId(Guid Value);

[Mapper]
public partial class ValueObjectMapper
{
    public partial CustomerDto Map(CustomerEntity source); // CustomerId -> Guid
    public partial CustomerEntity Map(CustomerDto source); // Guid -> CustomerId
}
```

---

## 8. Enum Mapping Strategies (`[EnumMappingStrategy]` and `[MapEnumValue]`)

### Scenario A: `ByName` with `IgnoreCase = true`
```csharp
[Mapper]
[EnumMappingStrategy(EnumMappingStrategy.ByName, IgnoreCase = true)]
public partial class NotificationMapper
{
    public partial NotificationDto Map(Notification source);
}
```

### Scenario B: `ByValue` (Direct integral casting)
```csharp
[Mapper]
[EnumMappingStrategy(EnumMappingStrategy.ByValue)]
public partial class PriorityMapper
{
    public partial ExternalPriorityDto Map(InternalPriority source);
}
```

### Scenario C: Explicit Member Pairing (`[MapEnumValue]`)
```csharp
[Mapper]
public partial class OrderStateMapper
{
    [MapEnumValue(SourceStatus.AwaitingPayment, TargetStatus.Pending)]
    [MapEnumValue(SourceStatus.Paid, TargetStatus.Completed)]
    public partial TargetOrder Map(SourceOrder source);
}
```

---

## 9. Assembly-Level Conventions (`[assembly: MapperDefaults]`)

Define global mapping defaults for the entire project in `AssemblyConfig.cs`:

```csharp
using EricksonLopez.Mapper;

[assembly: MapperDefaults(
    StrictMapping = true,
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumIgnoreCase = true
)]
```

---

## 10. Stateless Static Mappers (`static partial class`)

For maximum performance and zero-allocation LINQ pipeline compatibility:

```csharp
[Mapper]
public static partial class StaticMapperDemo
{
    public static partial LogEntryDto Map(LogEntry source);
}

// Usage in LINQ:
var dtos = logs.Select(StaticMapperDemo.Map).ToList();
```
