// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper.Generator.Tests.Infrastructure;

/// <summary>
/// Centralized reusable C# syntax snippets and models for source generator tests.
/// Eliminates duplicated string literals across test suites.
/// </summary>
public static class TestSnippets
{
    /// <summary>Simple Source and Destination class snippet with matching Name and Age properties.</summary>
    public const string SimpleUserAndDto = @"
public class User
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class UserDto
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}";

    /// <summary>Source and Destination classes with Address entities for nested mapping.</summary>
    public const string UserWithAddressAndDto = @"
public class Address { public string Street { get; set; } = string.Empty; public string City { get; set; } = string.Empty; }
public class AddressDto { public string Street { get; set; } = string.Empty; public string City { get; set; } = string.Empty; }
public class User { public string Name { get; set; } = string.Empty; public Address Address { get; set; } = new(); }
public class UserDto { public string Name { get; set; } = string.Empty; public AddressDto Address { get; set; } = new(); }";

    /// <summary>Collection source and destination classes with lists, dictionaries, and arrays.</summary>
    public const string CollectionsModels = @"
public class CollectionSource
{
    public List<string> Tags { get; set; } = new();
    public int[] Numbers { get; set; } = Array.Empty<int>();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class CollectionDest
{
    public List<string> Tags { get; set; } = new();
    public int[] Numbers { get; set; } = Array.Empty<int>();
    public Dictionary<string, string> Metadata { get; set; } = new();
}";

    /// <summary>Positional record models for constructor mapping verification.</summary>
    public const string PositionalRecordModels = @"
public record PersonSource(string FirstName, string LastName, int Age);
public record PersonDest(string FirstName, string LastName, int Age);";

    /// <summary>Domain models with private constructors and static factory methods.</summary>
    public const string FactoryMethodModels = @"
public class ProductSource { public string Sku { get; set; } = string.Empty; public decimal Price { get; set; } }
public class ProductDest
{
    public string Sku { get; }
    public decimal Price { get; }
    private ProductDest(string sku, decimal price) { Sku = sku; Price = price; }
    public static ProductDest Create(string sku, decimal price) => new(sku, price);
}";

    /// <summary>Polymorphic base and derived hierarchy models.</summary>
    public const string PolymorphicModels = @"
public abstract class ShapeSource { public string Name { get; set; } = string.Empty; }
public class CircleSource : ShapeSource { public double Radius { get; set; } }
public class SquareSource : ShapeSource { public double Side { get; set; } }

public abstract class ShapeDest { public string Name { get; set; } = string.Empty; }
public class CircleDest : ShapeDest { public double Radius { get; set; } }
public class SquareDest : ShapeDest { public double Side { get; set; } }";

    /// <summary>Wrap a mapper declaration in a standard test namespace.</summary>
    public static string CreateMapperSource(string mapperDeclaration, string modelDefinitions)
    {
        return $@"
using EricksonLopez.Mapper;
using System;
using System.Collections.Generic;

namespace TestNamespace;

{modelDefinitions}

{mapperDeclaration}
";
    }
}

