// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Strategies;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

/// <summary>
/// Exhaustive unit test suite verifying member conversion strategy resolution in <see cref="ConversionStrategyFactory"/>.
/// Modularized across partial classes:
/// - <see cref="ConversionStrategyExhaustiveTests"/>: Numeric Widening &amp; Narrowing
/// - <c>ConversionStrategyExhaustiveTests.EnumsAndPrimitives.cs</c>: Enums, Guid/String, Temporals, Custom Operators
/// - <c>ConversionStrategyExhaustiveTests.CollectionsAndVo.cs</c>: Collections, Dictionaries, Value Objects, Converters
/// Adheres strictly to ADR-021 (Method_Scenario_Result).
/// </summary>
[Trait("Category", "FastAst")]
public partial class ConversionStrategyExhaustiveTests
{
    [Fact]
    public void Resolve_WhenNumericWideningToDecimal_ShouldGenerateDirectAssignments()
    {
        string source = @"
using EricksonLopez.Mapper;

namespace TestNamespace;

public class Source
{
    public byte ByteVal { get; set; }
    public sbyte SByteVal { get; set; }
    public short ShortVal { get; set; }
    public ushort UShortVal { get; set; }
    public int IntVal { get; set; }
    public uint UIntVal { get; set; }
    public long LongVal { get; set; }
    public ulong ULongVal { get; set; }
}

public class Dest
{
    public decimal ByteVal { get; set; }
    public decimal SByteVal { get; set; }
    public decimal ShortVal { get; set; }
    public decimal UShortVal { get; set; }
    public decimal IntVal { get; set; }
    public decimal UIntVal { get; set; }
    public decimal LongVal { get; set; }
    public decimal ULongVal { get; set; }
}

[Mapper]
public partial class DecimalWideningMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("ByteVal = source.ByteVal");
        output.Should().Contain("SByteVal = source.SByteVal");
        output.Should().Contain("ShortVal = source.ShortVal");
        output.Should().Contain("UShortVal = source.UShortVal");
        output.Should().Contain("IntVal = source.IntVal");
        output.Should().Contain("UIntVal = source.UIntVal");
        output.Should().Contain("LongVal = source.LongVal");
        output.Should().Contain("ULongVal = source.ULongVal");
    }

    [Fact]
    public void Resolve_WhenNumericWideningToFloatingPoint_ShouldGenerateDirectAssignments()
    {
        string source = @"

namespace TestNamespace;

public class Source
{
    public byte ByteVal { get; set; }
    public short ShortVal { get; set; }
    public int IntVal { get; set; }
    public long LongVal { get; set; }
    public float FloatVal { get; set; }
}

public class Dest
{
    public float ByteToFloat { get; set; }
    public double ShortToDouble { get; set; }
    public double IntToDouble { get; set; }
    public double LongToDouble { get; set; }
    public double FloatToDouble { get; set; }
}

[Mapper]
public partial class FloatWideningMapper
{
    [MapProperty(nameof(Source.ByteVal), nameof(Dest.ByteToFloat))]
    [MapProperty(nameof(Source.ShortVal), nameof(Dest.ShortToDouble))]
    [MapProperty(nameof(Source.IntVal), nameof(Dest.IntToDouble))]
    [MapProperty(nameof(Source.LongVal), nameof(Dest.LongToDouble))]
    [MapProperty(nameof(Source.FloatVal), nameof(Dest.FloatToDouble))]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("ByteToFloat = source.ByteVal");
        output.Should().Contain("ShortToDouble = source.ShortVal");
        output.Should().Contain("IntToDouble = source.IntVal");
        output.Should().Contain("LongToDouble = source.LongVal");
        output.Should().Contain("FloatToDouble = source.FloatVal");
    }

    [Fact]
    public void Resolve_WhenNarrowingNumericConversions_ShouldEmitExplicitCastsAndELM015Warnings()
    {
        string source = @"

namespace TestNamespace;

public class Source
{
    public long LongVal { get; set; }
    public int IntVal { get; set; }
    public short ShortVal { get; set; }
    public double DoubleVal { get; set; }
    public decimal DecimalVal { get; set; }
}

public class Dest
{
    public int LongToInt { get; set; }
    public short IntToShort { get; set; }
    public byte ShortToByte { get; set; }
    public float DoubleToFloat { get; set; }
    public int DecimalToInt { get; set; }
}

[Mapper]
public partial class NarrowingMapper
{
    [MapProperty(nameof(Source.LongVal), nameof(Dest.LongToInt))]
    [MapProperty(nameof(Source.IntVal), nameof(Dest.IntToShort))]
    [MapProperty(nameof(Source.ShortVal), nameof(Dest.ShortToByte))]
    [MapProperty(nameof(Source.DoubleVal), nameof(Dest.DoubleToFloat))]
    [MapProperty(nameof(Source.DecimalVal), nameof(Dest.DecimalToInt))]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Should().Contain(d => d.Id == "ELM015");
        output.Should().Contain("LongToInt = (int)(source.LongVal)");
        output.Should().Contain("IntToShort = (short)(source.IntVal)");
        output.Should().Contain("ShortToByte = (byte)(source.ShortVal)");
        output.Should().Contain("DoubleToFloat = (float)(source.DoubleVal)");
        output.Should().Contain("DecimalToInt = (int)(source.DecimalVal)");
    }

    [Fact]
    public void Resolve_WhenNullableValueTypeTargetWithInnerWidening_ShouldEmitDirectAssignment()
    {
        string source = @"

namespace TestNamespace;

public class Source { public int SmallNum { get; set; } }
public class Dest   { public long? SmallNum { get; set; } }

[Mapper]
public partial class NullableWideningMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("SmallNum = source.SmallNum");
    }

    [Fact]
    public void Resolve_WhenNullableValueTypeSourceWithInnerWideningToNullableTarget_ShouldEmitDirectAssignment()
    {
        string source = @"

namespace TestNamespace;

public class Source { public int? SmallNum { get; set; } }
public class Dest   { public long? SmallNum { get; set; } }

[Mapper]
public partial class NullableBothWideningMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("SmallNum = source.SmallNum");
    }

    [Fact]
    public void Resolve_WhenNarrowingConversionFromDoubleToFloat_ShouldEmitExplicitCastAndELM015()
    {
        string source = @"

namespace TestNamespace;

public class Source { public double Amount { get; set; } }
public class Dest   { public float Amount { get; set; } }

[Mapper]
public partial class NarrowingMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        var warning = diagnostics.Should().ContainSingle(d => d.Id == "ELM015").Subject;
        warning.Severity.Should().Be(DiagnosticSeverity.Warning);
        output.Should().Contain("Amount = (float)(source.Amount)");
    }

    [Fact]
    public void Resolve_WhenNumericNarrowingBetweenSignedAndUnsigned_ShouldEmitELM015WarningAndCast()
    {
        string source = @"

namespace TestNamespace;

public class Source
{
    public long LongVal { get; set; }
    public double DoubleVal { get; set; }
    public decimal DecimalVal { get; set; }
}

public class Dest
{
    public short LongVal { get; set; }
    public float DoubleVal { get; set; }
    public int DecimalVal { get; set; }
}

[Mapper]
public partial class NarrowingMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        diagnostics.Where(d => d.Id == "ELM015" && d.Severity == DiagnosticSeverity.Warning).Should().HaveCount(3);
        output.Should().Contain("LongVal = (short)(source.LongVal)");
        output.Should().Contain("DoubleVal = (float)(source.DoubleVal)");
        output.Should().Contain("DecimalVal = (int)(source.DecimalVal)");
    }

    [Fact]
    public void Resolve_WhenAllWideningNumericCombinations_ShouldGenerateDirectAssignments()
    {
        string source = @"
namespace TestNamespace;

public class Source
{
    public sbyte SByteVal { get; set; }
    public byte ByteVal { get; set; }
    public short ShortVal { get; set; }
    public ushort UShortVal { get; set; }
    public int IntVal { get; set; }
    public uint UIntVal { get; set; }
    public long LongVal { get; set; }
    public ulong ULongVal { get; set; }
    public float FloatVal { get; set; }
}

public class Dest
{
    public short SByteToShort { get; set; }
    public int SByteToInt { get; set; }
    public long SByteToLong { get; set; }
    public float SByteToFloat { get; set; }
    public double SByteToDouble { get; set; }
    public decimal SByteToDecimal { get; set; }

    public int ShortToInt { get; set; }
    public long ShortToLong { get; set; }
    public float ShortToFloat { get; set; }
    public double ShortToDouble { get; set; }
    public decimal ShortToDecimal { get; set; }

    public int UShortToInt { get; set; }
    public long UShortToLong { get; set; }
    public float UShortToFloat { get; set; }
    public double UShortToDouble { get; set; }
    public decimal UShortToDecimal { get; set; }

    public long IntToLong { get; set; }
    public float IntToFloat { get; set; }
    public double IntToDouble { get; set; }
    public decimal IntToDecimal { get; set; }

    public long UIntToLong { get; set; }
    public float UIntToFloat { get; set; }
    public double UIntToDouble { get; set; }
    public decimal UIntToDecimal { get; set; }

    public float LongToFloat { get; set; }
    public double LongToDouble { get; set; }
    public decimal LongToDecimal { get; set; }

    public float ULongToFloat { get; set; }
    public double ULongToDouble { get; set; }
    public decimal ULongToDecimal { get; set; }

    public double FloatToDouble { get; set; }
}

[Mapper]
public partial class AllWideningMapper
{
    [MapProperty(nameof(Source.SByteVal), nameof(Dest.SByteToShort))]
    [MapProperty(nameof(Source.SByteVal), nameof(Dest.SByteToInt))]
    [MapProperty(nameof(Source.SByteVal), nameof(Dest.SByteToLong))]
    [MapProperty(nameof(Source.SByteVal), nameof(Dest.SByteToFloat))]
    [MapProperty(nameof(Source.SByteVal), nameof(Dest.SByteToDouble))]
    [MapProperty(nameof(Source.SByteVal), nameof(Dest.SByteToDecimal))]

    [MapProperty(nameof(Source.ShortVal), nameof(Dest.ShortToInt))]
    [MapProperty(nameof(Source.ShortVal), nameof(Dest.ShortToLong))]
    [MapProperty(nameof(Source.ShortVal), nameof(Dest.ShortToFloat))]
    [MapProperty(nameof(Source.ShortVal), nameof(Dest.ShortToDouble))]
    [MapProperty(nameof(Source.ShortVal), nameof(Dest.ShortToDecimal))]

    [MapProperty(nameof(Source.UShortVal), nameof(Dest.UShortToInt))]
    [MapProperty(nameof(Source.UShortVal), nameof(Dest.UShortToLong))]
    [MapProperty(nameof(Source.UShortVal), nameof(Dest.UShortToFloat))]
    [MapProperty(nameof(Source.UShortVal), nameof(Dest.UShortToDouble))]
    [MapProperty(nameof(Source.UShortVal), nameof(Dest.UShortToDecimal))]

    [MapProperty(nameof(Source.IntVal), nameof(Dest.IntToLong))]
    [MapProperty(nameof(Source.IntVal), nameof(Dest.IntToFloat))]
    [MapProperty(nameof(Source.IntVal), nameof(Dest.IntToDouble))]
    [MapProperty(nameof(Source.IntVal), nameof(Dest.IntToDecimal))]

    [MapProperty(nameof(Source.UIntVal), nameof(Dest.UIntToLong))]
    [MapProperty(nameof(Source.UIntVal), nameof(Dest.UIntToFloat))]
    [MapProperty(nameof(Source.UIntVal), nameof(Dest.UIntToDouble))]
    [MapProperty(nameof(Source.UIntVal), nameof(Dest.UIntToDecimal))]

    [MapProperty(nameof(Source.LongVal), nameof(Dest.LongToFloat))]
    [MapProperty(nameof(Source.LongVal), nameof(Dest.LongToDouble))]
    [MapProperty(nameof(Source.LongVal), nameof(Dest.LongToDecimal))]

    [MapProperty(nameof(Source.ULongVal), nameof(Dest.ULongToFloat))]
    [MapProperty(nameof(Source.ULongVal), nameof(Dest.ULongToDouble))]
    [MapProperty(nameof(Source.ULongVal), nameof(Dest.ULongToDecimal))]

    [MapProperty(nameof(Source.FloatVal), nameof(Dest.FloatToDouble))]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("SByteToShort = source.SByteVal");
        output.Should().Contain("SByteToInt = source.SByteVal");
        output.Should().Contain("SByteToLong = source.SByteVal");
        output.Should().Contain("SByteToFloat = source.SByteVal");
        output.Should().Contain("SByteToDouble = source.SByteVal");
        output.Should().Contain("SByteToDecimal = source.SByteVal");
        output.Should().Contain("ShortToInt = source.ShortVal");
        output.Should().Contain("UShortToInt = source.UShortVal");
        output.Should().Contain("IntToLong = source.IntVal");
        output.Should().Contain("UIntToLong = source.UIntVal");
        output.Should().Contain("LongToFloat = source.LongVal");
        output.Should().Contain("ULongToFloat = source.ULongVal");
        output.Should().Contain("FloatToDouble = source.FloatVal");
    }

    [Fact]
    public void Resolve_WhenAllNarrowingNumericCombinations_ShouldEmitCastsAndELM015()
    {
        string source = @"
namespace TestNamespace;

public class Source
{
    public long LongVal { get; set; }
    public int IntVal { get; set; }
    public short ShortVal { get; set; }
    public double DoubleVal { get; set; }
    public float FloatVal { get; set; }
    public decimal DecimalVal { get; set; }
}

public class Dest
{
    public int LongToInt { get; set; }
    public short LongToShort { get; set; }
    public byte LongToByte { get; set; }

    public short IntToShort { get; set; }
    public byte IntToByte { get; set; }
    public sbyte IntToSByte { get; set; }
    public ushort IntToUShort { get; set; }

    public byte ShortToByte { get; set; }
    public sbyte ShortToSByte { get; set; }

    public float DoubleToFloat { get; set; }
    public decimal DoubleToDecimal { get; set; }
    public long DoubleToLong { get; set; }
    public int DoubleToInt { get; set; }

    public decimal FloatToDecimal { get; set; }
    public long FloatToLong { get; set; }
    public int FloatToInt { get; set; }

    public double DecimalToDouble { get; set; }
    public float DecimalToFloat { get; set; }
    public long DecimalToLong { get; set; }
    public int DecimalToInt { get; set; }
}

[Mapper]
public partial class AllNarrowingMapper
{
    [MapProperty(nameof(Source.LongVal), nameof(Dest.LongToInt))]
    [MapProperty(nameof(Source.LongVal), nameof(Dest.LongToShort))]
    [MapProperty(nameof(Source.LongVal), nameof(Dest.LongToByte))]

    [MapProperty(nameof(Source.IntVal), nameof(Dest.IntToShort))]
    [MapProperty(nameof(Source.IntVal), nameof(Dest.IntToByte))]
    [MapProperty(nameof(Source.IntVal), nameof(Dest.IntToSByte))]
    [MapProperty(nameof(Source.IntVal), nameof(Dest.IntToUShort))]

    [MapProperty(nameof(Source.ShortVal), nameof(Dest.ShortToByte))]
    [MapProperty(nameof(Source.ShortVal), nameof(Dest.ShortToSByte))]

    [MapProperty(nameof(Source.DoubleVal), nameof(Dest.DoubleToFloat))]
    [MapProperty(nameof(Source.DoubleVal), nameof(Dest.DoubleToDecimal))]
    [MapProperty(nameof(Source.DoubleVal), nameof(Dest.DoubleToLong))]
    [MapProperty(nameof(Source.DoubleVal), nameof(Dest.DoubleToInt))]

    [MapProperty(nameof(Source.FloatVal), nameof(Dest.FloatToDecimal))]
    [MapProperty(nameof(Source.FloatVal), nameof(Dest.FloatToLong))]
    [MapProperty(nameof(Source.FloatVal), nameof(Dest.FloatToInt))]

    [MapProperty(nameof(Source.DecimalVal), nameof(Dest.DecimalToDouble))]
    [MapProperty(nameof(Source.DecimalVal), nameof(Dest.DecimalToFloat))]
    [MapProperty(nameof(Source.DecimalVal), nameof(Dest.DecimalToLong))]
    [MapProperty(nameof(Source.DecimalVal), nameof(Dest.DecimalToInt))]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        diagnostics.Where(d => d.Id == "ELM015" && d.Severity == DiagnosticSeverity.Warning).Should().HaveCount(20);
        output.Should().Contain("LongToInt = (int)(source.LongVal)");
        output.Should().Contain("LongToShort = (short)(source.LongVal)");
        output.Should().Contain("LongToByte = (byte)(source.LongVal)");
        output.Should().Contain("IntToShort = (short)(source.IntVal)");
        output.Should().Contain("IntToByte = (byte)(source.IntVal)");
        output.Should().Contain("IntToSByte = (sbyte)(source.IntVal)");
        output.Should().Contain("IntToUShort = (ushort)(source.IntVal)");
        output.Should().Contain("ShortToByte = (byte)(source.ShortVal)");
        output.Should().Contain("ShortToSByte = (sbyte)(source.ShortVal)");
        output.Should().Contain("DoubleToFloat = (float)(source.DoubleVal)");
        output.Should().Contain("DoubleToDecimal = (decimal)(source.DoubleVal)");
        output.Should().Contain("DoubleToLong = (long)(source.DoubleVal)");
        output.Should().Contain("DoubleToInt = (int)(source.DoubleVal)");
        output.Should().Contain("FloatToDecimal = (decimal)(source.FloatVal)");
        output.Should().Contain("FloatToLong = (long)(source.FloatVal)");
        output.Should().Contain("FloatToInt = (int)(source.FloatVal)");
        output.Should().Contain("DecimalToDouble = (double)(source.DecimalVal)");
        output.Should().Contain("DecimalToFloat = (float)(source.DecimalVal)");
        output.Should().Contain("DecimalToLong = (long)(source.DecimalVal)");
        output.Should().Contain("DecimalToInt = (int)(source.DecimalVal)");
    }

    [Fact]
    public void Resolve_WhenNullableTargetWithNarrowingNumeric_ShouldEmitCastAndELM015()
    {
        string source = @"
namespace TestNamespace;

public class Source { public long Amount { get; set; } }
public class Dest   { public int? Amount { get; set; } }

[Mapper]
public partial class NullableNarrowingMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        diagnostics.Should().ContainSingle(d => d.Id == "ELM015" && d.Severity == DiagnosticSeverity.Warning);
        output.Should().Contain("Amount = (int)(source.Amount)");
    }
}

