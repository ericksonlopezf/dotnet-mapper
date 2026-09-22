# Level 8 — Customization & Converter Injection

> **Showcase Source:** [`Level8_Customization/CustomizationDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level8_Customization/CustomizationDemo.cs)  
> **Complexity Level:** Intermediate / Advanced  
> **API Surface Covered:** `IConverter<TSource, TDestination>`, `[UseConverter(Type)]`, `[UseConverter(fieldName)]`

---

## 1. The `IConverter<TSource, TDestination>` Interface

For custom transformations requiring complex business logic, string parsing, or arithmetic calculations:

```csharp
namespace EricksonLopez.Mapper;

public interface IConverter<TSource, TDestination>
{
    TDestination Convert(TSource source);
}
```

---

## 2. Type-Based Converter (`[UseConverter(Type)]`)

When the converter has no external service dependencies or mutable state:

```csharp
public class LegacyToModernUserConverter : IConverter<LegacyUser, ModernUserDto>
{
    public ModernUserDto Convert(LegacyUser source)
    {
        var parts = source.FullName.Split(' ', 2);
        return new ModernUserDto
        {
            FirstName = parts.Length > 0 ? parts[0] : string.Empty,
            LastName = parts.Length > 1 ? parts[1] : string.Empty
        };
    }
}

[Mapper]
public partial class CustomizationMapper
{
    [UseConverter(typeof(LegacyToModernUserConverter))]
    public partial ModernUserDto Map(LegacyUser source);
}
```

---

## 3. Field-Based Converter Injection (`[UseConverter(fieldName)]`)

When the converter requires dependencies injected via the IoC container (e.g., encryption providers, `IMemoryCache`, or configuration services):

```csharp
[Mapper]
public partial class InjectedConverterMapper
{
    private readonly IConverter<LegacyUser, ModernUserDto> _userConverter;

    public InjectedConverterMapper(IConverter<LegacyUser, ModernUserDto> userConverter)
    {
        _userConverter = userConverter ?? throw new ArgumentNullException(nameof(userConverter));
    }

    [UseConverter(nameof(_userConverter))]
    public partial ModernUserDto Map(LegacyUser source);
}
```

The generator emits code that calls `this._userConverter.Convert(source)` directly, cleanly preserving IoC dependency resolution and lifecycle management.
