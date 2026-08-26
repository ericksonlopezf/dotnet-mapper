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

public class AddPartialMappingMethodCodeFixProviderTests
{
    private static string NormalizeLineEndings(string code) => code.Replace("\r\n", "\n").Replace("\n", "\r\n");

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenUnsupportedConversion_ShouldAddPartialMappingMethodStub()
    {
        var testCode = @"
namespace TestNamespace;

public class SpecialSource { public int Value { get; set; } }
public class SpecialDest { public int Value { get; set; } }

public partial class MyMapper
{
    public partial SpecialDest {|#0:Map|}(SpecialSource source);
}
";

        var fixedCode = @"
namespace TestNamespace;

public class SpecialSource { public int Value { get; set; } }
public class SpecialDest { public int Value { get; set; } }

public partial class MyMapper
{
    public partial SpecialDest Map(SpecialSource source);

    public partial SpecialDest MapSpecialDest(SpecialSource source);
}
";

        var expected = new DiagnosticResult("ELM003", DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("SpecialSource", "SpecialDest", "Value");

        var test = new CSharpCodeFixTest<MockELM003Analyzer, AddPartialMappingMethodCodeFixProvider, DefaultVerifier>
        {
            TestCode = NormalizeLineEndings(testCode),
            FixedCode = NormalizeLineEndings(fixedCode),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None
        };
        test.ExpectedDiagnostics.Add(expected);

        await test.RunAsync();
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenTypesAreNamespaceQualified_ShouldExtractSimpleTypeNames()
    {
        var testCode = @"
namespace TestNamespace;

public class SpecialSource { public int Value { get; set; } }
public class SpecialDest { public int Value { get; set; } }

public partial class MyMapper
{
    public partial SpecialDest {|#0:MapQualified|}(SpecialSource source);
}
";

        var fixedCode = @"
namespace TestNamespace;

public class SpecialSource { public int Value { get; set; } }
public class SpecialDest { public int Value { get; set; } }

public partial class MyMapper
{
    public partial SpecialDest MapQualified(SpecialSource source);

    public partial SpecialDest MapSpecialDest(SpecialSource source);
}
";

        var expected = new DiagnosticResult("ELM003", DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithArguments("MyNamespace.SpecialSource", "OtherNamespace.SpecialDest", "Value");

        var test = new CSharpCodeFixTest<MockELM003Analyzer, AddPartialMappingMethodCodeFixProvider, DefaultVerifier>
        {
            TestCode = NormalizeLineEndings(testCode),
            FixedCode = NormalizeLineEndings(fixedCode),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None
        };
        test.ExpectedDiagnostics.Add(expected);

        await test.RunAsync();
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenMessageIsMalformed_ShouldNotRegisterCodeFix()
    {
        var testCode = @"
namespace TestNamespace;

public class SpecialSource { public int Value { get; set; } }
public class SpecialDest { public int Value { get; set; } }

public partial class MyMapper
{
    public partial SpecialDest {|#0:MapMalformed|}(SpecialSource source);
}
";

        var expected = new DiagnosticResult("ELM003", DiagnosticSeverity.Error)
            .WithLocation(0);

        var test = new CSharpCodeFixTest<MockELM003Analyzer, AddPartialMappingMethodCodeFixProvider, DefaultVerifier>
        {
            TestCode = NormalizeLineEndings(testCode),
            FixedCode = NormalizeLineEndings(testCode),
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.None,
            NumberOfFixAllIterations = 0
        };
        test.ExpectedDiagnostics.Add(expected);

        await test.RunAsync();
    }

    [Fact]
    public void FixableDiagnosticIds_WhenQueried_ShouldContainELM003()
    {
        var provider = new AddPartialMappingMethodCodeFixProvider();
        provider.FixableDiagnosticIds.Should().ContainSingle().Which.Should().Be("ELM003");
        provider.GetFixAllProvider().Should().Be(WellKnownFixAllProviders.BatchFixer);
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenValidDiagnostic_RegistersExpectedCodeAction()
    {
        var provider = new AddPartialMappingMethodCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { public partial void Map(); }",
            descriptor: MockELM003Analyzer.UnsupportedConversion,
            span: new TextSpan(77, 3),
            messageArgs: new object[] { "SpecialSource", "SpecialDest", "Value" });

        actions.Should().ContainSingle();
        actions[0].Title.Should().Be("Add partial mapping method for SpecialSource → SpecialDest");
        actions[0].EquivalenceKey.Should().Be("AddPartialMapping_SpecialSource_SpecialDest");
    }

    [Theory]
    [InlineData("Cannot convert to 'Dest' for member 'Prop'")] // Missing "from '"
    [InlineData("Cannot convert from 'Source' for member 'Prop'")] // Missing "to '"
    [InlineData("Cannot convert from 'Source' to 'Dest'")] // Missing "for member '"
    [InlineData("Cannot convert from 'Source")] // Missing closing quote on source
    [InlineData("Cannot convert from 'Source' to 'Dest")] // Missing closing quote on dest
    [InlineData("Cannot convert from 'Source' to 'Dest' for member 'Prop")] // Missing closing quote on member
    [InlineData("to 'Dest' for member 'Prop' from 'SourceNoClosingQuote")] // fromIdx is last and has no closing quote (srcEnd == -1)
    [InlineData("from 'Source' for member 'Prop' to 'DestNoClosingQuote")] // toIdx is last and has no closing quote (dstEnd == -1)
    [InlineData("from 'Source' to 'Dest' for member 'PropNoClosingQuote")] // memberIdx is last and has no closing quote (memEnd == -1)
    [InlineData("Some arbitrary message")] // No parts at all
    public async Task RegisterCodeFixesAsync_WhenMessageHasMalformedSections_ShouldNotRegisterCodeFix(string malformedMessage)
    {
        var provider = new AddPartialMappingMethodCodeFixProvider();
        var customDescriptor = new DiagnosticDescriptor("ELM003", "Title", malformedMessage, "Category", DiagnosticSeverity.Error, true);
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { public partial void Map(); }",
            descriptor: customDescriptor,
            span: new TextSpan(77, 3));

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenDiagnosticNotOnMethod_ShouldNotRegisterCodeFix()
    {
        var provider = new AddPartialMappingMethodCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public partial class MyMapper { private int _field; }",
            descriptor: MockELM003Analyzer.UnsupportedConversion,
            span: new TextSpan(68, 6), // points to _field
            messageArgs: new object[] { "SpecialSource", "SpecialDest", "Value" });

        actions.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterCodeFixesAsync_WhenMethodInsideStruct_ShouldNotRegisterCodeFix()
    {
        var provider = new AddPartialMappingMethodCodeFixProvider();
        var actions = await CodeFixTestHelper.GetRegisteredCodeActionsAsync(
            provider,
            sourceText: "namespace TestNamespace; public struct MyStruct { public void Map() {} }",
            descriptor: MockELM003Analyzer.UnsupportedConversion,
            span: new TextSpan(62, 3),
            messageArgs: new object[] { "SpecialSource", "SpecialDest", "Value" });

        actions.Should().BeEmpty();
    }
}










