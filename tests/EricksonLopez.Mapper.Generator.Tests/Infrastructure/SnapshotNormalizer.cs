// Copyright © Erickson Lopez. MIT License.
using System.Collections.Generic;
using System.Linq;

namespace EricksonLopez.Mapper.Generator.Tests.Infrastructure;

public static class SnapshotNormalizer
{
    public static object NormalizeOutput(IReadOnlyList<Microsoft.CodeAnalysis.Diagnostic> diagnostics, IReadOnlyList<Microsoft.CodeAnalysis.SyntaxTree> generatedTrees)
    {
        return new
        {
            Diagnostics = diagnostics.Select(d => d.ToString()).ToList(),
            GeneratedSources = generatedTrees.Select(t => new
            {
                Path = t.FilePath.Replace("\\", "/"),
                Source = t.GetText().ToString().Replace("\r\n", "\n").Replace("\r", "\n")
            }).ToList()
        };
    }
}
