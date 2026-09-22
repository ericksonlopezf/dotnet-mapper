// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EricksonLopez.Mapper.Analyzers;

/// <summary>
/// Provides a code fix for <c>ELM003</c> that generates a partial mapping method stub
/// to allow custom conversion logic for unsupported type pairs.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddPartialMappingMethodCodeFixProvider)), Shared]
public class AddPartialMappingMethodCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("ELM003");

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Navigate up to the containing class declaration
        var token = root!.FindToken(diagnosticSpan.Start);
        var methodDecl = token.Parent!.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (methodDecl == null) return;

        var classDecl = methodDecl.Parent as ClassDeclarationSyntax;
        if (classDecl == null) return;

        // Extract source and destination type names from the diagnostic message
        // Format: "Cannot convert from '{0}' to '{1}' for member '{2}'"
        var message = diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture);
        var parts = ExtractTypes(message);
        if (parts == null) return;

        var (sourceTypeName, destTypeName, memberName) = parts.Value;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Add partial mapping method for {sourceTypeName} → {destTypeName}",
                createChangedDocument: c => AddPartialMethodAsync(context.Document, classDecl, sourceTypeName, destTypeName, memberName, c),
                equivalenceKey: $"AddPartialMapping_{sourceTypeName}_{destTypeName}"),
            diagnostic);
    }

    internal static (string sourceType, string destType, string memberName)? ExtractTypes(string message)
    {
        // "Cannot convert from 'X' to 'Y' for member 'Z'"
        var fromIdx = message.IndexOf("from '", StringComparison.Ordinal);
        var toIdx = message.IndexOf("to '", StringComparison.Ordinal);
        var memberIdx = message.IndexOf("for member '", StringComparison.Ordinal);

        if (fromIdx == -1 || toIdx == -1 || memberIdx == -1) return null;

        var srcStart = fromIdx + 6;
        var srcEnd = message.IndexOf('\'', srcStart);
        if (srcEnd == -1) return null;

        var dstStart = toIdx + 4;
        var dstEnd = message.IndexOf('\'', dstStart);
        if (dstEnd == -1) return null;

        var memStart = memberIdx + 12;
        var memEnd = message.IndexOf('\'', memStart);
        if (memEnd == -1) return null;

        var sourceType = message.Substring(srcStart, srcEnd - srcStart);
        var destType = message.Substring(dstStart, dstEnd - dstStart);
        var member = message.Substring(memStart, memEnd - memStart);

        // Use simple name (last segment after dot) for method naming
        var srcSimple = sourceType.Split('.').Last();
        var dstSimple = destType.Split('.').Last();

        return (srcSimple, dstSimple, member);
    }

    private async Task<Document> AddPartialMethodAsync(
        Document document,
        ClassDeclarationSyntax classDecl,
        string sourceTypeName,
        string destTypeName,
        string memberName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        // Generate a method name: Map{Source}To{Dest} or Map{Dest}
        var methodName = $"Map{destTypeName}";

        // Build: public partial DestType MapDest(SourceType source);
        var methodDecl = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.ParseTypeName(destTypeName),
                SyntaxFactory.Identifier(methodName))
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddParameterListParameters(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("source"))
                    .WithType(SyntaxFactory.ParseTypeName(sourceTypeName)))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
            .WithAdditionalAnnotations(Microsoft.CodeAnalysis.Formatting.Formatter.Annotation);

        var newClassDecl = classDecl.AddMembers(methodDecl);
        var newRoot = root!.ReplaceNode(classDecl, newClassDecl);

        return document.WithSyntaxRoot(newRoot);
    }
}




