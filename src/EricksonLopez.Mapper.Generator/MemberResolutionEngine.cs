// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator;

internal static class MemberResolutionEngine
{
    public class PropertyPathResolution
    {
        public ITypeSymbol LeafType { get; }
        public bool IsPathNullable { get; }
        public string FormattedPath { get; }
        public string LeafPropertyName { get; }

        public PropertyPathResolution(ITypeSymbol leafType, bool isPathNullable, string formattedPath, string leafPropertyName)
        {
            LeafType = leafType;
            IsPathNullable = isPathNullable;
            FormattedPath = formattedPath;
            LeafPropertyName = leafPropertyName;
        }
    }

    public static bool HasMapperIgnore(ISymbol symbol)
    {
        return symbol.GetAttributes().Any(a =>
        {
            var name = RoslynInvariants.GetAttributeClassName(a);
            return name is "MapperIgnoreAttribute" or "MapperIgnore";
        });
    }

    public static List<IPropertySymbol> GetAllProperties(ITypeSymbol typeSymbol)
    {
        var properties = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);

        if (typeSymbol.TypeKind == TypeKind.Interface)
        {
            foreach (var prop in typeSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                if (!prop.IsStatic && !prop.IsIndexer && !properties.ContainsKey(prop.Name)) properties.Add(prop.Name, prop);
            }
            foreach (var iface in typeSymbol.AllInterfaces)
            {
                foreach (var prop in iface.GetMembers().OfType<IPropertySymbol>())
                {
                    if (!prop.IsStatic && !prop.IsIndexer && !properties.ContainsKey(prop.Name)) properties.Add(prop.Name, prop);
                }
            }
        }
        else
        {
            foreach (var currentType in RoslynInvariants.GetBaseTypesAndSelf(typeSymbol))
            {
                foreach (var prop in currentType.GetMembers().OfType<IPropertySymbol>())
                {
                    if (!prop.IsStatic && !prop.IsIndexer && !properties.ContainsKey(prop.Name))
                    {
                        properties.Add(prop.Name, prop);
                    }
                }
            }
        }
        return properties.Values.ToList();
    }

    public static IPropertySymbol? MatchProperty(List<IPropertySymbol> sourceProperties, string targetName, IMethodSymbol methodSymbol, string destName, List<Models.DiagnosticInfo> diagnostics)
    {
        var exact = sourceProperties.FirstOrDefault(p => p.Name == targetName);
        if (exact != null) return exact;

        var matches = sourceProperties.Where(p => string.Equals(p.Name, targetName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count == 1) return matches[0];
        if (matches.Count > 0)
        {
            var loc = RoslynInvariants.GetLocation(methodSymbol);
            var lineSpan = loc.GetLineSpan();
            diagnostics.Add(new Models.DiagnosticInfo(
                DiagnosticDescriptors.AmbiguousPropertyMatch.Id,
                DiagnosticDescriptors.AmbiguousPropertyMatch.Title.ToString(System.Globalization.CultureInfo.InvariantCulture),
                DiagnosticDescriptors.AmbiguousPropertyMatch.MessageFormat.ToString(System.Globalization.CultureInfo.InvariantCulture),
                DiagnosticDescriptors.AmbiguousPropertyMatch.Category,
                (int)DiagnosticDescriptors.AmbiguousPropertyMatch.DefaultSeverity,
                DiagnosticDescriptors.AmbiguousPropertyMatch.IsEnabledByDefault,
                loc.SourceTree?.FilePath ?? "",
                lineSpan.StartLinePosition.Line,
                lineSpan.StartLinePosition.Character,
                new[] { destName }.AsEquatableArray()));
            return null;
        }
        return null;
    }

    public static PropertyPathResolution? ResolvePropertyPath(
        ITypeSymbol rootType,
        string path,
        IMethodSymbol methodSymbol,
        string destName,
        List<Models.DiagnosticInfo> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        var segments = path.Split('.');
        ITypeSymbol currentType = rootType;
        bool isPathNullable = false;
        var resolvedProps = new List<IPropertySymbol>();

        for (int i = 0; i < segments.Length; i++)
        {
            string segment = segments[i].Trim();
            if (string.IsNullOrEmpty(segment)) return null;

            var unwrappedType = UnwrapNullable(currentType);
            var props = GetAllProperties(unwrappedType);
            var matchedProp = MatchProperty(props, segment, methodSymbol, destName, diagnostics);
            if (matchedProp == null || HasMapperIgnore(matchedProp))
            {
                return null;
            }

            bool isLast = (i == segments.Length - 1);
            var propType = matchedProp.Type;

            if (IsNullableType(propType) || (!isLast && propType.IsReferenceType))
            {
                isPathNullable = true;
            }

            resolvedProps.Add(matchedProp);
            currentType = propType;
        }

        var lastProp = resolvedProps[resolvedProps.Count - 1];
        string formatted = BuildFormattedPath(rootType, resolvedProps);
        return new PropertyPathResolution(lastProp.Type, isPathNullable, formatted, lastProp.Name);
    }

    private static string BuildFormattedPath(ITypeSymbol rootType, List<IPropertySymbol> resolvedProps)
    {
        var sb = new StringBuilder();
        ITypeSymbol currentType = rootType;

        for (int i = 0; i < resolvedProps.Count; i++)
        {
            if (i > 0)
            {
                bool prevCanBeNull = currentType.IsReferenceType || IsNullableType(currentType);
                sb.Append(prevCanBeNull ? "?." : ".");
            }
            sb.Append(CodeEmitter.EscapeIdentifier(resolvedProps[i].Name));
            currentType = resolvedProps[i].Type;
        }
        return sb.ToString();
    }

    private static ITypeSymbol UnwrapNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return named.TypeArguments[0];
        }
        return type;
    }

    private static bool IsNullableType(ITypeSymbol type)
        => type.NullableAnnotation == NullableAnnotation.Annotated ||
           (type is INamedTypeSymbol named && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T);
}
