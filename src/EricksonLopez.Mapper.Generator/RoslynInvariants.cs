// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator;

// Justification: Roslyn metadata access is outside the supported test domain. These methods defend against invalid/inconsistent AST states that cannot be triggered by valid C# code.
[ExcludeFromCodeCoverage]
internal static class RoslynInvariants
{
    public static string? GetAttributeClassName(AttributeData attribute)
        => attribute.AttributeClass?.Name;

    public static string? GetAttributeClassFullName(AttributeData attribute)
        => attribute.AttributeClass?.ToDisplayString();

    public static Location GetLocation(ISymbol symbol)
        => symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;

    public static string? GetContainingNamespace(ISymbol symbol)
        => symbol.ContainingNamespace?.ToDisplayString();

    public static IEnumerable<ITypeSymbol> GetBaseTypesAndSelf(ITypeSymbol typeSymbol)
    {
        var current = typeSymbol;
        while (current != null)
        {
            yield return current;
            current = current.BaseType;
        }
    }
}


