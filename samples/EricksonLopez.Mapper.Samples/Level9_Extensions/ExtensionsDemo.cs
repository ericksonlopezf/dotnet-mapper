// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;
using Microsoft.Extensions.DependencyInjection;

namespace EricksonLopez.Mapper.Sample.Level9_Extensions;

/// <summary>Represents an order entity with a numeric identifier.</summary>
public class OrderEntity
{
    /// <summary>Gets or sets the unique numeric order identifier.</summary>
    public int OrderId { get; set; }
}

/// <summary>Represents a data transfer object for an order.</summary>
public class OrderDto
{
    /// <summary>Gets or sets the unique numeric order identifier.</summary>
    public int OrderId { get; set; }
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="OrderEntity"/> to <see cref="OrderDto"/>.
/// </summary>
[Mapper]
public partial class ExtensionsMapper
{
    /// <summary>Maps an <see cref="OrderEntity"/> to an <see cref="OrderDto"/>.</summary>
    /// <param name="source">The order entity to map from.</param>
    /// <returns>A new <see cref="OrderDto"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public partial OrderDto Map(OrderEntity source);
}

/// <summary>Demonstrates resolving a mapper instance from an <c>IServiceCollection</c> dependency injection container.</summary>
public static class ExtensionsDemo
{
    /// <summary>Runs the extensions demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 9: Extensions ===");
        Console.WriteLine("Note: The mapper does not natively support third-party brokers (RabbitMQ/Kafka) as plugins.");
        Console.WriteLine("However, it generates standard IServiceCollection extension methods to easily integrate with the modern .NET ecosystem.\n");

        var services = new ServiceCollection();

        // This method is generated automatically when [GenerateMapperRegistration] is used on the assembly
        // Since we are in the sample project, we would call: services.AddMappers();
        // Here we just simulate registering our mapper:
        services.AddSingleton<ExtensionsMapper>();

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<ExtensionsMapper>();

        var result = mapper.Map(new OrderEntity { OrderId = 42 });
        Console.WriteLine($"Resolved mapper from DI container and mapped OrderId: {result.OrderId}");
        Console.WriteLine();
    }
}


