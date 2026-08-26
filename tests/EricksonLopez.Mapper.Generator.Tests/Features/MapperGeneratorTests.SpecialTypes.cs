// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
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
    public async Task Generate_WhenUseConverter_ShouldApplyConverter()
    {
        var source = @"
using EricksonLopez.Mapper;

namespace TestNamespace;

public class CustomId { public Guid Value { get; set; } }

public class Source { public Guid Id { get; set; } }
public class Target { public CustomId Id { get; set; } }

public class MyConverter : IConverter<Source, Target>
{
    public Target Convert(Source source) => new Target { Id = new CustomId { Value = source.Id } };
}

[Mapper]
public partial class MyMapper
{
    [UseConverter(typeof(MyConverter))]
    public partial Target Map(Source source);
}";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenStronglyTypedId_ShouldMapCorrectly()
    {
        var source = @"
namespace TestNamespace;
public readonly record struct UserId(Guid Value);
public class Source { public Guid Id { get; set; } }
public class Dest { public UserId Id { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenValueObject_ShouldUnwrapValueProperty()
    {
        var source = @"
namespace TestNamespace;
public readonly record struct UserId(Guid Value);
public class Source { public UserId Id { get; set; } }
public class Dest { public Guid Id { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenValueObjectMultipleProperties_ShouldFailWithELM003()
    {
        var source = @"
namespace TestNamespace;
public readonly record struct UserId(Guid Value, string Extra);
public class Source { public Guid Id { get; set; } }
public class Dest { public UserId Id { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenWideningNumericConversionInt32ToInt64_ShouldMapSuccessfully()
    {
        var source = @"
namespace TestNamespace;
public class Source { public int Count { get; set; } }
public class Dest   { public long Count { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenNarrowingNumericConversionInt64ToInt32_ShouldEmitCast()
    {
        var source = @"
namespace TestNamespace;
public class Source { public long Count { get; set; } }
public class Dest   { public int  Count { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenEnumToString_ShouldMapSuccessfully()
    {
        var source = @"
namespace TestNamespace;
public enum Status { Active, Inactive }
public class Source { public Status Status { get; set; } }
public class Dest   { public string Status { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenStringToEnum_ShouldMapSuccessfully()
    {
        var source = @"
namespace TestNamespace;
public enum Status { Active, Inactive }
public class Source { public string Status { get; set; } }
public class Dest   { public Status Status { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenGuidToString_ShouldMapSuccessfully()
    {
        var source = @"
namespace TestNamespace;
public class Source { public Guid Id { get; set; } }
public class Dest   { public string Id { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenStringToGuid_ShouldMapSuccessfully()
    {
        var source = @"
namespace TestNamespace;
public class Source { public string Id { get; set; } }
public class Dest   { public Guid Id { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenDateTimeToDateOnly_ShouldMapSuccessfully()
    {
        var source = @"
namespace TestNamespace;
public class Source { public DateTime CreatedAt { get; set; } }
public class Dest   { public DateOnly CreatedAt { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenDateTimeToDateTimeOffset_ShouldMapSuccessfully()
    {
        var source = @"
namespace TestNamespace;
public class Source { public DateTime CreatedAt { get; set; } }
public class Dest   { public DateTimeOffset CreatedAt { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenAssemblyDIRegistration_ShouldGenerateExtensions()
    {
        var source = @"using EricksonLopez.Mapper;
[assembly: EricksonLopez.Mapper.GenerateMapperRegistration]
namespace TestNamespace;
public class Source { public string Name { get; set; } }
public class Dest { public string Name { get; set; } }
[Mapper]
public partial class Mapper { public partial Dest Map(Source source); }";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenDIRegistration_ShouldGenerateExtensions()
    {
        var source = @"
[assembly: EricksonLopez.Mapper.GenerateMapperRegistration]
namespace TestNamespace;
public class Source { public string Name { get; set; } }
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
    public async Task Generate_WhenEscapeIdentifier_ShouldEscapeProperly()
    {
        var source = @"
namespace @class.@event;
public class Source { public int @out { get; set; } }
public class Dest { public int @out { get; set; } }
[Mapper]
public partial class @namespace
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenExplicitCastAndConversions_ShouldEmitExplicitCasts()
    {
        var source = @"
namespace TestNamespace;
public struct ExplicitCastDest
{
    public int Inner { get; }
    private ExplicitCastDest(int inner) { Inner = inner; }
    public static explicit operator ExplicitCastDest(int i) => new ExplicitCastDest(i);
}
public readonly record struct UserId(Guid Value);
public class Source { public int CastVal { get; set; } public Guid Id { get; set; } public int[] Array { get; set; } public List<string> FallbackGen { get; set; } }
public class Dest { public ExplicitCastDest CastVal { get; set; } public UserId Id { get; set; } public System.Collections.Immutable.ImmutableArray<int> Array { get; set; } public System.Collections.Immutable.ImmutableArray<string> FallbackGen { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenNumericConversions_ShouldMapSuccessfully()
    {
        var source = @"
namespace TestNamespace;
public class Source { public short Widening { get; set; } public long Narrowing { get; set; } }
public class Dest { public int Widening { get; set; } public int Narrowing { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenAllNumericCombinations_ShouldMapSuccessfully()
    {
        var types = new[] { "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "decimal" };
        var srcProps = new System.Text.StringBuilder();
        var dstProps = new System.Text.StringBuilder();
        foreach (var s in types)
        {
            foreach (var t in types)
            {
                if (s != t)
                {
                    srcProps.AppendLine($"public {s} P_{s}_{t} {{ get; set; }}");
                    dstProps.AppendLine($"public {t} P_{s}_{t} {{ get; set; }}");
                }
            }
        }
        var source = $@"
namespace TestNamespace;
public class Source {{ {srcProps} }}
public class Dest {{ {dstProps} }}
[Mapper]
public partial class Mapper
{{
    public partial Dest Map(Source source);
}}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenStaticMapper_ShouldNotUseThisKeyword()
    {
        var source = @"

namespace TestNamespace;

public class Source { public string Name { get; set; } }
public class Dest { public string Name { get; set; } }

[Mapper]
public static partial class Mapper
{
    public static partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenUseConverterWithString_ShouldApplyFieldConverter()
    {
        var source = @"

namespace TestNamespace;

public class Source { public string Value { get; set; } }
public class Target { public int Value { get; set; } }

public class CustomConverter : IConverter<Source, Target>
{
    public Target Convert(Source source) => new Target { Value = int.Parse(source.Value) };
}

[Mapper]
public partial class MyMapper
{
    private readonly CustomConverter _converter;
    public MyMapper(CustomConverter converter) => _converter = converter;

    [UseConverter(nameof(_converter))]
    public partial Target Map(Source source);
}";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenReadOnlySpanToList_ShouldMapCorrectly()
    {
        var source = @"

namespace TestNamespace;

public class Source { public ReadOnlySpan<int> Items => new int[] { 1, 2, 3 }.AsSpan(); }
public class Dest { public List<int> Items { get; set; } }

[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenDateTimeOffsetToDateOnly_ShouldMapCorrectly()
    {
        var source = @"

namespace TestNamespace;

public class Source { public DateTimeOffset Date { get; set; } }
public class Dest { public DateOnly Date { get; set; } }

[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenDateOnlyToDateTimeOffset_ShouldMapCorrectly()
    {
        var source = @"

namespace TestNamespace;

public class Source { public DateOnly Date { get; set; } }
public class Dest { public DateTimeOffset Date { get; set; } }

[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public void Generate_WhenEnumsHaveFlagsAttribute_ShouldPreserveBitwiseCombinations()
    {
        var source = @"

namespace TestNamespace;

[Flags]
public enum SourcePermissions { None = 0, Read = 1, Write = 2, Execute = 4 }

[Flags]
public enum DestPermissions { None = 0, Read = 1, Write = 2, Execute = 4 }

public class Source { public SourcePermissions Permissions { get; set; } }
public class Dest { public DestPermissions Permissions { get; set; } }

[Mapper]
public partial class FlagsMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Permissions = (global::TestNamespace.DestPermissions)(source.Permissions)");
    }

    [Fact]
    public void TemporalConversions_WhenConvertingDateOnlyToDateTime_ShouldCallToDateTime()
    {
        string source = @"
public class Source { public DateOnly CreatedDate { get; set; } }
public class Target { public DateTime CreatedDate { get; set; } }

[Mapper]
public partial class DateMapper
{
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("CreatedDate = (source.CreatedDate).ToDateTime(global::System.TimeOnly.MinValue)");
    }

    [Fact]
    public void TemporalConversions_WhenConvertingDateTimeOffsetToDateTime_ShouldAccessDateTimeProperty()
    {
        string source = @"
public class Source { public DateTimeOffset Timestamp { get; set; } }
public class Target { public DateTime Timestamp { get; set; } }

[Mapper]
public partial class DateMapper
{
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Timestamp = (source.Timestamp).DateTime");
    }

    [Fact]
    public void DependencyInjectionRegistration_WhenAttributePresent_ShouldRegisterSingletonMappersAndTransientConverters()
    {
        string source = @"
[assembly: EricksonLopez.Mapper.GenerateMapperRegistration]
namespace TestNamespace;

public class Source { public string Name { get; set; } = string.Empty; }
public class Dest { public string Name { get; set; } = string.Empty; }
public class CustomConverter : IConverter<Source, Dest>
{
    public Dest Convert(Source source) => new Dest { Name = source.Name };
}

[Mapper]
public partial class InstanceMapper
{
    [UseConverter(typeof(CustomConverter))]
    public partial Dest Map(Source source);
}

[Mapper]
public static partial class StaticMapper
{
    public static partial Dest MapStatic(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("services.AddSingleton<TestNamespace.InstanceMapper>();");
        output.Should().Contain("services.AddTransient<global::TestNamespace.CustomConverter>();");
        output.Should().NotContain("services.AddSingleton<TestNamespace.StaticMapper>();");
    }

    [Fact]
    public void DependencyInjectionRegistration_WhenAttributeAbsent_ShouldNotEmitExtensionMethods()
    {
        string source = @"
namespace TestNamespace;

public class Source { public string Name { get; set; } = string.Empty; }
public class Dest { public string Name { get; set; } = string.Empty; }

[Mapper]
public partial class InstanceMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().NotContain("AddGeneratedMappers");
        output.Should().NotContain("MapperServiceCollectionExtensions");
    }

    [Fact]
    public void DependencyInjectionRegistration_WhenMultipleMappersShareSameCustomConverter_ShouldDeduplicateTransientRegistrations()
    {
        string source = @"
[assembly: EricksonLopez.Mapper.GenerateMapperRegistration]
namespace TestNamespace;

public class Source { public string Name { get; set; } = string.Empty; }
public class Dest { public string Name { get; set; } = string.Empty; }
public class SharedConverter : IConverter<Source, Dest>
{
    public Dest Convert(Source source) => new Dest { Name = source.Name };
}

[Mapper]
public partial class MapperOne
{
    [UseConverter(typeof(SharedConverter))]
    public partial Dest Map1(Source source);
}

[Mapper]
public partial class MapperTwo
{
    [UseConverter(typeof(SharedConverter))]
    public partial Dest Map2(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("services.AddSingleton<TestNamespace.MapperOne>();");
        output.Should().Contain("services.AddSingleton<TestNamespace.MapperTwo>();");

        // Should contain AddTransient for SharedConverter exactly once
        int firstIndex = output.IndexOf("services.AddTransient<global::TestNamespace.SharedConverter>();", StringComparison.Ordinal);
        firstIndex.Should().BeGreaterThan(-1);
        int secondIndex = output.IndexOf("services.AddTransient<global::TestNamespace.SharedConverter>();", firstIndex + 1, StringComparison.Ordinal);
        secondIndex.Should().Be(-1, "Shared custom converter should only be registered once in DI container");
    }
}





