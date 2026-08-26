// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading;
using AwesomeAssertions;
using EricksonLopez.Mapper.Generator.Tests.Infrastructure;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

public class MapperGeneratorComprehensiveTests
{
    [Fact]
    public void FactoryMethod_WithVariousParametersAndFallbacks_ShouldEmitProperFactoryCall()
    {
        string source = @"
#nullable enable
namespace TestNamespace;

public class AddressInfo { public string? City { get; set; } }
public class SourceModel
{
    public string? SimpleName { get; set; }
    public AddressInfo? Address { get; set; }
}

public class TargetModel
{
    public string Name { get; }
    public string City { get; }
    public string Status { get; }

    private TargetModel(string name, string city, string status)
    {
        Name = name;
        City = city;
        Status = status;
    }

    public static TargetModel Create(string name, string city, string status)
        => new TargetModel(name, city, status);
}

[Mapper]
public partial class FactoryMapper
{
    [MapFactory(""Create"")]
    [MapProperty(""SimpleName"", ""name"")]
    [MapProperty(""Address.City"", ""city"")]
    [MapNullFallback(""name"", ""\""Anonymous\"""")]
    [MapNullFallback(""city"", ""\""Unknown\"""")]
    [MapValue(""status"", ""\""Active\"""")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("TargetModel.Create((source.SimpleName ?? \"Anonymous\"), (source.Address?.City ?? \"Unknown\"), \"Active\")");
    }

    [Fact]
    public void FactoryMethod_WhenNullabilityMismatchWithoutFallback_ShouldEmitELM004()
    {
        string source = @"
#nullable enable
namespace TestNamespace;

public class AddressInfo { public string? City { get; set; } }
public class SourceModel
{
    public string? SimpleName { get; set; }
    public AddressInfo? Address { get; set; }
}

public class TargetModel
{
    public static TargetModel Create(string name, string city) => throw null!;
}

[Mapper]
public partial class FactoryMapper
{
    [MapFactory(""Create"")]
    [MapProperty(""SimpleName"", ""name"")]
    [MapProperty(""Address.City"", ""city"")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM004" && d.GetMessage().Contains("name"));
        diagnostics.Should().Contain(d => d.Id == "ELM004" && d.GetMessage().Contains("city"));
    }

    [Fact]
    public void FactoryMethod_WhenUnsupportedConversion_ShouldEmitELM003()
    {
        string source = @"
namespace TestNamespace;

public class Nested { public System.Guid G { get; set; } }
public class SourceModel { public System.Guid SimpleG { get; set; } public Nested Sub { get; set; } }
public class TargetModel
{
    public static TargetModel Create(System.DateTime simpleG, System.DateTime subG) => throw null!;
}

[Mapper]
public partial class FactoryMapper
{
    [MapFactory(""Create"")]
    [MapProperty(""Sub.G"", ""subG"")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM003" && d.GetMessage().Contains("simpleG"));
        diagnostics.Should().Contain(d => d.Id == "ELM003" && d.GetMessage().Contains("subG"));
    }

    [Fact]
    public void FactoryMethod_WhenUnmappedParameter_ShouldEmitELM001InStrictModeAndIgnoreInNonStrict()
    {
        string strictSource = @"
namespace TestNamespace;
public class SourceModel { }
public class TargetModel
{
    public static TargetModel Create(string unmappedParam) => throw null!;
}
[Mapper(StrictMapping = true)]
public partial class StrictMapper
{
    [MapFactory(""Create"")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (strictDiag, _, _) = GeneratorTestHelper.RunGenerator(strictSource, verifyEmittedCodeCompiles: false);
        strictDiag.Should().Contain(d => d.Id == "ELM001" && d.GetMessage().Contains("unmappedParam"));

        string nonStrictSource = @"
namespace TestNamespace;
public class SourceModel { }
public class TargetModel
{
    public static TargetModel Create(string unmappedParam = ""def"") => throw null!;
}
[Mapper(StrictMapping = false)]
public partial class NonStrictMapper
{
    [MapFactory(""Create"")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (nonStrictDiag, _, _) = GeneratorTestHelper.RunGenerator(nonStrictSource, verifyEmittedCodeCompiles: false);
        nonStrictDiag.Where(d => d.Id == "ELM001").Should().BeEmpty();
    }

    [Fact]
    public void FactoryMethod_MultipleOverloads_ShouldSelectLongestParameterListFirst()
    {
        string source = @"
namespace TestNamespace;

public class SourceModel { public int Id { get; set; } public string Name { get; set; } }
public class TargetModel
{
    public static TargetModel Create(int id) => throw null!;
    public static TargetModel Create(int id, string name) => throw null!;
}

[Mapper]
public partial class FactoryMapper
{
    [MapFactory(""Create"")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        output.Should().Contain("TargetModel.Create(source.Id, source.Name)");
    }

    [Fact]
    public void Constructor_WithVariousParametersAndFallbacks_ShouldEmitProperConstructorCall()
    {
        string source = @"
#nullable enable
namespace TestNamespace;

public class AddressInfo { public string? City { get; set; } }
public class SourceModel
{
    public string? SimpleName { get; set; }
    public AddressInfo? Address { get; set; }
}

public class TargetModel
{
    public string Name { get; }
    public string City { get; }
    public string Status { get; }

    public TargetModel(string name, string city, string status)
    {
        Name = name;
        City = city;
        Status = status;
    }
}

[Mapper]
public partial class CtorMapper
{
    [MapProperty(""SimpleName"", ""name"")]
    [MapProperty(""Address.City"", ""city"")]
    [MapNullFallback(""name"", ""\""Anonymous\"""")]
    [MapNullFallback(""city"", ""\""Unknown\"""")]
    [MapValue(""status"", ""\""Active\"""")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::TestNamespace.TargetModel((source.SimpleName ?? \"Anonymous\"), (source.Address?.City ?? \"Unknown\"), \"Active\")");
    }

    [Fact]
    public void Constructor_WhenNullabilityMismatchWithoutFallback_ShouldEmitELM004()
    {
        string source = @"
#nullable enable
namespace TestNamespace;

public class AddressInfo { public string? City { get; set; } }
public class SourceModel
{
    public string? SimpleName { get; set; }
    public AddressInfo? Address { get; set; }
}

public class TargetModel
{
    public TargetModel(string name, string city) { }
}

[Mapper]
public partial class CtorMapper
{
    [MapProperty(""SimpleName"", ""name"")]
    [MapProperty(""Address.City"", ""city"")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM004" && d.GetMessage().Contains("name"));
        diagnostics.Should().Contain(d => d.Id == "ELM004" && d.GetMessage().Contains("city"));
    }

    [Fact]
    public void Constructor_WhenUnsupportedConversion_ShouldEmitELM003()
    {
        string source = @"
namespace TestNamespace;

public class Nested { public System.Guid G { get; set; } }
public class SourceModel { public System.Guid SimpleG { get; set; } public Nested Sub { get; set; } }
public class TargetModel
{
    public TargetModel(System.DateTime simpleG, System.DateTime subG) { }
}

[Mapper]
public partial class CtorMapper
{
    [MapProperty(""Sub.G"", ""subG"")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM003" && d.GetMessage().Contains("simpleG"));
        diagnostics.Should().Contain(d => d.Id == "ELM003" && d.GetMessage().Contains("subG"));
    }

    [Fact]
    public void Constructor_WhenUnmappedParameter_ShouldEmitELM001InStrictModeAndIgnoreInNonStrict()
    {
        string strictSource = @"
namespace TestNamespace;
public class SourceModel { }
public class TargetModel
{
    public TargetModel(string unmappedParam) { }
}
[Mapper(StrictMapping = true)]
public partial class StrictMapper
{
    public partial TargetModel Map(SourceModel source);
}
";
        var (strictDiag, _, _) = GeneratorTestHelper.RunGenerator(strictSource, verifyEmittedCodeCompiles: false);
        strictDiag.Should().Contain(d => d.Id == "ELM001" && d.GetMessage().Contains("unmappedParam"));

        string nonStrictSource = @"
namespace TestNamespace;
public class SourceModel { }
public class TargetModel
{
    public TargetModel(string unmappedParam = ""def"") { }
}
[Mapper(StrictMapping = false)]
public partial class NonStrictMapper
{
    public partial TargetModel Map(SourceModel source);
}
";
        var (nonStrictDiag, _, _) = GeneratorTestHelper.RunGenerator(nonStrictSource, verifyEmittedCodeCompiles: false);
        nonStrictDiag.Where(d => d.Id == "ELM001").Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WhenAmbiguousParameterizedConstructors_ShouldEmitELM007()
    {
        string source = @"
namespace TestNamespace;
public class SourceModel { public int Id { get; set; } public string Name { get; set; } }
public class TargetModel
{
    public TargetModel(int id, string name) { }
    public TargetModel(string name, int id) { }
}
[Mapper]
public partial class AmbiguousMapper
{
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM007");
    }

    [Fact]
    public void Properties_WithNullFallbacksAndNestedPaths_ShouldEmitExpectedAccessors()
    {
        string source = @"
#nullable enable
namespace TestNamespace;

public class SubSource { public string? City { get; set; } }
public class SourceModel
{
    public string? SimpleName { get; set; }
    public SubSource? Sub { get; set; }
    public string IgnoredSrc { get; set; } = """";
}

public class TargetModel
{
    public string Name { get; set; } = """";
    public string City { get; set; } = """";
    public string Status { get; set; } = """";
    public string IgnoredSrc { get; set; } = """";
    public int ReadOnlyCount { get; }
    private string PrivateSet { get; set; } = """";
}

[Mapper]
public partial class PropertyMapper
{
    [MapProperty(""SimpleName"", ""Name"")]
    [MapProperty(""Sub.City"", ""City"")]
    [MapNullFallback(""Name"", ""\""Anon\"""")]
    [MapNullFallback(""City"", ""\""Unknown\"""")]
    [MapValue(""Status"", ""\""Active\"""")]
    [MapIgnoreSource(""IgnoredSrc"")]
    [MapIgnore(""ReadOnlyCount"")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Name = (source.SimpleName ?? \"Anon\")");
        output.Should().Contain("City = (source.Sub?.City ?? \"Unknown\")");
        output.Should().Contain("Status = \"Active\"");
        output.Should().NotContain("IgnoredSrc =");
        output.Should().NotContain("ReadOnlyCount =");
        output.Should().NotContain("PrivateSet =");
    }

    [Fact]
    public void Properties_WhenNullabilityMismatchWithoutFallback_ShouldEmitELM004()
    {
        string source = @"
#nullable enable
namespace TestNamespace;

public class SubSource { public string? City { get; set; } }
public class SourceModel
{
    public string? SimpleName { get; set; }
    public SubSource? Sub { get; set; }
}

public class TargetModel
{
    public string SimpleName { get; set; } = """";
    public string City { get; set; } = """";
}

[Mapper]
public partial class PropertyMapper
{
    [MapProperty(""Sub.City"", ""City"")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM004" && d.GetMessage().Contains("SimpleName"));
        diagnostics.Should().Contain(d => d.Id == "ELM004" && d.GetMessage().Contains("City"));
    }

    [Fact]
    public void Properties_WhenUnsupportedConversion_ShouldEmitELM003()
    {
        string source = @"
namespace TestNamespace;

public class SubSource { public System.Guid G { get; set; } }
public class SourceModel { public System.Guid SimpleG { get; set; } public SubSource Sub { get; set; } }
public class TargetModel
{
    public System.DateTime SimpleG { get; set; }
    public System.DateTime City { get; set; }
}

[Mapper]
public partial class PropertyMapper
{
    [MapProperty(""Sub.G"", ""City"")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM003" && d.GetMessage().Contains("SimpleG"));
        diagnostics.Should().Contain(d => d.Id == "ELM003" && d.GetMessage().Contains("City"));
    }

    [Fact]
    public void Properties_WhenMissingFactoryOrConstructor_ShouldEmitELM002()
    {
        string source = @"
namespace TestNamespace;
public class SourceModel { public int Id { get; set; } }
public class TargetModel
{
    private TargetModel() { }
    public int Id { get; }
}
[Mapper]
public partial class MissingCtorMapper
{
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM002");
    }

    [Fact]
    public void Polymorphism_AbstractTargetWithDerivedTypes_ShouldEmitELM011Warning()
    {
        string source = @"
namespace TestNamespace;

public abstract class Animal { }
public class Dog : Animal { }
public abstract class AnimalDto { }
public class DogDto : AnimalDto { }

[Mapper]
public partial class PolymorphicMapper
{
    [MapDerivedType(typeof(Dog), typeof(DogDto))]
    public partial AnimalDto Map(Animal animal);

    public partial DogDto MapDog(Dog dog);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Should().Contain(d => d.Id == "ELM011");
        output.Should().Contain("case global::TestNamespace.Dog derivedSource: return MapDog(derivedSource);");
    }

    [Fact]
    public void UseConverter_WhenConverterDoesNotImplementInterface_ShouldEmitELM013()
    {
        string source = @"
namespace TestNamespace;

public class Source { }
public class Dest { }
public class WrongConverter { }

[Mapper]
public partial class ConverterMapper
{
    [UseConverter(typeof(WrongConverter))]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM013");
    }

    [Fact]
    public void AssemblyMapperDefaults_WhenSpecified_ShouldControlDefaults()
    {
        string source = @"
using EricksonLopez.Mapper;
[assembly: MapperDefaults(StrictMapping = false, EnumMappingStrategy = EnumMappingStrategy.ByValue, EnumIgnoreCase = true)]

namespace TestNamespace;

public enum SrcEnum { ItemOne = 1, ItemTwo = 2 }
public enum DstEnum { itemone = 1, itemtwo = 2 }

public class Source { public SrcEnum E { get; set; } }
public class Dest { public DstEnum E { get; set; } public string Unmapped { get; set; } }

[Mapper]
public partial class AssemblyDefaultsMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("(global::TestNamespace.DstEnum)(source.E)");
    }

    [Fact]
    public void EnumMappingStrategy_ClassAndMethodOverrides_ShouldPrecedeAssemblyDefaults()
    {
        string source = @"
using EricksonLopez.Mapper;

namespace TestNamespace;

public enum SrcEnum { Alpha = 1, Beta = 2 }
public enum DstEnum { alpha = 1, beta = 2 }

[Mapper]
[EnumMappingStrategy(EnumMappingStrategy.ByValue, IgnoreCase = true)]
public partial class EnumStrategyMapper
{
    public partial DstEnum MapDirect(SrcEnum source);

    [EnumMappingStrategy(EnumMappingStrategy.ByName, IgnoreCase = true)]
    public partial DstEnum MapMethodOverride(SrcEnum source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("(global::TestNamespace.DstEnum)(source)");
        output.Should().Contain("(source) switch");
    }

    [Fact]
    public void MapEnumValue_WithStringAndEnumConstants_ShouldMapExplicitly()
    {
        string source = @"
using EricksonLopez.Mapper;
namespace TestNamespace;

public enum SrcEnum { A = 1, B = 2, C = 3 }
public enum DstEnum { X = 1, Y = 2, Z = 3 }

[Mapper]
public partial class ExplicitEnumMapper
{
    [MapEnumValue(SrcEnum.A, DstEnum.X)]
    [MapEnumValue(""B"", ""Y"")]
    [MapEnumValue(SrcEnum.C, DstEnum.Z)]
    public partial DstEnum Map(SrcEnum source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("global::TestNamespace.SrcEnum.A => global::TestNamespace.DstEnum.X");
        output.Should().Contain("global::TestNamespace.SrcEnum.B => global::TestNamespace.DstEnum.Y");
        output.Should().Contain("global::TestNamespace.SrcEnum.C => global::TestNamespace.DstEnum.Z");
    }

    [Fact]
    public void UseConverter_WithFieldName_ShouldEmitInstanceFieldCall()
    {
        string source = @"
using EricksonLopez.Mapper;
namespace TestNamespace;

public class Source { }
public class Dest { }
public class MyConv : IConverter<Source, Dest> { public Dest Convert(Source s) => new Dest(); }

[Mapper]
public partial class FieldMapper
{
    private readonly MyConv _myConv = new MyConv();

    [UseConverter(nameof(_myConv))]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("return this._myConv.Convert(source);");
    }

    [Fact]
    public void Generator_WhenMethodHasSpecialType_ShouldSkipMethod()
    {
        string source = @"
using EricksonLopez.Mapper;
namespace TestNamespace;

public class Source { }
public class Dest { }

[Mapper]
public partial class SpecialTypeMapper
{
    public partial Dest Map(Source source);
    public partial int MapInt(int source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Map(global::TestNamespace.Source source)");
        output.Should().NotContain("MapInt");
    }

    [Fact]
    public void Polymorphism_WhenDerivedTypeHasNoMatchingMethod_ShouldEmitThrowInGeneratedCode()
    {
        string source = @"
using EricksonLopez.Mapper;
namespace TestNamespace;

public abstract class BaseAnimal { }
public class Dog : BaseAnimal { }
public abstract class BaseAnimalDto { }
public class DogDto : BaseAnimalDto { }

[Mapper]
public partial class PolymorphicNoMethodMapper
{
    [MapDerivedType(typeof(Dog), typeof(DogDto))]
    public partial BaseAnimalDto Map(BaseAnimal animal);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        output.Should().Contain("case global::TestNamespace.Dog derivedSource: throw new global::System.InvalidOperationException");
    }

    [Fact]
    public void AssemblyHasDiAttribute_WhenPresent_ShouldEmitExtensionClass()
    {
        string source = @"
using EricksonLopez.Mapper;
[assembly: GenerateMapperRegistration]

namespace TestNamespace;

public class Source { }
public class Dest { }

[Mapper]
public partial class SimpleMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, compilation) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        var allGenerated = string.Join("\n", compilation.SyntaxTrees.Select(t => t.ToString()));
        allGenerated.Should().Contain("public static class MapperServiceCollectionExtensions");
        allGenerated.Should().Contain("services.AddSingleton<TestNamespace.SimpleMapper>();");
    }

    [Fact]
    public void UseConverter_WhenConverterImplementsOtherInterfaceOrWrongTypes_ShouldEmitELM013()
    {
        string source = @"
using EricksonLopez.Mapper;
namespace TestNamespace;

public interface IOtherInterface { }
public class Source { }
public class Dest { }
public class OtherType { }

public class OtherConv : IOtherInterface { }
public class WrongTypesConv : IConverter<Source, OtherType> { public OtherType Convert(Source s) => new OtherType(); }

[Mapper]
public partial class BadConvMapper
{
    [UseConverter(typeof(OtherConv))]
    public partial Dest Map1(Source s);

    [UseConverter(typeof(WrongTypesConv))]
    public partial Dest Map2(Source s);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM013" && d.GetMessage().Contains("OtherConv"));
        diagnostics.Should().Contain(d => d.Id == "ELM013" && d.GetMessage().Contains("WrongTypesConv"));
    }

    [Fact]
    public void GlobalNamespace_WhenNoNamespaceDeclared_ShouldGenerateValidCode()
    {
        string source = @"
public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }

[EricksonLopez.Mapper.Mapper]
public partial class RootMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("public partial class RootMapper");
    }

    [Fact]
    public void NullableValueAndReferenceTypes_WithFallbacks_ShouldHandleBothNullableForms()
    {
        string source = @"
#nullable enable
namespace TestNamespace;

public class Source
{
    public int? NullableInt { get; set; }
    public string? NullableStr { get; set; }
}

public class Dest
{
    public int NullableInt { get; set; }
    public string NullableStr { get; set; } = """";
}

[Mapper]
public partial class NullableMapper
{
    [MapNullFallback(""NullableInt"", ""0"")]
    [MapNullFallback(""NullableStr"", ""\""default\"""")]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("NullableInt = (source.NullableInt ?? 0)");
        output.Should().Contain("NullableStr = (source.NullableStr ?? \"default\")");
    }

    [Fact]
    public void NestedPaths_NullableParentAndNullableLeafPermutations_ShouldHandleCorrectly()
    {
        string source = @"
#nullable enable
namespace TestNamespace;

public class ParentWithNonNullLeaf { public string NonNullLeaf { get; set; } = """"; }
public class NonNullParentWithNullableLeaf { public string? NullableLeaf { get; set; } }

public class SourceModel
{
    public ParentWithNonNullLeaf? NullableParent { get; set; }
    public NonNullParentWithNullableLeaf NonNullParent { get; set; } = new();
}

public class TargetModel
{
    public string PropA { get; set; } = """";
    public string PropB { get; set; } = """";
}

[Mapper]
public partial class PermutationMapper
{
    [MapProperty(""NullableParent.NonNullLeaf"", ""PropA"")]
    [MapNullFallback(""PropA"", ""\""fallbackA\"""")]
    [MapProperty(""NonNullParent.NullableLeaf"", ""PropB"")]
    [MapNullFallback(""PropB"", ""\""fallbackB\"""")]
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("PropA = (source.NullableParent?.NonNullLeaf ?? \"fallbackA\")");
        output.Should().Contain("PropB = (source.NonNullParent?.NullableLeaf ?? \"fallbackB\")");
    }

    [Fact]
    public void MethodSignature_WhenAnnotatedNullableSourceAndTarget_ShouldEmitAnnotatedMethodMapping()
    {
        string source = @"
#nullable enable
namespace TestNamespace;

public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }

[Mapper]
public partial class NullableSigMapper
{
    public partial Dest? Map(Source? source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("public partial global::TestNamespace.Dest Map(global::TestNamespace.Source source)");
    }

    [Fact]
    public void Generator_WhenNonPartialOrMultiParameterOrReturnSpecialType_ShouldIgnoreThem()
    {
        string source = @"
namespace TestNamespace;

public class Source { public int Id { get; set; } }
public class Dest { public int Id { get; set; } }

[Mapper]
public partial class IgnoredMethodsMapper
{
    public partial Dest Map(Source source);
    public void NormalMethod() { }
    public partial Dest MultiParam(Source s, int extra);
    public partial int ReturnSpecial(Source s);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("public partial global::TestNamespace.Dest Map(global::TestNamespace.Source source)");
        output.Should().NotContain("NormalMethod");
        output.Should().NotContain("MultiParam");
        output.Should().NotContain("ReturnSpecial");
    }

    [Fact]
    public void TieBreaking_WhenMultipleConstructorsOrFactoriesHaveSameParameterCount_ShouldBeDeterministic()
    {
        string source = @"
namespace TestNamespace;

public class Source { public int A { get; set; } public string B { get; set; } }
public class TargetWithCtor
{
    public TargetWithCtor(int a, string b) { }
    public TargetWithCtor(string b, int a) { }
    public TargetWithCtor() { }
}

public class TargetWithFactory
{
    public static TargetWithFactory Create(int a, string b) => new TargetWithFactory();
    public static TargetWithFactory Create(string b, int a) => new TargetWithFactory();
}

[Mapper]
public partial class TieBreakMapper
{
    public partial TargetWithCtor MapCtor(Source s);

    [MapFactory(""Create"")]
    public partial TargetWithFactory MapFactory(Source s);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::TestNamespace.TargetWithCtor(");
        output.Should().Contain("TargetWithFactory.Create(");
    }

    [Fact]
    public void EmitTypeMapping_WhenCancelledBeforeExecution_ShouldThrowOperationCanceledException()
    {
        var mapping = new Models.TypeMapping("TestNs", "TestClass", false, EquatableArray<Models.MethodMapping>.Empty, EquatableArray<Models.DiagnosticInfo>.Empty);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Action act = () => MapperGenerator.EmitTypeMapping(mapping, _ => { }, (_, _) => { }, cts.Token);
        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void EmitTypeMapping_WhenDiagnosticsContainErrors_ShouldReportAndNotEmitSource()
    {
        var diag = new Models.DiagnosticInfo("ELM001", "Unmapped destination member", "Member '{0}' unmapped", "Category", (int)DiagnosticSeverity.Error, true, null!, 10, 5, new[] { "PropA" }.AsEquatableArray());
        var mapping = new Models.TypeMapping("TestNs", "TestClass", false, EquatableArray<Models.MethodMapping>.Empty, new[] { diag }.AsEquatableArray());

        Diagnostic? reported = null;
        bool sourceAdded = false;

        MapperGenerator.EmitTypeMapping(
            mapping,
            d => reported = d,
            (_, _) => sourceAdded = true,
            CancellationToken.None);

        reported.Should().NotBeNull();
        reported!.Id.Should().Be("ELM001");
        reported.Severity.Should().Be(DiagnosticSeverity.Error);
        reported.Location.GetLineSpan().Path.Should().Be("");
        sourceAdded.Should().BeFalse();
    }

    [Fact]
    public void EmitTypeMapping_WhenDiagnosticsContainWarningsOnly_ShouldReportAndEmitSource()
    {
        var diag = new Models.DiagnosticInfo("ELM011", "Warning title", "Warning msg", "Category", (int)DiagnosticSeverity.Warning, true, "SourceFile.cs", 20, 10, EquatableArray<string>.Empty);
        var methodMapping = new Models.MethodMapping("Map", new Models.TypeReference("global::TestNs.Source", false, false, false), new Models.TypeReference("global::TestNs.Dest", false, false, false), new Models.ConstructionStrategy.ObjectInitializer(), EquatableArray<Models.MemberMapping>.Empty, true, EquatableArray<Models.DerivedTypeMapping>.Empty);
        var mapping = new Models.TypeMapping("TestNs", "TestClass", false, new[] { methodMapping }.AsEquatableArray(), new[] { diag }.AsEquatableArray());

        Diagnostic? reported = null;
        string? addedName = null;
        Microsoft.CodeAnalysis.Text.SourceText? addedSource = null;

        MapperGenerator.EmitTypeMapping(
            mapping,
            d => reported = d,
            (name, text) => { addedName = name; addedSource = text; },
            CancellationToken.None);

        reported.Should().NotBeNull();
        reported!.Id.Should().Be("ELM011");
        reported.Severity.Should().Be(DiagnosticSeverity.Warning);
        reported.Location.GetLineSpan().Path.Should().Be("SourceFile.cs");
        addedName.Should().Be("TestClass.g.cs");
        addedSource.Should().NotBeNull();
        addedSource!.ToString().Should().Contain("public partial class TestClass");
    }

    [Fact]
    public void FactoryAndConstructor_WhenNestedPathNullabilityMismatchWithoutFallback_ShouldReportELM004()
    {
        string source = @"
#nullable enable
namespace TestNamespace;

public class SubNonNull { public string Name { get; set; } = """"; }
public class SubWithNull { public string? NullName { get; set; } }

public class SourceClass
{
    public SubNonNull? NullableSub { get; set; }
    public SubWithNull NonNullSub { get; set; } = new();
}

public class TargetCtor
{
    public TargetCtor(string propA, string propB) { }
}

public class TargetFactory
{
    public static TargetFactory Create(string propA, string propB) => new TargetFactory();
}

[Mapper]
public partial class CtorNullabilityMapper
{
    [MapProperty(""NullableSub.Name"", ""propA"")]
    [MapProperty(""NonNullSub.NullName"", ""propB"")]
    public partial TargetCtor MapCtor(SourceClass source);

    [MapFactory(""Create"")]
    [MapProperty(""NullableSub.Name"", ""propA"")]
    [MapProperty(""NonNullSub.NullName"", ""propB"")]
    public partial TargetFactory MapFactory(SourceClass source);
}
";
        var (diagnostics, _, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Where(d => d.Id == "ELM004").Should().HaveCount(4);
    }

    [Fact]
    public void FactoryAndConstructor_WhenStrictFalseAndUnmappedMembers_ShouldNotReportDiagnostics()
    {
        string source = @"
namespace TestNamespace;

public class SourceClass { public int A { get; set; } }

public class TargetCtor
{
    public TargetCtor(int a, string unmappedPath = "", string unmappedSimple = "") { }
}

public class TargetFactory
{
    public static TargetFactory Create(int a, string unmappedPath = "", string unmappedSimple = "") => new TargetFactory();
}

[Mapper(StrictMapping = false)]
public partial class NonStrictMapper
{
    [MapProperty(""NonExistent.Path"", ""unmappedPath"")]
    public partial TargetCtor MapCtor(SourceClass source);

    [MapFactory(""Create"")]
    [MapProperty(""NonExistent.Path"", ""unmappedPath"")]
    public partial TargetFactory MapFactory(SourceClass source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
    }

    [Fact]
    public void PropertyMapping_WhenNestedPathNullabilityMismatchWithoutFallback_ShouldReportELM004()
    {
        string source = @"
#nullable enable
namespace TestNamespace;

public class SubNonNull { public string Name { get; set; } = """"; }
public class SubWithNull { public string? NullName { get; set; } }

public class SourceClass
{
    public SubNonNull? NullableSub { get; set; }
    public SubWithNull NonNullSub { get; set; } = new();
}

public class TargetClass
{
    public string PropA { get; set; } = """";
    public string PropB { get; set; } = """";
}

[Mapper]
public partial class PropNullabilityMapper
{
    [MapProperty(""NullableSub.Name"", ""PropA"")]
    [MapProperty(""NonNullSub.NullName"", ""PropB"")]
    public partial TargetClass Map(SourceClass source);
}
";
        var (diagnostics, _, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Where(d => d.Id == "ELM004").Should().HaveCount(2);
    }

    [Fact]
    public void TargetClass_WhenOnlyPrivateConstructors_ShouldReportMissingFactoryOrConstructor()
    {
        string source = @"
namespace TestNamespace;

public class SourceClass { public int Id { get; set; } }
public class NoPublicCtorTarget
{
    private NoPublicCtorTarget() { }
    public int Id { get; set; }
}

[Mapper]
public partial class NoCtorMapper
{
    public partial NoPublicCtorTarget Map(SourceClass source);
}
";
        var (diagnostics, _, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM002");
    }

    [Fact]
    public void PolymorphicMapping_MultipleDerivedTypes_ShouldOrderDeterministically()
    {
        string source = @"
namespace TestNamespace;

public abstract class BaseSource { public int Id { get; set; } }
public class DerivedA : BaseSource { public string Name { get; set; } = """"; }
public class DerivedB : BaseSource { public string Title { get; set; } = """"; }

public abstract class BaseTarget { public int Id { get; set; } }
public class TargetA : BaseTarget { public string Name { get; set; } = """"; }
public class TargetB : BaseTarget { public string Title { get; set; } = """"; }

[Mapper]
public partial class PolyOrderMapper
{
    [MapDerivedType(typeof(DerivedB), typeof(TargetB))]
    [MapDerivedType(typeof(DerivedA), typeof(TargetA))]
    public partial BaseTarget Map(BaseSource source);

    public partial TargetA MapA(DerivedA a);
    public partial TargetB MapB(DerivedB b);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        output.Should().Contain("case global::TestNamespace.DerivedA derivedSource: return MapA(derivedSource);");
        output.Should().Contain("case global::TestNamespace.DerivedB derivedSource: return MapB(derivedSource);");
    }

    [Fact]
    public void AssemblyMapperDefaults_EnumMappingByNameAndIgnoreCase_ShouldApplyToAllMappers()
    {
        string source = @"
[assembly: EricksonLopez.Mapper.MapperDefaults(EnumMappingStrategy = EricksonLopez.Mapper.EnumMappingStrategy.ByName, EnumIgnoreCase = true)]

namespace TestNamespace;

public enum SourceColor { Red, Green, Blue }
public enum DestColor { RED, GREEN, BLUE }

[EricksonLopez.Mapper.Mapper]
public partial class AssemblyDefaultsMapper
{
    public partial DestColor Map(SourceColor source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("global::TestNamespace.SourceColor.Red => global::TestNamespace.DestColor.RED");
    }

    [Fact]
    public void CreateTypeReferenceAndIsNullableType_WithVariousSymbols_ShouldPopulateCorrectly()
    {
        string source = @"
#nullable enable
namespace TestNamespace;

public class TestTypes
{
    public int? NullableValueType { get; set; }
    public string? NullableRefType { get; set; }
    public int ValueType { get; set; }
    public string RefType { get; set; } = """";
    public abstract class AbstractType { }
}
";
        var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source);
        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
            "TestAssembly",
            new[] { syntaxTree },
            new[] { Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });

        var model = compilation.GetSemanticModel(syntaxTree);
        var classDecl = syntaxTree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>().First(c => c.Identifier.Text == "TestTypes");
        var classSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(classDecl)!;

        var propNullableVal = classSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NullableValueType").Type;
        var propNullableRef = classSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "NullableRefType").Type;
        var propVal = classSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "ValueType").Type;
        var propRef = classSymbol.GetMembers().OfType<IPropertySymbol>().First(p => p.Name == "RefType").Type;
        var abstractSymbol = classSymbol.GetTypeMembers("AbstractType").First();

        MapperGenerator.IsNullableType(propNullableVal).Should().BeTrue();
        MapperGenerator.IsNullableType(propNullableRef).Should().BeTrue();
        MapperGenerator.IsNullableType(propVal).Should().BeFalse();
        MapperGenerator.IsNullableType(propRef).Should().BeFalse();

        var refNullableVal = MapperGenerator.CreateTypeReference(propNullableVal);
        refNullableVal.IsValueType.Should().BeTrue();
        refNullableVal.IsNullable.Should().BeTrue();

        var refNullableRef = MapperGenerator.CreateTypeReference(propNullableRef);
        refNullableRef.IsValueType.Should().BeFalse();
        refNullableRef.IsNullable.Should().BeTrue();

        var refAbstract = MapperGenerator.CreateTypeReference(abstractSymbol);
        refAbstract.IsAbstract.Should().BeTrue();
        refAbstract.IsValueType.Should().BeFalse();
    }

    [Fact]
    public void FactoryAndConstructor_WhenMapIgnoreOnParameter_ShouldSkipThatParameter()
    {
        string source = @"
namespace TestNamespace;

public class SourceClass { public int A { get; set; } }

public class TargetCtor
{
    public TargetCtor(int a, int ignored = 0) { }
}

public class TargetFactory
{
    public static TargetFactory Create(int a, int ignored = 0) => new TargetFactory();
}

[Mapper]
public partial class IgnoreParamMapper
{
    [MapIgnore(""ignored"")]
    public partial TargetCtor MapCtor(SourceClass source);

    [MapFactory(""Create"")]
    [MapIgnore(""ignored"")]
    public partial TargetFactory MapFactory(SourceClass source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::TestNamespace.TargetCtor(source.A)");
        output.Should().Contain("TargetFactory.Create(source.A)");
    }

    [Fact]
    public void FactoryAndConstructor_WhenStrictAndUnmappedParameterWithNestedPath_ShouldReportELM001()
    {
        string source = @"
namespace TestNamespace;

public class SourceClass { public int A { get; set; } }

public class TargetCtor
{
    public TargetCtor(int a, string unmapped) { }
}

public class TargetFactory
{
    public static TargetFactory Create(int a, string unmapped) => new TargetFactory();
}

[Mapper(StrictMapping = true)]
public partial class StrictUnmappedMapper
{
    [MapProperty(""NonExistent.Path"", ""unmapped"")]
    public partial TargetCtor MapCtor(SourceClass source);

    [MapFactory(""Create"")]
    [MapProperty(""NonExistent.Path"", ""unmapped"")]
    public partial TargetFactory MapFactory(SourceClass source);
}
";
        var (diagnostics, _, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Where(d => d.Id == "ELM001").Should().HaveCount(2);
    }

    [Fact]
    public void FactoryResolution_WhenMethodIsNotStaticOrNotPublicOrWrongNameOrWrongReturn_ShouldNotSelectIt()
    {
        string source = @"
namespace TestNamespace;

public class SourceClass { public int A { get; set; } }

public class TargetFactoryPermutations
{
    public TargetFactoryPermutations Create(int a) => this;
    private static TargetFactoryPermutations Create(int a, string b) => new TargetFactoryPermutations();
    public static int Create(int a, string b, double c) => 0;
    public static TargetFactoryPermutations Other(int a) => new TargetFactoryPermutations();
    public TargetFactoryPermutations() { }
}

[Mapper]
public partial class InvalidFactoryMapper
{
    [MapFactory(""Create"")]
    public partial TargetFactoryPermutations Map(SourceClass source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        // Since no valid factory was found, falls back to parameterless ctor
        output.Should().Contain("new global::TestNamespace.TargetFactoryPermutations()");
    }

    [Fact]
    public void StructTarget_WhenNoExplicitConstructors_ShouldInstantiateWithoutError()
    {
        string source = @"
namespace TestNamespace;

public class SourceClass { public int A { get; set; } }
public struct TargetStruct { public int A { get; set; } }

[Mapper]
public partial class StructMapper
{
    public partial TargetStruct Map(SourceClass source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::TestNamespace.TargetStruct()");
    }

    [Fact]
    public void PropertyMapping_WhenMapIgnoreOnTargetProperty_ShouldSkipProperty()
    {
        string source = @"
namespace TestNamespace;

public class SourceClass { public int A { get; set; } public int Ignored { get; set; } }
public class TargetClass { public int A { get; set; } public int Ignored { get; set; } }

[Mapper]
public partial class PropIgnoreMapper
{
    [MapIgnore(""Ignored"")]
    public partial TargetClass Map(SourceClass source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("A = source.A");
        output.Should().NotContain("Ignored =");
    }

    [Fact]
    public void StandaloneEnumMapping_WithoutAssemblyDefaults_ShouldEmitSwitchReturn()
    {
        string source = @"
namespace TestNamespace;

public enum SourceEnum { Alpha = 1, Beta = 2 }
public enum DestEnum { Alpha = 1, Beta = 2 }

[Mapper]
public partial class StandaloneEnumMapper
{
    public partial DestEnum MapEnum(SourceEnum source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("return (global::TestNamespace.DestEnum)(source);");
        output.Should().NotContain("new global::TestNamespace.DestEnum");
    }

    [Fact]
    public void EmitTypeMapping_WhenCancelledAfterDiagnostics_ShouldThrowOperationCanceledException()
    {
        var diag = new Models.DiagnosticInfo("ELM011", "Warning", "Warning msg", "Category", (int)DiagnosticSeverity.Warning, true, "File.cs", 1, 1, EquatableArray<string>.Empty);
        var method = new Models.MethodMapping("Map", new Models.TypeReference("global::TestNs.Source", false), new Models.TypeReference("global::TestNs.Dest", false), new Models.ConstructionStrategy.ObjectInitializer(), EquatableArray<Models.MemberMapping>.Empty, true, EquatableArray<Models.DerivedTypeMapping>.Empty);
        var mapping = new Models.TypeMapping("TestNs", "TestClass", false, new[] { method }.AsEquatableArray(), new[] { diag }.AsEquatableArray());

        using var cts = new CancellationTokenSource();

        Action act = () => MapperGenerator.EmitTypeMapping(
            mapping,
            _ => cts.Cancel(),
            (_, _) => { },
            cts.Token);

        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void DiRegistrationAttribute_WhenPresentVsAbsent_ShouldControlDiEmission()
    {
        string sourceWithoutDi = @"
namespace TestNamespace;
public class Source { public int A { get; set; } }
public class Dest { public int A { get; set; } }
[Mapper]
public partial class MapperNoDi { public partial Dest Map(Source s); }
";
        var (_, outputWithoutDi, _) = GeneratorTestHelper.RunGenerator(sourceWithoutDi, verifyEmittedCodeCompiles: false);
        outputWithoutDi.Should().NotContain("AddGeneratedMappers");

        string sourceWithDi = @"
[assembly: EricksonLopez.Mapper.GenerateMapperRegistration]
namespace TestNamespace;
public class Source { public int A { get; set; } }
public class Dest { public int A { get; set; } }
[Mapper]
public partial class MapperWithDi { public partial Dest Map(Source s); }
";
        var (_, outputWithDi, _) = GeneratorTestHelper.RunGenerator(sourceWithDi, verifyEmittedCodeCompiles: false);
        outputWithDi.Should().Contain("AddGeneratedMappers");
    }

    [Fact]
    public void EnumToClassMapping_ShouldNotBeTreatedAsEnumToEnum()
    {
        string source = @"
namespace TestNamespace;
public enum SourceEnum { Alpha, Beta }
public class DestClass { public int Alpha { get; set; } }
[Mapper(StrictMapping = false)]
public partial class EnumToClassMapper
{
    public partial DestClass Map(SourceEnum source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::TestNamespace.DestClass()");
    }

    [Fact]
    public void Converter_WhenImplementingOtherGenericInterface_ShouldReportELM006()
    {
        string source = @"
namespace TestNamespace;
public interface IOtherConverter<TIn, TOut> { TOut Convert(TIn input); }
public class NotAnIConverter : IOtherConverter<int, string> { public string Convert(int input) => input.ToString(); }

public class SourceClass { public int A { get; set; } }
public class DestClass { public string A { get; set; } = """"; }

[Mapper]
public partial class InvalidInterfaceMapper
{
    [UseConverter(typeof(NotAnIConverter))]
    public partial DestClass Map(SourceClass source);
}
";
        var (diagnostics, _, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM013");
    }

    [Fact]
    public void FactoryWithMultipleOverloads_ShouldSelectGreediestCandidate()
    {
        string source = @"
namespace TestNamespace;

public class SourceClass { public int A { get; set; } public string B { get; set; } = """"; }

public class TargetWithMultipleFactories
{
    public static TargetWithMultipleFactories Create(int a) => new TargetWithMultipleFactories();
    public static TargetWithMultipleFactories Create(int a, string b) => new TargetWithMultipleFactories();
}

[Mapper]
public partial class GreedyFactoryMapper
{
    [MapFactory(""Create"")]
    public partial TargetWithMultipleFactories MapFactory(SourceClass source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("TargetWithMultipleFactories.Create(source.A, source.B)");
    }

    [Fact]
    public void InterfaceTarget_WhenNoConstructors_ShouldNotReportELM002()
    {
        string source = @"
namespace TestNamespace;
public class SourceClass { public int A { get; set; } }
public interface ITargetInterface { int A { get; } }

[Mapper]
public partial class InterfaceMapper
{
    public partial ITargetInterface Map(SourceClass source);
}
";
        var (diagnostics, _, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().NotContain(d => d.Id == "ELM002");
    }

    [Fact]
    public void Generator_WhenCancelled_ShouldHonorCancellationToken()
    {
        string source = @"
namespace TestNamespace;
public class Source { }
public class Dest { }
[Mapper]
public partial class CancelMapper { public partial Dest Map(Source s); }
";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Action act = () => GeneratorTestHelper.RunGeneratorSimple(source, cancellationToken: cts.Token);
        act.Should().Throw<OperationCanceledException>();
    }
}
