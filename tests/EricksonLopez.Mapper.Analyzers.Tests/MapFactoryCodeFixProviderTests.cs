// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Mapper.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace EricksonLopez.Mapper.Analyzers.Tests;

public class MapFactoryCodeFixProviderTests
{
    [Fact]
    public async Task RegisterCodeFixesAsync_WhenAmbiguousConstructor_ShouldAddMapFactoryAttribute()
    {
        var testCode = @"
namespace TestNamespace;

public class Source { public string Name { get; set; } }
public class Dest { }

public partial class MyMapper
{
    public partial Dest {|#0:Map|}(Source source);
}
";

        var fixedCode = @"
namespace TestNamespace;

public class Source { public string Name { get; set; } }
public class Dest { }

public partial class MyMapper
{
    [EricksonLopez.Mapper.MapFactory(""Create"")]
    public partial Dest Map(Source source);
}
";
        var expected = new DiagnosticResult("ELM007", DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("Dest");

        var test = new CSharpCodeFixTest<MockELM007Analyzer, MapFactoryCodeFixProvider, DefaultVerifier>
        {
            TestCode = testCode.Replace("\r\n", "\n").Replace("\n", "\r\n"),
            FixedCode = fixedCode.Replace("\r\n", "\n").Replace("\n", "\r\n"),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None
        };
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public void FixableDiagnosticIds_WhenQueried_ShouldContainELM007()
    {
        var provider = new MapFactoryCodeFixProvider();
        provider.FixableDiagnosticIds.Should().ContainSingle().Which.Should().Be("ELM007");
        provider.GetFixAllProvider().Should().Be(WellKnownFixAllProviders.BatchFixer);
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenNodeNotMethod_DoesNotRegisterCodeFix()
    {
        var provider = new MapFactoryCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public class MyClass { }",
            descriptor: MockELM007Analyzer.AmbiguousConstructor,
            span: new TextSpan(25, 7),
            messageArgs: "Dest");

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenValidDiagnostic_RegistersExpectedCodeAction()
    {
        var provider = new MapFactoryCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { public partial void Map(); }",
            descriptor: MockELM007Analyzer.AmbiguousConstructor,
            span: new TextSpan(77, 3),
            messageArgs: "Dest");

        actions.Should().ContainSingle();
        actions[0].Title.Should().Be("Add [MapFactory(\"Create\")]");
        actions[0].EquivalenceKey.Should().Be("MapFactory_Create");
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenDiagnosticNotOnMethod_ShouldNotRegisterCodeFix()
    {
        var provider = new MapFactoryCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { private int _field; }",
            descriptor: MockELM007Analyzer.AmbiguousConstructor,
            span: new TextSpan(68, 6), // points to _field
            messageArgs: "Dest");

        actions.Should().BeEmpty();
    }
}










