// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

public partial class MapperGeneratorTests
{
    [Fact]
    public async Task Generate_WhenDddEntity_ShouldUseFactoryMethod()
    {
        var source = @"
using EricksonLopez.Mapper;
namespace TestNamespace;
public class Source { public string Name { get; set; } }
public class Dest 
{ 
    public string Name { get; private set; } 
    private Dest(string name) { Name = name; }
    public static Dest Create(string name) => new Dest(name);
}
[Mapper]
public partial class Mapper
{
    [MapFactory(""Create"")]
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenFactoryWithProperties_ShouldMapSuccessfully()
    {
        var source = @"
namespace TestNamespace;
public class Source { public string Name { get; set; } }
public class Dest 
{ 
    public string Name { get; set; }
    public static Dest Create() => new Dest(); 
}
[Mapper]
public partial class Mapper
{
    [MapFactory(""Create"")]
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenMethodMappingWithFactory_ShouldMapSuccessfully()
    {
        var source = @"
using System.Collections.Generic;
namespace TestNamespace;
public class Source { public SourceItem Item { get; set; } }
public class Dest { public DestItem Item { get; set; } }
public class SourceItem { public string Name { get; set; } }
public class DestItem { 
    public string Name { get; } 
    private DestItem(string name) { Name = name; }
    public static DestItem Create(string name) => new DestItem(name);
}
[Mapper]
public partial class Mapper
{
    [MapFactory(nameof(DestItem.Create))]
    public partial DestItem MapItem(SourceItem source);

    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenConstructorWithNullFallback_ShouldMapSuccessfully()
    {
        var source = @"#nullable enable
namespace TestNamespace;
public class Source { public string? Name { get; set; } }
public class Dest { 
    public string Name { get; } 
    public Dest(string name) { Name = name; }
}
[Mapper]
public partial class Mapper
{
    [MapNullFallback(nameof(Dest.Name), ""\""default\"""")]
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenConstructorWithNullabilityMismatch_ShouldFail()
    {
        var source = @"#nullable enable
namespace TestNamespace;
public class Source { public string? Name { get; set; } }
public class Dest { 
    public string Name { get; } 
    public Dest(string name) { Name = name; }
}
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenConstructorWithUnmappedParameter_ShouldFail()
    {
        var source = @"#nullable enable
namespace TestNamespace;
public class Source { }
public class Dest { 
    public string Name { get; } 
    public Dest(string name) { Name = name; }
}
[Mapper(StrictMapping = true)]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenConstructorWithUnsupportedConversion_ShouldFail()
    {
        var source = @"
namespace TestNamespace;
public class Source { public int Value { get; set; } }
public class Dest { 
    public string Value { get; } 
    public Dest(string value) { Value = value; }
}
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenFactoryWithNullFallback_ShouldMapSuccessfully()
    {
        var source = @"#nullable enable
namespace TestNamespace;
public class Source { public string? Name { get; set; } }
public class Dest { 
    public string Name { get; } 
    private Dest(string name) { Name = name; }
    public static Dest Create(string name) => new Dest(name);
}
[Mapper]
public partial class Mapper
{
    [MapFactory(nameof(Dest.Create))]
    [MapNullFallback(nameof(Dest.Name), ""\""default\"""")]
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenFactoryWithNullabilityMismatch_ShouldFail()
    {
        var source = @"#nullable enable
namespace TestNamespace;
public class Source { public string? Name { get; set; } }
public class Dest { 
    public string Name { get; } 
    private Dest(string name) { Name = name; }
    public static Dest Create(string name) => new Dest(name);
}
[Mapper]
public partial class Mapper
{
    [MapFactory(nameof(Dest.Create))]
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenFactoryWithUnmappedParameter_ShouldFail()
    {
        var source = @"#nullable enable
namespace TestNamespace;
public class Source { }
public class Dest { 
    public string Name { get; } 
    private Dest(string name) { Name = name; }
    public static Dest Create(string name) => new Dest(name);
}
[Mapper(StrictMapping = true)]
public partial class Mapper
{
    [MapFactory(nameof(Dest.Create))]
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenFactoryWithUnsupportedConversion_ShouldFail()
    {
        var source = @"
namespace TestNamespace;
public class Source { public int Value { get; set; } }
public class Dest { 
    public string Value { get; } 
    private Dest(string value) { Value = value; }
    public static Dest Create(string value) => new Dest(value);
}
[Mapper]
public partial class Mapper
{
    [MapFactory(nameof(Dest.Create))]
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenAmbiguousConstructor_ShouldEmitELM007()
    {
        var source = @"

namespace TestNamespace;

public class Source { public string Name { get; set; } public int Age { get; set; } public Guid Id { get; set; } }
public class Dest 
{ 
    public Dest(string name, int age) { }
    public Dest(string name, Guid id) { }
}

[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenMissingConstructor_ShouldEmitELM006()
    {
        var source = @"

namespace TestNamespace;

public class Source { public string Name { get; set; } }
public class Dest 
{ 
    private Dest() { }
    public string Name { get; private set; } 
}

[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenMissingConstructor_ShouldFailWithELM002()
    {
        var source = @"
namespace TestNamespace;
public class Source { public string Name { get; set; } }
public class Dest { 
    public string Name { get; } 
    private Dest(string name) { Name = name; }
}
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenMissingFactoryOrConstructor_ShouldEmitELM002()
    {
        var source = @"
namespace TestNamespace;
public class Source { public string Name { get; set; } }
public class Dest 
{ 
    public string Name { get; private set; } 
    private Dest(string name) { Name = name; }
}
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public void Generate_WhenSourceIsClassAndDestIsMutableStruct_ShouldMapDirectly()
    {
        var source = @"
namespace TestNamespace;

public class UserClass { public string Name { get; set; } = string.Empty; public int Age { get; set; } }
public struct UserStruct { public string Name { get; set; } public int Age { get; set; } }

[Mapper]
public partial class StructMapper
{
    public partial UserStruct Map(UserClass source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Name = source.Name");
        output.Should().Contain("Age = source.Age");
    }

    [Fact]
    public void Generate_WhenSourceIsStructAndDestIsClass_ShouldMapDirectly()
    {
        var source = @"
namespace TestNamespace;

public struct PointStruct { public int X { get; set; } public int Y { get; set; } }
public class PointClass { public int X { get; set; } public int Y { get; set; } }

[Mapper]
public partial class PointMapper
{
    public partial PointClass Map(PointStruct source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("X = source.X");
        output.Should().Contain("Y = source.Y");
    }

    [Fact]
    public void Generate_WhenDestIsPositionalStructWithInitOnly_ShouldMapDirectly()
    {
        var source = @"
namespace TestNamespace;

public class OrderSource { public string OrderId { get; set; } = string.Empty; public decimal Amount { get; set; } }
public readonly struct OrderStruct
{
    public string OrderId { get; init; }
    public decimal Amount { get; init; }
}

[Mapper]
public partial class OrderMapper
{
    public partial OrderStruct Map(OrderSource source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("OrderId = source.OrderId");
        output.Should().Contain("Amount = source.Amount");
    }

    [Fact]
    public void Generate_WhenMutableStructWithPublicFields_ShouldIgnoreFields()
    {
        var source = @"
namespace TestNamespace;

public class UserClass { public string Name { get; set; } = string.Empty; public int Age { get; set; } }
public struct UserStruct { public string Name; public int Age; }

[Mapper]
public partial class StructFieldMapper
{
    public partial UserStruct Map(UserClass source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().NotContain("Name =");
        output.Should().NotContain("Age =");
    }

    [Fact]
    public void Generate_WhenSourceAndDestHaveIndexerProperties_ShouldIgnoreIndexersAndMapRegularProperties()
    {
        var source = @"
namespace TestNamespace;

public class IndexedSource
{
    public string Name { get; set; } = string.Empty;
    public string this[int index] { get => string.Empty; set { } }
}

public class IndexedDest
{
    public string Name { get; set; } = string.Empty;
    public string this[int index] { get => string.Empty; set { } }
}

[Mapper]
public partial class IndexedClassMapper
{
    public partial IndexedDest Map(IndexedSource source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Name = source.Name");
        output.Should().NotContain("this[");
    }

    [Fact]
    public void Generate_WhenStructHasIndexers_ShouldIgnoreIndexersAndMapRegularProperties()
    {
        var source = @"
namespace TestNamespace;

public struct IndexedStructSource
{
    public string Title { get; set; }
    public int this[int i] => i;
}

public struct IndexedStructDest
{
    public string Title { get; set; }
    public int this[int i] => i;
}

[Mapper]
public partial class IndexedStructMapper
{
    public partial IndexedStructDest Map(IndexedStructSource source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Title = source.Title");
    }
}




