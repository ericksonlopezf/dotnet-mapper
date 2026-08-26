// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Mapper.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Strategies;

public class ConversionStrategyFactoryDirectTests
{
    private static Compilation CreateCompilation(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code, path: "TestSource.cs");
        return CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            Basic.Reference.Assemblies.Net80.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static (ITypeSymbol Source, ITypeSymbol Target, List<IMethodSymbol> Methods, Location Loc) ExtractSymbols(
        string code,
        string sourceTypeName,
        string targetTypeName)
    {
        var compilation = CreateCompilation(code);
        var src = compilation.GetTypeByMetadataName(sourceTypeName)!;
        var dst = compilation.GetTypeByMetadataName(targetTypeName)!;
        var methods = new List<IMethodSymbol>();

        var mapperType = compilation.GetTypeByMetadataName("Mapper");
        if (mapperType != null)
        {
            methods.AddRange(mapperType.GetMembers().OfType<IMethodSymbol>());
        }

        var loc = src.Locations.Length > 0 ? src.Locations[0] : Location.None;
        return (src, dst, methods, loc);
    }

    [Fact]
    public void GetConversionStrategy_WhenSameType_ShouldReturnDirectAssignment()
    {
        string code = @"
public class SameTypeA {}
public class SameTypeB {}
";
        var (src, dst, methods, loc) = ExtractSymbols(code, "SameTypeA", "SameTypeA");
        var diagnostics = new List<DiagnosticInfo>();

        var strategy = ConversionStrategyFactory.GetConversionStrategy(src, dst, methods, diagnostics, loc, "Prop", isStrict: true);

        strategy.Should().BeOfType<ConversionStrategy.DirectAssignment>();
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void GetConversionStrategy_WhenUnwrappedSameTypeNullable_ShouldReturnDirectAssignment()
    {
        string code = @"
public struct MyStruct { public int X; }
public class Wrapper
{
    public MyStruct? NullableS { get; set; }
    public MyStruct NonNullS { get; set; }
}
";
        var comp = CreateCompilation(code);
        var wrapper = comp.GetTypeByMetadataName("Wrapper")!;
        var nullableS = wrapper.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NullableS").Type;
        var nonNullS = wrapper.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NonNullS").Type;
        var diagnostics = new List<DiagnosticInfo>();

        var s1 = ConversionStrategyFactory.GetConversionStrategy(nullableS, nonNullS, new List<IMethodSymbol>(), diagnostics, Location.None, "Prop", isStrict: true);
        var s2 = ConversionStrategyFactory.GetConversionStrategy(nonNullS, nullableS, new List<IMethodSymbol>(), diagnostics, Location.None, "Prop", isStrict: true);

        s1.Should().BeOfType<ConversionStrategy.DirectAssignment>();
        s2.Should().BeOfType<ConversionStrategy.DirectAssignment>();
    }

    [Fact]
    public void GetConversionStrategy_WhenTargetIsNullableValueTypeAndSourceIsNonNullable_ShouldResolveInnerStrategy()
    {
        string code = @"
public class Wrapper
{
    public byte ByteVal { get; set; }
    public int? NullableInt { get; set; }
    public string StrVal { get; set; }
}
";
        var comp = CreateCompilation(code);
        var wrapper = comp.GetTypeByMetadataName("Wrapper")!;
        var byteProp = wrapper.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "ByteVal").Type;
        var nullableIntProp = wrapper.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NullableInt").Type;
        var strProp = wrapper.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "StrVal").Type;
        var diagnostics = new List<DiagnosticInfo>();

        // byte -> int? (widening inner -> DirectAssignment)
        var s1 = ConversionStrategyFactory.GetConversionStrategy(byteProp, nullableIntProp, new List<IMethodSymbol>(), diagnostics, Location.None, "ByteVal", isStrict: true);
        s1.Should().BeOfType<ConversionStrategy.DirectAssignment>();

        // string -> int? (unsupported)
        var s2 = ConversionStrategyFactory.GetConversionStrategy(strProp, nullableIntProp, new List<IMethodSymbol>(), diagnostics, Location.None, "StrVal", isStrict: true);
        s2.Should().BeOfType<ConversionStrategy.Unsupported>();
    }

    [Theory]
    [InlineData(SpecialType.System_Byte, SpecialType.System_Int16)]
    [InlineData(SpecialType.System_Byte, SpecialType.System_Int32)]
    [InlineData(SpecialType.System_Byte, SpecialType.System_Int64)]
    [InlineData(SpecialType.System_Byte, SpecialType.System_Single)]
    [InlineData(SpecialType.System_Byte, SpecialType.System_Double)]
    [InlineData(SpecialType.System_Byte, SpecialType.System_Decimal)]
    [InlineData(SpecialType.System_SByte, SpecialType.System_Int16)]
    [InlineData(SpecialType.System_SByte, SpecialType.System_Int32)]
    [InlineData(SpecialType.System_SByte, SpecialType.System_Int64)]
    [InlineData(SpecialType.System_SByte, SpecialType.System_Single)]
    [InlineData(SpecialType.System_SByte, SpecialType.System_Double)]
    [InlineData(SpecialType.System_SByte, SpecialType.System_Decimal)]
    [InlineData(SpecialType.System_Int16, SpecialType.System_Int32)]
    [InlineData(SpecialType.System_Int16, SpecialType.System_Int64)]
    [InlineData(SpecialType.System_Int16, SpecialType.System_Single)]
    [InlineData(SpecialType.System_Int16, SpecialType.System_Double)]
    [InlineData(SpecialType.System_Int16, SpecialType.System_Decimal)]
    [InlineData(SpecialType.System_UInt16, SpecialType.System_Int32)]
    [InlineData(SpecialType.System_UInt16, SpecialType.System_Int64)]
    [InlineData(SpecialType.System_UInt16, SpecialType.System_Single)]
    [InlineData(SpecialType.System_UInt16, SpecialType.System_Double)]
    [InlineData(SpecialType.System_UInt16, SpecialType.System_Decimal)]
    [InlineData(SpecialType.System_Int32, SpecialType.System_Int64)]
    [InlineData(SpecialType.System_Int32, SpecialType.System_Single)]
    [InlineData(SpecialType.System_Int32, SpecialType.System_Double)]
    [InlineData(SpecialType.System_Int32, SpecialType.System_Decimal)]
    [InlineData(SpecialType.System_UInt32, SpecialType.System_Int64)]
    [InlineData(SpecialType.System_UInt32, SpecialType.System_Single)]
    [InlineData(SpecialType.System_UInt32, SpecialType.System_Double)]
    [InlineData(SpecialType.System_UInt32, SpecialType.System_Decimal)]
    [InlineData(SpecialType.System_Int64, SpecialType.System_Single)]
    [InlineData(SpecialType.System_Int64, SpecialType.System_Double)]
    [InlineData(SpecialType.System_Int64, SpecialType.System_Decimal)]
    [InlineData(SpecialType.System_UInt64, SpecialType.System_Single)]
    [InlineData(SpecialType.System_UInt64, SpecialType.System_Double)]
    [InlineData(SpecialType.System_UInt64, SpecialType.System_Decimal)]
    [InlineData(SpecialType.System_Single, SpecialType.System_Double)]
    public void GetConversionStrategy_WhenNumericWidening_ShouldReturnDirectAssignment(SpecialType srcType, SpecialType dstType)
    {
        var comp = CreateCompilation("");
        var src = comp.GetSpecialType(srcType);
        var dst = comp.GetSpecialType(dstType);
        var diagnostics = new List<DiagnosticInfo>();

        var strategy = ConversionStrategyFactory.GetConversionStrategy(src, dst, new List<IMethodSymbol>(), diagnostics, Location.None, "NumProp", isStrict: true);

        strategy.Should().BeOfType<ConversionStrategy.DirectAssignment>();
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData(SpecialType.System_Int64, SpecialType.System_Int32)]
    [InlineData(SpecialType.System_Int64, SpecialType.System_Int16)]
    [InlineData(SpecialType.System_Int64, SpecialType.System_Byte)]
    [InlineData(SpecialType.System_Int32, SpecialType.System_Int16)]
    [InlineData(SpecialType.System_Int32, SpecialType.System_Byte)]
    [InlineData(SpecialType.System_Int32, SpecialType.System_SByte)]
    [InlineData(SpecialType.System_Int32, SpecialType.System_UInt16)]
    [InlineData(SpecialType.System_Int16, SpecialType.System_Byte)]
    [InlineData(SpecialType.System_Int16, SpecialType.System_SByte)]
    [InlineData(SpecialType.System_Double, SpecialType.System_Single)]
    [InlineData(SpecialType.System_Double, SpecialType.System_Decimal)]
    [InlineData(SpecialType.System_Double, SpecialType.System_Int64)]
    [InlineData(SpecialType.System_Double, SpecialType.System_Int32)]
    [InlineData(SpecialType.System_Single, SpecialType.System_Decimal)]
    [InlineData(SpecialType.System_Single, SpecialType.System_Int64)]
    [InlineData(SpecialType.System_Single, SpecialType.System_Int32)]
    [InlineData(SpecialType.System_Decimal, SpecialType.System_Double)]
    [InlineData(SpecialType.System_Decimal, SpecialType.System_Single)]
    [InlineData(SpecialType.System_Decimal, SpecialType.System_Int64)]
    [InlineData(SpecialType.System_Decimal, SpecialType.System_Int32)]
    public void GetConversionStrategy_WhenNumericNarrowing_ShouldEmitELM015AndReturnBuiltinCast(SpecialType srcType, SpecialType dstType)
    {
        string code = "public class Dummy { public int X; }";
        var comp = CreateCompilation(code);
        var src = comp.GetSpecialType(srcType);
        var dst = comp.GetSpecialType(dstType);
        var loc = comp.SyntaxTrees.First().GetRoot().GetLocation();
        var diagnostics = new List<DiagnosticInfo>();

        var strategy = ConversionStrategyFactory.GetConversionStrategy(src, dst, new List<IMethodSymbol>(), diagnostics, loc, "NarrowProp", isStrict: true);

        strategy.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        var builtin = (ConversionStrategy.BuiltinConversion)strategy;
        builtin.ExpressionTemplate.Should().StartWith("(").And.EndWith("({0})");
        diagnostics.Should().ContainSingle();
        diagnostics[0].Id.Should().Be("ELM015");
        diagnostics[0].FilePath.Should().Be("TestSource.cs");
    }

    [Fact]
    public void GetConversionStrategy_WhenEnumToEnumByValueStrictMissing_ShouldEmitELM014AndReturnUnsupported()
    {
        string code = @"
public enum SrcEnum { A = 1, B = 2, C = 3 }
public enum DstEnum { A = 1, B = 2 }
";
        var (src, dst, methods, loc) = ExtractSymbols(code, "SrcEnum", "DstEnum");
        var diagnostics = new List<DiagnosticInfo>();

        var strategy = ConversionStrategyFactory.GetConversionStrategy(src, dst, methods, diagnostics, loc, "EnumProp", isStrict: true, enumStrategy: 1);

        strategy.Should().BeOfType<ConversionStrategy.Unsupported>();
        diagnostics.Should().ContainSingle();
        diagnostics[0].Id.Should().Be("ELM014");
        diagnostics[0].FilePath.Should().Be("TestSource.cs");
    }

    [Fact]
    public void GetConversionStrategy_WhenEnumToEnumByValueNonStrict_ShouldReturnBuiltinCast()
    {
        string code = @"
public enum SrcEnum { A = 1, B = 2, C = 3 }
public enum DstEnum { A = 1, B = 2 }
";
        var (src, dst, methods, loc) = ExtractSymbols(code, "SrcEnum", "DstEnum");
        var diagnostics = new List<DiagnosticInfo>();

        var strategy = ConversionStrategyFactory.GetConversionStrategy(src, dst, methods, diagnostics, loc, "EnumProp", isStrict: false, enumStrategy: 1);

        strategy.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        var builtin = (ConversionStrategy.BuiltinConversion)strategy;
        builtin.ExpressionTemplate.Should().Contain("(global::DstEnum)({0})");
    }

    [Fact]
    public void GetConversionStrategy_WhenEnumToEnumByNameIdenticalAndValuesMatch_ShouldReturnZeroCostCast()
    {
        string code = @"
public enum SrcEnum { Alpha = 1, Beta = 2 }
public enum DstEnum { Alpha = 1, Beta = 2 }
";
        var (src, dst, methods, loc) = ExtractSymbols(code, "SrcEnum", "DstEnum");
        var diagnostics = new List<DiagnosticInfo>();

        var strategy = ConversionStrategyFactory.GetConversionStrategy(src, dst, methods, diagnostics, loc, "EnumProp", isStrict: true, enumStrategy: 0);

        strategy.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        var builtin = (ConversionStrategy.BuiltinConversion)strategy;
        builtin.ExpressionTemplate.Should().Be("(global::DstEnum)({0})");
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void GetConversionStrategy_WhenEnumToEnumByNameValuesDiffer_ShouldEmitSwitchExpression()
    {
        string code = @"
public enum SrcEnum { Alpha = 1, Beta = 2 }
public enum DstEnum { Alpha = 1, Beta = 20 }
";
        var (src, dst, methods, loc) = ExtractSymbols(code, "SrcEnum", "DstEnum");
        var diagnostics = new List<DiagnosticInfo>();

        var strategy = ConversionStrategyFactory.GetConversionStrategy(src, dst, methods, diagnostics, loc, "EnumProp", isStrict: true, enumStrategy: 0);

        strategy.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        var builtin = (ConversionStrategy.BuiltinConversion)strategy;
        builtin.ExpressionTemplate.Should().Contain("switch").And.Contain("global::SrcEnum.Alpha => global::DstEnum.Alpha").And.Contain("global::SrcEnum.Beta => global::DstEnum.Beta");
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void GetConversionStrategy_WhenEnumToEnumByNameIgnoreCaseAndExplicitMapping_ShouldEmitSwitchExpression()
    {
        string code = @"
public enum SrcEnum { alpha = 1, Beta = 2 }
public enum DstEnum { Alpha = 1, Second = 2 }
";
        var (src, dst, methods, loc) = ExtractSymbols(code, "SrcEnum", "DstEnum");
        var diagnostics = new List<DiagnosticInfo>();
        var explicitMapping = new Dictionary<string, string> { ["Beta"] = "Second" };

        var strategy = ConversionStrategyFactory.GetConversionStrategy(
            src, dst, methods, diagnostics, loc, "EnumProp", isStrict: true, enumStrategy: 0, enumIgnoreCase: true, explicitEnumValues: explicitMapping);

        strategy.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        var builtin = (ConversionStrategy.BuiltinConversion)strategy;
        builtin.ExpressionTemplate.Should().Contain("global::SrcEnum.alpha => global::DstEnum.Alpha").And.Contain("global::SrcEnum.Beta => global::DstEnum.Second");
        diagnostics.Should().BeEmpty();

        // Explicit mapping to non-existent target member -> FirstOrDefault returns null and emits diagnostic
        var explicitNonExistent = new Dictionary<string, string> { ["Beta"] = "NonExistentTargetMember" };
        var diagNonExistent = new List<DiagnosticInfo>();
        var sNonExistent = ConversionStrategyFactory.GetConversionStrategy(
            src, dst, methods, diagNonExistent, loc, "EnumProp", isStrict: false, enumStrategy: 0, enumIgnoreCase: true, explicitEnumValues: explicitNonExistent);
        sNonExistent.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        diagNonExistent.Should().ContainSingle();
        diagNonExistent[0].Args.Should().Contain("Beta");
    }

    [Fact]
    public void GetConversionStrategy_WhenEnumToEnumByNameMissingStrictVsNonStrict()
    {
        string code = @"
public enum SrcEnum { A = 1, MissingMember = 2 }
public enum DstEnum { A = 1 }
";
        var (src, dst, methods, loc) = ExtractSymbols(code, "SrcEnum", "DstEnum");

        // Strict -> Error + Unsupported
        var diagStrict = new List<DiagnosticInfo>();
        var sStrict = ConversionStrategyFactory.GetConversionStrategy(src, dst, methods, diagStrict, loc, "EnumProp", isStrict: true, enumStrategy: 0);
        sStrict.Should().BeOfType<ConversionStrategy.Unsupported>();
        diagStrict.Should().ContainSingle();
        diagStrict[0].Id.Should().Be("ELM014");
        diagStrict[0].DefaultSeverity.Should().Be((int)DiagnosticSeverity.Error);
        diagStrict[0].FilePath.Should().Be("TestSource.cs");
        diagStrict[0].Args.Should().Contain("MissingMember");

        // Non-strict -> Warning + Switch
        var diagNonStrict = new List<DiagnosticInfo>();
        var sNonStrict = ConversionStrategyFactory.GetConversionStrategy(src, dst, methods, diagNonStrict, loc, "EnumProp", isStrict: false, enumStrategy: 0);
        sNonStrict.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        diagNonStrict.Should().ContainSingle();
        diagNonStrict[0].Id.Should().Be("ELM014");
        diagNonStrict[0].DefaultSeverity.Should().Be((int)DiagnosticSeverity.Warning);
        diagNonStrict[0].FilePath.Should().Be("TestSource.cs");
        diagNonStrict[0].Args.Should().Contain("MissingMember");

        // Location.None -> Fallback empty FilePath
        var diagLocNone = new List<DiagnosticInfo>();
        var sLocNone = ConversionStrategyFactory.GetConversionStrategy(src, dst, methods, diagLocNone, Location.None, "EnumProp", isStrict: false, enumStrategy: 0);
        diagLocNone.Should().ContainSingle();
        diagLocNone[0].FilePath.Should().Be("");
    }

    [Theory]
    [InlineData(SpecialType.System_Byte)]
    [InlineData(SpecialType.System_SByte)]
    [InlineData(SpecialType.System_Int16)]
    [InlineData(SpecialType.System_UInt16)]
    [InlineData(SpecialType.System_Int32)]
    [InlineData(SpecialType.System_UInt32)]
    [InlineData(SpecialType.System_Int64)]
    [InlineData(SpecialType.System_UInt64)]
    public void GetConversionStrategy_WhenIntegralToEnumAndEnumToIntegral_ShouldReturnCast(SpecialType integralType)
    {
        string code = @"
public enum MySampleEnum { A = 1 }
";
        var comp = CreateCompilation(code);
        var enumType = comp.GetTypeByMetadataName("MySampleEnum")!;
        var intType = comp.GetSpecialType(integralType);
        var diagnostics = new List<DiagnosticInfo>();

        var s1 = ConversionStrategyFactory.GetConversionStrategy(intType, enumType, new List<IMethodSymbol>(), diagnostics, Location.None, "Prop", isStrict: true);
        var s2 = ConversionStrategyFactory.GetConversionStrategy(enumType, intType, new List<IMethodSymbol>(), diagnostics, Location.None, "Prop", isStrict: true);

        s1.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        s2.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
    }

    [Fact]
    public void GetConversionStrategy_WhenEnumToString_ShouldReturnToStringBuiltin()
    {
        string code = @"
public enum MySampleEnum { A = 1 }
";
        var comp = CreateCompilation(code);
        var enumType = comp.GetTypeByMetadataName("MySampleEnum")!;
        var strType = comp.GetSpecialType(SpecialType.System_String);
        var diagnostics = new List<DiagnosticInfo>();

        var strategy = ConversionStrategyFactory.GetConversionStrategy(enumType, strType, new List<IMethodSymbol>(), diagnostics, Location.None, "Prop", isStrict: true);

        strategy.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        ((ConversionStrategy.BuiltinConversion)strategy).ExpressionTemplate.Should().Be("{0}.ToString()");
    }

    [Fact]
    public void GetConversionStrategy_WhenStringToEnum_ShouldEmitELM016AndReturnEnumParse()
    {
        string code = @"
public enum MySampleEnum { A = 1 }
";
        var comp = CreateCompilation(code);
        var enumType = comp.GetTypeByMetadataName("MySampleEnum")!;
        var strType = comp.GetSpecialType(SpecialType.System_String);
        var loc = comp.SyntaxTrees.First().GetRoot().GetLocation();
        var diagnostics = new List<DiagnosticInfo>();

        var strategy = ConversionStrategyFactory.GetConversionStrategy(strType, enumType, new List<IMethodSymbol>(), diagnostics, loc, "Prop", isStrict: true);

        strategy.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        ((ConversionStrategy.BuiltinConversion)strategy).ExpressionTemplate.Should().Contain("Enum.Parse<global::MySampleEnum>({0})");
        diagnostics.Should().ContainSingle();
        diagnostics[0].Id.Should().Be("ELM016");
        diagnostics[0].FilePath.Should().Be("TestSource.cs");
    }

    [Fact]
    public void GetConversionStrategy_WhenGuidToStringAndStringToGuid_ShouldReturnBuiltins()
    {
        var comp = CreateCompilation("");
        var guidType = comp.GetTypeByMetadataName("System.Guid")!;
        var strType = comp.GetSpecialType(SpecialType.System_String);
        var diagnostics = new List<DiagnosticInfo>();

        var s1 = ConversionStrategyFactory.GetConversionStrategy(guidType, strType, new List<IMethodSymbol>(), diagnostics, Location.None, "Prop", isStrict: true);
        var s2 = ConversionStrategyFactory.GetConversionStrategy(strType, guidType, new List<IMethodSymbol>(), diagnostics, Location.None, "Prop", isStrict: true);

        s1.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        ((ConversionStrategy.BuiltinConversion)s1).ExpressionTemplate.Should().Be("{0}.ToString()");

        s2.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        ((ConversionStrategy.BuiltinConversion)s2).ExpressionTemplate.Should().Be("global::System.Guid.Parse({0})");
    }

    [Fact]
    public void GetConversionStrategy_WhenTemporalConversions_ShouldReturnExactBuiltins()
    {
        var comp = CreateCompilation("");
        var dt = comp.GetTypeByMetadataName("System.DateTime")!;
        var doType = comp.GetTypeByMetadataName("System.DateOnly")!;
        var dto = comp.GetTypeByMetadataName("System.DateTimeOffset")!;
        var diagnostics = new List<DiagnosticInfo>();

        // DateTime <-> DateOnly
        var s1 = ConversionStrategyFactory.GetConversionStrategy(dt, doType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        ((ConversionStrategy.BuiltinConversion)s1).ExpressionTemplate.Should().Be("global::System.DateOnly.FromDateTime({0})");

        var s2 = ConversionStrategyFactory.GetConversionStrategy(doType, dt, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        ((ConversionStrategy.BuiltinConversion)s2).ExpressionTemplate.Should().Be("({0}).ToDateTime(global::System.TimeOnly.MinValue)");

        // DateTime <-> DateTimeOffset
        var s3 = ConversionStrategyFactory.GetConversionStrategy(dt, dto, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        ((ConversionStrategy.BuiltinConversion)s3).ExpressionTemplate.Should().Be("new global::System.DateTimeOffset({0})");

        var s4 = ConversionStrategyFactory.GetConversionStrategy(dto, dt, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        ((ConversionStrategy.BuiltinConversion)s4).ExpressionTemplate.Should().Be("({0}).DateTime");

        // DateOnly -> DateTimeOffset
        var s5 = ConversionStrategyFactory.GetConversionStrategy(doType, dto, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        ((ConversionStrategy.BuiltinConversion)s5).ExpressionTemplate.Should().Be("new global::System.DateTimeOffset(({0}).ToDateTime(global::System.TimeOnly.MinValue))");

        // DateTimeOffset -> DateOnly
        var s6 = ConversionStrategyFactory.GetConversionStrategy(dto, doType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        ((ConversionStrategy.BuiltinConversion)s6).ExpressionTemplate.Should().Be("global::System.DateOnly.FromDateTime(({0}).DateTime)");
    }

    [Fact]
    public void GetConversionStrategy_WhenExplicitOrImplicitCastOperatorsPresent_ShouldReturnValueObjectMappingWithMode2()
    {
        string code = @"
public class TargetWithCast
{
    public static explicit operator TargetWithCast(int val) => new TargetWithCast();
}
public class SourceWithCast
{
    public static implicit operator TargetWithCast(SourceWithCast s) => new TargetWithCast();
}
";
        var comp = CreateCompilation(code);
        var targetWithCast = comp.GetTypeByMetadataName("TargetWithCast")!;
        var sourceWithCast = comp.GetTypeByMetadataName("SourceWithCast")!;
        var intType = comp.GetSpecialType(SpecialType.System_Int32);
        var diagnostics = new List<DiagnosticInfo>();

        var s1 = ConversionStrategyFactory.GetConversionStrategy(intType, targetWithCast, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s1.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();
        ((ConversionStrategy.ValueObjectMapping)s1).Kind.Should().Be(2);

        var s2 = ConversionStrategyFactory.GetConversionStrategy(sourceWithCast, targetWithCast, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s2.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();
        ((ConversionStrategy.ValueObjectMapping)s2).Kind.Should().Be(2);
    }

    [Fact]
    public void GetConversionStrategy_WhenValueObjectHeuristic_ShouldReturnValueObjectMapping()
    {
        string code = @"
using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ValueObjectAttribute : Attribute {}

public class VoWithAttribute
{
    public string Name { get; }
    public VoWithAttribute(string name) { Name = name; }
}

public class VoWithValueProp
{
    public string Value { get; set; } = """";
    public VoWithValueProp(string value) { Value = value; }
}

public readonly record struct ReadOnlyRecordStructVo(string Code);
";
        var comp = CreateCompilation(code);
        var voAttr = comp.GetTypeByMetadataName("VoWithAttribute")!;
        var voVal = comp.GetTypeByMetadataName("VoWithValueProp")!;
        var voStruct = comp.GetTypeByMetadataName("ReadOnlyRecordStructVo")!;
        var strType = comp.GetSpecialType(SpecialType.System_String);
        var diagnostics = new List<DiagnosticInfo>();

        // Primitive -> VO (Kind 0)
        var s1 = ConversionStrategyFactory.GetConversionStrategy(strType, voVal, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s1.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();
        ((ConversionStrategy.ValueObjectMapping)s1).Kind.Should().Be(0);

        var s2 = ConversionStrategyFactory.GetConversionStrategy(strType, voStruct, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s2.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();
        ((ConversionStrategy.ValueObjectMapping)s2).Kind.Should().Be(0);

        // VO -> Primitive (Kind 1)
        var s3 = ConversionStrategyFactory.GetConversionStrategy(voVal, strType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s3.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();
        ((ConversionStrategy.ValueObjectMapping)s3).Kind.Should().Be(1);

        var s4 = ConversionStrategyFactory.GetConversionStrategy(voStruct, strType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s4.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();
        ((ConversionStrategy.ValueObjectMapping)s4).Kind.Should().Be(1);
    }

    [Fact]
    public void GetConversionStrategy_WhenArrayToArray_ShouldReturnEnumerableMapping()
    {
        var comp = CreateCompilation("");
        var srcArray = comp.CreateArrayTypeSymbol(comp.GetSpecialType(SpecialType.System_Int32));
        var dstArray = comp.CreateArrayTypeSymbol(comp.GetSpecialType(SpecialType.System_Int64));
        var dstUnsupportedArray = comp.CreateArrayTypeSymbol(comp.GetSpecialType(SpecialType.System_DateTime));
        var diagnostics = new List<DiagnosticInfo>();

        var s1 = ConversionStrategyFactory.GetConversionStrategy(srcArray, dstArray, new List<IMethodSymbol>(), diagnostics, Location.None, "Arr", isStrict: true);
        s1.Should().BeOfType<ConversionStrategy.EnumerableMapping>();
        var em = (ConversionStrategy.EnumerableMapping)s1;
        em.SourceIsArray.Should().BeTrue();
        em.IsArray.Should().BeTrue();
        em.IsList.Should().BeFalse();
        em.IsImmutableArray.Should().BeFalse();
        em.IsImmutableList.Should().BeFalse();
        em.IsFrozenSet.Should().BeFalse();
        em.IsHashSet.Should().BeFalse();
        em.SourceHasCount.Should().BeFalse();
        em.SourceIsValueType.Should().BeFalse();
        em.SourceIsImmutableArray.Should().BeFalse();

        var s2 = ConversionStrategyFactory.GetConversionStrategy(srcArray, dstUnsupportedArray, new List<IMethodSymbol>(), diagnostics, Location.None, "Arr", isStrict: true);
        s2.Should().BeOfType<ConversionStrategy.Unsupported>();
    }

    [Fact]
    public void GetConversionStrategy_WhenDictionaryToDictionary_ShouldReturnDictionaryMapping()
    {
        string code = @"
using System.Collections;
using System.Collections.Generic;

public class CustomDict<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => throw null!;
    IEnumerator IEnumerable.GetEnumerator() => throw null!;
}

public class DictHolder
{
    public Dictionary<string, int> Dict1 { get; set; } = new();
    public IDictionary<string, long> Dict2 { get; set; } = new Dictionary<string, long>();
    public IReadOnlyDictionary<string, short> Dict3 { get; set; } = new Dictionary<string, short>();
    public CustomDict<string, int> CustomDictProp { get; set; } = new();
    public Dictionary<object, object> UnsupportedDict { get; set; } = new();
}
";
        var comp = CreateCompilation(code);
        var holder = comp.GetTypeByMetadataName("DictHolder")!;
        var d1 = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "Dict1").Type;
        var d2 = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "Dict2").Type;
        var d3 = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "Dict3").Type;
        var dCustom = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "CustomDictProp").Type;
        var dUnsupported = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "UnsupportedDict").Type;
        var diagnostics = new List<DiagnosticInfo>();

        // Dictionary<string, int> -> IDictionary<string, long>
        var s1 = ConversionStrategyFactory.GetConversionStrategy(d1, d2, new List<IMethodSymbol>(), diagnostics, Location.None, "Dict", isStrict: true);
        s1.Should().BeOfType<ConversionStrategy.DictionaryMapping>();
        var dm1 = (ConversionStrategy.DictionaryMapping)s1;
        dm1.SourceHasCount.Should().BeTrue();

        // IDictionary<string, long> -> IReadOnlyDictionary<string, short> (narrowing long -> short with warning)
        var s2 = ConversionStrategyFactory.GetConversionStrategy(d2, d3, new List<IMethodSymbol>(), diagnostics, Location.None, "Dict", isStrict: false);
        s2.Should().BeOfType<ConversionStrategy.DictionaryMapping>();
        var dm2 = (ConversionStrategy.DictionaryMapping)s2;
        dm2.SourceHasCount.Should().BeTrue();

        // IReadOnlyDictionary<string, short> -> Dictionary<string, int> (widening short -> int)
        var s3 = ConversionStrategyFactory.GetConversionStrategy(d3, d1, new List<IMethodSymbol>(), diagnostics, Location.None, "Dict", isStrict: true);
        s3.Should().BeOfType<ConversionStrategy.DictionaryMapping>();
        var dm3 = (ConversionStrategy.DictionaryMapping)s3;
        dm3.SourceHasCount.Should().BeTrue();

        // CustomDict -> Dictionary (CustomDict has no ICollection interface -> SourceHasCount = false)
        var sCustom = ConversionStrategyFactory.GetConversionStrategy(dCustom, d1, new List<IMethodSymbol>(), diagnostics, Location.None, "Dict", isStrict: true);
        // Note: CustomDict is named "CustomDict", so it does not match "Dictionary" name check and returns Unsupported
        sCustom.Should().BeOfType<ConversionStrategy.Unsupported>();

        var sUnsupp = ConversionStrategyFactory.GetConversionStrategy(d1, dUnsupported, new List<IMethodSymbol>(), diagnostics, Location.None, "Dict", isStrict: true);
        sUnsupp.Should().BeOfType<ConversionStrategy.Unsupported>();
    }

    [Fact]
    public void GetConversionStrategy_WhenHashSetTarget_ShouldReturnEnumerableMappingWithIsHashSet()
    {
        string code = @"
using System.Collections;
using System.Collections.Generic;

public class CustomEnumSource<T> : IEnumerable<T>
{
    public IEnumerator<T> GetEnumerator() => throw null!;
    IEnumerator IEnumerable.GetEnumerator() => throw null!;
}

public class HsHolder
{
    public List<int> ListSource { get; set; } = new();
    public int[] ArraySource { get; set; } = new int[0];
    public CustomEnumSource<int> CustomEnum { get; set; } = new();
    public HashSet<long> TargetHashSet { get; set; } = new();
    public HashSet<object> TargetUnsupportedHashSet { get; set; } = new();
}
";
        var comp = CreateCompilation(code);
        var holder = comp.GetTypeByMetadataName("HsHolder")!;
        var listSrc = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "ListSource").Type;
        var arrSrc = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "ArraySource").Type;
        var customSrc = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "CustomEnum").Type;
        var targetHs = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "TargetHashSet").Type;
        var targetUnsuppHs = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "TargetUnsupportedHashSet").Type;
        var diagnostics = new List<DiagnosticInfo>();

        // Generic collection -> HashSet
        var s1 = ConversionStrategyFactory.GetConversionStrategy(listSrc, targetHs, new List<IMethodSymbol>(), diagnostics, Location.None, "Hs", isStrict: true);
        s1.Should().BeOfType<ConversionStrategy.EnumerableMapping>();
        var em1 = (ConversionStrategy.EnumerableMapping)s1;
        em1.IsHashSet.Should().BeTrue();
        em1.IsArray.Should().BeFalse();
        em1.IsList.Should().BeFalse();
        em1.IsImmutableArray.Should().BeFalse();
        em1.SourceIsArray.Should().BeFalse();
        em1.SourceHasCount.Should().BeTrue();

        // CustomEnum -> HashSet (no ICollection -> SourceHasCount = false)
        var sCustom = ConversionStrategyFactory.GetConversionStrategy(customSrc, targetHs, new List<IMethodSymbol>(), diagnostics, Location.None, "Hs", isStrict: true);
        sCustom.Should().BeOfType<ConversionStrategy.EnumerableMapping>();
        var emCustom = (ConversionStrategy.EnumerableMapping)sCustom;
        emCustom.SourceHasCount.Should().BeFalse();
        emCustom.IsHashSet.Should().BeTrue();

        // Array -> HashSet
        var s2 = ConversionStrategyFactory.GetConversionStrategy(arrSrc, targetHs, new List<IMethodSymbol>(), diagnostics, Location.None, "Hs", isStrict: true);
        s2.Should().BeOfType<ConversionStrategy.EnumerableMapping>();
        var em2 = (ConversionStrategy.EnumerableMapping)s2;
        em2.IsHashSet.Should().BeTrue();
        em2.IsList.Should().BeFalse();
        em2.IsImmutableArray.Should().BeFalse();
        em2.IsImmutableList.Should().BeFalse();
        em2.IsFrozenSet.Should().BeFalse();
        em2.IsArray.Should().BeFalse();
        em2.SourceIsArray.Should().BeTrue();
        em2.SourceHasCount.Should().BeFalse();
        em2.SourceIsValueType.Should().BeFalse();
        em2.SourceIsImmutableArray.Should().BeFalse();

        // Unsupported elements
        var s3 = ConversionStrategyFactory.GetConversionStrategy(listSrc, targetUnsuppHs, new List<IMethodSymbol>(), diagnostics, Location.None, "Hs", isStrict: true);
        s3.Should().BeOfType<ConversionStrategy.Unsupported>();

        var s4 = ConversionStrategyFactory.GetConversionStrategy(arrSrc, targetUnsuppHs, new List<IMethodSymbol>(), diagnostics, Location.None, "Hs", isStrict: true);
        s4.Should().BeOfType<ConversionStrategy.Unsupported>();
    }

    [Fact]
    public void GetConversionStrategy_WhenCollectionTargets_ShouldSupportAllCollectionKinds()
    {
        string code = @"
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Frozen;

public class CustomEnumerable<T> : System.Collections.Generic.IEnumerable<T>
{
    public System.Collections.Generic.IEnumerator<T> GetEnumerator() => throw null!;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw null!;
}

public class OnlyIList<T> : System.Collections.Generic.IList<T>
{
    public T this[int index] { get => throw null!; set => throw null!; }
    public int Count => 0;
    public bool IsReadOnly => false;
    public void Add(T item) {}
    public void Clear() {}
    public bool Contains(T item) => false;
    public void CopyTo(T[] array, int arrayIndex) {}
    public System.Collections.Generic.IEnumerator<T> GetEnumerator() => throw null!;
    public int IndexOf(T item) => -1;
    public void Insert(int index, T item) {}
    public bool Remove(T item) => false;
    public void RemoveAt(int index) {}
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw null!;
}

public class OnlyIReadOnlyList<T> : System.Collections.Generic.IReadOnlyList<T>
{
    public T this[int index] => throw null!;
    public int Count => 0;
    public System.Collections.Generic.IEnumerator<T> GetEnumerator() => throw null!;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw null!;
}

public class OnlyICollection<T> : System.Collections.Generic.ICollection<T>
{
    public int Count => 0;
    public bool IsReadOnly => false;
    public void Add(T item) {}
    public void Clear() {}
    public bool Contains(T item) => false;
    public void CopyTo(T[] array, int arrayIndex) {}
    public System.Collections.Generic.IEnumerator<T> GetEnumerator() => throw null!;
    public bool Remove(T item) => false;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw null!;
}

public class OnlyIReadOnlyCollection<T> : System.Collections.Generic.IReadOnlyCollection<T>
{
    public int Count => 0;
    public System.Collections.Generic.IEnumerator<T> GetEnumerator() => throw null!;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw null!;
}

public class CollHolder
{
    public List<int> ListInt { get; set; } = new();
    public int[] ArrayInt { get; set; } = new int[0];
    public ReadOnlySpan<int> SpanInt => ArrayInt;
    public Span<int> MutableSpanInt => ArrayInt;
    public CustomEnumerable<int> CustomEnum { get; set; } = new();
    public OnlyIList<int> OnlyList { get; set; } = new();
    public OnlyIReadOnlyList<int> OnlyRoList { get; set; } = new();
    public OnlyICollection<int> OnlyColl { get; set; } = new();
    public OnlyIReadOnlyCollection<int> OnlyRoColl { get; set; } = new();

    public List<long> TargetList { get; set; } = new();
    public HashSet<long> TargetHashSet { get; set; } = new();
    public IEnumerable<long> TargetEnumerable { get; set; } = new List<long>();
    public IList<long> TargetIList { get; set; } = new List<long>();
    public IReadOnlyList<long> TargetIReadOnlyList { get; set; } = new List<long>();
    public IReadOnlyCollection<long> TargetIReadOnlyCollection { get; set; } = new List<long>();
    public ICollection<long> TargetICollection { get; set; } = new List<long>();
    public ImmutableArray<long> TargetImmutableArray { get; set; }
    public ImmutableList<long> TargetImmutableList { get; set; }
    public IImmutableList<long> TargetIImmutableList { get; set; }
    public FrozenSet<long> TargetFrozenSet { get; set; }
}
";
        var comp = CreateCompilation(code);
        var holder = comp.GetTypeByMetadataName("CollHolder")!;
        var listSrc = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "ListInt").Type;
        var arrSrc = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "ArrayInt").Type;
        var spanSrc = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "SpanInt").Type;
        var mutSpanSrc = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "MutableSpanInt").Type;
        var customEnumSrc = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "CustomEnum").Type;
        var onlyListSrc = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "OnlyList").Type;
        var onlyRoListSrc = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "OnlyRoList").Type;
        var onlyCollSrc = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "OnlyColl").Type;
        var onlyRoCollSrc = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "OnlyRoColl").Type;
        var targetHs = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "TargetHashSet").Type;
        var targetList = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "TargetList").Type;

        var targetNames = new[]
        {
            "TargetList", "TargetEnumerable", "TargetIList", "TargetIReadOnlyList",
            "TargetIReadOnlyCollection", "TargetICollection", "TargetImmutableArray",
            "TargetImmutableList", "TargetIImmutableList", "TargetFrozenSet"
        };

        var diagnostics = new List<DiagnosticInfo>();

        foreach (var targetName in targetNames)
        {
            var targetType = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == targetName).Type;

            var fromList = (ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(listSrc, targetType, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true);
            fromList.Should().NotBeNull();
            if (targetName is "TargetList" or "TargetEnumerable" or "TargetIList" or "TargetIReadOnlyList" or "TargetIReadOnlyCollection" or "TargetICollection")
            {
                fromList.IsList.Should().BeTrue();
            }

            var fromArr = (ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(arrSrc, targetType, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true);
            fromArr.SourceIsArray.Should().BeTrue();
            fromArr.IsArray.Should().BeFalse();
            fromArr.SourceHasCount.Should().BeFalse();
            fromArr.SourceIsValueType.Should().BeFalse();
            fromArr.SourceIsImmutableArray.Should().BeFalse();
        }

        // Test all 4 individual interfaces to List
        ((ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(onlyListSrc, targetList, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true)).SourceHasCount.Should().BeTrue();
        ((ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(onlyRoListSrc, targetList, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true)).SourceHasCount.Should().BeTrue();
        ((ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(onlyCollSrc, targetList, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true)).SourceHasCount.Should().BeTrue();
        ((ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(onlyRoCollSrc, targetList, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true)).SourceHasCount.Should().BeTrue();

        // Test all 4 individual interfaces to HashSet
        ((ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(onlyListSrc, targetHs, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true)).SourceHasCount.Should().BeTrue();
        ((ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(onlyRoListSrc, targetHs, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true)).SourceHasCount.Should().BeTrue();
        ((ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(onlyCollSrc, targetHs, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true)).SourceHasCount.Should().BeTrue();
        ((ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(onlyRoCollSrc, targetHs, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true)).SourceHasCount.Should().BeTrue();

        // Span sources -> SourceIsArray = true, SourceHasCount = true
        var fromSpan = (ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(spanSrc, targetList, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true);
        fromSpan.SourceIsArray.Should().BeTrue();
        fromSpan.SourceHasCount.Should().BeTrue();

        var fromMutSpan = (ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(mutSpanSrc, targetList, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true);
        fromMutSpan.SourceIsArray.Should().BeTrue();
        fromMutSpan.SourceHasCount.Should().BeTrue();

        // Custom IEnumerable only -> SourceHasCount = false
        var fromCustomEnum = (ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(customEnumSrc, targetList, new List<IMethodSymbol>(), diagnostics, Location.None, "Coll", isStrict: true);
        fromCustomEnum.SourceHasCount.Should().BeFalse();
    }

    [Fact]
    public void GetConversionStrategy_WhenCrossMapperMethodWithNullableSignatures_ShouldMatchUnwrappedTypes()
    {
        string code = @"
public struct StructSrc { public int X; }
public struct StructDst { public int Y; }
public class Mapper
{
    public StructDst MapParamNullable(StructSrc? s) => new StructDst();
    public StructDst? MapReturnNullable(StructSrc s) => new StructDst();
    public int BadReturn(StructSrc s) => 0;
}
";
        var comp = CreateCompilation(code);
        var src = comp.GetTypeByMetadataName("StructSrc")!;
        var dst = comp.GetTypeByMetadataName("StructDst")!;
        var mapper = comp.GetTypeByMetadataName("Mapper")!;
        var method1 = mapper.GetMembers().OfType<IMethodSymbol>().First(m => m.Name == "MapParamNullable");
        var method2 = mapper.GetMembers().OfType<IMethodSymbol>().First(m => m.Name == "MapReturnNullable");
        var badMethod = mapper.GetMembers().OfType<IMethodSymbol>().First(m => m.Name == "BadReturn");
        var diagnostics = new List<DiagnosticInfo>();

        // Mapping non-nullable StructSrc -> StructDst using MapParamNullable (param is StructSrc?)
        var s1 = ConversionStrategyFactory.GetConversionStrategy(src, dst, new List<IMethodSymbol> { method1 }, diagnostics, Location.None, "P", isStrict: true);
        s1.Should().BeOfType<ConversionStrategy.MapMethodInvocation>();
        ((ConversionStrategy.MapMethodInvocation)s1).MethodName.Should().Be("MapParamNullable");
        ((ConversionStrategy.MapMethodInvocation)s1).MethodKey.Should().Be("MapParamNullable(global::StructSrc?)");

        // Mapping StructSrc -> non-nullable StructDst using MapReturnNullable (return is StructDst?)
        var s2 = ConversionStrategyFactory.GetConversionStrategy(src, dst, new List<IMethodSymbol> { method2 }, diagnostics, Location.None, "P", isStrict: true);
        s2.Should().BeOfType<ConversionStrategy.MapMethodInvocation>();
        ((ConversionStrategy.MapMethodInvocation)s2).MethodName.Should().Be("MapReturnNullable");
        ((ConversionStrategy.MapMethodInvocation)s2).MethodKey.Should().Be("MapReturnNullable(global::StructSrc)");

        // Mapping Nullable StructSrc -> non-nullable StructDst
        var nullableGen = comp.GetTypeByMetadataName("System.Nullable`1")!;
        var nullSrc = nullableGen.Construct(src);
        var nullDst = nullableGen.Construct(dst);

        var sNullSrc = ConversionStrategyFactory.GetConversionStrategy(nullSrc, dst, new List<IMethodSymbol> { method1 }, diagnostics, Location.None, "P", isStrict: true);
        sNullSrc.Should().BeOfType<ConversionStrategy.MapMethodInvocation>();
        ((ConversionStrategy.MapMethodInvocation)sNullSrc).IsSourceNullable.Should().BeTrue();
        ((ConversionStrategy.MapMethodInvocation)sNullSrc).IsTargetNullable.Should().BeFalse();

        var sNullDst = ConversionStrategyFactory.GetConversionStrategy(src, nullDst, new List<IMethodSymbol> { method2 }, diagnostics, Location.None, "P", isStrict: true);
        sNullDst.Should().BeOfType<ConversionStrategy.MapMethodInvocation>();
        ((ConversionStrategy.MapMethodInvocation)sNullDst).IsSourceNullable.Should().BeFalse();
        ((ConversionStrategy.MapMethodInvocation)sNullDst).IsTargetNullable.Should().BeTrue();

        // BadReturn method should not match StructSrc -> StructDst
        var sBad = ConversionStrategyFactory.GetConversionStrategy(src, dst, new List<IMethodSymbol> { badMethod }, diagnostics, Location.None, "P", isStrict: true);
        sBad.Should().BeOfType<ConversionStrategy.Unsupported>();
    }

    [Fact]
    public void GetConversionStrategy_WhenValueObjectNegativeCases_ShouldReturnUnsupported()
    {
        string code = @"
using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ValueObject : Attribute {}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ValueObjectAttribute : Attribute {}

[ValueObject]
public class VoWithShortAttr
{
    public string Code { get; }
    public VoWithShortAttr(string code) { Code = code; }
}

[ValueObjectAttribute]
public class VoWithLongAttr
{
    public string Code { get; }
    public VoWithLongAttr(string code) { Code = code; }
}

public class VoWithPrivateProp
{
    private string Hidden { get; set; } = """";
    public string Value { get; set; } = """";
    public VoWithPrivateProp(string value) { Value = value; }
}

public class MultiPropVo
{
    public string Val1 { get; set; } = """";
    public string Val2 { get; set; } = """";
    public MultiPropVo(string val1, string val2) { Val1 = val1; Val2 = val2; }
}

public class StaticPropVo
{
    public static string Default => """";
    public string Value { get; set; } = """";
    public StaticPropVo(string value) { Value = value; }
}

public class CtorMismatchVo
{
    public string Value { get; set; } = """";
    public CtorMismatchVo(int num) { Value = num.ToString(); }
}

public struct NonReadOnlyRecordStruct(string Code);
public readonly struct NonRecordReadOnlyStructWithoutValue { public string Code { get; } public NonRecordReadOnlyStructWithoutValue(string v) { Code = v; } }
public readonly struct NonRecordReadOnlyStruct { public string Value { get; } public NonRecordReadOnlyStruct(string v) { Value = v; } }
public record class RecordClassVo(string Name);
";
        var comp = CreateCompilation(code);
        var voShort = comp.GetTypeByMetadataName("VoWithShortAttr")!;
        var voLong = comp.GetTypeByMetadataName("VoWithLongAttr")!;
        var voPriv = comp.GetTypeByMetadataName("VoWithPrivateProp")!;
        var multiProp = comp.GetTypeByMetadataName("MultiPropVo")!;
        var staticProp = comp.GetTypeByMetadataName("StaticPropVo")!;
        var ctorMismatch = comp.GetTypeByMetadataName("CtorMismatchVo")!;
        var nonRoRecordStruct = comp.GetTypeByMetadataName("NonReadOnlyRecordStruct")!;
        var nonRecordRoWithoutVal = comp.GetTypeByMetadataName("NonRecordReadOnlyStructWithoutValue")!;
        var nonRecordRoStruct = comp.GetTypeByMetadataName("NonRecordReadOnlyStruct")!;
        var recordClass = comp.GetTypeByMetadataName("RecordClassVo")!;
        var strType = comp.GetSpecialType(SpecialType.System_String);
        var diagnostics = new List<DiagnosticInfo>();

        // VoWithShortAttr -> primitive and back
        var sShort1 = ConversionStrategyFactory.GetConversionStrategy(strType, voShort, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        sShort1.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();
        ((ConversionStrategy.ValueObjectMapping)sShort1).Kind.Should().Be(0);

        var sShort2 = ConversionStrategyFactory.GetConversionStrategy(voShort, strType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        sShort2.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();
        ((ConversionStrategy.ValueObjectMapping)sShort2).Kind.Should().Be(1);

        // VoWithLongAttr -> primitive and back (explicitly tests ValueObjectAttribute class name)
        var sLong1 = ConversionStrategyFactory.GetConversionStrategy(strType, voLong, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        sLong1.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();
        ((ConversionStrategy.ValueObjectMapping)sLong1).Kind.Should().Be(0);

        var sLong2 = ConversionStrategyFactory.GetConversionStrategy(voLong, strType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        sLong2.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();
        ((ConversionStrategy.ValueObjectMapping)sLong2).Kind.Should().Be(1);

        // VoWithPrivateProp -> source VO ignores private property and matches 1 public prop
        var sPrivSource = ConversionStrategyFactory.GetConversionStrategy(voPriv, strType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        sPrivSource.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();
        ((ConversionStrategy.ValueObjectMapping)sPrivSource).Kind.Should().Be(1);

        // StaticPropVo -> should ignore static prop and match 1 instance prop
        var sStatic = ConversionStrategyFactory.GetConversionStrategy(strType, staticProp, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        sStatic.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();

        // MultiProp -> not a VO
        var s1 = ConversionStrategyFactory.GetConversionStrategy(strType, multiProp, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s1.Should().BeOfType<ConversionStrategy.Unsupported>();

        var s1Rev = ConversionStrategyFactory.GetConversionStrategy(multiProp, strType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s1Rev.Should().BeOfType<ConversionStrategy.Unsupported>();

        // CtorMismatch -> cannot construct
        var s2 = ConversionStrategyFactory.GetConversionStrategy(strType, ctorMismatch, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s2.Should().BeOfType<ConversionStrategy.Unsupported>();

        // NonReadOnlyRecordStruct -> not readonly record struct
        var s3 = ConversionStrategyFactory.GetConversionStrategy(strType, nonRoRecordStruct, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s3.Should().BeOfType<ConversionStrategy.Unsupported>();

        var s3Source = ConversionStrategyFactory.GetConversionStrategy(nonRoRecordStruct, strType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s3Source.Should().BeOfType<ConversionStrategy.Unsupported>();

        // NonRecordReadOnlyStructWithoutValue -> not a VO
        var s3b = ConversionStrategyFactory.GetConversionStrategy(strType, nonRecordRoWithoutVal, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s3b.Should().BeOfType<ConversionStrategy.Unsupported>();

        var s3bSource = ConversionStrategyFactory.GetConversionStrategy(nonRecordRoWithoutVal, strType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s3bSource.Should().BeOfType<ConversionStrategy.Unsupported>();

        // NonRecordReadOnlyStruct with Value prop -> matches hasValueProp
        var s4 = ConversionStrategyFactory.GetConversionStrategy(strType, nonRecordRoStruct, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s4.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();

        var s4Source = ConversionStrategyFactory.GetConversionStrategy(nonRecordRoStruct, strType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s4Source.Should().BeOfType<ConversionStrategy.ValueObjectMapping>();

        // RecordClass without attribute and without Value prop -> not VO
        var s5 = ConversionStrategyFactory.GetConversionStrategy(strType, recordClass, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s5.Should().BeOfType<ConversionStrategy.Unsupported>();

        var s5Source = ConversionStrategyFactory.GetConversionStrategy(recordClass, strType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s5Source.Should().BeOfType<ConversionStrategy.Unsupported>();
    }

    [Fact]
    public void GetConversionStrategy_WhenCollectionDetailedFlagsChecked_ShouldMatchExpectedProperties()
    {
        string code = @"
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Frozen;

public class TypesHolder
{
    public List<int> ListType { get; set; } = new();
    public ImmutableArray<int> ImmArrayType { get; set; }
    public ImmutableList<int> ImmListType { get; set; }
    public IImmutableList<int> IImmListType { get; set; }
    public FrozenSet<int> FrozenSetType { get; set; }
    public ReadOnlySpan<int> SpanType => new int[0];
    public Span<int> MutableSpanType => new int[0];
}
";
        var comp = CreateCompilation(code);
        var holder = comp.GetTypeByMetadataName("TypesHolder")!;
        var listType = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "ListType").Type;
        var immArrayType = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "ImmArrayType").Type;
        var immListType = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "ImmListType").Type;
        var iImmListType = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "IImmListType").Type;
        var frozenSetType = holder.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "FrozenSetType").Type;
        var diagnostics = new List<DiagnosticInfo>();

        // ImmutableArray target from List
        var sImmArray = (ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(listType, immArrayType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        sImmArray.IsImmutableArray.Should().BeTrue();
        sImmArray.IsList.Should().BeFalse();
        sImmArray.IsImmutableList.Should().BeFalse();
        sImmArray.IsFrozenSet.Should().BeFalse();

        // ImmutableList target
        var sImmList = (ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(listType, immListType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        sImmList.IsImmutableList.Should().BeTrue();
        sImmList.IsList.Should().BeFalse();

        // IImmutableList target
        var sIImmList = (ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(listType, iImmListType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        sIImmList.IsImmutableList.Should().BeTrue();

        // FrozenSet target
        var sFrozen = (ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(listType, frozenSetType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        sFrozen.IsFrozenSet.Should().BeTrue();
        sFrozen.IsList.Should().BeFalse();

        // Source is ImmutableArray
        var sFromImmArray = (ConversionStrategy.EnumerableMapping)ConversionStrategyFactory.GetConversionStrategy(immArrayType, listType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        sFromImmArray.SourceIsImmutableArray.Should().BeTrue();
        sFromImmArray.SourceIsValueType.Should().BeTrue();
    }

    [Fact]
    public void GetConversionStrategy_WhenNonNumericTypes_ShouldReturnUnsupported()
    {
        var comp = CreateCompilation("");
        var boolType = comp.GetSpecialType(SpecialType.System_Boolean);
        var charType = comp.GetSpecialType(SpecialType.System_Char);
        var intType = comp.GetSpecialType(SpecialType.System_Int32);
        var diagnostics = new List<DiagnosticInfo>();

        var s1 = ConversionStrategyFactory.GetConversionStrategy(boolType, intType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s1.Should().BeOfType<ConversionStrategy.Unsupported>();

        var s2 = ConversionStrategyFactory.GetConversionStrategy(intType, boolType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s2.Should().BeOfType<ConversionStrategy.Unsupported>();

        var s3 = ConversionStrategyFactory.GetConversionStrategy(charType, boolType, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s3.Should().BeOfType<ConversionStrategy.Unsupported>();
    }

    [Fact]
    public void GetConversionStrategy_WhenEnumByNameCaseSensitivityAndGate1()
    {
        string code = @"
public enum LowerEnum { alpha = 1, beta = 2 }
public enum UpperEnum { Alpha = 1, Beta = 2 }
";
        var (src, dst, methods, loc) = ExtractSymbols(code, "LowerEnum", "UpperEnum");

        // Case-sensitive without explicit map -> does NOT match identical names, emits switch
        var diagOrdinal = new List<DiagnosticInfo>();
        var sOrdinal = ConversionStrategyFactory.GetConversionStrategy(src, dst, methods, diagOrdinal, loc, "P", isStrict: false, enumStrategy: 0, enumIgnoreCase: false);
        sOrdinal.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        // Unmapped in ordinal mode -> emits warning
        diagOrdinal.Should().NotBeEmpty();

        // Case-insensitive -> matches names, but Gate 1 requires identical names (Lower != Upper), so emits switch
        var diagIgnoreCase = new List<DiagnosticInfo>();
        var sIgnoreCase = ConversionStrategyFactory.GetConversionStrategy(src, dst, methods, diagIgnoreCase, loc, "P", isStrict: false, enumStrategy: 0, enumIgnoreCase: true);
        sIgnoreCase.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        diagIgnoreCase.Should().BeEmpty();
        ((ConversionStrategy.BuiltinConversion)sIgnoreCase).ExpressionTemplate.Should().Contain("switch");
    }

    [Fact]
    public void GetConversionStrategy_WhenDiagnosticsWithSourceTree_ShouldIncludeFilePath()
    {
        string code = @"
public enum SrcEnum { A = 1, Extra = 2 }
public enum DstEnum { A = 1 }
";
        var (src, dst, methods, loc) = ExtractSymbols(code, "SrcEnum", "DstEnum");
        var diagnostics = new List<DiagnosticInfo>();

        var s = ConversionStrategyFactory.GetConversionStrategy(src, dst, methods, diagnostics, loc, "P", isStrict: true, enumStrategy: 0);

        diagnostics.Should().ContainSingle();
        diagnostics[0].FilePath.Should().Be("TestSource.cs");
        diagnostics[0].Line.Should().BeGreaterThanOrEqualTo(0);
        diagnostics[0].Column.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetConversionStrategy_WhenDiagnosticsGeneratedFromLocationNone_ShouldHaveEmptyFilePath()
    {
        var comp = CreateCompilation("");
        var longType = comp.GetSpecialType(SpecialType.System_Int64);
        var intType = comp.GetSpecialType(SpecialType.System_Int32);
        var strType = comp.GetSpecialType(SpecialType.System_String);
        var diagNarrow = new List<DiagnosticInfo>();

        // Narrowing with Location.None
        ConversionStrategyFactory.GetConversionStrategy(longType, intType, new List<IMethodSymbol>(), diagNarrow, Location.None, "Prop", isStrict: true);
        diagNarrow.Should().ContainSingle();
        diagNarrow[0].FilePath.Should().Be("");

        // StringToEnum with Location.None
        string code = @"public enum LocNoneEnum { A }";
        var comp2 = CreateCompilation(code);
        var enumType = comp2.GetTypeByMetadataName("LocNoneEnum")!;
        var diagEnum = new List<DiagnosticInfo>();
        ConversionStrategyFactory.GetConversionStrategy(strType, enumType, new List<IMethodSymbol>(), diagEnum, Location.None, "Prop", isStrict: true);
        diagEnum.Should().ContainSingle();
        diagEnum[0].FilePath.Should().Be("");

        // Enum missing ByValue with Location.None
        string code3 = @"public enum LocSrc { A = 1, B = 2 } public enum LocDst { A = 1 }";
        var (src3, dst3, _, _) = ExtractSymbols(code3, "LocSrc", "LocDst");
        var diagByVal = new List<DiagnosticInfo>();
        ConversionStrategyFactory.GetConversionStrategy(src3, dst3, new List<IMethodSymbol>(), diagByVal, Location.None, "Prop", isStrict: true, enumStrategy: 1);
        diagByVal.Should().ContainSingle();
        diagByVal[0].FilePath.Should().Be("");
    }

    [Fact]
    public void GetConversionStrategy_WhenSourceAndTargetAreNullableValueTypes_ShouldNotTriggerSingleNullableWrap()
    {
        var comp = CreateCompilation("");
        var intType = comp.GetSpecialType(SpecialType.System_Int32);
        var longType = comp.GetSpecialType(SpecialType.System_Int64);
        var nullableGen = comp.GetTypeByMetadataName("System.Nullable`1")!;
        var nullableInt = nullableGen.Construct(intType);
        var nullableLong = nullableGen.Construct(longType);
        var diagnostics = new List<DiagnosticInfo>();

        // Widening: int? -> long?
        var s1 = ConversionStrategyFactory.GetConversionStrategy(nullableInt, nullableLong, new List<IMethodSymbol>(), diagnostics, Location.None, "P", isStrict: true);
        s1.Should().BeOfType<ConversionStrategy.DirectAssignment>();

        // Narrowing: long? -> int?
        var diagNarrow = new List<DiagnosticInfo>();
        var s2 = ConversionStrategyFactory.GetConversionStrategy(nullableLong, nullableInt, new List<IMethodSymbol>(), diagNarrow, Location.None, "P", isStrict: false);
        s2.Should().BeOfType<ConversionStrategy.BuiltinConversion>();
        diagNarrow.Should().ContainSingle();
        diagNarrow[0].Id.Should().Be("ELM015");
    }

    [Fact]
    public void GetConversionStrategy_WhenCustomStructCrossMapperWithNullables_ShouldMatch()
    {
        string code = @"
public struct StructA { public int X; }
public struct StructB { public int Y; }
public class StructMapper
{
    public StructB Map(StructA a) => new StructB { Y = a.X };
}
";
        var comp = CreateCompilation(code);
        var structA = comp.GetTypeByMetadataName("StructA")!;
        var structB = comp.GetTypeByMetadataName("StructB")!;
        var nullableGen = comp.GetTypeByMetadataName("System.Nullable`1")!;
        var nullA = nullableGen.Construct(structA);
        var nullB = nullableGen.Construct(structB);
        var mapper = comp.GetTypeByMetadataName("StructMapper")!;
        var mapMethod = mapper.GetMembers().OfType<IMethodSymbol>().First(m => m.Name == "Map");
        var diagnostics = new List<DiagnosticInfo>();

        var s = ConversionStrategyFactory.GetConversionStrategy(nullA, nullB, new List<IMethodSymbol> { mapMethod }, diagnostics, Location.None, "P", isStrict: true);
        s.Should().BeOfType<ConversionStrategy.MapMethodInvocation>();
    }

    [Fact]
    public void GetConversionStrategy_WhenUnsupportedTypes_ShouldReturnUnsupported()
    {
        string code = @"
public class ClassA {}
public class ClassB {}
";
        var (src, dst, methods, loc) = ExtractSymbols(code, "ClassA", "ClassB");
        var diagnostics = new List<DiagnosticInfo>();

        var strategy = ConversionStrategyFactory.GetConversionStrategy(src, dst, methods, diagnostics, loc, "Prop", isStrict: true);

        strategy.Should().BeOfType<ConversionStrategy.Unsupported>();
    }
}

