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
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace EricksonLopez.Mapper.Analyzers.Tests;

public class MapNullFallbackCodeFixProviderTests
{
    [Fact]
    public async Task RegisterCodeFixesAsync_WhenNullabilityMismatch_ShouldAddMapNullFallbackAttribute()
    {
        var testCode = @"
namespace TestNamespace;

public class Source { public string? NullableProp { get; set; } }
public class Dest { public string NullableProp { get; set; } }

public partial class MyMapper
{
    public partial Dest {|#0:Map|}(Source source);
}
";

        var fixedCode = @"
namespace TestNamespace;

public class Source { public string? NullableProp { get; set; } }
public class Dest { public string NullableProp { get; set; } }

public partial class MyMapper
{
    [EricksonLopez.Mapper.MapNullFallback(""NullableProp"", ""default!"")]
    public partial Dest Map(Source source);
}
";
        var expected = new DiagnosticResult("ELM004", DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("NullableProp", "string?", "string");

        var test = new CSharpCodeFixTest<MockELM004Analyzer, MapNullFallbackCodeFixProvider, DefaultVerifier>
        {
            TestCode = testCode.Replace("\r\n", "\n").Replace("\n", Environment.NewLine),
            FixedCode = fixedCode.Replace("\r\n", "\n").Replace("\n", Environment.NewLine),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None
        };
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public void FixableDiagnosticIds_WhenQueried_ShouldContainELM004()
    {
        var provider = new MapNullFallbackCodeFixProvider();
        provider.FixableDiagnosticIds.Should().ContainSingle().Which.Should().Be("ELM004");
        provider.GetFixAllProvider().Should().Be(WellKnownFixAllProviders.BatchFixer);
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenNodeIsNotMethod_ShouldNotRegisterCodeFix()
    {
        var provider = new MapNullFallbackCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public class MyClass { }",
            descriptor: MockELM004Analyzer.NullabilityMismatch,
            span: new TextSpan(25, 7),
            messageArgs: new object[] { "NullableProp", "string?", "string" });

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenDiagnosticMessageIsMalformed_ShouldNotRegisterCodeFix()
    {
        var provider = new MapNullFallbackCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { public partial void Map(); }",
            descriptor: MockELM004Analyzer.MalformedNullabilityMismatch,
            span: new TextSpan(77, 3));

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenValidDiagnostic_RegistersExpectedCodeAction()
    {
        var provider = new MapNullFallbackCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { public partial void Map(); }",
            descriptor: MockELM004Analyzer.NullabilityMismatch,
            span: new TextSpan(77, 3),
            messageArgs: new object[] { "NullableProp", "string?", "string" });

        actions.Should().ContainSingle();
        actions[0].Title.Should().Be("Add [MapNullFallback(\"NullableProp\", \"default!\")]");
        actions[0].EquivalenceKey.Should().Be("MapNullFallback_NullableProp");
    }

    [Theory]
    [InlineData("No quotes in message")]
    [InlineData("Nullability mismatch for 'OnlyOneQuote")]
    [InlineData("'LeadingQuoteOnly")]
    [InlineData("a'QuoteAtOneOnly")]
    public async Task RegisterCodeFixesAsync_WhenDiagnosticMessageHasMalformedQuotes_ShouldNotRegisterCodeFix(string message)
    {
        var provider = new MapNullFallbackCodeFixProvider();
        var customDescriptor = new DiagnosticDescriptor("ELM004", "Title", message, "Category", DiagnosticSeverity.Error, true);
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { public partial void Map(); }",
            descriptor: customDescriptor,
            span: new TextSpan(77, 3));

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenQuoteIsAtIndexOne_ShouldExtractPropertyNameAndRegisterCodeFix()
    {
        var provider = new MapNullFallbackCodeFixProvider();
        var customDescriptor = new DiagnosticDescriptor("ELM004", "Title", "a'Prop'b", "Category", DiagnosticSeverity.Error, true);
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { public partial void Map(); }",
            descriptor: customDescriptor,
            span: new TextSpan(77, 3));

        actions.Should().ContainSingle();
        actions[0].Title.Should().Be("Add [MapNullFallback(\"Prop\", \"default!\")]");
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenDiagnosticNotOnMethod_ShouldNotRegisterCodeFix()
    {
        var provider = new MapNullFallbackCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { private int _field; }",
            descriptor: MockELM004Analyzer.NullabilityMismatch,
            span: new TextSpan(68, 6),
            messageArgs: "NullableProperty");

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task CodeAction_WhenInvoked_ShouldAddMapNullFallbackAttributeWithCorrectArguments()
    {
        var provider = new MapNullFallbackCodeFixProvider();
        string sourceText = "namespace TestNamespace; public partial class MyMapper { public partial void Map(); }";
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: sourceText,
            descriptor: MockELM004Analyzer.NullabilityMismatch,
            span: new TextSpan(77, 3),
            messageArgs: new object[] { "Name", "string?", "string" });

        actions.Should().ContainSingle();
        var action = actions[0];
        action.Title.Should().Be("Add [MapNullFallback(\"Name\", \"default!\")]");
        action.EquivalenceKey.Should().Be("MapNullFallback_Name");

        var operations = await action.GetOperationsAsync(CancellationToken.None);
        var applyChanges = operations.OfType<ApplyChangesOperation>().FirstOrDefault();
        applyChanges.Should().NotBeNull();

        var changedDoc = applyChanges!.ChangedSolution.Projects.SelectMany(p => p.Documents).First();
        var text = await changedDoc.GetTextAsync();
        text.ToString().Should().Contain("[EricksonLopez.Mapper.MapNullFallback(\"Name\", \"default!\")]");
    }
}










