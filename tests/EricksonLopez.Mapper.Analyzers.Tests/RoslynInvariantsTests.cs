// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Mapper.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

#pragma warning disable IL3000

namespace EricksonLopez.Mapper.Analyzers.Tests;

public class RoslynInvariantsTests
{
    [Fact]
    public void GetAttributeClassFullName_WhenAttributeClassPresent_ShouldReturnDisplayString()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace CustomNamespace
{
    [System.Obsolete]
    public class SampleClass {}
}");
        var compilation = CSharpCompilation.Create("TestAssembly", new[] { syntaxTree }, new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
        var model = compilation.GetSemanticModel(syntaxTree);
        var classDecl = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var symbol = model.GetDeclaredSymbol(classDecl);
        var attributeData = symbol!.GetAttributes()[0];

        var result = RoslynInvariants.GetAttributeClassFullName(attributeData);

        result.Should().Be("System.ObsoleteAttribute");
    }

    [Fact]
    public void GetAttributeClassFullName_WhenAttributeClassIsNull_ShouldReturnNull()
    {
        var attributeData = new NullClassAttributeData();
        var result = RoslynInvariants.GetAttributeClassFullName(attributeData);

        result.Should().BeNull();
    }

    private sealed class NullClassAttributeData : AttributeData
    {
        protected override INamedTypeSymbol? CommonAttributeClass => null;
        protected override IMethodSymbol? CommonAttributeConstructor => null;
        protected override SyntaxReference? CommonApplicationSyntaxReference => null;
        protected override ImmutableArray<TypedConstant> CommonConstructorArguments => ImmutableArray<TypedConstant>.Empty;
        protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments => ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty;
    }

    [Fact]
    public void GetContainingNamespace_WhenSymbolIsGlobalNamespace_ShouldReturnNull()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("public class RootClass {}");
        var compilation = CSharpCompilation.Create("TestAssembly", new[] { syntaxTree }, new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });

        var globalNamespace = compilation.GlobalNamespace;
        var result = RoslynInvariants.GetContainingNamespace(globalNamespace);

        result.Should().BeNull();
    }

    [Fact]
    public void GetContainingNamespace_WhenNamespacePresent_ShouldReturnDisplayString()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace MyCompany.MyProduct
{
    public class MyService {}
}");
        var compilation = CSharpCompilation.Create("TestAssembly", new[] { syntaxTree }, new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
        var model = compilation.GetSemanticModel(syntaxTree);
        var classDecl = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var symbol = model.GetDeclaredSymbol(classDecl);

        var result = RoslynInvariants.GetContainingNamespace(symbol!);

        result.Should().Be("MyCompany.MyProduct");
    }

    [Fact]
    public void GetContainingNamespace_WhenNestedNamespaces_ShouldReturnFullNamespaceHierarchy()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace Level1.Level2.Level3
{
    public class NestedService {}
}");
        var compilation = CSharpCompilation.Create("TestAssembly", new[] { syntaxTree }, new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
        var model = compilation.GetSemanticModel(syntaxTree);
        var classDecl = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var symbol = model.GetDeclaredSymbol(classDecl);

        var result = RoslynInvariants.GetContainingNamespace(symbol!);

        result.Should().Be("Level1.Level2.Level3");
    }
}


