// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level2_Configuration;

// =============================================================================
// Level 2 - [assembly: MapperDefaults] (Assembly-Level Configuration)
//
// Problem:
//   You have many mappers across a large assembly and want a consistent default
//   configuration (e.g., enum case-insensitivity) without repeating attributes
//   on every class or method.
//
// Solution:
//   Apply [assembly: MapperDefaults(...)] once in AssemblyConfig.cs.
//   This sets the baseline for ALL mappers in the assembly.
//
// Options available on MapperDefaultsAttribute:
//   - StrictMapping          : bool (default: true)
//   - EnumMappingStrategy    : EnumMappingStrategy (default: ByName)
//   - EnumIgnoreCase         : bool (default: false)
//
// Precedence (highest to lowest):
//   1. Method-level [EnumMappingStrategy], [MapProperty], etc.
//   2. Class-level [Mapper(StrictMapping = ...)], [EnumMappingStrategy]
//   3. Assembly [assembly: MapperDefaults(EnumIgnoreCase = true)]
//   4. Built-in defaults
// =============================================================================

/// <summary>Source log level enum, using mixed casing as seen in legacy systems.</summary>
public enum SourceLogLevel
{
    /// <summary>Verbose diagnostic information.</summary>
    VERBOSE,
    /// <summary>Debug-level information.</summary>
    DEBUG,
    /// <summary>Informational message.</summary>
    INFO,
    /// <summary>Warning indication.</summary>
    WARN,
    /// <summary>Error condition.</summary>
    ERROR
}

/// <summary>Destination log level enum in PascalCase.</summary>
public enum DestLogLevel
{
    /// <summary>Verbose diagnostic information.</summary>
    Verbose,
    /// <summary>Debug-level information.</summary>
    Debug,
    /// <summary>Informational message.</summary>
    Info,
    /// <summary>Warning indication.</summary>
    Warn,
    /// <summary>Error condition.</summary>
    Error
}

/// <summary>Represents a log entry coming from a legacy system with uppercase enums.</summary>
public class LegacyLogEntry
{
    /// <summary>Gets or sets the log level in uppercase format.</summary>
    public SourceLogLevel Level { get; set; }
    /// <summary>Gets or sets the log message.</summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>Gets or sets the UTC timestamp of the log entry.</summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>Represents a normalized log entry DTO with PascalCase enum values.</summary>
public class NormalizedLogEntryDto
{
    /// <summary>Gets or sets the normalized log level.</summary>
    public DestLogLevel Level { get; set; }
    /// <summary>Gets or sets the log message.</summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>Gets or sets the UTC timestamp of the log entry.</summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="LegacyLogEntry"/> to <see cref="NormalizedLogEntryDto"/>.
/// </summary>
/// <remarks>
/// This mapper does NOT declare <see cref="EnumMappingStrategyAttribute"/> explicitly.
/// It inherits <c>EnumIgnoreCase = true</c> from the assembly-level <c>[assembly: MapperDefaults]</c>
/// declared in <c>AssemblyConfig.cs</c>, which causes <c>SourceLogLevel.VERBOSE</c> to match
/// <c>DestLogLevel.Verbose</c> despite the casing difference.
/// </remarks>
[Mapper]
public partial class LogEntryMapper
{
    /// <summary>
    /// Maps a <see cref="LegacyLogEntry"/> to a <see cref="NormalizedLogEntryDto"/>.
    /// Enum values are matched case-insensitively due to the assembly-level <c>[MapperDefaults(EnumIgnoreCase = true)]</c>.
    /// </summary>
    /// <param name="source">The legacy log entry to map from.</param>
    /// <returns>A new <see cref="NormalizedLogEntryDto"/> with normalized enum values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial NormalizedLogEntryDto Map(LegacyLogEntry source);
}

/// <summary>Demonstrates the effect of <see cref="MapperDefaultsAttribute"/> on mapper behavior.</summary>
public static class MapperDefaultsDemo
{
    /// <summary>Runs the <c>[assembly: MapperDefaults]</c> demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 2: [assembly: MapperDefaults] - Assembly-Level Configuration ===");
        Console.WriteLine();
        Console.WriteLine("  Declared in AssemblyConfig.cs:");
        Console.WriteLine("    [assembly: MapperDefaults(");
        Console.WriteLine("        StrictMapping = true,");
        Console.WriteLine("        EnumMappingStrategy = EnumMappingStrategy.ByName,");
        Console.WriteLine("        EnumIgnoreCase = true)]");
        Console.WriteLine();
        Console.WriteLine("  Effect: ALL mappers in this assembly inherit EnumIgnoreCase = true.");
        Console.WriteLine("  LogEntryMapper maps SourceLogLevel.VERBOSE → DestLogLevel.Verbose");
        Console.WriteLine("  without any [EnumMappingStrategy] attribute on the class or method.");
        Console.WriteLine();

        var mapper = new LogEntryMapper();
        var entries = new[]
        {
            new LegacyLogEntry { Level = SourceLogLevel.VERBOSE, Message = "Starting up...",      Timestamp = DateTime.UtcNow.AddMinutes(-5) },
            new LegacyLogEntry { Level = SourceLogLevel.INFO,    Message = "Server ready.",        Timestamp = DateTime.UtcNow.AddMinutes(-4) },
            new LegacyLogEntry { Level = SourceLogLevel.WARN,    Message = "High memory usage.",   Timestamp = DateTime.UtcNow.AddMinutes(-3) },
            new LegacyLogEntry { Level = SourceLogLevel.ERROR,   Message = "Connection refused.",  Timestamp = DateTime.UtcNow.AddMinutes(-1) },
        };

        foreach (var entry in entries)
        {
            var dto = mapper.Map(entry);
            Console.WriteLine($"    {entry.Level,-10} → {dto.Level,-10}  '{dto.Message}'");
        }

        Console.WriteLine();
        Console.WriteLine("  Notice: No [EnumMappingStrategy] on LogEntryMapper class or method.");
        Console.WriteLine("  The case-insensitive matching is provided by [assembly: MapperDefaults].");
        Console.WriteLine();
    }
}
