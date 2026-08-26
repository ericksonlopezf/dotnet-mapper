// Copyright © Erickson Lopez. MIT License.
using System;
using Microsoft.CodeAnalysis;

namespace EricksonLopez.Mapper.Analyzers;

internal static class RoslynInvariants
{
    public static string? GetAttributeClassFullName(AttributeData attribute)
        => attribute.AttributeClass?.ToDisplayString();

    public static string? GetContainingNamespace(ISymbol symbol)
        => symbol.ContainingNamespace?.ToDisplayString();
}
