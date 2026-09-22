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

public class MapPropertyCodeFixProviderTests
{
    [Fact]
    public async Task RegisterCodeFixesAsync_WhenUnmappedMember_ShouldAddMapPropertyAttribute()
    {
        var testCode = @"
namespace TestNamespace;

public class Source { public int SourceUnmappedProperty { get; set; } }
public class Dest { public int UnmappedProperty { get; set; } }

public partial class MyMapper
{
    public partial Dest {|#0:Map|}(Source source);
}
";

        var fixedCode = @"
namespace TestNamespace;

public class Source { public int SourceUnmappedProperty { get; set; } }
public class Dest { public int UnmappedProperty { get; set; } }

public partial class MyMapper
{
    [EricksonLopez.Mapper.MapProperty(""SourceUnmappedProperty"", ""UnmappedProperty"")]
    public partial Dest Map(Source source);
}
";
        var expected = new DiagnosticResult("ELM001", DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("UnmappedProperty");

        var test = new CSharpCodeFixTest<MockELM001PropertyAnalyzer, MapPropertyCodeFixProvider, DefaultVerifier>
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
    public void FixableDiagnosticIds_WhenQueried_ShouldContainELM001()
    {
        var provider = new MapPropertyCodeFixProvider();
        provider.FixableDiagnosticIds.Should().ContainSingle().Which.Should().Be("ELM001");
        provider.GetFixAllProvider().Should().Be(WellKnownFixAllProviders.BatchFixer);
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenNodeIsNotMethod_ShouldNotRegisterCodeFix()
    {
        var provider = new MapPropertyCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public class MyClass { }",
            descriptor: MockELM001PropertyAnalyzer.UnmappedMember,
            span: new TextSpan(25, 7),
            messageArgs: "UnmappedProperty");

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenDiagnosticMessageIsMalformed_ShouldNotRegisterCodeFix()
    {
        var provider = new MapPropertyCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { public partial void Map(); }",
            descriptor: MockELM001PropertyAnalyzer.MalformedUnmappedMember,
            span: new TextSpan(77, 3));

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenValidDiagnostic_RegistersExpectedCodeAction()
    {
        var provider = new MapPropertyCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { public partial void Map(); }",
            descriptor: MockELM001PropertyAnalyzer.UnmappedMember,
            span: new TextSpan(77, 3),
            messageArgs: "UnmappedProperty");

        actions.Should().ContainSingle();
        actions[0].Title.Should().Be("Add [MapProperty(\"SourceUnmappedProperty\", \"UnmappedProperty\")]");
        actions[0].EquivalenceKey.Should().Be("MapProperty_UnmappedProperty");
    }

    [Theory]
    [InlineData("No quotes in message")]
    [InlineData("Unmapped property 'OnlyOneQuote")]
    [InlineData("'LeadingQuoteOnly")]
    [InlineData("a'QuoteAtOneOnly")]
    public async Task RegisterCodeFixesAsync_WhenDiagnosticMessageHasMalformedQuotes_ShouldNotRegisterCodeFix(string message)
    {
        var provider = new MapPropertyCodeFixProvider();
        var customDescriptor = new DiagnosticDescriptor("ELM001", "Title", message, "Category", DiagnosticSeverity.Error, true);
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
        var provider = new MapPropertyCodeFixProvider();
        var customDescriptor = new DiagnosticDescriptor("ELM001", "Title", "a'Prop'b", "Category", DiagnosticSeverity.Error, true);
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { public partial void Map(); }",
            descriptor: customDescriptor,
            span: new TextSpan(77, 3));

        actions.Should().ContainSingle();
        actions[0].Title.Should().Be("Add [MapProperty(\"SourceProp\", \"Prop\")]");
    }

    [Fact]
    public async Task CodeAction_WhenInvoked_ShouldAddMapPropertyAttributeWithCorrectArguments()
    {
        var provider = new MapPropertyCodeFixProvider();
        string sourceText = "namespace TestNamespace; public partial class MyMapper { public partial void Map(); }";
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: sourceText,
            descriptor: MockELM001PropertyAnalyzer.UnmappedMember,
            span: new TextSpan(77, 3),
            messageArgs: "Age");

        actions.Should().ContainSingle();
        var action = actions[0];
        action.Title.Should().Be("Add [MapProperty(\"SourceAge\", \"Age\")]");
        action.EquivalenceKey.Should().Be("MapProperty_Age");

        var operations = await action.GetOperationsAsync(CancellationToken.None);
        var applyChanges = operations.OfType<ApplyChangesOperation>().FirstOrDefault();
        applyChanges.Should().NotBeNull();

        var changedDoc = applyChanges!.ChangedSolution.Projects.SelectMany(p => p.Documents).First();
        var text = await changedDoc.GetTextAsync();
        text.ToString().Should().Contain("[EricksonLopez.Mapper.MapProperty(\"SourceAge\", \"Age\")]");
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenDiagnosticNotOnMethod_ShouldNotRegisterCodeFix()
    {
        var provider = new MapPropertyCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { private int _field; }",
            descriptor: MockELM001PropertyAnalyzer.UnmappedMember,
            span: new TextSpan(68, 6),
            messageArgs: "UnmappedProperty");

        actions.Should().BeEmpty();
    }
}










