// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading;
using AwesomeAssertions;
using EricksonLopez.Mapper;
using EricksonLopez.Mapper.Generator;
using EricksonLopez.Mapper.Generator.Tests.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

public class MapperGeneratorDirectExecutionTests
{

    [Fact]
    public void ProcessMapperSymbol_WithAssemblyDefaultsAndAllAttributes_ShouldPopulateCorrectly()
    {
        string source = @"
[assembly: EricksonLopez.Mapper.MapperDefaults(StrictMapping = false, EnumMappingStrategy = EricksonLopez.Mapper.EnumMappingStrategy.ByName, EnumIgnoreCase = true)]

namespace TestNamespace;

public class SourceClass
{
    public int A { get; set; }
    public string B { get; set; } = """";
    public string? C { get; set; }
}

public class TargetClass
{
    public int A { get; set; }
    public string B { get; set; } = """";
    public string C { get; set; } = """";
    public string Val { get; set; } = """";
}

public enum SourceColor { Red, Green }
public enum DestColor { RED, GREEN }

public class BaseSource { }
public class DerivedSource : BaseSource { }
public class BaseTarget { }
public class DerivedTarget : BaseTarget { }

[EricksonLopez.Mapper.Mapper(StrictMapping = true)]
[EricksonLopez.Mapper.EnumMappingStrategy(EricksonLopez.Mapper.EnumMappingStrategy.ByName, IgnoreCase = true)]
public static partial class ComprehensiveDirectMapper
{
    [EricksonLopez.Mapper.MapProperty(""A"", ""A"")]
    [EricksonLopez.Mapper.MapIgnore(""B"")]
    [EricksonLopez.Mapper.MapIgnoreSource(""B"")]
    [EricksonLopez.Mapper.MapNullFallback(""C"", ""\""default\"""")]
    [EricksonLopez.Mapper.MapValue(""Val"", ""\""custom\"""")]
    public static partial TargetClass Map(SourceClass source);

    [EricksonLopez.Mapper.EnumMappingStrategy(EricksonLopez.Mapper.EnumMappingStrategy.ByValue, IgnoreCase = false)]
    [EricksonLopez.Mapper.MapEnumValue(""Red"", ""RED"")]
    public static partial DestColor MapEnum(SourceColor source);

    [EricksonLopez.Mapper.MapDerivedType(typeof(DerivedSource), typeof(DerivedTarget))]
    public static partial BaseTarget MapPoly(BaseSource source);

    public static partial DerivedTarget MapDerived(DerivedSource source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "ComprehensiveDirectMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Namespace.Should().Be("TestNamespace");
        result.ClassName.Should().Be("ComprehensiveDirectMapper");
        result.IsStatic.Should().BeTrue();
        result.Methods.Should().HaveCount(4);

        var mapMethod = result.Methods.First(m => m.MethodName == "Map");
        mapMethod.IsStrict.Should().BeTrue();
        mapMethod.Members.Should().Contain(m => m.TargetName == "Val" && m.CustomValueExpression == "\"custom\"");
        mapMethod.Members.Should().Contain(m => m.TargetName == "C" && m.Fallback == "\"default\"");
        mapMethod.Members.Should().NotContain(m => m.TargetName == "B");

        var enumMethod = result.Methods.First(m => m.MethodName == "MapEnum");
        enumMethod.Members.Should().HaveCount(1);
        enumMethod.Members[0].SourceName.Should().BeEmpty();
        enumMethod.Members[0].TargetName.Should().BeEmpty();
        enumMethod.Members[0].Strategy.Should().BeOfType<Models.ConversionStrategy.BuiltinConversion>();

        var polyMethod = result.Methods.First(m => m.MethodName == "MapPoly");
        polyMethod.DerivedTypes.Should().HaveCount(1);
        polyMethod.DerivedTypes[0].MethodName.Should().Be("MapDerived");
    }

    [Fact]
    public void ProcessMapperSymbol_InterfaceWithoutDerivedTypes_ShouldNotReportELM012()
    {
        string source = @"
namespace TestNamespace;
public class SourceClass { public int A { get; set; } }
public interface ITarget { int A { get; } }

[EricksonLopez.Mapper.Mapper]
public partial class InterfaceNoDerivedMapper
{
    public partial ITarget Map(SourceClass source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "InterfaceNoDerivedMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Diagnostics.Should().NotContain(d => d.Id == "ELM012");
    }

    [Fact]
    public void ProcessMapperSymbol_InGlobalNamespace_ShouldHaveEmptyNamespaceString()
    {
        string source = @"
using EricksonLopez.Mapper;

public class SourceClass { public int A { get; set; } }
public class TargetClass { public int A { get; set; } }

[EricksonLopez.Mapper.Mapper]
public partial class GlobalNsMapper
{
    public partial TargetClass Map(SourceClass source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "GlobalNsMapper", normalize: false);
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Namespace.Should().BeEmpty();
        result.ClassName.Should().Be("GlobalNsMapper");
        result.IsStatic.Should().BeFalse();
    }

    [Fact]
    public void ProcessMapperSymbol_WithConverterAndFieldConverter_ShouldPopulateCorrectly()
    {
        string source = @"
namespace TestNamespace;

public class SourceClass { public int A { get; set; } }
public class TargetClass { public int A { get; set; } }

public class ValidConverter : EricksonLopez.Mapper.IConverter<SourceClass, TargetClass>
{
    public TargetClass Convert(SourceClass source) => new TargetClass { A = source.A };
}

[EricksonLopez.Mapper.Mapper]
public partial class ConverterDirectMapper
{
    [EricksonLopez.Mapper.UseConverter(typeof(ValidConverter))]
    public partial TargetClass MapWithClass(SourceClass source);

    [EricksonLopez.Mapper.UseConverter(""_myConverterField"")]
    public partial TargetClass MapWithField(SourceClass source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "ConverterDirectMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Methods.Should().HaveCount(2);

        var classMethod = result.Methods.First(m => m.MethodName == "MapWithClass");
        classMethod.CustomConverter.Should().Contain("ValidConverter");
        classMethod.CustomConverterField.Should().BeNull();

        var fieldMethod = result.Methods.First(m => m.MethodName == "MapWithField");
        fieldMethod.CustomConverterField.Should().Be("_myConverterField");
        fieldMethod.CustomConverter.Should().BeNull();
    }

    [Fact]
    public void ProcessMapperSymbol_WithMapDerivedTypeWithoutMatchingMethod_ShouldHaveNullMethodName()
    {
        string source = @"
namespace TestNamespace;

public class BaseSource { }
public class DerivedSource : BaseSource { }
public class BaseTarget { }
public class DerivedTarget : BaseTarget { }

[EricksonLopez.Mapper.Mapper]
public partial class DerivedWithoutMethodMapper
{
    [EricksonLopez.Mapper.MapDerivedType(typeof(DerivedSource), typeof(DerivedTarget))]
    public partial BaseTarget Map(BaseSource source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "DerivedWithoutMethodMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Methods[0].DerivedTypes.Should().HaveCount(1);
        result.Methods[0].DerivedTypes[0].MethodName.Should().BeNull();
    }

    [Fact]
    public void ProcessMapperSymbol_WithFactoryAndNullFallbacks_ShouldPopulateCorrectly()
    {
        string source = @"
namespace TestNamespace;

public class SourceClass { public string? Name { get; set; } }

public class TargetClass
{
    public static TargetClass Create(string name) => new TargetClass();
}

[EricksonLopez.Mapper.Mapper]
public partial class FactoryFallbackMapper
{
    [EricksonLopez.Mapper.MapFactory(""Create"")]
    [EricksonLopez.Mapper.MapNullFallback(""name"", ""\""default\"""")]
    public partial TargetClass Map(SourceClass source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "FactoryFallbackMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        var method = result!.Methods[0];
        method.Construction.Should().BeOfType<Models.ConstructionStrategy.FactoryMethod>();
        var factory = (Models.ConstructionStrategy.FactoryMethod)method.Construction;
        factory.Parameters.Should().HaveCount(1);
        factory.Parameters[0].Fallback.Should().Be("\"default\"");
    }

    [Fact]
    public void ProcessMapperSymbol_WithCtorAndNullFallbacks_ShouldPopulateCorrectly()
    {
        string source = @"
namespace TestNamespace;

public class SourceClass { public string? Name { get; set; } }

public class TargetClass
{
    public TargetClass(string name) { }
}

[EricksonLopez.Mapper.Mapper]
public partial class CtorFallbackMapper
{
    [EricksonLopez.Mapper.MapNullFallback(""name"", ""\""default\"""")]
    public partial TargetClass Map(SourceClass source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "CtorFallbackMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        var method = result!.Methods[0];
        method.Construction.Should().BeOfType<Models.ConstructionStrategy.ParameterizedConstructor>();
        var ctor = (Models.ConstructionStrategy.ParameterizedConstructor)method.Construction;
        ctor.Parameters.Should().HaveCount(1);
        ctor.Parameters[0].Fallback.Should().Be("\"default\"");
    }

    [Fact]
    public void ProcessMapperSymbol_WithFactoryTieBreak_ShouldOrderByDisplayStringDeterministically()
    {
        string source = @"
namespace TestNamespace;

public class SourceClass { public int A { get; set; } public string B { get; set; } = """"; }

public class TargetClass
{
    public static TargetClass Create(int a, string b) => new TargetClass();
    public static TargetClass Create(string b, int a) => new TargetClass();
}

[EricksonLopez.Mapper.Mapper]
public partial class TieBreakFactoryMapper
{
    [EricksonLopez.Mapper.MapFactory(""Create"")]
    public partial TargetClass Map(SourceClass source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "TieBreakFactoryMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        var factory = (Models.ConstructionStrategy.FactoryMethod)result!.Methods[0].Construction;
        factory.Parameters.Should().HaveCount(2);
        // int a comes before string b alphabetically by parameter types in ToDisplayString()
        factory.Parameters[0].TargetName.Should().Be("a");
        factory.Parameters[1].TargetName.Should().Be("b");
    }

    [Fact]
    public void ProcessMapperSymbol_WithSingleArityIConverter_ShouldReportELM013()
    {
        string source = @"
namespace TestNamespace;

public interface IConverter<T> { T Convert(T input); }
public class SingleGenericConverter : IConverter<SourceClass>
{
    public SourceClass Convert(SourceClass input) => input;
}

public class SourceClass { public int A { get; set; } }
public class TargetClass { public int A { get; set; } }

[EricksonLopez.Mapper.Mapper]
public partial class SingleGenericConverterMapper
{
    [EricksonLopez.Mapper.UseConverter(typeof(SingleGenericConverter))]
    public partial TargetClass Map(SourceClass source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "SingleGenericConverterMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Diagnostics.Should().Contain(d => d.Id == "ELM013");
    }

    [Fact]
    public void ProcessMapperSymbol_WithMultipleAssemblyAttributes_ShouldFindMapperDefaults()
    {
        string source = @"
[assembly: System.Reflection.AssemblyTitle(""MyAssembly"")]
[assembly: EricksonLopez.Mapper.MapperDefaults(StrictMapping = false, EnumMappingStrategy = EricksonLopez.Mapper.EnumMappingStrategy.ByName, EnumIgnoreCase = true)]

namespace TestNamespace;
public enum SourceEnum { Alpha }
public enum TargetEnum { ALPHA }
public class SourceClass { public int A { get; set; } }
public class TargetClass { public int A { get; set; } }

[EricksonLopez.Mapper.Mapper]
public partial class AssemblyPrecedingMapper
{
    public partial TargetClass Map(SourceClass source);
    public partial TargetEnum MapEnum(SourceEnum source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "AssemblyPrecedingMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Methods[0].IsStrict.Should().BeFalse();

        var enumMethod = result.Methods.First(m => m.MethodName == "MapEnum");
        enumMethod.Members[0].Strategy.Should().BeOfType<Models.ConversionStrategy.BuiltinConversion>();
        var builtin = (Models.ConversionStrategy.BuiltinConversion)enumMethod.Members[0].Strategy;
        builtin.ExpressionTemplate.Should().Contain("TargetEnum.ALPHA");
    }

    [Fact]
    public void ProcessMapperSymbol_WithPrecedingClassAttributes_ShouldFindMapperAttribute()
    {
        string source = @"
namespace TestNamespace;
public class SourceClass { public int A { get; set; } }
public class TargetClass { public int A { get; set; } }

[System.Obsolete(""Use NewMapper"")]
[EricksonLopez.Mapper.Mapper(StrictMapping = false)]
public partial class PrecedingClassAttrMapper
{
    public partial TargetClass Map(SourceClass source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "PrecedingClassAttrMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Methods[0].IsStrict.Should().BeFalse();
    }

    [Fact]
    public void ProcessMapperSymbol_WithMultipleDerivedTypesSameSource_ShouldOrderTargetTypesDeterministically()
    {
        string source = @"
namespace TestNamespace;
public class BaseSource { }
public class DerivedSource : BaseSource { }
public class BaseTarget { }
public class TargetB : BaseTarget { }
public class TargetA : BaseTarget { }

[EricksonLopez.Mapper.Mapper]
public partial class PolyOrderingMapper
{
    [EricksonLopez.Mapper.MapDerivedType(typeof(DerivedSource), typeof(TargetB))]
    [EricksonLopez.Mapper.MapDerivedType(typeof(DerivedSource), typeof(TargetA))]
    public partial BaseTarget Map(BaseSource source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "PolyOrderingMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        var derived = result!.Methods[0].DerivedTypes;
        derived.Should().HaveCount(2);
        derived[0].TargetType.Should().Contain("TargetA");
        derived[1].TargetType.Should().Contain("TargetB");
    }

    [Fact]
    public void ProcessMapperSymbol_WithCtorAndPropertyMapIgnore_ShouldSkipMembers()
    {
        string source = @"
namespace TestNamespace;
public class SourceClass { public int A { get; set; } public int Ignored { get; set; } }
public class TargetCtor { public TargetCtor(int a, int ignored = 0) { } }
public class TargetProp { public int A { get; set; } public int Ignored { get; set; } }

[EricksonLopez.Mapper.Mapper]
public partial class IgnoreDirectMapper
{
    [EricksonLopez.Mapper.MapIgnore(""ignored"")]
    public partial TargetCtor MapCtor(SourceClass source);

    [EricksonLopez.Mapper.MapIgnore(""Ignored"")]
    public partial TargetProp MapProp(SourceClass source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "IgnoreDirectMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        var ctorMethod = result!.Methods.First(m => m.MethodName == "MapCtor");
        var ctor = (Models.ConstructionStrategy.ParameterizedConstructor)ctorMethod.Construction;
        ctor.Parameters.Should().HaveCount(1);
        ctor.Parameters[0].TargetName.Should().Be("a");

        var propMethod = result.Methods.First(m => m.MethodName == "MapProp");
        propMethod.Members.Should().HaveCount(1);
        propMethod.Members[0].TargetName.Should().Be("A");
    }

    [Fact]
    public void EmitTypeMapping_WhenErrorDiagnostic_ShouldNotInvokeAddSource()
    {
        var diag = new Models.DiagnosticInfo("ELM001", "Error", "Error msg", "Category", (int)DiagnosticSeverity.Error, true, "File.cs", 1, 1, EquatableArray<string>.Empty);
        var method = new Models.MethodMapping("Map", new Models.TypeReference("global::TestNs.Source", false), new Models.TypeReference("global::TestNs.Dest", false), new Models.ConstructionStrategy.ObjectInitializer(), EquatableArray<Models.MemberMapping>.Empty, true, EquatableArray<Models.DerivedTypeMapping>.Empty);
        var mapping = new Models.TypeMapping("TestNs", "TestClass", false, new[] { method }.AsEquatableArray(), new[] { diag }.AsEquatableArray());

        bool addSourceCalled = false;
        MapperGenerator.EmitTypeMapping(
            mapping,
            _ => { },
            (_, _) => { addSourceCalled = true; },
            CancellationToken.None);

        addSourceCalled.Should().BeFalse();
    }

    [Fact]
    public void EmitTypeMapping_WhenWarningDiagnostic_ShouldInvokeAddSource()
    {
        var diag = new Models.DiagnosticInfo("ELM009", "Warning", "Warning msg", "Category", (int)DiagnosticSeverity.Warning, true, "File.cs", 1, 1, EquatableArray<string>.Empty);
        var method = new Models.MethodMapping("Map", new Models.TypeReference("global::TestNs.Source", false), new Models.TypeReference("global::TestNs.Dest", false), new Models.ConstructionStrategy.ObjectInitializer(), EquatableArray<Models.MemberMapping>.Empty, true, EquatableArray<Models.DerivedTypeMapping>.Empty);
        var mapping = new Models.TypeMapping("TestNs", "TestClass", false, new[] { method }.AsEquatableArray(), new[] { diag }.AsEquatableArray());

        bool addSourceCalled = false;
        MapperGenerator.EmitTypeMapping(
            mapping,
            _ => { },
            (_, _) => { addSourceCalled = true; },
            CancellationToken.None);

        addSourceCalled.Should().BeTrue();
    }

    [Fact]
    public void ProcessMapperSymbol_WhenCancelled_ShouldThrow()
    {
        string source = @"
namespace TestNamespace;
public class Source { }
public class Dest { }
[EricksonLopez.Mapper.Mapper]
public partial class CancelDirectMapper { public partial Dest Map(Source s); }
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "CancelDirectMapper");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Action act = () => MapperGenerator.ProcessMapperSymbol(symbol, compilation, cts.Token);
        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void ProcessMapperSymbol_WhenFactoryWithIgnoredParameter_ShouldSkipParameter()
    {
        string source = @"
namespace TestNamespace;
public class Source { public int A { get; set; } }
public class Dest
{
    public static Dest Create(int a, int extra) => new Dest();
}

[EricksonLopez.Mapper.Mapper]
public partial class FactoryIgnoreMapper
{
    [EricksonLopez.Mapper.MapFactory(""Create"")]
    [EricksonLopez.Mapper.MapIgnore(""extra"")]
    public partial Dest Map(Source source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "FactoryIgnoreMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        var method = result!.Methods[0];
        var factory = (Models.ConstructionStrategy.FactoryMethod)method.Construction;
        factory.Parameters.Should().HaveCount(1);
        factory.Parameters[0].TargetName.Should().Be("a");
    }

    [Fact]
    public void ProcessMapperSymbol_WhenMapInterfaceWithoutDerivedTypes_ShouldNotEmitELM006()
    {
        string source = @"
namespace TestNamespace;
public interface ISource { }
public interface ITarget { }

[EricksonLopez.Mapper.Mapper]
public partial class InterfaceNoPolyMapper
{
    public partial ITarget Map(ISource source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "InterfaceNoPolyMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Diagnostics.Should().NotContain(d => d.Id == "ELM006");
    }

    [Fact]
    public void ProcessMapperSymbol_WhenNestedPathFailsAndStrictFalse_ShouldNotEmitELM001()
    {
        string source = @"
namespace TestNamespace;
public class Source { public int A { get; set; } }
public class Dest { public int B { get; set; } }

[EricksonLopez.Mapper.Mapper(StrictMapping = false)]
public partial class NonStrictNestedMapper
{
    [EricksonLopez.Mapper.MapProperty(""NonExistent.Path"", ""B"")]
    public partial Dest Map(Source source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "NonStrictNestedMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Diagnostics.Should().NotContain(d => d.Id == "ELM001");
    }

    [Fact]
    public void ProcessMapperSymbol_WhenReadonlyRecordStructWithoutValueProperty_ShouldRecognizeValueObject()
    {
        string source = @"
namespace TestNamespace;
public readonly record struct CustomVo(int Id);

public class Source { public int Vo { get; set; } }
public class Dest { public CustomVo Vo { get; set; } }

[EricksonLopez.Mapper.Mapper]
public partial class VoMapper
{
    public partial Dest Map(Source source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "VoMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Diagnostics.Where(d => d.DefaultSeverity == (int)DiagnosticSeverity.Error).Should().BeEmpty();
        var method = result!.Methods[0];
        method.Members.Should().ContainSingle();
        method.Members[0].Strategy.Should().BeOfType<Models.ConversionStrategy.ValueObjectMapping>();
    }

    [Fact]
    public void ProcessMapperSymbol_WhenReadonlyRecordStructSourceWithoutValueProperty_ShouldRecognizeValueObject()
    {
        string source = @"
namespace TestNamespace;
public readonly record struct CustomVo(int Id);

public class Source { public CustomVo Vo { get; set; } }
public class Dest { public int Vo { get; set; } }

[EricksonLopez.Mapper.Mapper]
public partial class VoSourceMapper
{
    public partial Dest Map(Source source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "VoSourceMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Diagnostics.Where(d => d.DefaultSeverity == (int)DiagnosticSeverity.Error).Should().BeEmpty();
        var method = result!.Methods[0];
        method.Members.Should().ContainSingle();
        method.Members[0].Strategy.Should().BeOfType<Models.ConversionStrategy.ValueObjectMapping>();
    }

    [Fact]
    public void ProcessMapperSymbol_WhenEnumMappingStrategyAttributeWithLengthOne_ShouldSetStrategy()
    {
        string source = @"
namespace TestNamespace;
public enum SourceEnum { A = 1, B = 2 }
public enum DestEnum { A = 1, B = 2 }

[EricksonLopez.Mapper.Mapper]
public partial class ClassEnumMapper
{
    [EricksonLopez.Mapper.EnumMappingStrategy(EricksonLopez.Mapper.EnumMappingStrategy.ByValue)]
    public partial DestEnum Map(SourceEnum source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("(global::TestNamespace.DestEnum)(source)");
    }

    [Fact]
    public void EmitTypeMapping_WhenCancelledToken_ShouldThrow()
    {
        var mapping = new Models.TypeMapping("TestNs", "TestClass", false, EquatableArray<Models.MethodMapping>.Empty, EquatableArray<Models.DiagnosticInfo>.Empty);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Action act = () => MapperGenerator.EmitTypeMapping(mapping, _ => { }, (_, _) => { }, cts.Token);
        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void ProcessMapperSymbol_WhenMapperInGlobalNamespace_ShouldHaveEmptyNamespace()
    {
        string source = @"
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }

[EricksonLopez.Mapper.Mapper]
public partial class GlobalMapper
{
    public partial Dest Map(Source source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "GlobalMapper", normalize: false);
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Namespace.Should().Be("");
    }

    [Fact]
    public void ProcessMapperSymbol_WhenAmbiguousConstructors_ShouldReportELM005AndSkipMethodMapping()
    {
        string source = @"
namespace TestNamespace;
public class Source { public int A { get; set; } }
public class Dest
{
    public Dest(int a) { }
    public Dest(string b) { }
}

[EricksonLopez.Mapper.Mapper]
public partial class AmbiguousCtorMapper
{
    public partial Dest Map(Source source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "AmbiguousCtorMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Diagnostics.Should().Contain(d => d.Id == "ELM007");
        result.Methods.Should().BeEmpty();
    }

    [Fact]
    public void ProcessMapperSymbol_WhenPropertyHasNonPublicSetterAndNotReadOnly_ShouldSkipProperty()
    {
        string source = @"
namespace TestNamespace;
public class Source { public int Id { get; set; } public int InternalVal { get; set; } }
public class Dest
{
    public int Id { get; set; }
    public int InternalVal { get; internal set; }
}

[EricksonLopez.Mapper.Mapper]
public partial class InternalSetterMapper
{
    public partial Dest Map(Source source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "InternalSetterMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Methods[0].Members.Should().ContainSingle(m => m.TargetName == "Id");
    }

    [Fact]
    public void ProcessMapperSymbol_WhenClassLevelEnumMappingStrategyByNameIgnoreCase_ShouldMatchCaseInsensitive()
    {
        string source = @"
namespace TestNamespace;
public enum SourceEnum { First = 1, Second = 2 }
public enum TargetEnum { FIRST = 1, SECOND = 2 }

[EricksonLopez.Mapper.Mapper]
[EricksonLopez.Mapper.EnumMappingStrategy(EricksonLopez.Mapper.EnumMappingStrategy.ByName, IgnoreCase = true)]
public partial class ClassLevelEnumMapper
{
    public partial TargetEnum Map(SourceEnum source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("SourceEnum.First => global::TestNamespace.TargetEnum.FIRST");
    }

    [Fact]
    public void IsNullableType_WhenNullableValueType_ShouldReturnTrue()
    {
        var (_, comp) = GeneratorTestHelper.CreateCompilation("public class Dummy { public int? Val { get; set; } }", "Dummy");
        var prop = comp.GetTypeByMetadataName("TestNamespace.Dummy")!.GetMembers().OfType<IPropertySymbol>().First();
        MapperGenerator.IsNullableType(prop.Type).Should().BeTrue();
    }

    [Fact]
    public void ProcessMapperSymbol_WhenMapEnumValueWithStrings_ShouldMapExplicitly()
    {
        string source = @"
namespace TestNamespace;
public enum SourceColor { Alpha }
public enum DestColor { Beta }

[EricksonLopez.Mapper.Mapper]
public partial class ExplicitColorMapper
{
    [EricksonLopez.Mapper.MapEnumValue(""Alpha"", ""Beta"")]
    public partial DestColor Map(SourceColor source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "ExplicitColorMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void ProcessMapperSymbol_WhenNestedPathHasUnsupportedConversion_ShouldReportELM003()
    {
        string source = @"
namespace TestNamespace;
public class NestedSource { public string Val { get; set; } = """"; }
public class Source { public NestedSource Nested { get; set; } = new(); }
public class CustomClass { }
public class Dest { public CustomClass Custom { get; set; } = new(); }

[EricksonLopez.Mapper.Mapper]
public partial class UnsupportedNestedMapper
{
    [EricksonLopez.Mapper.MapProperty(""Nested.Val"", ""Custom"")]
    public partial Dest Map(Source source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "UnsupportedNestedMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Diagnostics.Should().Contain(d => d.Id == "ELM003");
    }

    [Fact]
    public void ProcessMapperSymbol_WhenClassLevelEnumMappingStrategyByValue_ShouldSetStrategy()
    {
        string source = @"
namespace TestNamespace;
public enum SourceEnum { First = 1, Second = 2 }
public enum TargetEnum { First = 1, Second = 2 }

[EricksonLopez.Mapper.Mapper]
[EricksonLopez.Mapper.EnumMappingStrategy(EricksonLopez.Mapper.EnumMappingStrategy.ByValue)]
public partial class ClassLevelByValueEnumMapper
{
    public partial TargetEnum Map(SourceEnum source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "ClassLevelByValueEnumMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void ProcessMapperSymbol_WhenTargetIsAbstractWithoutDerivedTypes_ShouldNotReportELM006()
    {
        string source = @"
namespace TestNamespace;
public class Source { public int Id { get; set; } }
public abstract class AbstractDest { public int Id { get; set; } }

[EricksonLopez.Mapper.Mapper]
public partial class AbstractDestMapper
{
    public partial AbstractDest Map(Source source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "AbstractDestMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Diagnostics.Should().NotContain(d => d.Id == "ELM006");
    }

    [Fact]
    public void ProcessMapperSymbol_WhenTargetHasPrivateSetter_ShouldSkip()
    {
        string source = @"
namespace TestNamespace;
public class Source { public int Id { get; set; } }
public class DestWithPrivate { public int Id { get; private set; } }

[EricksonLopez.Mapper.Mapper]
public partial class PrivateSetterMapper
{
    public partial DestWithPrivate Map(Source source);
}
";
        var (symbol, compilation) = GeneratorTestHelper.CreateCompilation(source, "PrivateSetterMapper");
        var result = MapperGenerator.ProcessMapperSymbol(symbol, compilation, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Methods[0].Members.Should().BeEmpty();
    }

    [Fact]
    public void EmitTypeMapping_WhenCancelledAtStart_ShouldThrowOperationCanceledException()
    {
        var typeMapping = TestDataBuilders.CreateType("TestMapper").Build();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => MapperGenerator.EmitTypeMapping(typeMapping, _ => { }, (_, _) => { }, cts.Token);
        act.Should().Throw<OperationCanceledException>();
    }
}
