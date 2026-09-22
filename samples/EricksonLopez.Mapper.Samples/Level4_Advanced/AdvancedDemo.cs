// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level4_Advanced;

/// <summary>Represents a product entity in the domain layer.</summary>
public class ProductEntity
{
    /// <summary>Gets or sets the unique identifier of the product.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the name of the product.</summary>
    public string Name { get; set; }
    /// <summary>Gets or sets the unit price of the product.</summary>
    public decimal Price { get; set; }
}

/// <summary>
/// Represents an immutable data transfer object for a product, using a primary constructor.
/// </summary>
/// <remarks>
/// The generator detects the primary constructor and maps source properties
/// to constructor parameters by name (case-insensitive), producing type-safe construction.
/// </remarks>
/// <param name="Id">The unique identifier of the product.</param>
/// <param name="Name">The name of the product.</param>
/// <param name="Price">The unit price of the product.</param>
public record ProductDto(Guid Id, string Name, decimal Price);

/// <summary>
/// Provides compile-time-generated mapping from <see cref="ProductEntity"/> to the immutable <see cref="ProductDto"/> record.
/// </summary>
[Mapper]
public partial class ProductMapper
{
    /// <summary>Maps a <see cref="ProductEntity"/> to a <see cref="ProductDto"/>.</summary>
    /// <param name="source">The product entity to map from.</param>
    /// <returns>A new immutable <see cref="ProductDto"/> populated via its primary constructor.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial ProductDto Map(ProductEntity source);
}

/// <summary>Demonstrates mapping to an immutable record type using a primary constructor.</summary>
public static class AdvancedDemo
{
    /// <summary>Runs the advanced records mapping demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 4: Advanced (Records & Primary Constructors) ===");

        var source = new ProductEntity { Id = Guid.NewGuid(), Name = "Laptop", Price = 1200.50m };
        var mapper = new ProductMapper();

        var target = mapper.Map(source);

        Console.WriteLine($"Mapped into Immutable Record: Id={target.Id}, Name={target.Name}, Price={target.Price}");
        Console.WriteLine();
    }
}
