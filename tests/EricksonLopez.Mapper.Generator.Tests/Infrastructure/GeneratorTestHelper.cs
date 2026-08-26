// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;

namespace EricksonLopez.Mapper.Generator.Tests.Infrastructure;

/// <summary>
/// Centralized test fixture and helper methods for executing and validating Roslyn Source Generator compilation runs.
/// </summary>
public static class GeneratorTestHelper
{
    private static readonly Lazy<ImmutableArray<MetadataReference>> CachedReferences = new(() =>
    {
        var refs = new List<MetadataReference>(Basic.Reference.Assemblies.Net80.References.All);
        refs.Add(MetadataReference.CreateFromFile(typeof(EricksonLopez.Mapper.MapperAttribute).Assembly.Location));
        return refs.ToImmutableArray();
    });

    public static (INamedTypeSymbol Symbol, Compilation Compilation) CreateCompilation(string source, string typeName, bool normalize = true)
    {
        string normalizedSource = normalize ? SourceNormalizer.Normalize(source) : source;
        var syntaxTree = CSharpSyntaxTree.ParseText(normalizedSource);
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestsAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: CachedReferences.Value,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var model = compilation.GetSemanticModel(syntaxTree);
        var typeDecl = syntaxTree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>().First(t => t.Identifier.Text == typeName);
        var symbol = (INamedTypeSymbol)model.GetDeclaredSymbol(typeDecl)!;
        return (symbol, compilation);
    }



    /// <summary>
    /// Compiles the input source and runs the <see cref="MapperGenerator"/>, optionally validating that the emitted C# compiles cleanly.
    /// </summary>
    public static (ImmutableArray<Diagnostic> Diagnostics, string GeneratedSource, Compilation OutputCompilation) RunGenerator(
        string source,
        bool verifyEmittedCodeCompiles = true,
        CancellationToken cancellationToken = default)
    {
        string normalizedSource = SourceNormalizer.Normalize(source);

        var syntaxTree = CSharpSyntaxTree.ParseText(normalizedSource, cancellationToken: cancellationToken);
        var references = CachedReferences.Value;

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestsAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var initialDiagnostics = compilation.GetDiagnostics(cancellationToken);
        var initialErrors = initialDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error && d.Id != "CS8795").ToList();
        if (initialErrors.Any())
        {
            throw new InvalidOperationException("Test source compilation failed: " + string.Join(", ", initialErrors.Select(d => d.ToString())));
        }

        var generator = new MapperGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics, cancellationToken);

        string generatedSource = outputCompilation.SyntaxTrees.LastOrDefault()?.ToString() ?? string.Empty;

        if (verifyEmittedCodeCompiles && !string.IsNullOrWhiteSpace(generatedSource) && !generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            var finalDiagnostics = outputCompilation.GetDiagnostics(cancellationToken);
            var finalErrors = finalDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error && d.Id != "CS8795").ToList();
            if (finalErrors.Any())
            {
                throw new InvalidOperationException("Generated code produced semantic compilation errors: " + string.Join(", ", finalErrors.Select(d => d.ToString())) + "\n\nGenerated Code:\n" + generatedSource);
            }
        }

        return (generateDiagnostics, generatedSource, outputCompilation);
    }

    /// <summary>
    /// Lightweight test helper that only returns diagnostics and emitted C# without semantic verification.
    /// </summary>
    public static (ImmutableArray<Diagnostic> Diagnostics, string Output) RunGeneratorSimple(string source, CancellationToken cancellationToken = default)
    {
        var (diagnostics, output, _) = RunGenerator(source, verifyEmittedCodeCompiles: false, cancellationToken: cancellationToken);
        return (diagnostics, output);
    }

    /// <summary>
    /// Compiles the input source and runs the generator with semantic verification, returning diagnostics and emitted C#.
    /// </summary>
    public static (ImmutableArray<Diagnostic> Diagnostics, string Output) RunGeneratorWithValidation(string source, CancellationToken cancellationToken = default)
    {
        var (diagnostics, output, _) = RunGenerator(source, verifyEmittedCodeCompiles: true, cancellationToken: cancellationToken);
        return (diagnostics, output);
    }

    /// <summary>
    /// Executes the generator and runs snapshot verification against <c>Snapshots/</c>.
    /// </summary>
    public static async Task RunGeneratorAndVerify(string source, [System.Runtime.CompilerServices.CallerMemberName] string testName = "", [System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
    {
        string normalizedSource = SourceNormalizer.Normalize(source);

        var syntaxTree = CSharpSyntaxTree.ParseText(normalizedSource);
        var references = CachedReferences.Value;

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var compDiags = compilation.GetDiagnostics();
        var errors = compDiags.Where(d => d.Severity == DiagnosticSeverity.Error && d.Id != "CS8795").ToList();
        if (errors.Any())
        {
            throw new InvalidOperationException("Test Compilation Failed: " + string.Join(", ", errors.Select(d => d.ToString())));
        }

        var generator = new MapperGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);

        var runResult = driver.GetRunResult();

        var output = SnapshotNormalizer.NormalizeOutput(runResult.Diagnostics, runResult.GeneratedTrees);

        string category = System.IO.Path.GetFileNameWithoutExtension(filePath).Replace("MapperGeneratorTests", "").Trim('.');
        string dir = string.IsNullOrEmpty(category) ? "../Snapshots/Core" : $"../Snapshots/{category}";

        await Verifier.Verify(output, sourceFile: filePath).UseDirectory(dir).UseMethodName(testName);
    }
}
