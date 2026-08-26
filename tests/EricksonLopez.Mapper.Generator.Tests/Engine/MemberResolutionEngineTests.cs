// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Engine;

/// <summary>
/// Unit tests verifying property collection and matching logic in <see cref="MemberResolutionEngine"/>.
/// Adheres strictly to ADR-021 (Method_Scenario_Result).
/// </summary>
[Trait("Category", "FastAst")]
public class MemberResolutionEngineTests
{
    private static (ITypeSymbol Type, IMethodSymbol Method) CompileAndExtractSymbols(string sourceCode, string typeName, string methodName = "Map")
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create(
            "TestAsm",
            new[] { syntaxTree },
            Basic.Reference.Assemblies.Net80.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var typeSymbol = compilation.GetTypeByMetadataName(typeName)!;
        var mapperType = compilation.GetTypeByMetadataName("Mapper") ?? typeSymbol;
        var methodSymbol = mapperType.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m => m.Name == methodName)
            ?? typeSymbol.GetMembers().OfType<IMethodSymbol>().FirstOrDefault()!;
        return (typeSymbol, methodSymbol);
    }

    [Fact]
    public void GetAllProperties_WhenClassInheritsBaseClass_ShouldReturnAllProperties()
    {
        string code = @"
public class BaseClass { public int BaseId { get; set; } }
public class ChildClass : BaseClass { public string ChildName { get; set; } = """"; }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, _) = CompileAndExtractSymbols(code, "ChildClass");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);

        props.Select(p => p.Name).Should().Contain(new[] { "BaseId", "ChildName" });
    }

    [Fact]
    public void GetAllProperties_WhenInterfaceInheritsInterfaces_ShouldReturnAllProperties()
    {
        string code = @"
public interface IBaseA { int IdA { get; set; } }
public interface IBaseB { string IdB { get; set; } }
public interface IChild : IBaseA, IBaseB { double Value { get; set; } }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, _) = CompileAndExtractSymbols(code, "IChild");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);

        props.Select(p => p.Name).Should().Contain(new[] { "IdA", "IdB", "Value" });
    }

    [Fact]
    public void MatchProperty_WhenExactMatchExists_ShouldReturnExactProperty()
    {
        string code = @"
public class Source { public string Name { get; set; } = """"; public string name { get; set; } = """"; }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Source");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);
        var diagnostics = new List<Models.DiagnosticInfo>();

        var matched = MemberResolutionEngine.MatchProperty(props, "Name", methodSymbol, "Name", diagnostics);

        matched.Should().NotBeNull();
        matched!.Name.Should().Be("Name");
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void MatchProperty_WhenCaseInsensitiveMatchExists_ShouldReturnMatchedProperty()
    {
        string code = @"
public class Source { public string customer_name { get; set; } = """"; }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Source");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);
        var diagnostics = new List<Models.DiagnosticInfo>();

        var matched = MemberResolutionEngine.MatchProperty(props, "Customer_Name", methodSymbol, "Customer_Name", diagnostics);

        matched.Should().NotBeNull();
        matched!.Name.Should().Be("customer_name");
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void MatchProperty_WhenMultipleCaseInsensitiveMatchesExist_ShouldEmitELM005AndReturnNull()
    {
        string code = @"
public class Source { public string nAme { get; set; } = """"; public string naMe { get; set; } = """"; }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Source");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);
        var diagnostics = new List<Models.DiagnosticInfo>();

        var matched = MemberResolutionEngine.MatchProperty(props, "Name", methodSymbol, "Name", diagnostics);

        matched.Should().BeNull();
        diagnostics.Should().ContainSingle();
        diagnostics[0].Id.Should().Be("ELM005");
        diagnostics[0].FilePath.Should().Be("");
    }

    [Fact]
    public void MatchProperty_WhenSyntaxTreeHasFilePath_ShouldPopulateFilePathInDiagnostic()
    {
        string code = @"
public class SourceWithFilePath { public string nAme { get; set; } = """"; public string naMe { get; set; } = """"; }
public class Mapper { public void Map() {} }
";
        var syntaxTree = CSharpSyntaxTree.ParseText(code, path: "TestSourceWithFile.cs");
        var compilation = CSharpCompilation.Create(
            "TestAsm",
            new[] { syntaxTree },
            Basic.Reference.Assemblies.Net80.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var typeSymbol = compilation.GetTypeByMetadataName("SourceWithFilePath")!;
        var mapperType = compilation.GetTypeByMetadataName("Mapper")!;
        var methodSymbol = mapperType.GetMembers().OfType<IMethodSymbol>().First();
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);
        var diagnostics = new List<Models.DiagnosticInfo>();

        var matched = MemberResolutionEngine.MatchProperty(props, "Name", methodSymbol, "Name", diagnostics);

        matched.Should().BeNull();
        diagnostics.Should().ContainSingle();
        diagnostics[0].FilePath.Should().Be("TestSourceWithFile.cs");
    }

    [Fact]
    public void MatchProperty_WhenMethodSymbolFromMetadata_ShouldFallbackToEmptyFilePathInDiagnostic()
    {
        string code = @"
public class SourceWithAmbiguity { public string nAme { get; set; } = """"; public string naMe { get; set; } = """"; }
";
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create(
            "TestAsm",
            new[] { syntaxTree },
            Basic.Reference.Assemblies.Net80.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var typeSymbol = compilation.GetTypeByMetadataName("SourceWithAmbiguity")!;
        var methodFromMetadata = compilation.GetSpecialType(SpecialType.System_Object).GetMembers().OfType<IMethodSymbol>().First();
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);
        var diagnostics = new List<Models.DiagnosticInfo>();

        var matched = MemberResolutionEngine.MatchProperty(props, "Name", methodFromMetadata, "Name", diagnostics);

        matched.Should().BeNull();
        diagnostics.Should().ContainSingle();
        diagnostics[0].FilePath.Should().Be("");
    }

    [Fact]
    public void ResolvePropertyPath_WhenNullableDisabledWithTwoSegmentNullableStruct_ShouldFormatWithQuestionDotAndMarkNullable()
    {
        string code = @"
#nullable disable
public struct SubStruct { public string Code { get; set; } }
public class RootDisabled { public SubStruct? StructProp { get; set; } }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "RootDisabled");
        var diagnostics = new List<Models.DiagnosticInfo>();

        var resolution = MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "StructProp.Code", methodSymbol, "Code", diagnostics);

        resolution.Should().NotBeNull();
        resolution!.FormattedPath.Should().Be("StructProp?.Code");
        resolution.IsPathNullable.Should().BeTrue();
    }

    [Fact]
    public void ResolvePropertyPath_WhenSingleSegmentNullableReferenceType_ShouldMarkPathNullable()
    {
        string code = @"
#nullable enable
public class CustomerWithNullableRef { public string? OptionalDescription { get; set; } }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "CustomerWithNullableRef");
        var diagnostics = new List<Models.DiagnosticInfo>();

        var resolution = MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "OptionalDescription", methodSymbol, "OptionalDescription", diagnostics);

        resolution.Should().NotBeNull();
        resolution!.IsPathNullable.Should().BeTrue();
        resolution.FormattedPath.Should().Be("OptionalDescription");
    }

    [Fact]
    public void ResolvePropertyPath_WhenRootIsArrayType_ShouldHandleNonNamedType()
    {
        string code = @"
public class Mapper { public void Map() {} }
";
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create(
            "TestAsm",
            new[] { syntaxTree },
            Basic.Reference.Assemblies.Net80.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var mapperType = compilation.GetTypeByMetadataName("Mapper")!;
        var methodSymbol = mapperType.GetMembers().OfType<IMethodSymbol>().First();
        var arrayType = compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_String));
        var diagnostics = new List<Models.DiagnosticInfo>();

        var resolution = MemberResolutionEngine.ResolvePropertyPath(arrayType, "Length", methodSymbol, "Length", diagnostics);
        resolution.Should().NotBeNull();
        resolution!.FormattedPath.Should().Be("Length");
    }

    [Fact]
    public void ResolvePropertyPath_WhenPathContainsEmptySegment_ShouldReturnNull()
    {
        string code = @"
public class SourceRoot { public string Name { get; set; } = """"; }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "SourceRoot");
        var diagnostics = new List<Models.DiagnosticInfo>();

        var resolution = MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "Name..Other", methodSymbol, "Other", diagnostics);
        resolution.Should().BeNull();
    }

    [Fact]
    public void HasMapperIgnore_WithUnrelatedAttribute_ShouldReturnFalse()
    {
        string code = @"
using System;
public class Sample { [Obsolete] public string Name { get; set; } = """"; }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, _) = CompileAndExtractSymbols(code, "Sample");
        var prop = typeSymbol.GetMembers().OfType<IPropertySymbol>().First();

        var result = MemberResolutionEngine.HasMapperIgnore(prop);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasMapperIgnore_WithAllAttributeVariants_ShouldReturnTrue()
    {
        string code = @"
using System;
public class MapperIgnoreAttribute : Attribute {}
public class MapperIgnore : Attribute {}
public class TestClass
{
    [MapperIgnoreAttribute]
    public int Prop1 { get; set; }

    [MapperIgnore]
    public int Prop2 { get; set; }

    public int Prop3 { get; set; }
}
public class Mapper { public void Map() {} }
";
        var (typeSymbol, _) = CompileAndExtractSymbols(code, "TestClass");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);

        var p1 = props.First(p => p.Name == "Prop1");
        var p2 = props.First(p => p.Name == "Prop2");
        var p3 = props.First(p => p.Name == "Prop3");

        MemberResolutionEngine.HasMapperIgnore(p1).Should().BeTrue();
        MemberResolutionEngine.HasMapperIgnore(p2).Should().BeTrue();
        MemberResolutionEngine.HasMapperIgnore(p3).Should().BeFalse();
    }

    [Fact]
    public void ResolvePropertyPath_WhenSingleNonNullableReferenceType_ShouldHaveIsPathNullableFalse()
    {
        string code = @"
public class Customer { public string Name { get; set; } = """"; }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Customer");
        var diagnostics = new List<Models.DiagnosticInfo>();

        var resolution = MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "Name", methodSymbol, "Name", diagnostics);

        resolution.Should().NotBeNull();
        resolution!.IsPathNullable.Should().BeFalse();
        resolution.FormattedPath.Should().Be("Name");
    }

    [Fact]
    public void ResolvePropertyPath_WhenNullableStructInChainWithNonValueMember_ShouldUnwrapAndResolve()
    {
        string code = @"
public struct AddressStruct { public string ZipCode { get; set; } }
public class Customer { public AddressStruct? Address { get; set; } }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Customer");
        var diagnostics = new List<Models.DiagnosticInfo>();

        var resolution = MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "Address.ZipCode", methodSymbol, "ZipCode", diagnostics);

        resolution.Should().NotBeNull();
        resolution!.FormattedPath.Should().Be("Address?.ZipCode");
        resolution.IsPathNullable.Should().BeTrue();
        resolution.LeafPropertyName.Should().Be("ZipCode");
    }

    [Fact]
    public void MatchProperty_WhenNoPropertyMatches_ShouldReturnNullWithoutDiagnostics()
    {
        string code = @"
public class Source { public int OtherProp { get; set; } }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Source");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);
        var diagnostics = new List<Models.DiagnosticInfo>();

        var matched = MemberResolutionEngine.MatchProperty(props, "NonExistent", methodSymbol, "NonExistent", diagnostics);

        matched.Should().BeNull();
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void GetAllProperties_WhenFourLevelInheritanceHierarchy_ShouldTraverseEntireChain()
    {
        string code = @"
public class Level0 { public int Prop0 { get; set; } }
public class Level1 : Level0 { public string Prop1 { get; set; } = """"; }
public class Level2 : Level1 { public bool Prop2 { get; set; } }
public class Level3 : Level2 { public double Prop3 { get; set; } }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, _) = CompileAndExtractSymbols(code, "Level3");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);

        props.Select(p => p.Name).Should().Contain(new[] { "Prop0", "Prop1", "Prop2", "Prop3" });
    }

    [Fact]
    public void GetAllProperties_WhenPropertyShadowedWithNew_ShouldReturnDerivedProperty()
    {
        string code = @"
public class BaseType { public virtual string Name { get; set; } = ""Base""; }
public class DerivedType : BaseType { public new string Name { get; set; } = ""Derived""; }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "DerivedType");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);
        var diagnostics = new List<Models.DiagnosticInfo>();

        var matched = MemberResolutionEngine.MatchProperty(props, "Name", methodSymbol, "Name", diagnostics);

        matched.Should().NotBeNull();
        matched!.Name.Should().Be("Name");
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData("CustomerId", "CustomerID")]
    [InlineData("Url", "URL")]
    [InlineData("HttpEndpoint", "HTTPEndpoint")]
    [InlineData("ApiVersion", "APIVersion")]
    public void MatchProperty_WhenAcronymCasingDiffers_ShouldMatchCaseInsensitively(string sourcePropName, string targetPropName)
    {
        string code = $@"
public class Source {{ public string {sourcePropName} {{ get; set; }} = """"; }}
public class Mapper {{ public void Map() {{}} }}
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Source");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);
        var diagnostics = new List<Models.DiagnosticInfo>();

        var matched = MemberResolutionEngine.MatchProperty(props, targetPropName, methodSymbol, targetPropName, diagnostics);

        matched.Should().NotBeNull();
        matched!.Name.Should().Be(sourcePropName);
        diagnostics.Should().BeEmpty();
    }

    [Property]
    public bool MatchProperty_WhenSearchingArbitraryNonExistentNames_ShouldNeverThrowAndReturnNull(NonNull<string> randomTargetName)
    {
        string code = @"
public class Source { public int KnownId { get; set; } public string KnownName { get; set; } = """"; }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Source");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);
        var diagnostics = new List<Models.DiagnosticInfo>();

        string target = randomTargetName.Get;
        if (string.Equals(target, "KnownId", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(target, "KnownName", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var matched = MemberResolutionEngine.MatchProperty(props, target, methodSymbol, target, diagnostics);
        return matched == null && diagnostics.Count == 0;
    }

    [Fact]
    public void ResolvePropertyPath_WhenValidThreeLevelPath_ShouldResolveLeafTypeAndFormattedPath()
    {
        string code = @"
public class Address { public string City { get; set; } = """"; }
public class Customer { public Address Address { get; set; } = new(); }
public class Order { public Customer Customer { get; set; } = new(); }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Order");
        var diagnostics = new List<Models.DiagnosticInfo>();

        var resolution = MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "Customer.Address.City", methodSymbol, "City", diagnostics);

        resolution.Should().NotBeNull();
        resolution!.LeafPropertyName.Should().Be("City");
        resolution.FormattedPath.Should().Be("Customer?.Address?.City");
        resolution.IsPathNullable.Should().BeTrue();
        resolution.LeafType.Name.Should().Be("String");
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void ResolvePropertyPath_WhenEmptyOrWhitespacePath_ShouldReturnNull()
    {
        string code = @"
public class Order { public int Id { get; set; } }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Order");
        var diagnostics = new List<Models.DiagnosticInfo>();

        MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "", methodSymbol, "Id", diagnostics).Should().BeNull();
        MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "   ", methodSymbol, "Id", diagnostics).Should().BeNull();
        MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "Customer..City", methodSymbol, "Id", diagnostics).Should().BeNull();
    }

    [Fact]
    public void ResolvePropertyPath_WhenIntermediatePropertyNotFound_ShouldReturnNull()
    {
        string code = @"
public class Order { public int Id { get; set; } }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Order");
        var diagnostics = new List<Models.DiagnosticInfo>();

        var resolution = MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "NonExistent.City", methodSymbol, "City", diagnostics);

        resolution.Should().BeNull();
    }

    [Fact]
    public void ResolvePropertyPath_WhenPropertyHasMapperIgnoreAttribute_ShouldReturnNull()
    {
        string code = @"
using System;
public class MapperIgnoreAttribute : Attribute {}
public class Address { public string City { get; set; } = """"; }
public class Order 
{ 
    [MapperIgnore]
    public Address Address { get; set; } = new(); 
}
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Order");
        var diagnostics = new List<Models.DiagnosticInfo>();

        var resolution = MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "Address.City", methodSymbol, "City", diagnostics);

        resolution.Should().BeNull();
    }

    [Fact]
    public void ResolvePropertyPath_WhenSingleSegmentNonNullableValueType_ShouldResolvePath()
    {
        string code = @"
public class Order { public int OrderId { get; set; } }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Order");
        var diagnostics = new List<Models.DiagnosticInfo>();

        var resolution = MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "OrderId", methodSymbol, "OrderId", diagnostics);

        resolution.Should().NotBeNull();
        resolution!.LeafPropertyName.Should().Be("OrderId");
        resolution.FormattedPath.Should().Be("OrderId");
        resolution.IsPathNullable.Should().BeFalse();
        resolution.LeafType.Name.Should().Be("Int32");
    }

    [Fact]
    public void GetAllProperties_WhenClassHasStaticPropertiesAndIndexers_ShouldIgnoreStaticAndIndexers()
    {
        string code = @"
public class SourceWithSpecialProps
{
    public static int StaticProp { get; set; }
    public int InstanceProp { get; set; }
    public string this[int index] { get => """"; set {} }
}
public class Mapper { public void Map() {} }
";
        var (typeSymbol, _) = CompileAndExtractSymbols(code, "SourceWithSpecialProps");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);

        props.Select(p => p.Name).Should().ContainSingle().Which.Should().Be("InstanceProp");
    }

    [Fact]
    public void GetAllProperties_WhenInterfaceHasStaticPropertiesAndIndexers_ShouldIgnoreStaticAndIndexers()
    {
        string code = @"
public interface ISpecialInterface
{
    static int StaticInterfaceProp { get; set; }
    int InterfaceInstanceProp { get; set; }
    string this[int index] { get; set; }
}
public class Mapper { public void Map() {} }
";
        var (typeSymbol, _) = CompileAndExtractSymbols(code, "ISpecialInterface");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);

        props.Select(p => p.Name).Should().ContainSingle().Which.Should().Be("InterfaceInstanceProp");
    }

    [Fact]
    public void GetAllProperties_WhenBaseInterfaceHasStaticPropertiesAndIndexers_ShouldIgnoreStaticAndIndexers()
    {
        string code = @"
public interface IBaseSpecial
{
    static int StaticBaseProp { get; set; }
    int BaseProp { get; set; }
    string this[int index] { get; set; }
}
public interface IDerivedSpecial : IBaseSpecial
{
    int DerivedProp { get; set; }
}
public class Mapper { public void Map() {} }
";
        var (typeSymbol, _) = CompileAndExtractSymbols(code, "IDerivedSpecial");
        var props = MemberResolutionEngine.GetAllProperties(typeSymbol);

        props.Select(p => p.Name).Should().BeEquivalentTo(new[] { "BaseProp", "DerivedProp" });
    }

    [Fact]
    public void ResolvePropertyPath_WhenStructChainWithoutNullability_ShouldUseDotSeparator()
    {
        string code = @"
public struct InnerStruct { public int Value { get; set; } }
public struct OuterStruct { public InnerStruct Child { get; set; } }
public class Root { public OuterStruct StructProp { get; set; } }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Root");
        var diagnostics = new List<Models.DiagnosticInfo>();

        var resolution = MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "StructProp.Child.Value", methodSymbol, "Value", diagnostics);

        resolution.Should().NotBeNull();
        resolution!.FormattedPath.Should().Be("StructProp.Child.Value");
        resolution.IsPathNullable.Should().BeFalse();
    }

    [Fact]
    public void ResolvePropertyPath_WhenNullableAnnotatedReferenceType_ShouldMarkPathNullable()
    {
        string code = @"
#nullable enable
public class Child { public string Name { get; set; } = """"; }
public class Root { public Child? NullableChild { get; set; } }
public class Mapper { public void Map() {} }
";
        var (typeSymbol, methodSymbol) = CompileAndExtractSymbols(code, "Root");
        var diagnostics = new List<Models.DiagnosticInfo>();

        var resolution = MemberResolutionEngine.ResolvePropertyPath(typeSymbol, "NullableChild.Name", methodSymbol, "Name", diagnostics);

        resolution.Should().NotBeNull();
        resolution!.IsPathNullable.Should().BeTrue();
    }
}



