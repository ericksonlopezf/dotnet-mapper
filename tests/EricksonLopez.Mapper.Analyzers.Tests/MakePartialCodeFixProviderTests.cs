// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Mapper.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using VerifyCS = EricksonLopez.Mapper.Analyzers.Tests.CSharpCodeFixVerifier<
    EricksonLopez.Mapper.Analyzers.MapperAnalyzer,
    EricksonLopez.Mapper.Analyzers.MakePartialCodeFixProvider>;

namespace EricksonLopez.Mapper.Analyzers.Tests;

/// <summary>
/// Provides helper methods for verifying Roslyn code fix providers in C# test scenarios.
/// </summary>
public static class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer, new()
    where TCodeFix : Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(diagnosticId);

    public static async Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            TestCode = source.Replace("\r\n", "\n").Replace("\n", Environment.NewLine),
            FixedCode = fixedSource.Replace("\r\n", "\n").Replace("\n", Environment.NewLine),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None
        };
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }
}

/// <summary>Contains unit tests that verify <see cref="EricksonLopez.Mapper.Analyzers.MakePartialCodeFixProvider"/>.</summary>
public class MakePartialCodeFixProviderTests
{
    private const string MapperAttributeCode = TestConstants.MapperAttributeCode;

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenClassIsNotPartial_ShouldAddPartialKeywordToClass()
    {
        var test = MapperAttributeCode + @"
namespace TestNamespace;

public class Source { }
public class Dest { }

[EricksonLopez.Mapper.Mapper]
public class {|#0:MyMapper|}
{
    public partial Dest Map(Source source);
}
";

        var expected = MapperAttributeCode + @"
namespace TestNamespace;

public class Source { }
public class Dest { }

[EricksonLopez.Mapper.Mapper]
public partial class MyMapper
{
    public partial Dest Map(Source source);
}
";

        var expectedDiagnostic = VerifyCS.Diagnostic("ELM012").WithLocation(0).WithArguments("MyMapper");

        await VerifyCS.VerifyCodeFixAsync(test, expectedDiagnostic, expected);
    }

    [Fact]
    public void FixableDiagnosticIds_WhenQueried_ShouldContainELM012()
    {
        var provider = new MakePartialCodeFixProvider();
        provider.FixableDiagnosticIds.Should().ContainSingle().Which.Should().Be("ELM012");
        provider.GetFixAllProvider().Should().Be(WellKnownFixAllProviders.BatchFixer);
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenNodeNotClass_DoesNotRegisterCodeFix()
    {
        var provider = new MakePartialCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public enum MyEnum { Value }",
            descriptor: MapperAnalyzer.MustBePartial,
            span: new TextSpan(25, 6),
            messageArgs: "MyEnum");

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenValidDiagnostic_RegistersExpectedCodeAction()
    {
        var provider = new MakePartialCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public class MyMapper { }",
            descriptor: MapperAnalyzer.MustBePartial,
            span: new TextSpan(39, 8),
            messageArgs: "MyMapper");

        actions.Should().ContainSingle();
        actions[0].Title.Should().Be("Make class partial");
        actions[0].EquivalenceKey.Should().Be("Make class partial");
    }

    [Fact]
    public async Task CodeAction_WhenInvoked_ShouldProduceValidSyntaxWithPartialModifier()
    {
        var provider = new MakePartialCodeFixProvider();
        string sourceText = "namespace TestNamespace;\npublic class MyMapper\n{\n}";
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: sourceText,
            descriptor: MapperAnalyzer.MustBePartial,
            span: new TextSpan(sourceText.IndexOf("MyMapper", StringComparison.Ordinal), 8),
            messageArgs: "MyMapper");

        actions.Should().ContainSingle();
        var action = actions[0];

        var operations = await action.GetOperationsAsync(CancellationToken.None);
        operations.Should().NotBeEmpty();

        var applyChangesOperation = operations.OfType<ApplyChangesOperation>().FirstOrDefault();
        applyChangesOperation.Should().NotBeNull();

        var changedDoc = applyChangesOperation!.ChangedSolution.Projects.SelectMany(p => p.Documents).First();
        var text = await changedDoc.GetTextAsync();
        var newSyntaxRoot = await changedDoc.GetSyntaxRootAsync();

        text.ToString().Should().Contain("public partial class MyMapper");

        var classDecl = newSyntaxRoot!.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        classDecl.Should().NotBeNull();
        classDecl!.Modifiers.Any(SyntaxKind.PartialKeyword).Should().BeTrue();
    }
}







