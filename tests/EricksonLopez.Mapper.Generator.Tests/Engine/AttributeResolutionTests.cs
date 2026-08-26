// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Engine;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

[Trait("Category", "FastAst")]
public class AttributeResolutionTests
{
    [Fact]
    public void Resolve_WhenShortAttributeNamesUsed_ShouldGenerateCorrectMapping()
    {
        string source = @"
using EricksonLopez.Mapper;

namespace TestNamespace;

public class BaseSource { public string Name { get; set; } = """"; }
public class DerivedSource : BaseSource { public int Age { get; set; } }

public class BaseDest { public string Name { get; set; } = """"; }
public class DerivedDest : BaseDest { public int Age { get; set; } }

public class Source
{
    public string Name { get; set; } = """";
    public string IgnoredSrc { get; set; } = """";
    public string IgnoredDst { get; set; } = """";
    public string? NullableVal { get; set; }
}

public class Dest
{
    public string Name { get; set; } = """";
    public string IgnoredDst { get; set; } = """";
    public string FallbackVal { get; set; } = """";
}

public class FactoryDest
{
    public string Name { get; }
    public string Extra { get; }
    private FactoryDest(string name, string extra) { Name = name; Extra = extra; }
    public static FactoryDest Create(string name, string extra) => new FactoryDest(name, extra);
    public static FactoryDest Create(string name) => new FactoryDest(name, """");
}

public class ConvSource { public string Text { get; set; } = """"; }
public class ConvDest { public string Text { get; set; } = """"; }

public class CustomConv : IConverter<ConvSource, ConvDest>
{
    public ConvDest Convert(ConvSource source) => new ConvDest { Text = source.Text.ToUpper() };
}

[Mapper(StrictMapping = false)]
public partial class MultiAttrMapper
{
    [MapIgnore(""IgnoredDst"")]
    [MapIgnoreSource(""IgnoredSrc"")]
    [MapNullFallback(""FallbackVal"", ""\""Default\"""")]
    [MapProperty(""NullableVal"", ""FallbackVal"")]
    public partial Dest Map(Source source);

    [MapDerivedType(typeof(DerivedSource), typeof(DerivedDest))]
    public partial BaseDest MapPoly(BaseSource source);
    public partial DerivedDest MapDerived(DerivedSource source);

    [MapFactory(""Create"")]
    public partial FactoryDest MapFactoryMethod(Source source);

    [UseConverter(typeof(CustomConv))]
    public partial ConvDest MapWithConv(ConvSource source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Name = source.Name");
        output.Should().Contain("FallbackVal = (source.NullableVal ?? \"Default\")");
        output.Should().NotContain("IgnoredDst =");
        output.Should().Contain("FactoryDest.Create(source.Name)");
        output.Should().Contain("var converter = new global::TestNamespace.CustomConv();");
        output.Should().Contain("return converter.Convert(source);");
    }

    [Fact]
    public void Resolve_WhenFullAttributeNamesUsed_ShouldGenerateCorrectMapping()
    {
        string source = @"

namespace TestNamespace;

public class BaseSource { public string Name { get; set; } = """"; }
public class DerivedSource : BaseSource { public int Age { get; set; } }

public class BaseDest { public string Name { get; set; } = """"; }
public class DerivedDest : BaseDest { public int Age { get; set; } }

public class Source
{
    public string Name { get; set; } = """";
    public string IgnoredSrc { get; set; } = """";
    public string IgnoredDst { get; set; } = """";
    public string? NullableVal { get; set; }
}

public class Dest
{
    public string Name { get; set; } = """";
    public string IgnoredDst { get; set; } = """";
    public string FallbackVal { get; set; } = """";
}

public class FactoryDest
{
    public string Name { get; }
    private FactoryDest(string name) { Name = name; }
    public static FactoryDest Make(string name) => new FactoryDest(name);
}

public class ConvSource { public string Text { get; set; } = """"; }
public class ConvDest { public string Text { get; set; } = """"; }

public class CustomConv : IConverter<ConvSource, ConvDest>
{
    public ConvDest Convert(ConvSource source) => new ConvDest { Text = source.Text.ToLower() };
}

[Mapper(StrictMapping = false)]
public partial class MultiAttrMapperFull
{
    [MapIgnoreAttribute(""IgnoredDst"")]
    [MapIgnoreSourceAttribute(""IgnoredSrc"")]
    [MapNullFallbackAttribute(""FallbackVal"", ""\""FallbackVal\"""")]
    [MapProperty(""NullableVal"", ""FallbackVal"")]
    public partial Dest Map(Source source);

    [MapDerivedTypeAttribute(typeof(DerivedSource), typeof(DerivedDest))]
    public partial BaseDest MapPoly(BaseSource source);
    public partial DerivedDest MapDerived(DerivedSource source);

    [MapFactoryAttribute(""Make"")]
    public partial FactoryDest MapFactoryMethod(Source source);

    [UseConverterAttribute(typeof(CustomConv))]
    public partial ConvDest MapWithConv(ConvSource source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Name = source.Name");
        output.Should().Contain("FallbackVal = (source.NullableVal ?? \"FallbackVal\")");
        output.Should().NotContain("IgnoredDst =");
        output.Should().Contain("FactoryDest.Make(source.Name)");
        output.Should().Contain("var converter = new global::TestNamespace.CustomConv();");
        output.Should().Contain("return converter.Convert(source);");
    }

    [Fact]
    public void Resolve_WhenMultipleConstructorsAmbiguous_ShouldEmitELM007()
    {
        string source = @"

namespace TestNamespace;

public class Source
{
    public int Id { get; set; }
    public string Name { get; set; } = """";
    public string Email { get; set; } = """";
}

public class TargetWithManyCtors
{
    public int Id { get; }
    public string Name { get; }
    public string Email { get; }

    public TargetWithManyCtors(int id) { Id = id; Name = """"; Email = """"; }
    public TargetWithManyCtors(int id, string name) { Id = id; Name = name; Email = """"; }
    public TargetWithManyCtors(int id, string name, string email) { Id = id; Name = name; Email = email; }
}

[Mapper]
public partial class CtorOrderMapper
{
    public partial TargetWithManyCtors Map(Source source);
}
";
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Should().Contain(d => d.Id == "ELM007");
    }

    [Fact]
    public void Resolve_WhenAmbiguousPropertyMatchingCaseInsensitive_ShouldEmitELM005()
    {
        string source = @"

namespace TestNamespace;

public class Source
{
    public string USERNAME { get; set; } = """";
    public string username { get; set; } = """";
}

public class Dest
{
    public string UserName { get; set; } = """";
}

[Mapper]
public partial class AmbiguousMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorSimple(source);
        var ambiguousDiag = diagnostics.FirstOrDefault(d => d.Id == "ELM005");
        ambiguousDiag.Should().NotBeNull();
        ambiguousDiag!.Location.GetLineSpan().Path.Should().NotBeNull();
    }
}


