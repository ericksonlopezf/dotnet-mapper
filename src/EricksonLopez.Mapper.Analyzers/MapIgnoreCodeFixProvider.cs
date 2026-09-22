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

namespace EricksonLopez.Mapper.Analyzers
{
    /// <summary>
    /// Provides a code fix for <c>ELM001</c> that adds a <c>[MapIgnore("MemberName")]</c> attribute to suppress unmapped destination member errors.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MapIgnoreCodeFixProvider)), Shared]
    public class MapIgnoreCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("ELM001");

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

            var message = diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture);
            var startIndex = message.IndexOf('\'');
            var endIndex = message.IndexOf('\'', startIndex + 1);
            if (startIndex == -1 || endIndex == -1) return;

            var propertyName = message.Substring(startIndex + 1, endIndex - startIndex - 1);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Add [MapIgnore(\"{propertyName}\")]",
                    createChangedDocument: c => AddMapIgnoreAttributeAsync(context.Document, declaration, propertyName, c),
                    equivalenceKey: $"MapIgnore_{propertyName}"),
                diagnostic);
        }

        private async Task<Document> AddMapIgnoreAttributeAsync(Document document, MethodDeclarationSyntax methodDecl, string propertyName, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("EricksonLopez.Mapper.MapIgnore"))
                .WithArgumentList(
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(propertyName))))));

            var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
                .WithTrailingTrivia(SyntaxFactory.ElasticMarker)
                .WithAdditionalAnnotations(Microsoft.CodeAnalysis.Formatting.Formatter.Annotation);

            var newMethodDecl = methodDecl.AddAttributeLists(attributeList);
            var newRoot = root!.ReplaceNode(methodDecl, newMethodDecl);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}






