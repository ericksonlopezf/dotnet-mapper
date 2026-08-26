// Copyright © Erickson Lopez. MIT License.
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Strategies;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

[Trait("Category", "FastAst")]
public class EnumStrategyTests
{
    [Fact]
    public void EnumStrategy_ByNameIgnoreCase_ShouldMatchDifferentlyCasedMembers()
    {
        string source = @"
public enum SourcePriority { low, medium, high }
public enum TargetPriority { Low, Medium, High }

public class Source { public SourcePriority Priority { get; set; } }
public class Target { public TargetPriority Priority { get; set; } }

[Mapper]
[EnumMappingStrategy(EnumMappingStrategy.ByName, IgnoreCase = true)]
public partial class PriorityMapper
{
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("TargetPriority");
    }

    [Fact]
    public void EnumStrategy_ByValue_ShouldEmitDirectCastWhenValuesMatch()
    {
        string source = @"
public enum SourceType { A = 1, B = 2 }
public enum TargetType { DifferentA = 1, DifferentB = 2 }

public class Source { public SourceType Type { get; set; } }
public class Target { public TargetType Type { get; set; } }

[Mapper]
public partial class TypeMapper
{
    [EnumMappingStrategy(EnumMappingStrategy.ByValue)]
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("(global::TestNamespace.TargetType)(source.Type)");
    }

    [Fact]
    public void EnumStrategy_ByValue_WhenMissingTargetValueInStrict_ShouldEmitELM014Error()
    {
        string source = @"
public enum SourceType { A = 1, B = 2, C = 3 }
public enum TargetType { A = 1, B = 2 }

public class Source { public SourceType Type { get; set; } }
public class Target { public TargetType Type { get; set; } }

[Mapper(StrictMapping = true)]
public partial class TypeMapper
{
    [EnumMappingStrategy(EnumMappingStrategy.ByValue)]
    public partial Target Map(Source source);
}
";
        var (diagnostics, _, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);

        diagnostics.Should().Contain(d => d.Id == "ELM014" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void MapEnumValue_ExplicitMapping_ShouldRemapSpecifiedMembers()
    {
        string source = @"
public enum PaymentStatus { InProgress, Succeeded, Failed }
public enum OrderStatus { Processing, Completed, Cancelled }

public class Source { public PaymentStatus Status { get; set; } }
public class Target { public OrderStatus Status { get; set; } }

[Mapper]
public partial class StatusMapper
{
    [MapEnumValue(PaymentStatus.InProgress, OrderStatus.Processing)]
    [MapEnumValue(PaymentStatus.Succeeded, OrderStatus.Completed)]
    [MapEnumValue(PaymentStatus.Failed, OrderStatus.Cancelled)]
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("global::TestNamespace.PaymentStatus.InProgress => global::TestNamespace.OrderStatus.Processing");
        output.Should().Contain("global::TestNamespace.PaymentStatus.Succeeded => global::TestNamespace.OrderStatus.Completed");
        output.Should().Contain("global::TestNamespace.PaymentStatus.Failed => global::TestNamespace.OrderStatus.Cancelled");
    }

    [Fact]
    public void StandaloneEnumMethod_ShouldGenerateDirectEnumMappingMethod()
    {
        string source = @"
public enum SourceColor { Red, Green, Blue }
public enum TargetColor { Red, Green, Blue }

[Mapper]
public partial class ColorMapper
{
    public partial TargetColor MapColor(SourceColor source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("public partial global::TestNamespace.TargetColor MapColor(global::TestNamespace.SourceColor source)");
    }

    [Fact]
    public void EnumToEnum_WhenMembersMatch_ShouldGenerateExplicitCast()
    {
        string source = @"
public enum SourceStatus { Active, Inactive }
public enum TargetStatus { Active, Inactive }

public class Source { public SourceStatus Status { get; set; } }
public class Target { public TargetStatus Status { get; set; } }

[Mapper]
public partial class StatusMapper
{
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Status = (global::TestNamespace.TargetStatus)(source.Status)");
    }

    [Fact]
    public void EnumToEnum_WhenMembersMismatchedAndStrict_ShouldEmitELM014Error()
    {
        string source = @"
public enum SourceStatus { Active, Inactive, Pending }
public enum TargetStatus { Active, Inactive }

public class Source { public SourceStatus Status { get; set; } }
public class Target { public TargetStatus Status { get; set; } }

[Mapper(StrictMapping = true)]
public partial class StatusMapper
{
    public partial Target Map(Source source);
}
";
        var (diagnostics, _, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);

        diagnostics.Should().Contain(d => d.Id == "ELM014" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void EnumToEnum_WhenMembersMismatchedAndNonStrict_ShouldEmitELM014Warning()
    {
        string source = @"
public enum SourceStatus { Active, Inactive, Pending }
public enum TargetStatus { Active, Inactive }

public class Source { public SourceStatus Status { get; set; } }
public class Target { public TargetStatus Status { get; set; } }

[Mapper(StrictMapping = false)]
public partial class StatusMapper
{
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        diagnostics.Should().Contain(d => d.Id == "ELM014" && d.Severity == DiagnosticSeverity.Warning);
        output.Should().Contain("Status = (global::TestNamespace.TargetStatus)(source.Status)");
    }

    [Fact]
    public void Map_WhenConvertingBetweenIntAndEnum_ShouldGenerateExplicitCasts()
    {
        string source = @"
public enum Priority { Low = 1, High = 2 }

public class Source { public int Level { get; set; } public Priority PriorityVal { get; set; } }
public class Target { public Priority Level { get; set; } public int PriorityVal { get; set; } }

[Mapper]
public partial class PriorityMapper
{
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Level = (global::TestNamespace.Priority)(source.Level)");
        output.Should().Contain("PriorityVal = (int)(source.PriorityVal)");
    }

    [Fact]
    public void MapEnumValue_WhenCombinedWithIgnoreCase_ShouldRespectExplicitRemappingAndCaseInsensitiveFallback()
    {
        string source = @"
public enum SourceStatus { low, medium, failed }
public enum TargetStatus { Low, Medium, Default }

public class Source { public SourceStatus Status { get; set; } }
public class Target { public TargetStatus Status { get; set; } }

[Mapper]
public partial class StatusMapper
{
    [EnumMappingStrategy(EnumMappingStrategy.ByName, IgnoreCase = true)]
    [MapEnumValue(SourceStatus.failed, TargetStatus.Default)]
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("SourceStatus.failed => global::TestNamespace.TargetStatus.Default");
        output.Should().Contain("SourceStatus.low => global::TestNamespace.TargetStatus.Low");
        output.Should().Contain("SourceStatus.medium => global::TestNamespace.TargetStatus.Medium");
    }
}
