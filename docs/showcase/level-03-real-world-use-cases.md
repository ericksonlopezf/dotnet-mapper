# Level 03: Built-In Conversions & Custom Converters

## 1. Out-of-the-Box Primitive & Collection Conversions

The compiler generator automatically handles the following conversions with zero manual code:
- **Enum to String / String to Enum**: Optimized static switch statements without `Enum.Parse` allocations.
- **Guid to String / String to Guid**: Fast Span-based parsing.
- **Numeric Widening**: `int` $\rightarrow$ `long`, `float` $\rightarrow$ `double`, `int` $\rightarrow$ `decimal`.
- **Collections & Arrays**: `List<T>`, `T[]`, `HashSet<T>`, `IReadOnlyList<T>`, `IEnumerable<T>`.

---

## 2. Implementing Custom Converters (`IConverter<TSource, TDestination>`)

For complex domain translations (such as temporal parsing, cryptographic masking, or currency formatting):

```csharp
public sealed class DateOnlyToIsoConverter : IConverter<DateOnly, string>
{
    public string Convert(DateOnly source) => source.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

public sealed class IsoToDateOnlyConverter : IConverter<string, DateOnly>
{
    public DateOnly Convert(string source) => DateOnly.ParseExact(source, "yyyy-MM-dd", CultureInfo.InvariantCulture);
}
```

### Registering on a Mapper
```csharp
[Mapper]
[UseConverter(typeof(DateOnlyToIsoConverter))]
[UseConverter(typeof(IsoToDateOnlyConverter))]
public static partial class FiscalMapper
{
    public static partial InvoiceResponse ToResponse(InvoiceEntity entity);
}
```

The generator detects the registered converter and inlines instance or static method calls directly into the mapping body.
