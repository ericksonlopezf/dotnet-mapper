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
/// Provides a code fix for <c>ELM004</c> that adds a <c>[MapNullFallback("MemberName", "defaultValue")]</c> attribute to handle nullable-to-non-nullable assignments.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MapNullFallbackCodeFixProvider)), Shared]
public class MapNullFallbackCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("ELM004");

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var token = root!.FindToken(diagnosticSpan.Start);
        var declaration = token.Parent!.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (declaration == null) return;

        var message = diagnostic.GetMessage();
        var startIndex = message.IndexOf('\'');
        var endIndex = message.IndexOf('\'', startIndex + 1);
        if (startIndex == -1 || endIndex == -1) return;

        var propertyName = message.Substring(startIndex + 1, endIndex - startIndex - 1);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Add [MapNullFallback(\"{propertyName}\", \"default!\")]",
                createChangedDocument: c => AddMapNullFallbackAttributeAsync(context.Document, declaration, propertyName, c),
                equivalenceKey: $"MapNullFallback_{propertyName}"),
            diagnostic);
    }

    private async Task<Document> AddMapNullFallbackAttributeAsync(Document document, MethodDeclarationSyntax methodDecl, string propertyName, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        var attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("EricksonLopez.Mapper.MapNullFallback"))
            .WithArgumentList(
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(propertyName))),
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal("default!")))
                    })));

        var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
            .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
            .WithAdditionalAnnotations(Microsoft.CodeAnalysis.Formatting.Formatter.Annotation);

        var newMethodDecl = methodDecl.AddAttributeLists(attributeList);
        var newRoot = root!.ReplaceNode(methodDecl, newMethodDecl);

        return document.WithSyntaxRoot(newRoot);
    }
}




