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
/// Provides a code fix for <c>ELM007</c> that adds a <c>[MapFactory("Create")]</c> attribute to resolve constructor ambiguity.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MapFactoryCodeFixProvider)), Shared]
public class MapFactoryCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("ELM007");

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

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add [MapFactory(\"Create\")]",
                createChangedDocument: c => AddMapFactoryAttributeAsync(context.Document, declaration, c),
                equivalenceKey: "MapFactory_Create"),
            diagnostic);
    }

    private async Task<Document> AddMapFactoryAttributeAsync(Document document, MethodDeclarationSyntax methodDecl, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        var attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("EricksonLopez.Mapper.MapFactory"))
            .WithArgumentList(
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal("Create"))))));

        var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
            .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
            .WithAdditionalAnnotations(Microsoft.CodeAnalysis.Formatting.Formatter.Annotation);

        var newMethodDecl = methodDecl.AddAttributeLists(attributeList);
        var newRoot = root!.ReplaceNode(methodDecl, newMethodDecl);

        return document.WithSyntaxRoot(newRoot);
    }
}




