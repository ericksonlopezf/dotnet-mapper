// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level2_Configuration;

// =============================================================================
// Level 2 - Static Partial Class Mapper (Stateless, No Instance Required)
//
// Problem:
//   You want a mapper that does not require instantiation, has no state,
//   and can be used directly in LINQ pipelines or functional code without
//   creating an object instance.
//
// Solution:
//   Declare the mapper class as `static partial`. The generator emits
//   `public static partial` method implementations.
//
// Advantages:
//   - No instance creation overhead.
//   - Can be used directly as a method group in LINQ: items.Select(NotificationMapper.Map)
//   - Not registered by [GenerateMapperRegistration] (correct — no DI instance needed).
//
// Limitation:
//   - Cannot use [UseConverter(nameof(_field))] (no instance fields on static classes).
//   - Cannot be injected as a service. For DI scenarios, use instance mapper instead.
// =============================================================================

/// <summary>Represents a raw notification record from an external queue.</summary>
public class RawNotification
{
    /// <summary>Gets or sets the unique identifier of the notification.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the notification title.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Gets or sets the notification body text.</summary>
    public string Body { get; set; } = string.Empty;
    /// <summary>Gets or sets the UTC creation time.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>Represents a notification data transfer object for the presentation layer.</summary>
public class NotificationDto
{
    /// <summary>Gets or sets the unique identifier of the notification.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the notification title.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Gets or sets the notification body text.</summary>
    public string Body { get; set; } = string.Empty;
    /// <summary>Gets or sets the UTC creation time.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Provides compile-time-generated static mapping from <see cref="RawNotification"/> to <see cref="NotificationDto"/>.
/// </summary>
/// <remarks>
/// Declared as <see langword="static partial"/> so the generator emits <c>public static partial</c> implementations.
/// Static mappers require no instance and can be used directly as method groups in LINQ pipelines.
/// They are NOT registered by <c>[assembly: GenerateMapperRegistration]</c> — static mappers need no DI instance.
/// </remarks>
[Mapper]
public static partial class NotificationMapper
{
    /// <summary>
    /// Maps a <see cref="RawNotification"/> to a <see cref="NotificationDto"/>.
    /// </summary>
    /// <param name="source">The raw notification to map from.</param>
    /// <returns>A new <see cref="NotificationDto"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public static partial NotificationDto Map(RawNotification source);
}

/// <summary>Demonstrates the <c>static partial class</c> mapper pattern.</summary>
public static class StaticMapperDemo
{
    /// <summary>Runs the static mapper demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 2: Static Partial Class Mapper (Stateless, No Instance) ===");
        Console.WriteLine();
        Console.WriteLine("  Declared as: [Mapper] public static partial class NotificationMapper");
        Console.WriteLine("  Generated:   public static partial NotificationDto Map(RawNotification source)");
        Console.WriteLine();
        Console.WriteLine("  Advantages:");
        Console.WriteLine("    - No instance creation overhead.");
        Console.WriteLine("    - Direct use as method group in LINQ: items.Select(NotificationMapper.Map)");
        Console.WriteLine("    - Not registered by [GenerateMapperRegistration] (no DI instance required).");
        Console.WriteLine();

        // Direct call (no new() required)
        var single = new RawNotification
        {
            Id = Guid.NewGuid(),
            Title = "Welcome to EricksonLopez.Mapper",
            Body = "Your account has been activated.",
            CreatedAt = DateTime.UtcNow
        };

        var dto = NotificationMapper.Map(single);
        Console.WriteLine($"  Direct call (no instance): NotificationMapper.Map(source)");
        Console.WriteLine($"    Id: {dto.Id}");
        Console.WriteLine($"    Title: '{dto.Title}'");
        Console.WriteLine();

        // LINQ method group (most common static mapper usage pattern)
        var batch = Enumerable.Range(1, 5)
            .Select(i => new RawNotification
            {
                Id = Guid.NewGuid(),
                Title = $"Notification #{i}",
                Body = $"Message body for notification {i}.",
                CreatedAt = DateTime.UtcNow.AddSeconds(-i)
            })
            .ToList();

        // NotificationMapper.Map as a method group — no lambda, no closure allocation
        List<NotificationDto> dtos = batch.Select(NotificationMapper.Map).ToList();

        Console.WriteLine($"  LINQ method group: batch.Select(NotificationMapper.Map)");
        Console.WriteLine($"    Mapped {dtos.Count} notifications without instantiating a mapper object.");
        foreach (var n in dtos)
        {
            Console.WriteLine($"      - '{n.Title}'");
        }
        Console.WriteLine();
    }
}
