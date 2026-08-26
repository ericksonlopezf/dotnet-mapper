// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Mapper;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Core;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

[Trait("Category", "FastAst")]
public class ConverterValidationTests
{
    [Fact]
    public void ValidateConverter_WhenTypeDoesNotImplementIConverter_ShouldEmitELM013()
    {
        string source = @"

public class MySource { }
public class MyDest { }

public class BadConverter { }

[Mapper]
public partial class MyMapper
{
    [UseConverter(typeof(BadConverter))]
    public partial MyDest Map(MySource source);
}
";
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Should().Contain(d => d.Id == "ELM013" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ValidateConverter_WhenTypeArgumentsDoNotMatchSourceAndDestination_ShouldEmitELM013()
    {
        string source = @"

public class MySource { }
public class MyDest { }

public class WrongTypeConverter : IConverter<int, string>
{
    public string Convert(int source) => source.ToString();
}

[Mapper]
public partial class MyMapper
{
    [UseConverter(typeof(WrongTypeConverter))]
    public partial MyDest Map(MySource source);
}
";
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Should().Contain(d => d.Id == "ELM013" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void ValidateConverter_WhenTypeImplementsCorrectIConverter_ShouldNotEmitDiagnostics()
    {
        string source = @"

public class MySource { }
public class MyDest { }

public class GoodConverter : IConverter<MySource, MyDest>
{
    public MyDest Convert(MySource source) => new MyDest();
}

[Mapper]
public partial class MyMapper
{
    [UseConverter(typeof(GoodConverter))]
    public partial MyDest Map(MySource source);
}
";
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Should().NotContain(d => d.Id == "ELM013");
    }
}



