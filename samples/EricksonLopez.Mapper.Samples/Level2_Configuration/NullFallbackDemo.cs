// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level2_Configuration;

// =============================================================================
// Level 2 - [MapNullFallback] Attribute
//
// Problem:
//   When the source has a nullable value-type property (int?, decimal?) and the
//   destination has the same property as non-nullable (int, decimal), the generator
//   emits ELM004 (Nullability Mismatch). [UseConverter] solves this but is
//   excessive for simple cases.
//
// Solution:
//   [MapNullFallback("DestinationPropertyName", "fallbackCSharpExpression")]
//   Embeds directly into the generated code: `(source.Prop ?? fallbackExpr)`.
//
// How it works:
//   The generator emits literally:
//     Price = (decimal)(source.Price ?? 0m),
//     StockQuantity = (int)(source.StockQuantity ?? 0),
//
// The second argument is a literal C# expression embedded verbatim.
// It must be a valid C# expression compatible with the destination type.
//
// When to use:
//   - Nullable value-type properties (int?, decimal?, Guid?, DateTime?) that
//     map to non-nullable destinations and have a sensible default value.
//   - Lightweight alternative to [UseConverter] when the logic is trivial.
//
// When NOT to use:
//   - When fallback logic is conditional or complex -> use IConverter.
//   - When the fallback depends on other source fields.
// =============================================================================

/// <summary>Represents a product catalog entry with optional stock and pricing data.</summary>
public class ProductCatalogEntity
{
    /// <summary>Gets or sets the unique numeric identifier of the product.</summary>
    public int ProductId { get; set; }
    /// <summary>Gets or sets the display name of the product.</summary>
    public string Name { get; set; }
    /// <summary>Gets or sets the product description.</summary>
    public string Description { get; set; }

    /// <summary>Gets or sets the available stock quantity, or <see langword="null"/> when unknown.</summary>
    public int? StockQuantity { get; set; }

    /// <summary>Gets or sets the unit price, or <see langword="null"/> when no price has been assigned.</summary>
    public decimal? Price { get; set; }
}

/// <summary>Represents a product catalog data transfer object with all properties required to be non-nullable.</summary>
public class ProductCatalogDto
{
    /// <summary>Gets or sets the unique numeric identifier of the product.</summary>
    public int ProductId { get; set; }
    /// <summary>Gets or sets the display name of the product.</summary>
    public string Name { get; set; }
    /// <summary>Gets or sets the product description.</summary>
    public string Description { get; set; }
    /// <summary>Gets or sets the available stock quantity. Defaults to zero when the source value was <see langword="null"/>.</summary>
    public int StockQuantity { get; set; }
    /// <summary>Gets or sets the unit price. Defaults to zero when the source value was <see langword="null"/>.</summary>
    public decimal Price { get; set; }
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="ProductCatalogEntity"/> to <see cref="ProductCatalogDto"/>,
/// applying null fallback expressions for nullable-to-non-nullable value-type conversions.
/// </summary>
[Mapper]
public partial class ProductCatalogMapper
{
    /// <summary>
    /// Maps a <see cref="ProductCatalogEntity"/> to a <see cref="ProductCatalogDto"/>,
    /// substituting <c>0</c> for <see langword="null"/> stock quantities and <c>0m</c> for <see langword="null"/> prices.
    /// </summary>
    /// <param name="source">The product catalog entity to map from.</param>
    /// <returns>A new <see cref="ProductCatalogDto"/> with all fields populated.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    [MapNullFallback("StockQuantity", "0")]
    [MapNullFallback("Price", "0m")]
    public partial ProductCatalogDto Map(ProductCatalogEntity source);
}

/// <summary>Demonstrates null fallback expressions using <see cref="MapNullFallbackAttribute"/> for nullable-to-non-nullable mappings.</summary>
public static class NullFallbackDemo
{
    /// <summary>Runs the null fallback demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 2: [MapNullFallback] - Null Safety for Nullable Value Types ===");
        Console.WriteLine();

        var mapper = new ProductCatalogMapper();

        // Complete product
        var completeProduct = new ProductCatalogEntity
        {
            ProductId = 1,
            Name = "Laptop Pro",
            Description = "High-end developer laptop",
            StockQuantity = 50,
            Price = 1299.99m
        };

        // Incomplete product (e.g., legacy data)
        var incompleteProduct = new ProductCatalogEntity
        {
            ProductId = 2,
            Name = "Legacy Item",
            Description = "Item without stock/price data",
            StockQuantity = null,
            Price = null
        };

        var dto1 = mapper.Map(completeProduct);
        var dto2 = mapper.Map(incompleteProduct);

        Console.WriteLine("  Complete product:");
        Console.WriteLine($"    StockQuantity : {dto1.StockQuantity}");
        Console.WriteLine($"    Price         : {dto1.Price:C}");
        Console.WriteLine();
        Console.WriteLine("  Product with null int? and decimal? (applying fallbacks):");
        Console.WriteLine($"    StockQuantity : {dto2.StockQuantity}  <- fallback: 0  (source: null)");
        Console.WriteLine($"    Price         : {dto2.Price:C}   <- fallback: 0m (source: null)");
        Console.WriteLine();
        Console.WriteLine("  Equivalent generated code:");
        Console.WriteLine("    StockQuantity = (int)(source.StockQuantity ?? 0),");
        Console.WriteLine("    Price         = (decimal)(source.Price ?? 0m),");
        Console.WriteLine();
    }
}
