// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level3_RealWorld;

// =============================================================================
// Level 3 - Hierarchy Flattening with Deep Dot-Path [MapProperty]
//
// Problem:
//   A deep object hierarchy (e.g., Order.Customer.Address.City) must be
//   projected into a scalar property of a flat DTO (FlatOrderDto.CityName).
//   Without dot-path support this would require either a custom IConverter
//   or multiple intermediate mappers.
//
// Solution:
//   [MapProperty("Customer.Address.City.City", "CityName")]
//   The generator resolves the dot-separated path at compile time and emits
//   null-safe navigation code automatically (via MemberResolutionEngine.ResolvePropertyPath).
//
// Generator behavior:
//   - Segments are resolved case-insensitively.
//   - If any intermediate segment is a nullable reference type or nullable
//     value type, the generator inserts null-conditional operators (?.).
//   - The feature is implemented in MemberResolutionEngine.ResolvePropertyPath().
//
// When to use:
//   - Flattening rich domain objects to DTOs/ViewModels.
//   - Extracting a scalar value from a deeply nested aggregate.
//   - Report generation where only leaf-level values are needed.
//
// When NOT to use:
//   - When the flat DTO needs many fields — consider splitting into sub-mappers.
//   - When the nested structure is too deep (prefer explicit sub-mapping methods).
// =============================================================================

// --- Domain Model ---

/// <summary>Represents city-level address details.</summary>
public class CityInfo
{
    /// <summary>Gets or sets the city name.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the postal/ZIP code.</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the country code (ISO 3166-1 alpha-2).</summary>
    public string CountryCode { get; set; } = string.Empty;
}

/// <summary>Represents a physical address, including a reference to city details.</summary>
public class AddressInfo
{
    /// <summary>Gets or sets the street line (street name and number).</summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>Gets or sets the city details for this address.</summary>
    public CityInfo City { get; set; } = new();
}

/// <summary>Represents customer profile data.</summary>
public class CustomerProfile
{
    /// <summary>Gets or sets the full name of the customer.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer's primary shipping address.</summary>
    public AddressInfo Address { get; set; } = new();
}

/// <summary>Represents a sales order aggregate, combining order metadata with customer profile data.</summary>
public class OrderAggregate
{
    /// <summary>Gets or sets the unique order reference code.</summary>
    public string OrderRef { get; set; } = string.Empty;

    /// <summary>Gets or sets the total order value in the base currency.</summary>
    public decimal Total { get; set; }

    /// <summary>Gets or sets the customer profile associated with this order.</summary>
    public CustomerProfile Customer { get; set; } = new();
}

// --- Flat DTO ---

/// <summary>Represents a flattened order projection for reporting and read-only view models.</summary>
public class FlatOrderDto
{
    /// <summary>Gets or sets the unique order reference code.</summary>
    public string OrderRef { get; set; } = string.Empty;

    /// <summary>Gets or sets the total order value.</summary>
    public decimal Total { get; set; }

    /// <summary>Gets or sets the customer's full name (flattened from <c>Customer.FullName</c>).</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer's street (flattened from <c>Customer.Address.Street</c>).</summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer's city name (flattened from <c>Customer.Address.City.City</c>).</summary>
    public string CityName { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer's postal code (flattened from <c>Customer.Address.City.PostalCode</c>).</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer's country code (flattened from <c>Customer.Address.City.CountryCode</c>).</summary>
    public string CountryCode { get; set; } = string.Empty;
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="OrderAggregate"/> to <see cref="FlatOrderDto"/>,
/// demonstrating multi-level dot-path flattening with <see cref="MapPropertyAttribute"/>.
/// </summary>
/// <remarks>
/// The generator resolves each dot-separated path at compile time via
/// <c>MemberResolutionEngine.ResolvePropertyPath()</c>, producing null-safe
/// navigation chains. No intermediate mapper types are required.
/// </remarks>
[Mapper]
public partial class HierarchyFlatteningMapper
{
    /// <summary>
    /// Maps an <see cref="OrderAggregate"/> to a <see cref="FlatOrderDto"/>,
    /// flattening multiple nested property paths into scalar destination members.
    /// </summary>
    /// <param name="source">The order aggregate to flatten.</param>
    /// <returns>A new <see cref="FlatOrderDto"/> with all nested values projected as scalar properties.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    /// <remarks>
    /// When intermediate path segments are reference types, the generator produces a nullable
    /// chain (<c>source.Customer?.FullName</c>). Because the destination properties are non-nullable
    /// <see langword="string"/>, <see cref="MapNullFallbackAttribute"/> is required alongside each
    /// <see cref="MapPropertyAttribute"/> to provide the null-coalescing fallback. The generated code is:
    /// <code>
    /// FullName    = source.Customer?.FullName ?? string.Empty,
    /// Street      = source.Customer?.Address?.Street ?? string.Empty,
    /// CityName    = source.Customer?.Address?.City?.City ?? string.Empty,
    /// PostalCode  = source.Customer?.Address?.City?.PostalCode ?? string.Empty,
    /// CountryCode = source.Customer?.Address?.City?.CountryCode ?? string.Empty,
    /// </code>
    /// </remarks>
    [MapProperty("Customer.FullName", "FullName")]
    [MapProperty("Customer.Address.Street", "Street")]
    [MapProperty("Customer.Address.City.City", "CityName")]
    [MapProperty("Customer.Address.City.PostalCode", "PostalCode")]
    [MapProperty("Customer.Address.City.CountryCode", "CountryCode")]
    [MapNullFallback("FullName", "string.Empty")]
    [MapNullFallback("Street", "string.Empty")]
    [MapNullFallback("CityName", "string.Empty")]
    [MapNullFallback("PostalCode", "string.Empty")]
    [MapNullFallback("CountryCode", "string.Empty")]
    public partial FlatOrderDto Flatten(OrderAggregate source);
}

/// <summary>Demonstrates hierarchy flattening using multi-level dot-path notation in <see cref="MapPropertyAttribute"/>.</summary>
public static class HierarchyFlatteningDemo
{
    /// <summary>Runs the hierarchy flattening demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 3: Hierarchy Flattening ([MapProperty(\"A.B.C\", \"Dest\")]) ===");
        Console.WriteLine();
        Console.WriteLine("  The generator resolves dot-separated paths at compile time.");
        Console.WriteLine("  Intermediate reference types automatically use ?. null-conditional operators.");
        Console.WriteLine();

        var mapper = new HierarchyFlatteningMapper();

        var order = new OrderAggregate
        {
            OrderRef = "ORD-2024-001",
            Total = 2499.99m,
            Customer = new CustomerProfile
            {
                FullName = "Jane Smith",
                Address = new AddressInfo
                {
                    Street = "42 Oak Avenue",
                    City = new CityInfo
                    {
                        City = "Springfield",
                        PostalCode = "62701",
                        CountryCode = "US"
                    }
                }
            }
        };

        var dto = mapper.Flatten(order);

        Console.WriteLine("  Source (3 levels deep):");
        Console.WriteLine($"    Customer.FullName           = \"{order.Customer.FullName}\"");
        Console.WriteLine($"    Customer.Address.Street     = \"{order.Customer.Address.Street}\"");
        Console.WriteLine($"    Customer.Address.City.City  = \"{order.Customer.Address.City.City}\"");
        Console.WriteLine($"    Customer.Address.City.Zip   = \"{order.Customer.Address.City.PostalCode}\"");
        Console.WriteLine();
        Console.WriteLine("  Target FlatOrderDto (all scalar):");
        Console.WriteLine($"    dto.OrderRef    = \"{dto.OrderRef}\"");
        Console.WriteLine($"    dto.Total       = {dto.Total:C}");
        Console.WriteLine($"    dto.FullName    = \"{dto.FullName}\"      ← [MapProperty(\"Customer.FullName\", \"FullName\")]");
        Console.WriteLine($"    dto.Street      = \"{dto.Street}\"   ← [MapProperty(\"Customer.Address.Street\", \"Street\")]");
        Console.WriteLine($"    dto.CityName    = \"{dto.CityName}\"     ← [MapProperty(\"Customer.Address.City.City\", \"CityName\")]");
        Console.WriteLine($"    dto.PostalCode  = \"{dto.PostalCode}\"       ← [MapProperty(\"Customer.Address.City.PostalCode\", \"PostalCode\")]");
        Console.WriteLine($"    dto.CountryCode = \"{dto.CountryCode}\"         ← [MapProperty(\"Customer.Address.City.CountryCode\", \"CountryCode\")]");
        Console.WriteLine();
    }
}
