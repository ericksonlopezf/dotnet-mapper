# Level 6 — Error Handling & Failure Boundaries

> **Showcase Source:** [`Level6_ErrorHandling/ErrorHandlingDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level6_ErrorHandling/ErrorHandlingDemo.cs)  
> **Complexity Level:** Intermediate  
> **API Surface Covered:** Compile-Time Diagnostic Gates, Runtime Exception Boundaries, `IConverter<TSource, TDestination>` Fault Isolation

---

## 1. Error Handling Philosophy: Compile Time vs Runtime

`EricksonLopez.Mapper` is engineered around the **Fail-Fast** principle:
1. **Structural Mapping Errors (99% of cases):** Detected at compile time via Roslyn analyzers (`ELM001`–`ELM016`), completely preventing the generation of broken binaries.
2. **Domain or Data Format Errors at Runtime:** Occur strictly when custom converters (`IConverter`) or static domain factories (`[MapFactory]`) throw validation exceptions on malformed payloads.

---

## 2. Runtime Exception Isolation in `IConverter`

When custom conversions require strict validation that can fail on malformed data:

```csharp
public class StrictDataConverter : IConverter<string, int>
{
    public int Convert(string source)
    {
        if (!int.TryParse(source, out var value))
        {
            throw new FormatException($"Value '{source}' is not a valid integer.");
        }
        return value;
    }
}

[Mapper]
public partial class ErrorHandlingMapper
{
    [UseConverter(typeof(StrictDataConverter))]
    public partial NumericReportDto Map(RawReport source);
}
```

---

## 3. Host Recovery and Dead-Lettering Patterns

If malformed data causes a conversion exception during mapping execution:

```csharp
try
{
    var dto = mapper.Map(rawPayload);
    await ProcessDtoAsync(dto);
}
catch (FormatException ex)
{
    // Permanent data defect: divert to Dead Letter Queue (DLQ)
    logger.LogError(ex, "Corrupt payload received. Forwarding to DLQ.");
    await deadLetterQueue.PublishAsync(rawPayload);
}
```

> [!TIP]
> To avoid throwing exceptions for routine business failures, consult **Level 9 (`EricksonLopez.Mapper.Result`)**, which provides Railway-Oriented Programming (ROP) support.
