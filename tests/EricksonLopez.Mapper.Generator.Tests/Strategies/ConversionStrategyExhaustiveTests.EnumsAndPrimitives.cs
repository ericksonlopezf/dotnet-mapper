// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Strategies;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

public partial class ConversionStrategyExhaustiveTests
{
    [Fact]
    public void Resolve_WhenIntegralToEnumAndEnumToIntegral_ShouldEmitExplicitCasts()
    {
        string source = @"

namespace TestNamespace;

public enum UserStatus : int { Inactive = 0, Active = 1, Suspended = 2 }

public class Source
{
    public int IntStatus { get; set; }
    public UserStatus EnumStatus { get; set; }
}

public class Dest
{
    public UserStatus IntToEnum { get; set; }
    public int EnumToInt { get; set; }
}

[Mapper]
public partial class EnumIntegralMapper
{
    [MapProperty(nameof(Source.IntStatus), nameof(Dest.IntToEnum))]
    [MapProperty(nameof(Source.EnumStatus), nameof(Dest.EnumToInt))]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("IntToEnum = (global::TestNamespace.UserStatus)(source.IntStatus)");
        output.Should().Contain("EnumToInt = (int)(source.EnumStatus)");
    }

    [Fact]
    public void Resolve_WhenTemporalConversions_ShouldEmitProperRuntimeTransformations()
    {
        string source = @"

namespace TestNamespace;

public class Source
{
    public DateTime Dt1 { get; set; }
    public DateOnly Do1 { get; set; }
    public DateTime Dt2 { get; set; }
    public DateTimeOffset Dto1 { get; set; }
    public DateOnly Do2 { get; set; }
    public DateTimeOffset Dto2 { get; set; }
}

public class Dest
{
    public DateOnly DtToDateOnly { get; set; }
    public DateTime DateOnlyToDt { get; set; }
    public DateTimeOffset DtToDto { get; set; }
    public DateTime DtoToDt { get; set; }
    public DateTimeOffset DateOnlyToDto { get; set; }
    public DateOnly DtoToDateOnly { get; set; }
}

[Mapper]
public partial class TemporalMapper
{
    [MapProperty(nameof(Source.Dt1), nameof(Dest.DtToDateOnly))]
    [MapProperty(nameof(Source.Do1), nameof(Dest.DateOnlyToDt))]
    [MapProperty(nameof(Source.Dt2), nameof(Dest.DtToDto))]
    [MapProperty(nameof(Source.Dto1), nameof(Dest.DtoToDt))]
    [MapProperty(nameof(Source.Do2), nameof(Dest.DateOnlyToDto))]
    [MapProperty(nameof(Source.Dto2), nameof(Dest.DtoToDateOnly))]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("DtToDateOnly = global::System.DateOnly.FromDateTime(source.Dt1)");
        output.Should().Contain("DateOnlyToDt = (source.Do1).ToDateTime(global::System.TimeOnly.MinValue)");
        output.Should().Contain("DtToDto = new global::System.DateTimeOffset(source.Dt2)");
        output.Should().Contain("DtoToDt = (source.Dto1).DateTime");
        output.Should().Contain("DateOnlyToDto = new global::System.DateTimeOffset((source.Do2).ToDateTime(global::System.TimeOnly.MinValue, global::System.DateTimeKind.Utc))");
        output.Should().Contain("DtoToDateOnly = global::System.DateOnly.FromDateTime((source.Dto2).DateTime)");
    }

    [Fact]
    public void Resolve_WhenCustomImplicitOperatorOnSource_ShouldEmitConversion()
    {
        string source = @"

namespace TestNamespace;

public class SpecialType
{
    public int Value { get; set; }
    public static implicit operator int(SpecialType s) => s.Value;
}

public class Source { public SpecialType Value { get; set; } = new(); }
public class Dest   { public int Value { get; set; } }

[Mapper]
public partial class ImplicitConvMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Value = (int)source.Value");
    }

    [Fact]
    public void Resolve_WhenCustomExplicitOperatorOnTarget_ShouldEmitConversion()
    {
        string source = @"

namespace TestNamespace;

public class TargetType
{
    public int Value { get; }
    public TargetType(int v) => Value = v;
    public static explicit operator TargetType(int v) => new TargetType(v);
}

public class Source { public int Value { get; set; } }
public class Dest   { public TargetType Value { get; set; } = null!; }

[Mapper]
public partial class ExplicitConvMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Value = (global::TestNamespace.TargetType)source.Value");
    }

    [Fact]
    public void Resolve_WhenEnumNonStrictMappingWithMissingTargetValues_ShouldEmitELM014WarningAndSwitchExpression()
    {
        string source = @"

namespace TestNamespace;

public enum SourceEnum { Alpha = 1, Beta = 2, Gamma = 3 }
public enum DestEnum   { Alpha = 10, Beta = 20 }

public class Source { public SourceEnum Item { get; set; } }
public class Dest   { public DestEnum Item { get; set; } }

[Mapper(StrictMapping = false)]
public partial class EnumPermissiveMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        var diag = diagnostics.Should().ContainSingle(d => d.Id == "ELM014").Subject;
        diag.Severity.Should().Be(DiagnosticSeverity.Warning);
        diag.GetMessage().Should().Contain("Gamma");
        output.Should().Contain("(source.Item) switch");
    }

    [Fact]
    public void Resolve_WhenEnumStrictMappingWithMissingTargetValues_ShouldEmitELM014Error()
    {
        string source = @"

namespace TestNamespace;

public enum SourceEnum { Alpha = 1, Beta = 2, Gamma = 3 }
public enum DestEnum   { Alpha = 1, Beta = 2 }

public class Source { public SourceEnum Item { get; set; } }
public class Dest   { public DestEnum Item { get; set; } }

[Mapper(StrictMapping = true)]
public partial class EnumStrictMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorSimple(source);
        var diag = diagnostics.Should().ContainSingle(d => d.Id == "ELM014").Subject;
        diag.Severity.Should().Be(DiagnosticSeverity.Error);
        diag.GetMessage().Should().Contain("Gamma");
    }

    [Fact]
    public void Resolve_WhenEnumWithIdenticalConstants_ShouldEmitDirectNumericCast()
    {
        string source = @"

namespace TestNamespace;

public enum SourceRole : int { User = 1, Admin = 2 }
public enum DestRole : int   { User = 1, Admin = 2 }

public class Source { public SourceRole Role { get; set; } }
public class Dest   { public DestRole Role { get; set; } }

[Mapper]
public partial class EnumCastMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Role = (global::TestNamespace.DestRole)(source.Role)");
        output.Should().NotContain("switch");
    }

    [Fact]
    public void Resolve_WhenGuidToString_ShouldEmitToStringCall()
    {
        string source = @"

namespace TestNamespace;

public class Source { public Guid Id { get; set; } }
public class Dest   { public string Id { get; set; } = string.Empty; }

[Mapper]
public partial class GuidStringMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Id = source.Id.ToString()");
    }

    [Fact]
    public void Resolve_WhenEnumToString_ShouldEmitToStringCall()
    {
        string source = @"

namespace TestNamespace;

public enum Status { Active, Inactive }

public class Source { public Status StatusVal { get; set; } }
public class Dest   { public string StatusVal { get; set; } = string.Empty; }

[Mapper]
public partial class EnumStringMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("StatusVal = source.StatusVal.ToString()");
    }

    [Fact]
    public void Resolve_WhenEnumToIntegral_ShouldEmitExplicitCast()
    {
        string source = @"

namespace TestNamespace;

public enum Status { Active = 1, Inactive = 2 }

public class Source { public Status StatusVal { get; set; } }
public class Dest   { public int StatusVal { get; set; } }

[Mapper]
public partial class EnumIntMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("StatusVal = (int)(source.StatusVal)");
    }

    [Fact]
    public void Resolve_WhenAllIntegralTypesToEnum_ShouldEmitExplicitCasts()
    {
        string source = @"

namespace TestNamespace;

public enum ByteEnum : byte { A = 1, B = 2 }
public enum ShortEnum : short { A = 1, B = 2 }
public enum LongEnum : long { A = 1, B = 2 }

public class Source
{
    public byte BVal { get; set; }
    public short SVal { get; set; }
    public long LVal { get; set; }
}

public class Dest
{
    public ByteEnum BVal { get; set; }
    public ShortEnum SVal { get; set; }
    public LongEnum LVal { get; set; }
}

[Mapper]
public partial class IntegralToEnumMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("BVal = (global::TestNamespace.ByteEnum)(source.BVal)");
        output.Should().Contain("SVal = (global::TestNamespace.ShortEnum)(source.SVal)");
        output.Should().Contain("LVal = (global::TestNamespace.LongEnum)(source.LVal)");
    }

    [Fact]
    public void Resolve_WhenCombiningMapIgnoreAndMapIgnoreSource_ShouldMapOnlyUnignoredMembers()
    {
        string source = @"

namespace TestNamespace;

public class Source
{
    public string Kept { get; set; } = """";
    public string IgnoredSrc { get; set; } = """";
}

public class Dest
{
    public string Kept { get; set; } = """";
    public string IgnoredDst { get; set; } = """";
}

[Mapper]
public partial class IgnoreComboMapper
{
    [MapIgnoreSource(nameof(Source.IgnoredSrc))]
    [MapIgnore(nameof(Dest.IgnoredDst))]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Kept = source.Kept");
        output.Should().NotContain("IgnoredSrc");
        output.Should().NotContain("IgnoredDst");
    }

    [Fact]
    public void Resolve_WhenStaticMembersPresentInSourceOrDest_ShouldIgnoreStaticMembers()
    {
        string source = @"

namespace TestNamespace;

public class Source
{
    public static string StaticProp { get; set; } = """";
    public string InstanceProp { get; set; } = """";
}

public class Dest
{
    public static string StaticProp { get; set; } = """";
    public string InstanceProp { get; set; } = """";
}

[Mapper]
public partial class StaticMemberMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("InstanceProp = source.InstanceProp");
        output.Should().NotContain("StaticProp =");
    }

    [Fact]
    public void Resolve_WhenPrivateMembersPresent_ShouldIgnorePrivateMembers()
    {
        string source = @"

namespace TestNamespace;

public class Source
{
    private string PrivateProp { get; set; } = """";
    public string PublicProp { get; set; } = """";
}

public class Dest
{
    private string PrivateProp { get; set; } = """";
    public string PublicProp { get; set; } = """";
}

[Mapper]
public partial class PrivateMemberMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("PublicProp = source.PublicProp");
        output.Should().NotContain("PrivateProp =");
    }

    [Fact]
    public void Resolve_WhenEnumByValueWithMissingValuesAndStrictTrue_ShouldEmitELM014Error()
    {
        string source = @"
namespace TestNamespace;

public enum SourceStatus { A = 1, B = 2, C = 3 }
public enum DestStatus { A = 1, B = 2 }

public class Source { public SourceStatus Status { get; set; } }
public class Dest   { public DestStatus Status { get; set; } }

[Mapper(StrictMapping = true)]
[EnumMappingStrategy(EnumMappingStrategy.ByValue)]
public partial class StrictEnumByValueMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Should().Contain(d => d.Id == "ELM014" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Resolve_WhenEnumByNameWithIgnoreCase_ShouldMatchCaseInsensitively()
    {
        string source = @"
namespace TestNamespace;

public enum SourceColor { red = 1, green = 2, blue = 3 }
public enum DestColor { Red = 1, Green = 2, Blue = 3 }

public class Source { public SourceColor Color { get; set; } }
public class Dest   { public DestColor Color { get; set; } }

[Mapper]
[EnumMappingStrategy(EnumMappingStrategy.ByName, IgnoreCase = true)]
public partial class CaseInsensitiveEnumMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Color = (source.Color) switch {");
        output.Should().Contain("global::TestNamespace.SourceColor.red => global::TestNamespace.DestColor.Red");
    }

    [Fact]
    public void Resolve_WhenEnumByNameWithMissingMembersAndNonStrict_ShouldEmitWarningAndFallback()
    {
        string source = @"
namespace TestNamespace;

public enum SourceRoles { Admin = 1, User = 2, Guest = 3 }
public enum DestRoles { Admin = 1, User = 2 }

public class Source { public SourceRoles Role { get; set; } }
public class Dest   { public DestRoles Role { get; set; } }

[Mapper(StrictMapping = false)]
public partial class NonStrictEnumMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        diagnostics.Should().Contain(d => d.Id == "ELM014" && d.Severity == DiagnosticSeverity.Warning);
        output.Should().Contain("Role = (global::TestNamespace.DestRoles)(source.Role)");
    }



    [Fact]
    public void Resolve_WhenStringToEnumConversion_ShouldEmitEnumParseAndELM016Warning()
    {
        string source = @"
namespace TestNamespace;

public enum Priority { Low, Medium, High }

public class Source { public string Priority { get; set; } = """"; }
public class Dest   { public Priority Priority { get; set; } }

[Mapper]
public partial class StringToEnumMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        var warning = diagnostics.Should().ContainSingle(d => d.Id == "ELM016").Subject;
        warning.Severity.Should().Be(DiagnosticSeverity.Warning);
        output.Should().Contain("Priority = global::System.Enum.Parse<global::TestNamespace.Priority>(source.Priority)");
    }

    [Fact]
    public void Resolve_WhenGuidToStringAndStringToGuid_ShouldEmitToStringAndGuidParse()
    {
        string source = @"
using System;

namespace TestNamespace;

public class Source
{
    public Guid Id { get; set; }
    public string StrId { get; set; } = """";
}

public class Dest
{
    public string Id { get; set; } = """";
    public Guid StrId { get; set; }
}

[Mapper]
public partial class GuidMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Id = source.Id.ToString()");
        output.Should().Contain("StrId = global::System.Guid.Parse(source.StrId)");
    }
}

