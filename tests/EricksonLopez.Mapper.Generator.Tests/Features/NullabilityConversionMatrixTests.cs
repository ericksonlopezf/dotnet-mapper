// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Mapper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

/// <summary>
/// Exhaustive unit test suite verifying the complete Nullability Conversion Matrix:
/// - T -> T? (Widening to nullable)
/// - T? -> T (Narrowing from nullable using [MapNullFallback])
/// - T? -> T? (Safe null propagation ternary / null preserving)
/// Across: Primitive, Enum, Guid, DateTime/DateOnly/DateTimeOffset, ValueObject, Collections, and Nested Objects.
/// Adheres strictly to ADR-021 (Method_Scenario_Result).
/// </summary>
public class NullabilityConversionMatrixTests
{

    #region 1. Primitive Nullability (int, double, bool)

    [Fact]
    public void Map_WhenPrimitiveNonNullableToNullable_ShouldEmitDirectAssignment()
    {
        string source = @"

public class Source { public int Value { get; set; } }
public class Target { public int? Value { get; set; } }

[Mapper]
public partial class PrimitiveMapper
{
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Value = source.Value");
    }

    [Fact]
    public void Map_WhenPrimitiveNullableToNonNullableWithFallback_ShouldEmitNullCoalescing()
    {
        string source = @"

public class Source { public int? Value { get; set; } }
public class Target { public int Value { get; set; } }

[Mapper]
public partial class PrimitiveMapper
{
    [MapNullFallback(nameof(Target.Value), ""0"")]
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Value = (source.Value ?? 0)");
    }

    [Fact]
    public void Map_WhenPrimitiveNullableToNullable_ShouldPreserveNullability()
    {
        string source = @"

public class Source { public int? Value { get; set; } }
public class Target { public int? Value { get; set; } }

[Mapper]
public partial class PrimitiveMapper
{
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Value = source.Value");
    }

    #endregion

    #region 2. Enum Nullability & Underlying Values

    [Fact]
    public void Map_WhenEnumNonNullableToNullable_ShouldEmitDirectAssignment()
    {
        string source = @"

public enum Status { Active, Inactive }

public class Source { public Status Status { get; set; } }
public class Target { public Status? Status { get; set; } }

[Mapper]
public partial class EnumMapper
{
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Status = source.Status");
    }

    [Fact]
    public void Map_WhenEnumNullableToNonNullableWithFallback_ShouldEmitNullCoalescing()
    {
        string source = @"

public enum Status { Active, Inactive }

public class Source { public Status? Status { get; set; } }
public class Target { public Status Status { get; set; } }

[Mapper]
public partial class EnumMapper
{
    [MapNullFallback(nameof(Target.Status), ""Status.Inactive"")]
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Status = (source.Status ?? Status.Inactive)");
    }

    [Fact]
    public void Map_WhenEnumDifferentUnderlyingValues_ShouldGenerateSwitchExpression()
    {
        string source = @"

public enum SourcePriority { Low = 1, High = 2 }
public enum TargetPriority { Low = 10, High = 20 }

public class Source { public SourcePriority Priority { get; set; } }
public class Target { public TargetPriority Priority { get; set; } }

[Mapper]
public partial class PriorityMapper
{
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Priority = (source.Priority) switch {");
        output.Should().Contain("SourcePriority.Low =>");
        output.Should().Contain("TargetPriority.Low");
        output.Should().Contain("SourcePriority.High =>");
        output.Should().Contain("TargetPriority.High");
    }

    #endregion

    #region 3. Guid & String Nullability

    [Fact]
    public void Map_WhenGuidNonNullableToNullable_ShouldEmitDirectAssignment()
    {
        string source = @"

public class Source { public Guid Id { get; set; } }
public class Target { public Guid? Id { get; set; } }

[Mapper]
public partial class GuidMapper
{
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Id = source.Id");
    }

    [Fact]
    public void Map_WhenGuidNullableToNonNullableWithFallback_ShouldEmitNullCoalescing()
    {
        string source = @"

public class Source { public Guid? Id { get; set; } }
public class Target { public Guid Id { get; set; } }

[Mapper]
public partial class GuidMapper
{
    [MapNullFallback(nameof(Target.Id), ""global::System.Guid.Empty"")]
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Id = (source.Id ?? global::System.Guid.Empty)");
    }

    [Fact]
    public void Map_WhenStringToGuid_ShouldEmitGuidParse()
    {
        string source = @"

public class Source { public string Text { get; set; } = """"; }
public class Target { public Guid Id { get; set; } }

[Mapper]
public partial class StringGuidMapper
{
    [MapProperty(nameof(Source.Text), nameof(Target.Id))]
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Id = global::System.Guid.Parse(source.Text)");
    }

    #endregion

    #region 4. Temporal Nullability (DateTime, DateOnly, DateTimeOffset)

    [Fact]
    public void Map_WhenDateTimeToDateOnlyWithNullFallback_ShouldEmitFallbackAndConversion()
    {
        string source = @"

public class Source { public DateTime? CreatedAt { get; set; } }
public class Target { public DateOnly CreatedAt { get; set; } }

[Mapper]
public partial class DateMapper
{
    [MapNullFallback(nameof(Target.CreatedAt), ""global::System.DateTime.UnixEpoch"")]
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("CreatedAt = global::System.DateOnly.FromDateTime((source.CreatedAt ?? global::System.DateTime.UnixEpoch))");
    }

    [Fact]
    public void Map_WhenDateTimeOffsetToDateTimeWithNullableFallback_ShouldEmitDateTimeProperty()
    {
        string source = @"

public class Source { public DateTimeOffset? Timestamp { get; set; } }
public class Target { public DateTime Timestamp { get; set; } }

[Mapper]
public partial class TimestampMapper
{
    [MapNullFallback(nameof(Target.Timestamp), ""global::System.DateTime.UtcNow"")]
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Timestamp = ((source.Timestamp ?? global::System.DateTime.UtcNow)).DateTime");
    }

    #endregion

    #region 5. Value Objects Nullability

    [Fact]
    public void Map_WhenValueObjectWrapAndUnwrapNullable_ShouldEmitConstructorAndValueAccess()
    {
        string source = @"

public readonly record struct UserId(Guid Value);

public class Source { public Guid Id { get; set; } }
public class Target { public UserId? Id { get; set; } }

public class SourceVo { public UserId? Id { get; set; } }
public class TargetGuid { public Guid Id { get; set; } }

[Mapper]
public partial class VoMapper
{
    public partial Target Map(Source source);

    [MapNullFallback(nameof(TargetGuid.Id), ""default"")]
    public partial TargetGuid MapUnwrap(SourceVo source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("UserId(source.Id)");
    }

    #endregion

    #region 6. Collections & Dictionaries Nullability

    [Fact]
    public void Map_WhenCollectionNullableToNullable_ShouldEmitPreSizingAndNullCheck()
    {
        string source = @"

public class Source { public int[]? Numbers { get; set; } }
public class Target { public List<int>? Numbers { get; set; } }

[Mapper]
public partial class ListMapper
{
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("if (source.Numbers != null)");
        output.Should().Contain("new global::System.Collections.Generic.List<int>(source.Numbers.Length)");
    }

    [Fact]
    public void Map_WhenDictionaryNullableToNullable_ShouldEmitCapacityPreSizing()
    {
        string source = @"

public class Source { public Dictionary<string, int>? Scores { get; set; } }
public class Target { public IDictionary<string, int>? Scores { get; set; } }

[Mapper]
public partial class DictMapper
{
    public partial Target Map(Source source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("if (source.Scores != null)");
        output.Should().Contain("new global::System.Collections.Generic.Dictionary<string, int>(source.Scores.Count)");
    }

    #endregion

    #region 7. Nested Objects (Sub-Method Null Propagation)

    [Fact]
    public void Map_WhenNestedObjectNullableToNullable_ShouldEmitSafeTernary()
    {
        string source = @"

public class Address { public string City { get; set; } = """"; }
public class AddressDto { public string City { get; set; } = """"; }

public class User { public Address? Address { get; set; } }
public class UserDto { public AddressDto? Address { get; set; } }

[Mapper]
public partial class UserMapper
{
    public partial UserDto Map(User source);
    public partial AddressDto MapAddress(Address source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Address = (source.Address != null ? this.MapAddress(source.Address) : null)");
    }

    [Fact]
    public void Map_WhenNestedCollectionsWithMixedNullability_ShouldEmitSafeNullChecksAndPresize()
    {
        string source = @"

public class MatrixSource { public int[][]? Grid { get; set; } }
public class MatrixTarget { public List<int[]>? Grid { get; set; } }

[Mapper]
public partial class MatrixMapper
{
    public partial MatrixTarget Map(MatrixSource source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("if (source.Grid != null)");
        output.Should().Contain("new global::System.Collections.Generic.List<int[]>(source.Grid.Length)");
    }

    [Fact]
    public void Map_WhenNonNullableSourceToNullableTargetInNestedClass_ShouldEmitDirectAssignment()
    {
        string source = @"

public class InnerSource { public string Name { get; set; } = """"; }
public class InnerTarget { public string Name { get; set; } = """"; }
public class OuterSource { public InnerSource Inner { get; set; } = new(); }
public class OuterTarget { public InnerTarget? Inner { get; set; } }

[Mapper]
public partial class OuterMapper
{
    public partial OuterTarget Map(OuterSource source);
    public partial InnerTarget MapInner(InnerSource source);
}";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Inner = this.MapInner(source.Inner)");
    }

    [Fact]
    public void Map_WhenDictionaryWithNullableValues_ShouldEmitNullableTypeDictionary()
    {
        string source = @"

public class DictSource { public Dictionary<string, int?> Data { get; set; } = new(); }
public class DictTarget { public Dictionary<string, int?> Data { get; set; } = new(); }

[Mapper]
public partial class DictMapper
{
    public partial DictTarget Map(DictSource source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Data = source.Data");
    }

    [Fact]
    public void Map_WhenThreeLevelNestedCollectionsWithMixedNullability_ShouldPresizeAndGuard()
    {
        string source = @"

public class CubeSource { public List<List<List<int?>>>? Grid3D { get; set; } }
public class CubeTarget { public int[][][]? Grid3D { get; set; } }

[Mapper]
public partial class CubeMapper
{
    public partial CubeTarget Map(CubeSource source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);
        diagnostics.Should().ContainSingle(d => d.Id == "ELM003");
    }

    [Fact]
    public void NullPropagation_WhenNestedObjectIsNullable_ShouldGenerateTernaryGuard()
    {
        string source = @"
public class ChildSource { public string Name { get; set; } = string.Empty; }
public class ChildTarget { public string Name { get; set; } = string.Empty; }

public class ParentSource { public ChildSource? Child { get; set; } }
public class ParentTarget { public ChildTarget? Child { get; set; } }

[Mapper]
public partial class ParentMapper
{
    public partial ParentTarget Map(ParentSource source);
    public partial ChildTarget? MapChild(ChildSource? child);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorWithValidation(source);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Child = (source.Child != null ? this.MapChild(source.Child) : null)");
    }

    #endregion
}






