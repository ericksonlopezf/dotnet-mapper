// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using EricksonLopez.Mapper;


namespace EricksonLopez.Mapper.Sample.Level5_Processing;

/// <summary>Represents a raw data record to be processed.</summary>
public class DataRecord
{
    /// <summary>Gets or sets the unique numeric identifier of the record.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the raw data payload of the record.</summary>
    public string Data { get; set; } = string.Empty;
}

/// <summary>Represents a data transfer object for a processed data record.</summary>
public class DataRecordDto
{
    /// <summary>Gets or sets the unique numeric identifier of the record.</summary>
    public int Id { get; set; }
    /// <summary>Gets or sets the data payload.</summary>
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="DataRecord"/> to <see cref="DataRecordDto"/>.
/// </summary>
/// <remarks>
/// The generated mapping method is stateless and thread-safe, making it safe to use
/// in parallel or concurrent scenarios such as PLINQ pipelines.
/// </remarks>
[Mapper]
public partial class BatchMapper
{
    /// <summary>Maps a <see cref="DataRecord"/> to a <see cref="DataRecordDto"/>.</summary>
    /// <param name="source">The data record to map from.</param>
    /// <returns>A new <see cref="DataRecordDto"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial DataRecordDto Map(DataRecord source);
}

/// <summary>Demonstrates batch and parallel mapping using PLINQ with a source-generated mapper.</summary>
public static class ProcessingDemo
{
    /// <summary>Runs the batch processing demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 5: Processing (Batch & Concurrency) ===");
        Console.WriteLine("Note: The mapper is purely synchronous and does not have native async/background processing APIs.");
        Console.WriteLine("However, because the generated code is stateless and thread-safe, it can be easily used in PLINQ or Task.Run.\n");

        var mapper = new BatchMapper();

        // Simulating a batch of 100,000 records
        var sourceBatch = Enumerable.Range(1, 100000).Select(i => new DataRecord { Id = i, Data = $"Data {i}" }).ToList();

        Console.WriteLine($"Starting parallel mapping of {sourceBatch.Count:N0} records...");

        // Processing in parallel using PLINQ
        var parallelMapped = sourceBatch.AsParallel().Select(mapper.Map).ToList();

        Console.WriteLine($"Successfully mapped {parallelMapped.Count:N0} records concurrently.");
        Console.WriteLine();
    }
}



