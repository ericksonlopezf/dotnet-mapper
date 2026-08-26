// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EricksonLopez.Mapper.Analyzers;

/// <summary>
/// Analyzes mapper declarations to enforce AOT compilation principles,
/// prohibiting reflection and <see langword="dynamic"/> keywords, and requiring the
/// <see langword="partial"/> modifier on types annotated with <c>MapperAttribute</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MapperAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// Represents the diagnostic descriptor for <c>ELM008</c>, raised when a mapper uses reflection APIs.
    /// </summary>
    public static readonly DiagnosticDescriptor UsesReflection = new(
        id: "ELM008",
        title: "Mapper uses reflection",
        messageFormat: "Mapper uses reflection which violates AOT-First principles",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Represents the diagnostic descriptor for <c>ELM009</c>, raised when a mapper uses the <see langword="dynamic"/> keyword.
    /// </summary>
    public static readonly DiagnosticDescriptor UsesDynamic = new(
        id: "ELM009",
        title: "Mapper uses dynamic",
        messageFormat: "Mapper uses dynamic which violates AOT-First principles",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Represents the diagnostic descriptor for <c>ELM012</c>, raised when a mapper type is not declared with the <see langword="partial"/> modifier.
    /// </summary>
    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "ELM012",
        title: "Mapper must be partial",
        messageFormat: "Mapper class '{0}' must be declared as partial",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableHashSet<string> ForbiddenReflectionTypes = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "System.Activator",
        "System.Runtime.InteropServices.Marshal",
        "System.Runtime.CompilerServices.RuntimeHelpers",
        "System.Runtime.Serialization.FormatterServices");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UsesReflection, UsesDynamic, MustBePartial);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.IdentifierName, SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var typeSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(classDecl)!;

        bool isMapper = typeSymbol.GetAttributes().Any(a => RoslynInvariants.GetAttributeClassFullName(a) == "EricksonLopez.Mapper.MapperAttribute");
        if (!isMapper) return;

        if (!classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            context.ReportDiagnostic(Diagnostic.Create(MustBePartial, classDecl.Identifier.GetLocation(), classDecl.Identifier.Text));
        }
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node;

        var typeDecl = node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeDecl is null) return;

        var typeSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(typeDecl)!;

        bool isMapper = typeSymbol.GetAttributes().Any(a => RoslynInvariants.GetAttributeClassFullName(a) == "EricksonLopez.Mapper.MapperAttribute");
        if (!isMapper) return;

        if (node is IdentifierNameSyntax idNode && idNode.Identifier.Text == "dynamic")
        {
            context.ReportDiagnostic(Diagnostic.Create(UsesDynamic, node.GetLocation()));
        }
        else if (node is MemberAccessExpressionSyntax memberAccess)
        {
            var targetSymbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;
            if (targetSymbol != null)
            {
                var ns = RoslynInvariants.GetContainingNamespace(targetSymbol);
                var containingType = targetSymbol.ContainingType?.ToDisplayString();

                if (ns == "System.Reflection")
                {
                    context.ReportDiagnostic(Diagnostic.Create(UsesReflection, node.GetLocation()));
                }
                else if (containingType == "System.Type" && targetSymbol.Name.StartsWith("Get", StringComparison.Ordinal))
                {
                    context.ReportDiagnostic(Diagnostic.Create(UsesReflection, node.GetLocation()));
                }
                else if (containingType is not null && ForbiddenReflectionTypes.Contains(containingType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(UsesReflection, node.GetLocation()));
                }
            }
        }
    }
}


