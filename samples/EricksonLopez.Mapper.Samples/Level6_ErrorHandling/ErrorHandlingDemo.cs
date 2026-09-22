// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level6_ErrorHandling;

/// <summary>Represents an entity whose data may require validation before conversion.</summary>
public class UnreliableEntity
{
    /// <summary>Gets or sets the raw data string to convert. Pass <c>"Error"</c> to trigger a simulated failure.</summary>
    public string Data { get; set; } = string.Empty;
}

/// <summary>Represents a data transfer object with a validated and converted data payload.</summary>
public class ReliableDto
{
    /// <summary>Gets or sets the converted data string.</summary>
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Converts an <see cref="UnreliableEntity"/> to a <see cref="ReliableDto"/>,
/// throwing when the source data contains an invalid value.
/// </summary>
public class UnreliableDataConverter : IConverter<UnreliableEntity, ReliableDto>
{
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The <see cref="UnreliableEntity.Data"/> value is <c>"Error"</c></exception>
    public ReliableDto Convert(UnreliableEntity source)
    {
        if (source.Data == "Error")
        {
            throw new InvalidOperationException("Simulated conversion error");
        }
        return new ReliableDto { Data = source.Data + " (Converted)" };
    }
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="UnreliableEntity"/> to <see cref="ReliableDto"/>
/// using a custom <see cref="UnreliableDataConverter"/>.
/// </summary>
[Mapper]
public partial class ErrorHandlingMapper
{
    /// <summary>Converts an <see cref="UnreliableEntity"/> to a <see cref="ReliableDto"/> using the custom converter.</summary>
    /// <param name="source">The entity to convert.</param>
    /// <returns>A <see cref="ReliableDto"/> containing the converted data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">The source data value is invalid</exception>
    [UseConverter(typeof(UnreliableDataConverter))]
    public partial ReliableDto Map(UnreliableEntity source);
}

/// <summary>Demonstrates runtime error handling when a custom converter throws an exception.</summary>
public static class ErrorHandlingDemo
{
    /// <summary>Runs the error handling demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 6: Error Handling ===");
        Console.WriteLine("Note: The mapper generates highly optimized AOT-safe code, so errors are typically compile-time.");
        Console.WriteLine("For runtime errors (e.g., from custom converters), standard try-catch is used, as the mapper does not implement specific retry/backoff policies natively.\n");

        var mapper = new ErrorHandlingMapper();

        var validSource = new UnreliableEntity { Data = "Valid" };
        var errorSource = new UnreliableEntity { Data = "Error" };

        Console.WriteLine("Mapping valid source:");
        try
        {
            var result = mapper.Map(validSource);
            Console.WriteLine($"Result: {result.Data}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("\nMapping invalid source (will throw):");
        try
        {
            var result = mapper.Map(errorSource);
            Console.WriteLine($"Result: {result.Data}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Caught Error: {ex.Message}");
            Console.WriteLine("  -> Here you could apply Polly or custom Retry/DeadLetter strategies if needed.");
        }
        Console.WriteLine();
    }
}


