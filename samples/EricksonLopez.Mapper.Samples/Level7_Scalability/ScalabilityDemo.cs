// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level7_Scalability;

/// <summary>Represents an entity with a large data payload for throughput measurement.</summary>
public class LargePayloadEntity
{
    /// <summary>Gets or sets the unique identifier of the entity.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the data payload.</summary>
    public string Data { get; set; } = string.Empty;
    /// <summary>Gets or sets the UTC date and time the entity was created.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>Represents a data transfer object for a large payload entity.</summary>
public class LargePayloadDto
{
    /// <summary>Gets or sets the unique identifier of the entity.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the data payload.</summary>
    public string Data { get; set; } = string.Empty;
    /// <summary>Gets or sets the UTC date and time the entity was created.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="LargePayloadEntity"/> to <see cref="LargePayloadDto"/>
/// for high-throughput scenarios.
/// </summary>
[Mapper]
public partial class ScalabilityMapper
{
    /// <summary>Maps a <see cref="LargePayloadEntity"/> to a <see cref="LargePayloadDto"/>.</summary>
    /// <param name="source">The entity to map from.</param>
    /// <returns>A new <see cref="LargePayloadDto"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial LargePayloadDto Map(LargePayloadEntity source);
}

/// <summary>Demonstrates the throughput characteristics of the source-generated mapper under sustained load.</summary>
public static class ScalabilityDemo
{
    /// <summary>Runs the scalability demonstration by mapping one million entities and reporting throughput.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 7: Scalability ===");
        Console.WriteLine("Note: Source-generated mappers provide horizontal scalability implicitly due to zero-allocation where possible,");
        Console.WriteLine("and no reflection overhead. They act as pure functions mapping from A to B.\n");

        var mapper = new ScalabilityMapper();
        var entity = new LargePayloadEntity { Id = Guid.NewGuid(), Data = "Performance Test", CreatedAt = DateTime.UtcNow };

        // Warm up
        _ = mapper.Map(entity);

        int iterations = 1_000_000;
        Console.WriteLine($"Running {iterations:N0} mapping iterations to demonstrate throughput...");

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            _ = mapper.Map(entity);
        }
        sw.Stop();

        Console.WriteLine($"Completed {iterations:N0} mappings in {sw.ElapsedMilliseconds}ms.");
        Console.WriteLine($"Approximate throughput: {(iterations / sw.Elapsed.TotalSeconds):N0} ops/sec.");
        Console.WriteLine();
    }
}
