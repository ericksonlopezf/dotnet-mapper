// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS1019
#pragma warning disable RS1036
#pragma warning disable RS2008

namespace EricksonLopez.Mapper.Analyzers.Tests;

/// <summary>
/// Centralized collection of mock diagnostic analyzers used across CodeFixProvider test suites.
/// Eliminates duplication and isolates test triggering mechanics.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MockELM001Analyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor UnmappedMember = TestConstants.UnmappedMemberDescriptor;
    public static readonly DiagnosticDescriptor MalformedUnmappedMember = TestConstants.MalformedUnmappedMemberDescriptor;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UnmappedMember, MalformedUnmappedMember);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(ctx =>
        {
            var method = (MethodDeclarationSyntax)ctx.Node;
            if (method.Identifier.Text == "Map" && !method.AttributeLists.ToString().Contains("MapIgnore"))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(UnmappedMember, method.Identifier.GetLocation(), "UnmappedProperty"));
            }
            else if (method.Identifier.Text == "MapMalformed")
            {
                ctx.ReportDiagnostic(Diagnostic.Create(MalformedUnmappedMember, method.Identifier.GetLocation()));
            }
        }, SyntaxKind.MethodDeclaration);
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MockELM001PropertyAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor UnmappedMember = TestConstants.UnmappedMemberDescriptor;
    public static readonly DiagnosticDescriptor MalformedUnmappedMember = TestConstants.MalformedUnmappedMemberDescriptor;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UnmappedMember, MalformedUnmappedMember);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(ctx =>
        {
            var method = (MethodDeclarationSyntax)ctx.Node;
            if (method.Identifier.Text == "Map" && !method.AttributeLists.ToString().Contains("MapProperty"))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(UnmappedMember, method.Identifier.GetLocation(), "UnmappedProperty"));
            }
            else if (method.Identifier.Text == "MapMalformed")
            {
                ctx.ReportDiagnostic(Diagnostic.Create(MalformedUnmappedMember, method.Identifier.GetLocation()));
            }
        }, SyntaxKind.MethodDeclaration);
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MockELM003Analyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor UnsupportedConversion = TestConstants.UnsupportedConversionDescriptor;
    public static readonly DiagnosticDescriptor MalformedMessage = TestConstants.MalformedUnsupportedConversionDescriptor;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(UnsupportedConversion, MalformedMessage);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(ctx =>
        {
            var method = (MethodDeclarationSyntax)ctx.Node;
            var classDecl = method.Parent as ClassDeclarationSyntax;
            bool hasHelper = classDecl?.Members.OfType<MethodDeclarationSyntax>().Any(m => m.Identifier.Text == "MapSpecialDest") ?? false;

            if (!hasHelper && method.Identifier.Text == "Map")
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    UnsupportedConversion,
                    method.Identifier.GetLocation(),
                    "SpecialSource",
                    "SpecialDest",
                    "Value"));
            }
            else if (!hasHelper && method.Identifier.Text == "MapQualified")
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    UnsupportedConversion,
                    method.Identifier.GetLocation(),
                    "MyNamespace.SpecialSource",
                    "OtherNamespace.SpecialDest",
                    "Value"));
            }
            else if (method.Identifier.Text == "MapMalformed")
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    MalformedMessage,
                    method.Identifier.GetLocation()));
            }
        }, SyntaxKind.MethodDeclaration);
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MockELM004Analyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor NullabilityMismatch = TestConstants.NullabilityMismatchDescriptor;
    public static readonly DiagnosticDescriptor MalformedNullabilityMismatch = TestConstants.MalformedNullabilityMismatchDescriptor;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NullabilityMismatch, MalformedNullabilityMismatch);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(ctx =>
        {
            var method = (MethodDeclarationSyntax)ctx.Node;
            if (method.Identifier.Text == "Map" && !method.AttributeLists.ToString().Contains("MapNullFallback"))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(NullabilityMismatch, method.Identifier.GetLocation(), "NullableProp", "string?", "string"));
            }
            else if (method.Identifier.Text == "MapMalformed")
            {
                ctx.ReportDiagnostic(Diagnostic.Create(MalformedNullabilityMismatch, method.Identifier.GetLocation()));
            }
        }, SyntaxKind.MethodDeclaration);
    }
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MockELM007Analyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor AmbiguousConstructor = TestConstants.AmbiguousConstructorDescriptor;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AmbiguousConstructor);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(ctx =>
        {
            var method = (MethodDeclarationSyntax)ctx.Node;
            if (method.Identifier.Text == "Map" && !method.AttributeLists.ToString().Contains("MapFactory"))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(AmbiguousConstructor, method.Identifier.GetLocation(), "Dest"));
            }
        }, SyntaxKind.MethodDeclaration);
    }
}

