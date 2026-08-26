// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

public partial class MapperGeneratorTests
{
    [Fact]
    public async Task Generate_WhenUnmappedProperty_ShouldEmitELM001()
    {
        var source = @"
using EricksonLopez.Mapper;

namespace TestNamespace;

public class Source { public string Name { get; set; } }
public class Dest { public string Name { get; set; } public string Unmapped { get; set; } }

[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenStrictMappingFalse_ShouldIgnoreUnmapped()
    {
        var source = @"

namespace TestNamespace;

public class Source { public string Name { get; set; } }
public class Dest { public string Name { get; set; } public string Unmapped { get; set; } }

[Mapper(StrictMapping = false)]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenStrictMappingTrueViaSyntax_ShouldFailOnUnmapped()
    {
        var source = @"
namespace TestNamespace;
public class Source { }
public class Dest { public string Unmapped { get; set; } }
[Mapper(StrictMapping = true)]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenUnsupportedConversion_ShouldEmitELM003()
    {
        var source = @"

namespace TestNamespace;

public class Source { public Guid Id { get; set; } }
public class Dest { public DateTime Id { get; set; } }

[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenNullabilityMismatch_ShouldEmitELM004()
    {
        var source = @"#nullable enable

namespace TestNamespace;

public class Source { public string? Name { get; set; } }
public class Dest { public string Name { get; set; } = """"; }

[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenNullabilityMismatchWithFallback_ShouldGenerateCorrectCode()
    {
        var source = @"#nullable enable

namespace TestNamespace;

public class Source { public string? Name { get; set; } }
public class Dest { public string Name { get; set; } }

[Mapper]
public partial class Mapper
{
    [MapNullFallback(""Name"", ""\""Unknown\"""")]
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenAmbiguousPropertyMatch_ShouldEmitELM005()
    {
        var source = @"

namespace TestNamespace;

public class Source { public string nAme { get; set; } public string naMe { get; set; } }
public class Dest { public string Name { get; set; } }

[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenCircularReference_ShouldEmitELM010()
    {
        var source = @"#nullable enable

namespace TestNamespace;

public class Parent
{
    public Child Child { get; set; } = null!;
}

public class Child
{
    public Parent Parent { get; set; } = null!;
}

public class ParentDto
{
    public ChildDto Child { get; set; } = null!;
}

public class ChildDto
{
    public ParentDto Parent { get; set; } = null!;
}

[Mapper]
public partial class Mapper
{
    public partial ParentDto Map(Parent source);
    public partial ChildDto Map(Child source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public void MapIgnoreSource_WhenSingleIgnoredSource_ShouldNotGenerateMapping()
    {
        // T042: [MapIgnoreSource("ExtraField")] causes ExtraField to be excluded from mapping.
        // The destination ExtraField will be treated as unmapped (no assignment in generated code).
        string source = @"

public class Source { public string Name { get; set; } = string.Empty; public string ExtraField { get; set; } = string.Empty; }
public class Dest   { public string Name { get; set; } = string.Empty; }

[Mapper(StrictMapping = false)]
public partial class MyMapper
{
    [MapIgnoreSource(""ExtraField"")]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = RunGeneratorSimple(source);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        // ExtraField is ignored — only Name should appear
        output.Should().Contain("Name = source.Name");
        output.Should().NotContain("ExtraField");
    }

    [Fact]
    public void MapIgnoreSource_WhenMultipleIgnoredSources_ShouldOnlyMapRemainingMembers()
    {
        // T042: Multiple [MapIgnoreSource] on a single method
        string source = @"

public class Source { public string A { get; set; } = string.Empty; public string B { get; set; } = string.Empty; public string C { get; set; } = string.Empty; }
public class Dest   { public string A { get; set; } = string.Empty; public string C { get; set; } = string.Empty; }

[Mapper(StrictMapping = false)]
public partial class MyMapper
{
    [MapIgnoreSource(""B"")]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = RunGeneratorSimple(source);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("A = source.A");
        output.Should().Contain("C = source.C");
        output.Should().NotContain("B");
    }

    [Fact]
    public void ConversionStrategy_WhenTargetIsNullableValueTypeAndInnerIsUnsupported_ShouldEmitELM003()
    {
        string source = @"

public class Source { public string Value { get; set; } }
public class Dest   { public int? Value { get; set; } }

[Mapper]
public partial class MyMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, _) = RunGeneratorSimple(source);
        var diag = diagnostics.Should().ContainSingle(d => d.Id == "ELM003").Subject;
        diag.Severity.Should().Be(DiagnosticSeverity.Error);
        diag.GetMessage().Should().Contain("Value");
    }

    [Fact]
    public void ConversionStrategy_WhenSinglePropertyTypeIsNotValueObject_ShouldFallThrough()
    {
        string source = @"

public class RegularSinglePropClass { public int Age { get; set; } }
public class Source { public int Age { get; set; } }
public class Dest   { public RegularSinglePropClass Target { get; set; } }

[Mapper(StrictMapping = false)]
public partial class MyMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = RunGeneratorSimple(source);
        // Regular single prop class is not treated as VO since property name is Age (not Value) and not annotated
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().NotContain("Target = ");
    }

    [Fact]
    public void ConversionStrategy_WhenTargetHasValuePropertyButNoSingleParamConstructor_ShouldFallThrough()
    {
        string source = @"

public class No1ParamCtorVo { public int Value { get; set; } }
public class Source { public int Value { get; set; } }
public class Dest   { public No1ParamCtorVo Target { get; set; } }

[Mapper(StrictMapping = false)]
public partial class MyMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().NotContain("Target = ");
    }

    [Fact]
    public void ConversionStrategy_WhenSourceHasSinglePropertyNotNamedValueAndNoAttribute_ShouldFallThrough()
    {
        string source = @"

public class NonVoSource { public int OtherProp { get; set; } }
public class Source { public NonVoSource Wrapped { get; set; } }
public class Dest   { public int Wrapped { get; set; } }

[Mapper(StrictMapping = false)]
public partial class MyMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, _) = RunGeneratorSimple(source);
        var diag = diagnostics.Should().ContainSingle(d => d.Id == "ELM003").Subject;
        diag.Severity.Should().Be(DiagnosticSeverity.Error);
        diag.GetMessage().Should().Contain("Wrapped");
    }
}





