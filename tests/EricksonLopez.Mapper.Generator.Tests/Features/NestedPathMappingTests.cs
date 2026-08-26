// Copyright © Erickson Lopez. MIT License.
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

public class NestedPathMappingTests
{
    [Fact]
    public void MapProperty_WithNestedPath_ShouldEmitSafeNavigatedAccess()
    {
        string source = @"
public class Address { public string City { get; set; } }
public class Customer { public Address Address { get; set; } }
public class Order { public Customer Customer { get; set; } }
public class OrderDto { public string? City { get; set; } }

[Mapper]
public partial class OrderMapper
{
    [MapProperty(""Customer.Address.City"", ""City"")]
    public partial OrderDto Map(Order source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("City = source.Customer?.Address?.City");
    }

    [Fact]
    public void MapProperty_WithNestedPathAndNullFallback_ShouldEmitFallbackExpression()
    {
        string source = @"
public class Address { public string City { get; set; } }
public class Customer { public Address Address { get; set; } }
public class Order { public Customer Customer { get; set; } }
public class OrderDto { public string City { get; set; } }

[Mapper]
public partial class OrderMapper
{
    [MapProperty(""Customer.Address.City"", ""City"")]
    [MapNullFallback(""City"", ""\""Unknown\"""")]
    public partial OrderDto Map(Order source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("City = (source.Customer?.Address?.City ?? \"Unknown\")");
    }

    [Fact]
    public void MapProperty_WithNestedPathIntoConstructor_ShouldPassNestedAccessor()
    {
        string source = @"
public class Address { public string City { get; set; } }
public class Customer { public Address Address { get; set; } }
public class Order { public Customer Customer { get; set; } }
public class OrderDto
{
    public string? City { get; }
    public OrderDto(string? city) { City = city; }
}

[Mapper]
public partial class OrderMapper
{
    [MapProperty(""Customer.Address.City"", ""city"")]
    public partial OrderDto Map(Order source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::TestNamespace.OrderDto(source.Customer?.Address?.City)");
    }

    [Fact]
    public void MapProperty_WithNestedPathIntoFactoryMethod_ShouldPassNestedAccessor()
    {
        string source = @"
public class Address { public string City { get; set; } }
public class Customer { public Address Address { get; set; } }
public class Order { public Customer Customer { get; set; } }
public class OrderDto
{
    public string? City { get; }
    private OrderDto(string? city) { City = city; }
    public static OrderDto Create(string? city) => new(city);
}

[Mapper]
public partial class OrderMapper
{
    [MapFactory(""Create"")]
    [MapProperty(""Customer.Address.City"", ""city"")]
    public partial OrderDto Map(Order source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("global::TestNamespace.OrderDto.Create(source.Customer?.Address?.City)");
    }

    [Fact]
    public void MapProperty_WithInvalidNestedPathSegment_ShouldEmitELM001InStrictMode()
    {
        string source = @"
public class Address { public string City { get; set; } }
public class Customer { public Address Address { get; set; } }
public class Order { public Customer Customer { get; set; } }
public class OrderDto { public string? City { get; set; } }

[Mapper]
public partial class OrderMapper
{
    [MapProperty(""Customer.NonExistent.City"", ""City"")]
    public partial OrderDto Map(Order source);
}
";
        var (diagnostics, _, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);

        diagnostics.Should().Contain(d => d.Id == "ELM001" && d.Severity == DiagnosticSeverity.Error);
    }
}
