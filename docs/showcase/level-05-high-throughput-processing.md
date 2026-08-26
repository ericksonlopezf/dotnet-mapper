# Level 05: High-Throughput Batch Collections & Span Projections

## 1. High-Performance Collection Mapping

Batch processing scenarios (such as database query result hydration or event streaming payloads) require zero overhead during iteration.

```csharp
public sealed record LogEntry(Guid Id, string Level, string Message, DateTime Timestamp);
public sealed record LogEntryDto(Guid Id, string Level, string Message);

[Mapper]
public static partial class LogMapper
{
    public static partial LogEntryDto ToDto(LogEntry entry);

    // List projection
    public static partial List<LogEntryDto> ToDtoList(IReadOnlyList<LogEntry> entries);

    // Array projection
    public static partial LogEntryDto[] ToDtoArray(LogEntry[] entries);
}
```

---

## 2. Generated Code for Collections

The generator avoids LINQ `.Select().ToList()` allocations by pre-allocating the destination capacity using a simple, unrolled `for` loop:

```csharp
public static partial List<LogEntryDto> ToDtoList(IReadOnlyList<LogEntry> entries)
{
    if (entries is null) return default!;

    var count = entries.Count;
    var list = new List<LogEntryDto>(count); // Pre-allocated capacity
    for (int i = 0; i < count; i++)
    {
        list.Add(ToDto(entries[i]));
    }
    return list;
}
```

By pre-sizing the list and utilizing indexed loop iteration, collection re-allocation spikes are eliminated.
