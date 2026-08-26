// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator;

internal static class ConversionStrategyFactory
{
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

    public static Models.ConversionStrategy GetConversionStrategy(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        List<IMethodSymbol> allMethods,
        List<Models.DiagnosticInfo> diagnostics,
        Location location,
        string memberName,
        bool isStrict,
        int enumStrategy = 0,
        bool enumIgnoreCase = false,
        Dictionary<string, string>? explicitEnumValues = null)
    {
        var unwrappedSource = UnwrapNullable(sourceType);
        var unwrappedTarget = UnwrapNullable(targetType);

        // --- Same type or Direct Assignment ---
        if (SymbolEqualityComparer.Default.Equals(unwrappedSource, unwrappedTarget))
        {
            return new Models.ConversionStrategy.DirectAssignment();
        }

        // --- Widening / Narrowing Numerics & Built-ins ---
        var builtinConv = GetBuiltinConversionStrategy(unwrappedSource, unwrappedTarget, diagnostics, location, memberName, isStrict, enumStrategy, enumIgnoreCase, explicitEnumValues);
        if (builtinConv != null) return builtinConv;

        // --- Explicit/Implicit Cast Operators on Types ---
        var castMethod = unwrappedTarget.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m =>
            m.MethodKind == MethodKind.Conversion &&
            m.Parameters.Length == 1 &&
            SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, unwrappedSource) &&
            SymbolEqualityComparer.Default.Equals(m.ReturnType, unwrappedTarget));

        if (castMethod == null)
        {
            castMethod = unwrappedSource.GetMembers().OfType<IMethodSymbol>().FirstOrDefault(m =>
                m.MethodKind == MethodKind.Conversion &&
                m.Parameters.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, unwrappedSource) &&
                SymbolEqualityComparer.Default.Equals(m.ReturnType, unwrappedTarget));
        }

        if (castMethod != null)
        {
            return new Models.ConversionStrategy.ValueObjectMapping(
                2,
                new Models.ConversionStrategy.DirectAssignment(),
                unwrappedSource.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                unwrappedTarget.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }

        // --- Strongly Typed ID / Value Object heuristic ---
        if (unwrappedTarget is INamedTypeSymbol ntt)
        {
            var properties = ntt.GetMembers().OfType<IPropertySymbol>().Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic).ToList();
            if (properties.Count == 1 && SymbolEqualityComparer.Default.Equals(properties[0].Type, unwrappedSource))
            {
                bool isReadonlyRecordStruct = ntt.IsValueType && ntt.IsRecord && ntt.IsReadOnly;
                bool hasValueProp = properties[0].Name == "Value";
                bool hasAttribute = ntt.GetAttributes().Any(a => RoslynInvariants.GetAttributeClassName(a) is "ValueObjectAttribute" or "ValueObject");

                if (hasAttribute || hasValueProp || isReadonlyRecordStruct)
                {
                    if (ntt.Constructors.Any(c => c.Parameters.Length == 1 && SymbolEqualityComparer.Default.Equals(c.Parameters[0].Type, unwrappedSource)))
                    {
                        return new Models.ConversionStrategy.ValueObjectMapping(
                            0,
                            new Models.ConversionStrategy.DirectAssignment(),
                            unwrappedSource.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            unwrappedTarget.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                    }
                }
            }
        }

        if (unwrappedSource is INamedTypeSymbol nst)
        {
            var properties = nst.GetMembers().OfType<IPropertySymbol>().Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic).ToList();
            if (properties.Count == 1 && SymbolEqualityComparer.Default.Equals(properties[0].Type, unwrappedTarget))
            {
                bool isReadonlyRecordStruct = nst.IsValueType && nst.IsRecord && nst.IsReadOnly;
                bool hasValueProp = properties[0].Name == "Value";
                bool hasAttribute = nst.GetAttributes().Any(a => RoslynInvariants.GetAttributeClassName(a) is "ValueObjectAttribute" or "ValueObject");

                if (hasAttribute || hasValueProp || isReadonlyRecordStruct)
                {
                    var valueProp = properties[0];
                    return new Models.ConversionStrategy.ValueObjectMapping(
                        1,
                        new Models.ConversionStrategy.DirectAssignment(),
                        valueProp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        unwrappedTarget.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }
            }
        }

        // --- Array → Array ---
        if (targetType is IArrayTypeSymbol targetArray && sourceType is IArrayTypeSymbol sourceArray)
        {
            var elementStrategy = GetConversionStrategy(sourceArray.ElementType, targetArray.ElementType, allMethods, diagnostics, location, memberName, isStrict, enumStrategy, enumIgnoreCase, explicitEnumValues);
            if (elementStrategy is Models.ConversionStrategy.Unsupported) return elementStrategy;
            return new Models.ConversionStrategy.EnumerableMapping(
                elementStrategy,
                sourceArray.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                targetArray.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                true, false, false, true, false);
        }

        // --- Dictionary types ---
        if (targetType is INamedTypeSymbol namedTarget && namedTarget.IsGenericType && namedTarget.Name is "Dictionary" or "IDictionary" or "IReadOnlyDictionary")
        {
            if (sourceType is INamedTypeSymbol namedSource && namedSource.IsGenericType && namedSource.Name is "Dictionary" or "IDictionary" or "IReadOnlyDictionary")
            {
                var keyStrategy = GetConversionStrategy(namedSource.TypeArguments[0], namedTarget.TypeArguments[0], allMethods, diagnostics, location, memberName, isStrict, enumStrategy, enumIgnoreCase, explicitEnumValues);
                if (keyStrategy is Models.ConversionStrategy.Unsupported) return keyStrategy;
                var valueStrategy = GetConversionStrategy(namedSource.TypeArguments[1], namedTarget.TypeArguments[1], allMethods, diagnostics, location, memberName, isStrict, enumStrategy, enumIgnoreCase, explicitEnumValues);
                if (valueStrategy is Models.ConversionStrategy.Unsupported) return valueStrategy;
                return new Models.ConversionStrategy.DictionaryMapping(
                    keyStrategy,
                    valueStrategy,
                    namedSource.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    namedTarget.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    namedSource.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    namedTarget.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    SourceHasCount: true);
            }
        }

        // --- HashSet<T> as target ---
        if (targetType is INamedTypeSymbol namedTargetHashSet && namedTargetHashSet.IsGenericType && namedTargetHashSet.Name == "HashSet")
        {
            if (sourceType is INamedTypeSymbol namedSourceHs && namedSourceHs.IsGenericType)
            {
                var elementStrategy = GetConversionStrategy(namedSourceHs.TypeArguments[0], namedTargetHashSet.TypeArguments[0], allMethods, diagnostics, location, memberName, isStrict, enumStrategy, enumIgnoreCase, explicitEnumValues);
                if (elementStrategy is Models.ConversionStrategy.Unsupported) return elementStrategy;
                bool sourceHasCount = namedSourceHs.AllInterfaces.Any(i => i.Name is "ICollection" or "IReadOnlyCollection");
                return new Models.ConversionStrategy.EnumerableMapping(
                    elementStrategy,
                    namedSourceHs.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    namedTargetHashSet.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    false, false, false, false, sourceHasCount, IsHashSet: true);
            }
            else if (sourceType is IArrayTypeSymbol sourceArrHs)
            {
                var elementStrategy = GetConversionStrategy(sourceArrHs.ElementType, namedTargetHashSet.TypeArguments[0], allMethods, diagnostics, location, memberName, isStrict, enumStrategy, enumIgnoreCase, explicitEnumValues);
                if (elementStrategy is Models.ConversionStrategy.Unsupported) return elementStrategy;
                return new Models.ConversionStrategy.EnumerableMapping(
                    elementStrategy,
                    sourceArrHs.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    namedTargetHashSet.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    false, false, false, true, false, IsHashSet: true);
            }
        }

        // --- List / IEnumerable / IList / IReadOnlyList / ImmutableArray / ImmutableList / FrozenSet etc. ---
        if (targetType is INamedTypeSymbol namedTargetEnum && namedTargetEnum.IsGenericType &&
            namedTargetEnum.Name is "List" or "IEnumerable" or "IList" or "IReadOnlyList" or "IReadOnlyCollection" or "ICollection" or "ImmutableArray" or "ImmutableList" or "IImmutableList" or "FrozenSet")
        {
            bool isList = namedTargetEnum.Name is "List" or "IList" or "ICollection" or "IEnumerable" or "IReadOnlyList" or "IReadOnlyCollection";
            bool isImmutableArray = namedTargetEnum.Name == "ImmutableArray";
            bool isImmutableList = namedTargetEnum.Name is "ImmutableList" or "IImmutableList";
            bool isFrozenSet = namedTargetEnum.Name == "FrozenSet";

            if (sourceType is INamedTypeSymbol namedSource && namedSource.IsGenericType)
            {
                var elementStrategy = GetConversionStrategy(namedSource.TypeArguments[0], namedTargetEnum.TypeArguments[0], allMethods, diagnostics, location, memberName, isStrict, enumStrategy, enumIgnoreCase, explicitEnumValues);
                if (elementStrategy is Models.ConversionStrategy.Unsupported) return elementStrategy;
                bool sourceHasCount = namedSource.Name is "ReadOnlySpan" or "Span" || namedSource.AllInterfaces.Any(i => i.Name is "ICollection" or "IReadOnlyCollection");
                return new Models.ConversionStrategy.EnumerableMapping(
                    elementStrategy,
                    namedSource.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    namedTargetEnum.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    false, isList, isImmutableArray, namedSource.Name is "ReadOnlySpan" or "Span", sourceHasCount,
                    IsImmutableList: isImmutableList, IsFrozenSet: isFrozenSet, SourceIsValueType: namedSource.IsValueType, SourceIsImmutableArray: namedSource.Name == "ImmutableArray");
            }
            else if (sourceType is IArrayTypeSymbol sourceArrayInfo)
            {
                var elementStrategy = GetConversionStrategy(sourceArrayInfo.ElementType, namedTargetEnum.TypeArguments[0], allMethods, diagnostics, location, memberName, isStrict, enumStrategy, enumIgnoreCase, explicitEnumValues);
                if (elementStrategy is Models.ConversionStrategy.Unsupported) return elementStrategy;
                return new Models.ConversionStrategy.EnumerableMapping(
                    elementStrategy,
                    sourceArrayInfo.ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    namedTargetEnum.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    false, isList, isImmutableArray, true, false,
                    IsImmutableList: isImmutableList, IsFrozenSet: isFrozenSet);
            }
        }

        // --- Cross-mapper method invocation ---
        var mappingMethod = allMethods.FirstOrDefault(m =>
            (SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, sourceType) || SymbolEqualityComparer.Default.Equals(UnwrapNullable(m.Parameters[0].Type), unwrappedSource)) &&
            (SymbolEqualityComparer.Default.Equals(m.ReturnType, targetType) || SymbolEqualityComparer.Default.Equals(UnwrapNullable(m.ReturnType), unwrappedTarget)));

        if (mappingMethod != null)
        {
            bool isSourceNullable = IsNullableType(sourceType);
            bool isTargetNullable = IsNullableType(targetType);
            string methodKey = $"{mappingMethod.Name}({mappingMethod.Parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})";
            return new Models.ConversionStrategy.MapMethodInvocation(mappingMethod.Name, isSourceNullable, isTargetNullable, methodKey);
        }

        return new Models.ConversionStrategy.Unsupported();
    }

    private static Models.ConversionStrategy? GetBuiltinConversionStrategy(
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        List<Models.DiagnosticInfo> diagnostics,
        Location location,
        string memberName,
        bool isStrict,
        int enumStrategy,
        bool enumIgnoreCase,
        Dictionary<string, string>? explicitEnumValues)
    {
        var srcSpec = sourceType.SpecialType;
        var dstSpec = targetType.SpecialType;

        // 1. Numeric widening (safe, no cast needed)
        if (IsWideningNumeric(srcSpec, dstSpec))
            return new Models.ConversionStrategy.DirectAssignment();

        // 2. Numeric narrowing (explicit cast + ELM015 warning) - T002 / ADR-020
        if (IsNarrowingNumeric(srcSpec, dstSpec))
        {
            var lineSpan = location.GetLineSpan();
            diagnostics.Add(new Models.DiagnosticInfo(
                DiagnosticDescriptors.NarrowingConversionPotentialDataLoss.Id,
                DiagnosticDescriptors.NarrowingConversionPotentialDataLoss.Title.ToString(),
                DiagnosticDescriptors.NarrowingConversionPotentialDataLoss.MessageFormat.ToString(),
                DiagnosticDescriptors.NarrowingConversionPotentialDataLoss.Category,
                (int)DiagnosticDescriptors.NarrowingConversionPotentialDataLoss.DefaultSeverity,
                DiagnosticDescriptors.NarrowingConversionPotentialDataLoss.IsEnabledByDefault,
                location.SourceTree?.FilePath ?? "",
                lineSpan.StartLinePosition.Line,
                lineSpan.StartLinePosition.Character,
                new[] { sourceType.ToDisplayString(), targetType.ToDisplayString(), memberName }.AsEquatableArray()));

            string targetFqn = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return new Models.ConversionStrategy.BuiltinConversion($"({targetFqn})({{0}})");
        }

        // 3. Enum → Enum mapping (ByName, ByValue, IgnoreCase, MapEnumValue) (ADR-016 / T010 / Gate 1)
        if (sourceType.TypeKind == TypeKind.Enum && targetType.TypeKind == TypeKind.Enum)
        {
            string targetEnumFqn = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string sourceEnumFqn = sourceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var srcFields = sourceType.GetMembers().OfType<IFieldSymbol>().Where(f => f.HasConstantValue).ToList();
            var dstFields = targetType.GetMembers().OfType<IFieldSymbol>().Where(f => f.HasConstantValue).ToList();

            // Strategy 1: ByValue
            if (enumStrategy == 1)
            {
                var dstValues = new HashSet<object?>(dstFields.Select(f => f.ConstantValue));
                if (isStrict)
                {
                    var unmappedValues = srcFields.Where(sf => !dstValues.Contains(sf.ConstantValue)).Select(sf => sf.Name).ToList();
                    if (unmappedValues.Count > 0)
                    {
                        var lineSpan = location.GetLineSpan();
                        foreach (var missing in unmappedValues)
                        {
                            diagnostics.Add(new Models.DiagnosticInfo(
                                DiagnosticDescriptors.EnumMappingMissingDestinationMember.Id,
                                DiagnosticDescriptors.EnumMappingMissingDestinationMember.Title.ToString(),
                                DiagnosticDescriptors.EnumMappingMissingDestinationMember.MessageFormat.ToString(),
                                DiagnosticDescriptors.EnumMappingMissingDestinationMember.Category,
                                (int)DiagnosticSeverity.Error,
                                DiagnosticDescriptors.EnumMappingMissingDestinationMember.IsEnabledByDefault,
                                location.SourceTree != null ? location.SourceTree.FilePath : "",
                                lineSpan.StartLinePosition.Line,
                                lineSpan.StartLinePosition.Character,
                                new[] { missing, sourceType.ToDisplayString(), targetType.ToDisplayString() }.AsEquatableArray()));
                        }
                        return new Models.ConversionStrategy.Unsupported();
                    }
                }
                return new Models.ConversionStrategy.BuiltinConversion($"({targetEnumFqn})({{0}})");
            }

            // Strategy 0: ByName
            var comp = enumIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var mappedPairs = new List<(IFieldSymbol SourceField, IFieldSymbol TargetField)>();
            var unmapped = new List<string>();

            foreach (var sf in srcFields)
            {
                IFieldSymbol? matchedDf = null;
                if (explicitEnumValues != null && explicitEnumValues.TryGetValue(sf.Name, out var explicitTargetName))
                {
                    matchedDf = dstFields.FirstOrDefault(df => string.Equals(df.Name, explicitTargetName, StringComparison.OrdinalIgnoreCase));
                }
                if (matchedDf == null)
                {
                    matchedDf = dstFields.FirstOrDefault(df => string.Equals(df.Name, sf.Name, comp));
                }

                if (matchedDf != null)
                {
                    mappedPairs.Add((sf, matchedDf));
                }
                else
                {
                    unmapped.Add(sf.Name);
                }
            }

            if (unmapped.Count > 0)
            {
                var lineSpan = location.GetLineSpan();
                foreach (var missing in unmapped)
                {
                    diagnostics.Add(new Models.DiagnosticInfo(
                        DiagnosticDescriptors.EnumMappingMissingDestinationMember.Id,
                        DiagnosticDescriptors.EnumMappingMissingDestinationMember.Title.ToString(),
                        DiagnosticDescriptors.EnumMappingMissingDestinationMember.MessageFormat.ToString(),
                        DiagnosticDescriptors.EnumMappingMissingDestinationMember.Category,
                        isStrict ? (int)DiagnosticSeverity.Error : (int)DiagnosticSeverity.Warning,
                        DiagnosticDescriptors.EnumMappingMissingDestinationMember.IsEnabledByDefault,
                        location.SourceTree?.FilePath ?? "",
                        lineSpan.StartLinePosition.Line,
                        lineSpan.StartLinePosition.Character,
                        new[] { missing, sourceType.ToDisplayString(), targetType.ToDisplayString() }.AsEquatableArray()));
                }

                if (isStrict)
                {
                    return new Models.ConversionStrategy.Unsupported();
                }
            }

            // Gate 1: Check if all matching enum members have identical names AND identical underlying constant values
            bool allMatchedValuesMatch = mappedPairs.All(p => p.SourceField.Name == p.TargetField.Name && object.Equals(p.SourceField.ConstantValue, p.TargetField.ConstantValue));
            if (allMatchedValuesMatch && (explicitEnumValues == null || explicitEnumValues.Count == 0))
            {
                // All matched underlying numeric values are compatible: emit zero-cost numeric cast
                return new Models.ConversionStrategy.BuiltinConversion($"({targetEnumFqn})({{0}})");
            }

            // Underlying numeric values or member names differ: emit an explicit member switch expression
            var switchBuilder = new System.Text.StringBuilder();
            switchBuilder.Append("({0}) switch { ");
            foreach (var pair in mappedPairs)
            {
                switchBuilder.Append($"{sourceEnumFqn}.{pair.SourceField.Name} => {targetEnumFqn}.{pair.TargetField.Name}, ");
            }
            switchBuilder.Append($"_ => ({targetEnumFqn})({{0}}) }}");
            return new Models.ConversionStrategy.BuiltinConversion(switchBuilder.ToString());
        }

        // 4. Integral ↔ Enum (T011)
        if (IsIntegralType(srcSpec) && targetType.TypeKind == TypeKind.Enum)
        {
            string targetEnumFqn = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return new Models.ConversionStrategy.BuiltinConversion($"({targetEnumFqn})({{0}})");
        }
        if (sourceType.TypeKind == TypeKind.Enum && IsIntegralType(dstSpec))
        {
            string targetIntegralFqn = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return new Models.ConversionStrategy.BuiltinConversion($"({targetIntegralFqn})({{0}})");
        }

        // 5. enum → string
        if (sourceType.TypeKind == TypeKind.Enum && dstSpec == SpecialType.System_String)
            return new Models.ConversionStrategy.BuiltinConversion("{0}.ToString()");

        // 6. string → enum — emit ELM016 warning: potential ArgumentException at runtime (ADR-020 / T051)
        if (srcSpec == SpecialType.System_String && targetType.TypeKind == TypeKind.Enum)
        {
            var lineSpan = location.GetLineSpan();
            diagnostics.Add(new Models.DiagnosticInfo(
                DiagnosticDescriptors.StringToEnumRuntimeRisk.Id,
                DiagnosticDescriptors.StringToEnumRuntimeRisk.Title.ToString(),
                DiagnosticDescriptors.StringToEnumRuntimeRisk.MessageFormat.ToString(),
                DiagnosticDescriptors.StringToEnumRuntimeRisk.Category,
                (int)DiagnosticDescriptors.StringToEnumRuntimeRisk.DefaultSeverity,
                DiagnosticDescriptors.StringToEnumRuntimeRisk.IsEnabledByDefault,
                location.SourceTree?.FilePath ?? "",
                lineSpan.StartLinePosition.Line,
                lineSpan.StartLinePosition.Character,
                new[] { memberName, targetType.ToDisplayString() }.AsEquatableArray()));

            string enumFqn = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return new Models.ConversionStrategy.BuiltinConversion($"global::System.Enum.Parse<{enumFqn}>({{0}})");
        }

        // 7. Guid → string / string → Guid
        if (sourceType is INamedTypeSymbol srcNamed && srcNamed.ToDisplayString() == "System.Guid" && dstSpec == SpecialType.System_String)
            return new Models.ConversionStrategy.BuiltinConversion("{0}.ToString()");

        if (srcSpec == SpecialType.System_String && targetType is INamedTypeSymbol dstNamed && dstNamed.ToDisplayString() == "System.Guid")
            return new Models.ConversionStrategy.BuiltinConversion("global::System.Guid.Parse({0})");

        // 8. Temporal Conversions (T012)
        // DateTime ↔ DateOnly
        if (sourceType is INamedTypeSymbol srcDt && srcDt.ToDisplayString() == "System.DateTime"
            && targetType is INamedTypeSymbol dstDo && dstDo.ToDisplayString() == "System.DateOnly")
            return new Models.ConversionStrategy.BuiltinConversion("global::System.DateOnly.FromDateTime({0})");

        if (sourceType is INamedTypeSymbol srcDo && srcDo.ToDisplayString() == "System.DateOnly"
            && targetType is INamedTypeSymbol dstDt2 && dstDt2.ToDisplayString() == "System.DateTime")
            return new Models.ConversionStrategy.BuiltinConversion("({0}).ToDateTime(global::System.TimeOnly.MinValue)");

        // DateTime ↔ DateTimeOffset
        if (sourceType is INamedTypeSymbol srcDt3 && srcDt3.ToDisplayString() == "System.DateTime"
            && targetType is INamedTypeSymbol dstDto && dstDto.ToDisplayString() == "System.DateTimeOffset")
            return new Models.ConversionStrategy.BuiltinConversion("new global::System.DateTimeOffset({0})");

        if (sourceType is INamedTypeSymbol srcDto && srcDto.ToDisplayString() == "System.DateTimeOffset"
            && targetType is INamedTypeSymbol dstDt4 && dstDt4.ToDisplayString() == "System.DateTime")
            return new Models.ConversionStrategy.BuiltinConversion("({0}).DateTime");

        // DateOnly → DateTimeOffset
        if (sourceType is INamedTypeSymbol srcDo2 && srcDo2.ToDisplayString() == "System.DateOnly"
            && targetType is INamedTypeSymbol dstDto2 && dstDto2.ToDisplayString() == "System.DateTimeOffset")
            return new Models.ConversionStrategy.BuiltinConversion("new global::System.DateTimeOffset(({0}).ToDateTime(global::System.TimeOnly.MinValue))");

        // DateTimeOffset → DateOnly
        if (sourceType is INamedTypeSymbol srcDto3 && srcDto3.ToDisplayString() == "System.DateTimeOffset"
            && targetType is INamedTypeSymbol dstDo3 && dstDo3.ToDisplayString() == "System.DateOnly")
            return new Models.ConversionStrategy.BuiltinConversion("global::System.DateOnly.FromDateTime(({0}).DateTime)");

        return null;
    }

    private static bool IsIntegralType(SpecialType type)
    {
        return type is SpecialType.System_Byte or SpecialType.System_SByte
                    or SpecialType.System_Int16 or SpecialType.System_UInt16
                    or SpecialType.System_Int32 or SpecialType.System_UInt32
                    or SpecialType.System_Int64 or SpecialType.System_UInt64;
    }

    private static bool IsWideningNumeric(SpecialType source, SpecialType target)
    {
        return (source, target) switch
        {
            (SpecialType.System_Byte, SpecialType.System_Int16) => true,
            (SpecialType.System_Byte, SpecialType.System_Int32) => true,
            (SpecialType.System_Byte, SpecialType.System_Int64) => true,
            (SpecialType.System_Byte, SpecialType.System_Single) => true,
            (SpecialType.System_Byte, SpecialType.System_Double) => true,
            (SpecialType.System_Byte, SpecialType.System_Decimal) => true,
            (SpecialType.System_SByte, SpecialType.System_Int16) => true,
            (SpecialType.System_SByte, SpecialType.System_Int32) => true,
            (SpecialType.System_SByte, SpecialType.System_Int64) => true,
            (SpecialType.System_SByte, SpecialType.System_Single) => true,
            (SpecialType.System_SByte, SpecialType.System_Double) => true,
            (SpecialType.System_SByte, SpecialType.System_Decimal) => true,
            (SpecialType.System_Int16, SpecialType.System_Int32) => true,
            (SpecialType.System_Int16, SpecialType.System_Int64) => true,
            (SpecialType.System_Int16, SpecialType.System_Single) => true,
            (SpecialType.System_Int16, SpecialType.System_Double) => true,
            (SpecialType.System_Int16, SpecialType.System_Decimal) => true,
            (SpecialType.System_UInt16, SpecialType.System_Int32) => true,
            (SpecialType.System_UInt16, SpecialType.System_Int64) => true,
            (SpecialType.System_UInt16, SpecialType.System_Single) => true,
            (SpecialType.System_UInt16, SpecialType.System_Double) => true,
            (SpecialType.System_UInt16, SpecialType.System_Decimal) => true,
            (SpecialType.System_Int32, SpecialType.System_Int64) => true,
            (SpecialType.System_Int32, SpecialType.System_Single) => true,
            (SpecialType.System_Int32, SpecialType.System_Double) => true,
            (SpecialType.System_Int32, SpecialType.System_Decimal) => true,
            (SpecialType.System_UInt32, SpecialType.System_Int64) => true,
            (SpecialType.System_UInt32, SpecialType.System_Single) => true,
            (SpecialType.System_UInt32, SpecialType.System_Double) => true,
            (SpecialType.System_UInt32, SpecialType.System_Decimal) => true,
            (SpecialType.System_Int64, SpecialType.System_Single) => true,
            (SpecialType.System_Int64, SpecialType.System_Double) => true,
            (SpecialType.System_Int64, SpecialType.System_Decimal) => true,
            (SpecialType.System_UInt64, SpecialType.System_Single) => true,
            (SpecialType.System_UInt64, SpecialType.System_Double) => true,
            (SpecialType.System_UInt64, SpecialType.System_Decimal) => true,
            (SpecialType.System_Single, SpecialType.System_Double) => true,
            _ => false
        };
    }

    private static bool IsNarrowingNumeric(SpecialType source, SpecialType target)
    {
        return (source, target) switch
        {
            (SpecialType.System_Int64, SpecialType.System_Int32) => true,
            (SpecialType.System_Int64, SpecialType.System_Int16) => true,
            (SpecialType.System_Int64, SpecialType.System_Byte) => true,
            (SpecialType.System_Int32, SpecialType.System_Int16) => true,
            (SpecialType.System_Int32, SpecialType.System_Byte) => true,
            (SpecialType.System_Int32, SpecialType.System_SByte) => true,
            (SpecialType.System_Int32, SpecialType.System_UInt16) => true,
            (SpecialType.System_Int16, SpecialType.System_Byte) => true,
            (SpecialType.System_Int16, SpecialType.System_SByte) => true,
            (SpecialType.System_Double, SpecialType.System_Single) => true,
            (SpecialType.System_Double, SpecialType.System_Decimal) => true,
            (SpecialType.System_Double, SpecialType.System_Int64) => true,
            (SpecialType.System_Double, SpecialType.System_Int32) => true,
            (SpecialType.System_Single, SpecialType.System_Decimal) => true,
            (SpecialType.System_Single, SpecialType.System_Int64) => true,
            (SpecialType.System_Single, SpecialType.System_Int32) => true,
            (SpecialType.System_Decimal, SpecialType.System_Double) => true,
            (SpecialType.System_Decimal, SpecialType.System_Single) => true,
            (SpecialType.System_Decimal, SpecialType.System_Int64) => true,
            (SpecialType.System_Decimal, SpecialType.System_Int32) => true,
            _ => false
        };
    }
}
