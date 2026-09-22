// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level3_RealWorld;

/// <summary>Represents a physical address.</summary>
public class Address
{
    /// <summary>Gets or sets the street name and number.</summary>
    public string Street { get; set; }
    /// <summary>Gets or sets the city name.</summary>
    public string City { get; set; }
}

/// <summary>Represents a data transfer object for a physical address.</summary>
public class AddressDto
{
    /// <summary>Gets or sets the street name and number.</summary>
    public string Street { get; set; }
    /// <summary>Gets or sets the city name.</summary>
    public string City { get; set; }
}

/// <summary>Represents a sales order with a shipping address and a list of items.</summary>
public class Order
{
    /// <summary>Gets or sets the unique order identifier.</summary>
    public string OrderId { get; set; }
    /// <summary>Gets or sets the shipping address for the order.</summary>
    public Address ShippingAddress { get; set; }
    /// <summary>Gets or sets the list of item names included in the order.</summary>
    public List<string> Items { get; set; }
}

/// <summary>Represents a data transfer object for a sales order.</summary>
public class OrderDto
{
    /// <summary>Gets or sets the unique order identifier.</summary>
    public string OrderId { get; set; }
    /// <summary>Gets or sets the mapped shipping address.</summary>
    public AddressDto ShippingAddress { get; set; }
    /// <summary>Gets or sets the list of item names included in the order.</summary>
    public List<string> Items { get; set; }
}

/// <summary>
/// Provides compile-time-generated mapping for <see cref="Order"/> and <see cref="Address"/>,
/// demonstrating nested object and collection mapping.
/// </summary>
[Mapper]
public partial class OrderMapper
{
    /// <summary>Maps an <see cref="Order"/> to an <see cref="OrderDto"/>, including its nested address and items.</summary>
    /// <param name="source">The order to map from.</param>
    /// <returns>A new <see cref="OrderDto"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial OrderDto MapOrder(Order source);

    // A source generator requires that nested complex type mappings be declared as separate partial methods.
    /// <summary>Maps an <see cref="Address"/> to an <see cref="AddressDto"/>.</summary>
    /// <param name="source">The address to map from.</param>
    /// <returns>A new <see cref="AddressDto"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial AddressDto MapAddress(Address source);
}

/// <summary>Demonstrates nested object and collection mapping using an order with an address and items.</summary>
public static class RealWorldDemo
{
    /// <summary>Runs the real-world nested mapping demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 3: Real World (Nested & Collections) ===");

        var source = new Order
        {
            OrderId = "ORD-001",
            ShippingAddress = new Address { Street = "123 Main St", City = "Metropolis" },
            Items = new List<string> { "Item1", "Item2", "Item3" }
        };

        var mapper = new OrderMapper();
        var target = mapper.MapOrder(source);

        Console.WriteLine($"Order: {target.OrderId}");
        Console.WriteLine($"Shipping to: {target.ShippingAddress.Street}, {target.ShippingAddress.City}");
        Console.WriteLine($"Items count: {target.Items.Count}");
        Console.WriteLine();
    }
}
