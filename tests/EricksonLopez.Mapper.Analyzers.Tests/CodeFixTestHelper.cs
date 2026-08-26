// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace EricksonLopez.Mapper.Analyzers.Tests;

/// <summary>
/// Centralized test helper for synthesizing Roslyn <see cref="CodeFixContext"/> instances and capturing registered code actions.
/// Eliminates boilerplate in workspace configuration and context synthesis across all CodeFixProvider test suites.
/// </summary>
public static class CodeFixTestHelper
{
    /// <summary>
    /// Synthesizes an adhoc document and invokes <see cref="CodeFixProvider.RegisterCodeFixesAsync"/> with a diagnostic built directly from a descriptor and text span.
    /// <para><b>Usage:</b> Preferred for standard single-location diagnostics when the exact character span within <paramref name="sourceText"/> is known.</para>
    /// </summary>
    public static async Task<List<CodeAction>> GetRegisteredCodeActionsAsync(
        CodeFixProvider provider,
        string sourceText,
        DiagnosticDescriptor descriptor,
        TextSpan span,
        params object[] messageArgs)
    {
        using var adhocWorkspace = new AdhocWorkspace();
        var project = adhocWorkspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = adhocWorkspace.AddDocument(project.Id, "Test.cs", SourceText.From(sourceText));
        var syntaxTree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);

        var diagnostic = Diagnostic.Create(
            descriptor,
            Location.Create(syntaxTree!, span),
            messageArgs);

        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None);

        await provider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

        return actions;
    }

    /// <summary>
    /// Synthesizes an adhoc document and invokes <see cref="CodeFixProvider.RegisterCodeFixesAsync"/> with a custom diagnostic factory.
    /// <para><b>Usage:</b> Preferred when the diagnostic location or properties must be dynamically resolved from the parsed <see cref="SyntaxTree"/> (e.g. node-relative location).</para>
    /// </summary>
    public static async Task<List<CodeAction>> GetRegisteredCodeActionsAsync(
        CodeFixProvider provider,
        string sourceText,
        Func<SyntaxTree, Diagnostic> diagnosticFactory)
    {
        using var adhocWorkspace = new AdhocWorkspace();
        var project = adhocWorkspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = adhocWorkspace.AddDocument(project.Id, "Test.cs", SourceText.From(sourceText));
        var syntaxTree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);

        var diagnostic = diagnosticFactory(syntaxTree!);

        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None);

        await provider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

        return actions;
    }
}
