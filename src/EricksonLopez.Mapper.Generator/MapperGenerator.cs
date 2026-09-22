// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EricksonLopez.Mapper.Generator;

/// <summary>
/// Provides compile-time source generation for strongly-typed object mappers annotated with <c>MapperAttribute</c>.
/// </summary>
[Generator]
public class MapperGenerator : IIncrementalGenerator
{
    private static Models.DiagnosticInfo CreateDiagnostic(DiagnosticDescriptor descriptor, Location location, params string[] args)
    {
        var lineSpan = location.GetLineSpan();
        return new Models.DiagnosticInfo(
            descriptor.Id,
            descriptor.Title.ToString(System.Globalization.CultureInfo.InvariantCulture),
            descriptor.MessageFormat.ToString(System.Globalization.CultureInfo.InvariantCulture),
            descriptor.Category,
            (int)descriptor.DefaultSeverity,
            descriptor.IsEnabledByDefault,
            location.SourceTree != null ? location.SourceTree.FilePath : "",
            lineSpan.StartLinePosition.Line,
            lineSpan.StartLinePosition.Character,
            args.AsEquatableArray());
    }

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "EricksonLopez.Mapper.MapperAttribute",
                predicate: (node, _) => node is ClassDeclarationSyntax or InterfaceDeclarationSyntax,
                transform: GetSemanticTargetForGeneration)
            .Where(m => m is not null)!;

        context.RegisterSourceOutput(provider, (spc, source) => Execute(source!, spc));

        var assemblyHasDiAttrProvider = context.CompilationProvider
            .Select((comp, _) => comp.Assembly.GetAttributes().Any(a =>
                a.AttributeClass?.ToDisplayString() == "EricksonLopez.Mapper.GenerateMapperRegistrationAttribute"));

        var diProvider = provider.Collect().Combine(assemblyHasDiAttrProvider);

        context.RegisterSourceOutput(diProvider, (spc, source) => DependencyInjectionEmitter.EmitDIRegistration(source.Left!, source.Right, spc));
    }

    private static Models.TypeMapping? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.TargetSymbol is not INamedTypeSymbol mapperSymbol) return null;
        return ProcessMapperSymbol(mapperSymbol, context.SemanticModel.Compilation, cancellationToken);
    }

    internal static Models.TypeMapping? ProcessMapperSymbol(INamedTypeSymbol mapperSymbol, Compilation compilation, CancellationToken cancellationToken)
    {
        var diagnostics = new List<Models.DiagnosticInfo>();
        bool isStrict = true;
        int defaultEnumStrategy = 0; // ByName
        bool defaultEnumIgnoreCase = false;

        // 1. Assembly-level defaults via [MapperDefaults]
        var assemblyDefaults = compilation.Assembly.GetAttributes().FirstOrDefault(a =>
        {
            var name = RoslynInvariants.GetAttributeClassName(a);
            return name is "MapperDefaultsAttribute";
        });

        if (assemblyDefaults != null)
        {
            foreach (var namedArg in assemblyDefaults.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "StrictMapping" when namedArg.Value.Value is bool s:
                        isStrict = s;
                        break;
                    case "EnumMappingStrategy" when namedArg.Value.Value is int es:
                        defaultEnumStrategy = es;
                        break;
                    case "EnumIgnoreCase" when namedArg.Value.Value is bool eic:
                        defaultEnumIgnoreCase = eic;
                        break;
                }
            }
        }

        // 2. Class-level / Interface-level [Mapper(StrictMapping = ...)]
        var mapperAttr = mapperSymbol.GetAttributes().FirstOrDefault(a =>
        {
            var name = RoslynInvariants.GetAttributeClassName(a);
            return name is "MapperAttribute";
        });
        if (mapperAttr != null)
        {
            foreach (var namedArg in mapperAttr.NamedArguments)
            {
                if (namedArg.Key == "StrictMapping" && namedArg.Value.Value is bool strictVal)
                {
                    isStrict = strictVal;
                }
            }
        }

        // Class-level [EnumMappingStrategy]
        int classEnumStrategy = defaultEnumStrategy;
        bool classEnumIgnoreCase = defaultEnumIgnoreCase;
        var classEnumAttr = mapperSymbol.GetAttributes().FirstOrDefault(a =>
        {
            var name = RoslynInvariants.GetAttributeClassName(a);
            return name is "EnumMappingStrategyAttribute";
        });
        if (classEnumAttr != null)
        {
            if (classEnumAttr.ConstructorArguments[0].Value is int csVal)
            {
                classEnumStrategy = csVal;
            }
            foreach (var named in classEnumAttr.NamedArguments)
            {
                if (named.Key == "IgnoreCase" && named.Value.Value is bool ic)
                {
                    classEnumIgnoreCase = ic;
                }
            }
        }

        var allMethods = mapperSymbol.GetMembers().OfType<IMethodSymbol>().Where(m => m.IsPartialDefinition && m.Parameters.Length == 1).ToList();
        var methodMappings = new List<Models.MethodMapping>();

        foreach (var methodSymbol in allMethods)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sourceType = methodSymbol.Parameters[0].Type;
            var targetType = methodSymbol.ReturnType;

            if (sourceType.SpecialType != SpecialType.None || targetType.SpecialType != SpecialType.None) continue;

            var customMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var ignoredDestinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var ignoredSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var nullFallbacks = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var mapValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var explicitEnumValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var derivedTypeMappings = new List<Models.DerivedTypeMapping>();
            string? customConverter = null;
            string? customConverterField = null;

            int methodEnumStrategy = classEnumStrategy;
            bool methodEnumIgnoreCase = classEnumIgnoreCase;

            foreach (var attr in methodSymbol.GetAttributes())
            {
                var attrName = RoslynInvariants.GetAttributeClassName(attr);
                if (attrName is "MapPropertyAttribute")
                {
                    if (attr.ConstructorArguments[0].Value is string src &&
                        attr.ConstructorArguments[1].Value is string dst)
                    {
                        // SEM-003 fix: Detect duplicate [MapProperty] destination to prevent silent overwrite.
                        if (customMappings.ContainsKey(dst))
                        {
                            var attrLocation = attr.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                                ?? methodSymbol.Locations.FirstOrDefault()
                                ?? Location.None;
                            diagnostics.Add(CreateDiagnostic(
                                DiagnosticDescriptors.DuplicateMapPropertyDestination,
                                attrLocation,
                                dst, src));
                        }
                        customMappings[dst] = src;
                    }
                }
                else if (attrName is "MapIgnoreAttribute")
                {
                    if (attr.ConstructorArguments[0].Value is string dst)
                    {
                        ignoredDestinations.Add(dst);
                    }
                }
                else if (attrName is "MapIgnoreSourceAttribute")
                {
                    if (attr.ConstructorArguments[0].Value is string src)
                    {
                        ignoredSources.Add(src);
                    }
                }
                else if (attrName is "MapNullFallbackAttribute")
                {
                    if (attr.ConstructorArguments[0].Value is string dst &&
                        attr.ConstructorArguments[1].Value is string fallback)
                    {
                        nullFallbacks[dst] = fallback;
                    }
                }
                else if (attrName is "MapValueAttribute")
                {
                    if (attr.ConstructorArguments[0].Value is string dst &&
                        attr.ConstructorArguments[1].Value is string valExpr)
                    {
                        mapValues[dst] = valExpr;
                    }
                }
                else if (attrName is "EnumMappingStrategyAttribute")
                {
                    if (attr.ConstructorArguments[0].Value is int sVal)
                    {
                        methodEnumStrategy = sVal;
                    }
                    foreach (var named in attr.NamedArguments)
                    {
                        if (named.Key == "IgnoreCase" && named.Value.Value is bool ic)
                        {
                            methodEnumIgnoreCase = ic;
                        }
                    }
                }
                else if (attrName is "MapEnumValueAttribute")
                {
                    var srcVal = attr.ConstructorArguments[0].Value;
                    var dstVal = attr.ConstructorArguments[1].Value;
                    var srcT = attr.ConstructorArguments[0].Type;
                    var dstT = attr.ConstructorArguments[1].Type;

                    string? srcMemberName = null;
                    string? dstMemberName = null;

                    if (srcT is INamedTypeSymbol srcEnumNamed && srcVal != null)
                    {
                        srcMemberName = srcEnumNamed.GetMembers().OfType<IFieldSymbol>()
                            .FirstOrDefault(f => f.HasConstantValue && object.Equals(f.ConstantValue, srcVal))?.Name;
                    }
                    if (dstT is INamedTypeSymbol dstEnumNamed && dstVal != null)
                    {
                        dstMemberName = dstEnumNamed.GetMembers().OfType<IFieldSymbol>()
                            .FirstOrDefault(f => f.HasConstantValue && object.Equals(f.ConstantValue, dstVal))?.Name;
                    }

                    if (srcMemberName == null && srcVal is string sStr) srcMemberName = sStr;
                    if (dstMemberName == null && dstVal is string dStr) dstMemberName = dStr;

                    if (srcMemberName != null && dstMemberName != null)
                    {
                        explicitEnumValues[srcMemberName] = dstMemberName;
                    }
                }
                else if (attrName is "MapDerivedTypeAttribute")
                {
                    if (attr.ConstructorArguments[0].Value is ITypeSymbol srcType &&
                        attr.ConstructorArguments[1].Value is ITypeSymbol dstType)
                    {
                        var matchingMethod = allMethods.FirstOrDefault(m => SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, srcType) && SymbolEqualityComparer.Default.Equals(m.ReturnType, dstType));
                        derivedTypeMappings.Add(new Models.DerivedTypeMapping(srcType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), dstType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), matchingMethod?.Name));
                    }
                }
                else if (attrName is "UseConverterAttribute")
                {
                    if (attr.ConstructorArguments[0].Value is ITypeSymbol converterType)
                    {
                        bool implementsConverter = converterType.AllInterfaces.Any(i =>
                            i.Name == "IConverter" &&
                            i.TypeArguments.Length == 2 &&
                            SymbolEqualityComparer.Default.Equals(i.TypeArguments[0], sourceType) &&
                            SymbolEqualityComparer.Default.Equals(i.TypeArguments[1], targetType));

                        if (!implementsConverter)
                        {
                            var location = attr.ApplicationSyntaxReference != null
                                ? attr.ApplicationSyntaxReference.GetSyntax().GetLocation()
                                : RoslynInvariants.GetLocation(methodSymbol);
                            diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.InvalidConverterType, location, converterType.ToDisplayString(), sourceType.ToDisplayString(), targetType.ToDisplayString()));
                        }
                        else
                        {
                            customConverter = converterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        }
                    }
                    else if (attr.ConstructorArguments[0].Value is string fieldName)
                    {
                        customConverterField = fieldName;
                    }
                }
            }

            derivedTypeMappings = derivedTypeMappings.OrderBy(x => x.SourceType).ThenBy(x => x.TargetType).ToList();
            if (derivedTypeMappings.Count > 0 && (targetType.IsAbstract || targetType.TypeKind == TypeKind.Interface))
            {
                diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.AbstractBaseIncompletePolymorphism, RoslynInvariants.GetLocation(methodSymbol), targetType.ToDisplayString()));
                methodMappings.Add(new Models.MethodMapping(
                    methodSymbol.Name,
                    CreateTypeReference(sourceType),
                    CreateTypeReference(targetType),
                    new Models.ConstructionStrategy.ObjectInitializer(),
                    ImmutableArray<Models.MemberMapping>.Empty.AsEquatableArray(),
                    isStrict,
                    derivedTypeMappings.AsEquatableArray()));
                continue;
            }

            if (customConverter != null || customConverterField != null)
            {
                methodMappings.Add(new Models.MethodMapping(
                    methodSymbol.Name,
                    CreateTypeReference(sourceType),
                    CreateTypeReference(targetType),
                    new Models.ConstructionStrategy.ObjectInitializer(),
                    ImmutableArray<Models.MemberMapping>.Empty.AsEquatableArray(),
                    isStrict,
                    derivedTypeMappings.AsEquatableArray(),
                    customConverter,
                    customConverterField));
                continue;
            }

            // Standalone Enum-to-Enum mapping method
            if (sourceType.TypeKind == TypeKind.Enum && targetType.TypeKind == TypeKind.Enum)
            {
                var strategy = ConversionStrategyFactory.GetConversionStrategy(
                    sourceType, targetType, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol),
                    methodSymbol.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues);

                methodMappings.Add(new Models.MethodMapping(
                    methodSymbol.Name,
                    CreateTypeReference(sourceType),
                    CreateTypeReference(targetType),
                    new Models.ConstructionStrategy.ObjectInitializer(),
                    new[] { new Models.MemberMapping("", "", strategy) }.AsEquatableArray(),
                    isStrict,
                    derivedTypeMappings.AsEquatableArray()));
                continue;
            }

            Models.ConstructionStrategy constructionStrategy = new Models.ConstructionStrategy.ObjectInitializer();

            var constructors = targetType is INamedTypeSymbol namedTargetType
                ? namedTargetType.Constructors.Where(c => c.DeclaredAccessibility == Accessibility.Public).ToList()
                : new List<IMethodSymbol>();

            var factories = new List<IMethodSymbol>();
            string? specifiedFactoryName = null;
            if (targetType is INamedTypeSymbol ntt)
            {
                var factoryAttr = methodSymbol.GetAttributes().FirstOrDefault(a => RoslynInvariants.GetAttributeClassName(a) is "MapFactoryAttribute");
                if (factoryAttr != null && factoryAttr.ConstructorArguments.Length == 1 && factoryAttr.ConstructorArguments[0].Value is string factoryName)
                {
                    specifiedFactoryName = factoryName;
                    factories = ntt.GetMembers().OfType<IMethodSymbol>()
                        .Where(m => m.IsStatic && m.DeclaredAccessibility == Accessibility.Public && m.Name == factoryName && SymbolEqualityComparer.Default.Equals(m.ReturnType, targetType))
                        .ToList();

                    // FIX-A (DIAG-007): [MapFactory] references a factory method that does not exist.
                    // Previously this was silently ignored, causing the generator to fall through to
                    // constructors. Now we emit ELM017 (Error) so the developer is aware immediately.
                    if (factories.Count == 0)
                    {
                        var factoryAttrLocation = factoryAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation()
                            ?? RoslynInvariants.GetLocation(methodSymbol);
                        diagnostics.Add(CreateDiagnostic(
                            DiagnosticDescriptors.MapFactoryMethodNotFound,
                            factoryAttrLocation,
                            factoryName,
                            targetType.ToDisplayString()));
                        continue;
                    }
                }
            }

            var targetProperties = MemberResolutionEngine.GetAllProperties(targetType)
                .Where(p => p.Name != "EqualityContract" && !MemberResolutionEngine.HasMapperIgnore(p) && (p.SetMethod?.DeclaredAccessibility == Accessibility.Public || p.IsReadOnly))
                .OrderBy(p => p.Name)
                .ToList();

            var sourceProperties = MemberResolutionEngine.GetAllProperties(sourceType)
                .Where(p => !MemberResolutionEngine.HasMapperIgnore(p))
                .ToList();

            var paramlessCtor = constructors.FirstOrDefault(c => c.Parameters.Length == 0);

            if (factories.Count > 0)
            {
                var factory = factories
                    .OrderByDescending(c => c.Parameters.Length)
                    .ThenBy(c => c.ToDisplayString())
                    .ToList()[0];
                var paramMappings = new List<Models.ParameterMapping>();

                foreach (var param in factory.Parameters)
                {
                    if (ignoredDestinations.Contains(param.Name)) continue;

                    if (mapValues.TryGetValue(param.Name, out var mapVal))
                    {
                        paramMappings.Add(new Models.ParameterMapping(param.Name, param.Name, new Models.ConversionStrategy.DirectAssignment(), CustomValueExpression: mapVal));
                        continue;
                    }

                    string expectedSourceName = customMappings.TryGetValue(param.Name, out var customSrc) ? customSrc : param.Name;

                    if (expectedSourceName.Contains('.'))
                    {
                        var pathRes = MemberResolutionEngine.ResolvePropertyPath(sourceType, expectedSourceName, methodSymbol, param.Name, diagnostics);
                        if (pathRes != null)
                        {
                            bool isSourceNullable = pathRes.IsPathNullable || IsNullableType(pathRes.LeafType);
                            bool isTargetNullable = IsNullableType(param.Type);

                            if (!isTargetNullable && isSourceNullable)
                            {
                                if (nullFallbacks.TryGetValue(param.Name, out var fallback))
                                {
                                    var strat = ConversionStrategyFactory.GetConversionStrategy(pathRes.LeafType, param.Type, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol), param.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues);
                                    paramMappings.Add(new Models.ParameterMapping(pathRes.FormattedPath, param.Name, strat, fallback, isSourceNullable, isTargetNullable, CustomSourceExpression: $"source.{pathRes.FormattedPath}"));
                                    continue;
                                }
                                else
                                {
                                    diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.NullabilityMismatch, RoslynInvariants.GetLocation(methodSymbol), param.Name, pathRes.LeafType.ToDisplayString(), param.Type.ToDisplayString()));
                                }
                            }

                            var strategy = ConversionStrategyFactory.GetConversionStrategy(pathRes.LeafType, param.Type, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol), param.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues);
                            if (strategy is Models.ConversionStrategy.Unsupported)
                            {
                                diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.UnsupportedConversion, RoslynInvariants.GetLocation(methodSymbol), pathRes.LeafType.ToDisplayString(), param.Type.ToDisplayString(), param.Name));
                            }
                            nullFallbacks.TryGetValue(param.Name, out string? fallbackVal);
                            paramMappings.Add(new Models.ParameterMapping(pathRes.FormattedPath, param.Name, strategy, fallbackVal, isSourceNullable, isTargetNullable, CustomSourceExpression: $"source.{pathRes.FormattedPath}"));
                        }
                        else if (isStrict)
                        {
                            diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.UnmappedDestinationMember, RoslynInvariants.GetLocation(methodSymbol), param.Name));
                        }
                        continue;
                    }

                    var sourceProp = MemberResolutionEngine.MatchProperty(sourceProperties, expectedSourceName, methodSymbol, param.Name, diagnostics);
                    if (sourceProp != null)
                    {
                        bool isSourceNullable = IsNullableType(sourceProp.Type);
                        bool isTargetNullable = IsNullableType(param.Type);

                        if (!isTargetNullable && isSourceNullable)
                        {
                            if (nullFallbacks.TryGetValue(param.Name, out var fallback))
                            {
                                paramMappings.Add(new Models.ParameterMapping(sourceProp.Name, param.Name, ConversionStrategyFactory.GetConversionStrategy(sourceProp.Type, param.Type, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol), param.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues), fallback, isSourceNullable, isTargetNullable));
                                continue;
                            }
                            else
                            {
                                diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.NullabilityMismatch, RoslynInvariants.GetLocation(methodSymbol), param.Name, sourceProp.Type.ToDisplayString(), param.Type.ToDisplayString()));
                            }
                        }

                        var strategy = ConversionStrategyFactory.GetConversionStrategy(sourceProp.Type, param.Type, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol), param.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues);
                        if (strategy is Models.ConversionStrategy.Unsupported)
                        {
                            diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.UnsupportedConversion, RoslynInvariants.GetLocation(methodSymbol), sourceProp.Type.ToDisplayString(), param.Type.ToDisplayString(), param.Name));
                        }
                        nullFallbacks.TryGetValue(param.Name, out string? fallbackVal);
                        paramMappings.Add(new Models.ParameterMapping(sourceProp.Name, param.Name, strategy, fallbackVal, isSourceNullable, isTargetNullable));
                    }
                    else if (isStrict)
                    {
                        diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.UnmappedDestinationMember, RoslynInvariants.GetLocation(methodSymbol), param.Name));
                    }
                }
                constructionStrategy = new Models.ConstructionStrategy.FactoryMethod(factory.Name, paramMappings.AsEquatableArray());

                var factoryParamNames = new HashSet<string>(factory.Parameters.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);
                targetProperties = targetProperties.Where(p => !factoryParamNames.Contains(p.Name)).ToList();
            }
            else if (paramlessCtor == null && !targetType.IsValueType && constructors.Count > 1)
            {
                diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.AmbiguousConstructor, RoslynInvariants.GetLocation(methodSymbol), targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
                continue;
            }
            else if (paramlessCtor == null && constructors.Count > 0 && !targetType.IsValueType)
            {
                var ctor = constructors[0];
                var paramMappings = new List<Models.ParameterMapping>();

                foreach (var param in ctor.Parameters)
                {
                    if (ignoredDestinations.Contains(param.Name)) continue;

                    if (mapValues.TryGetValue(param.Name, out var mapVal))
                    {
                        paramMappings.Add(new Models.ParameterMapping(param.Name, param.Name, new Models.ConversionStrategy.DirectAssignment(), CustomValueExpression: mapVal));
                        continue;
                    }

                    string expectedSourceName = customMappings.TryGetValue(param.Name, out var customSrc) ? customSrc : param.Name;

                    if (expectedSourceName.Contains('.'))
                    {
                        var pathRes = MemberResolutionEngine.ResolvePropertyPath(sourceType, expectedSourceName, methodSymbol, param.Name, diagnostics);
                        if (pathRes != null)
                        {
                            bool isSourceNullable = pathRes.IsPathNullable || IsNullableType(pathRes.LeafType);
                            bool isTargetNullable = IsNullableType(param.Type);

                            if (!isTargetNullable && isSourceNullable)
                            {
                                if (nullFallbacks.TryGetValue(param.Name, out var fallback))
                                {
                                    var strat = ConversionStrategyFactory.GetConversionStrategy(pathRes.LeafType, param.Type, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol), param.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues);
                                    paramMappings.Add(new Models.ParameterMapping(pathRes.FormattedPath, param.Name, strat, fallback, isSourceNullable, isTargetNullable, CustomSourceExpression: $"source.{pathRes.FormattedPath}"));
                                    continue;
                                }
                                else
                                {
                                    diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.NullabilityMismatch, RoslynInvariants.GetLocation(methodSymbol), param.Name, pathRes.LeafType.ToDisplayString(), param.Type.ToDisplayString()));
                                }
                            }

                            var strategy = ConversionStrategyFactory.GetConversionStrategy(pathRes.LeafType, param.Type, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol), param.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues);
                            if (strategy is Models.ConversionStrategy.Unsupported)
                            {
                                diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.UnsupportedConversion, RoslynInvariants.GetLocation(methodSymbol), pathRes.LeafType.ToDisplayString(), param.Type.ToDisplayString(), param.Name));
                            }
                            nullFallbacks.TryGetValue(param.Name, out string? fallbackVal);
                            paramMappings.Add(new Models.ParameterMapping(pathRes.FormattedPath, param.Name, strategy, fallbackVal, isSourceNullable, isTargetNullable, CustomSourceExpression: $"source.{pathRes.FormattedPath}"));
                        }
                        else if (isStrict)
                        {
                            diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.UnmappedDestinationMember, RoslynInvariants.GetLocation(methodSymbol), param.Name));
                        }
                        continue;
                    }

                    var sourceProp = MemberResolutionEngine.MatchProperty(sourceProperties, expectedSourceName, methodSymbol, param.Name, diagnostics);
                    if (sourceProp != null)
                    {
                        bool isSourceNullable = IsNullableType(sourceProp.Type);
                        bool isTargetNullable = IsNullableType(param.Type);

                        if (!isTargetNullable && isSourceNullable)
                        {
                            if (nullFallbacks.TryGetValue(param.Name, out var fallback))
                            {
                                paramMappings.Add(new Models.ParameterMapping(sourceProp.Name, param.Name, ConversionStrategyFactory.GetConversionStrategy(sourceProp.Type, param.Type, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol), param.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues), fallback, isSourceNullable, isTargetNullable));
                                continue;
                            }
                            else
                            {
                                diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.NullabilityMismatch, RoslynInvariants.GetLocation(methodSymbol), param.Name, sourceProp.Type.ToDisplayString(), param.Type.ToDisplayString()));
                            }
                        }

                        var strategy = ConversionStrategyFactory.GetConversionStrategy(sourceProp.Type, param.Type, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol), param.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues);
                        if (strategy is Models.ConversionStrategy.Unsupported)
                        {
                            diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.UnsupportedConversion, RoslynInvariants.GetLocation(methodSymbol), sourceProp.Type.ToDisplayString(), param.Type.ToDisplayString(), param.Name));
                        }
                        nullFallbacks.TryGetValue(param.Name, out string? fallbackVal);
                        paramMappings.Add(new Models.ParameterMapping(sourceProp.Name, param.Name, strategy, fallbackVal, isSourceNullable, isTargetNullable));
                    }
                    else if (isStrict)
                    {
                        diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.UnmappedDestinationMember, RoslynInvariants.GetLocation(methodSymbol), param.Name));
                    }
                }
                constructionStrategy = new Models.ConstructionStrategy.ParameterizedConstructor(paramMappings.AsEquatableArray());

                var ctorParamNames = new HashSet<string>(ctor.Parameters.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);
                targetProperties = targetProperties.Where(p => !ctorParamNames.Contains(p.Name)).ToList();
            }
            else if (paramlessCtor == null && !targetType.IsValueType && targetType.TypeKind != TypeKind.Interface)
            {
                diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.MissingFactoryOrConstructor, RoslynInvariants.GetLocation(methodSymbol), targetType.ToDisplayString()));
            }

            var memberMappings = new List<Models.MemberMapping>();
            foreach (var targetProp in targetProperties)
            {
                if (ignoredDestinations.Contains(targetProp.Name)) continue;
                if (targetProp.SetMethod?.DeclaredAccessibility != Accessibility.Public && !targetProp.IsReadOnly) continue;

                if (mapValues.TryGetValue(targetProp.Name, out var mapVal))
                {
                    memberMappings.Add(new Models.MemberMapping(targetProp.Name, targetProp.Name, new Models.ConversionStrategy.DirectAssignment(), CustomValueExpression: mapVal));
                    continue;
                }

                string expectedSourceName = customMappings.TryGetValue(targetProp.Name, out var customSrc) ? customSrc : targetProp.Name;

                if (ignoredSources.Contains(expectedSourceName)) continue;

                if (expectedSourceName.Contains('.'))
                {
                    var pathRes = MemberResolutionEngine.ResolvePropertyPath(sourceType, expectedSourceName, methodSymbol, targetProp.Name, diagnostics);
                    if (pathRes != null)
                    {
                        bool isSourceNullable = pathRes.IsPathNullable || IsNullableType(pathRes.LeafType);
                        bool isTargetNullable = IsNullableType(targetProp.Type);

                        if (!isTargetNullable && isSourceNullable)
                        {
                            if (nullFallbacks.TryGetValue(targetProp.Name, out var fallback))
                            {
                                var fallbackStrategy = ConversionStrategyFactory.GetConversionStrategy(pathRes.LeafType, targetProp.Type, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol), targetProp.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues);
                                memberMappings.Add(new Models.MemberMapping(pathRes.FormattedPath, targetProp.Name, fallbackStrategy, fallback, isSourceNullable, isTargetNullable, CustomSourceExpression: $"source.{pathRes.FormattedPath}"));
                                continue;
                            }
                            else
                            {
                                diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.NullabilityMismatch, RoslynInvariants.GetLocation(methodSymbol), targetProp.Name, pathRes.LeafType.ToDisplayString(), targetProp.Type.ToDisplayString()));
                            }
                        }

                        var strategy = ConversionStrategyFactory.GetConversionStrategy(pathRes.LeafType, targetProp.Type, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol), targetProp.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues);
                        if (strategy is Models.ConversionStrategy.Unsupported)
                        {
                            diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.UnsupportedConversion, RoslynInvariants.GetLocation(methodSymbol), pathRes.LeafType.ToDisplayString(), targetProp.Type.ToDisplayString(), targetProp.Name));
                        }
                        nullFallbacks.TryGetValue(targetProp.Name, out string? fallbackVal);
                        memberMappings.Add(new Models.MemberMapping(pathRes.FormattedPath, targetProp.Name, strategy, fallbackVal, isSourceNullable, isTargetNullable, CustomSourceExpression: $"source.{pathRes.FormattedPath}"));
                    }
                    else if (isStrict)
                    {
                        diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.UnmappedDestinationMember, RoslynInvariants.GetLocation(methodSymbol), targetProp.Name));
                    }
                    continue;
                }

                var sourceProp = MemberResolutionEngine.MatchProperty(sourceProperties, expectedSourceName, methodSymbol, targetProp.Name, diagnostics);
                if (sourceProp != null)
                {
                    bool isSourceNullable = IsNullableType(sourceProp.Type);
                    bool isTargetNullable = IsNullableType(targetProp.Type);

                    if (!isTargetNullable && isSourceNullable)
                    {
                        if (nullFallbacks.TryGetValue(targetProp.Name, out var fallback))
                        {
                            var fallbackStrategy = ConversionStrategyFactory.GetConversionStrategy(sourceProp.Type, targetProp.Type, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol), targetProp.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues);
                            memberMappings.Add(new Models.MemberMapping(sourceProp.Name, targetProp.Name, fallbackStrategy, fallback, isSourceNullable, isTargetNullable));
                            continue;
                        }
                        else
                        {
                            diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.NullabilityMismatch, RoslynInvariants.GetLocation(methodSymbol), targetProp.Name, sourceProp.Type.ToDisplayString(), targetProp.Type.ToDisplayString()));
                        }
                    }

                    var strategy = ConversionStrategyFactory.GetConversionStrategy(sourceProp.Type, targetProp.Type, allMethods, diagnostics, RoslynInvariants.GetLocation(methodSymbol), targetProp.Name, isStrict, methodEnumStrategy, methodEnumIgnoreCase, explicitEnumValues);
                    if (strategy is Models.ConversionStrategy.Unsupported)
                    {
                        diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.UnsupportedConversion, RoslynInvariants.GetLocation(methodSymbol), sourceProp.Type.ToDisplayString(), targetProp.Type.ToDisplayString(), targetProp.Name));
                    }
                    nullFallbacks.TryGetValue(targetProp.Name, out string? fallbackVal);
                    memberMappings.Add(new Models.MemberMapping(sourceProp.Name, targetProp.Name, strategy, fallbackVal, isSourceNullable, isTargetNullable));
                }
                else if (isStrict)
                {
                    diagnostics.Add(CreateDiagnostic(DiagnosticDescriptors.UnmappedDestinationMember, RoslynInvariants.GetLocation(methodSymbol), targetProp.Name));
                }
            }

            var methodMapping = new Models.MethodMapping(
                methodSymbol.Name,
                CreateTypeReference(sourceType),
                CreateTypeReference(targetType),
                constructionStrategy,
                memberMappings.AsEquatableArray(),
                isStrict,
                derivedTypeMappings.AsEquatableArray(),
                customConverter);

            methodMappings.Add(methodMapping);
        }

        CycleDetector.DetectCycles(methodMappings, diagnostics, RoslynInvariants.GetLocation(mapperSymbol));

        return new Models.TypeMapping(
            mapperSymbol.ContainingNamespace.IsGlobalNamespace ? "" : (RoslynInvariants.GetContainingNamespace(mapperSymbol) ?? ""),
            mapperSymbol.Name,
            mapperSymbol.IsStatic,
            methodMappings.AsEquatableArray(),
            diagnostics.AsEquatableArray());
    }

    internal static Models.TypeReference CreateTypeReference(ITypeSymbol type) =>
        new(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            type.NullableAnnotation == NullableAnnotation.Annotated,
            type.IsAbstract,
            type.IsValueType);

    internal static bool IsNullableType(ITypeSymbol type)
    {
        if (type.NullableAnnotation == NullableAnnotation.Annotated) return true;
        if (type is INamedTypeSymbol named && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T) return true;
        return false;
    }

    private static void Execute(Models.TypeMapping typeMapping, SourceProductionContext context)
    {
        EmitTypeMapping(typeMapping, context.ReportDiagnostic, context.AddSource, context.CancellationToken);
    }

    internal static void EmitTypeMapping(
        Models.TypeMapping typeMapping,
        Action<Diagnostic> reportDiagnostic,
        Action<string, SourceText> addSource,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool hasErrors = false;
        foreach (var diag in typeMapping.Diagnostics)
        {
            var linePosition = new LinePosition(diag.Line, diag.Column);
            Location location = Location.Create(diag.FilePath ?? "", new Microsoft.CodeAnalysis.Text.TextSpan(), new LinePositionSpan(linePosition, linePosition));

            var descriptor = new DiagnosticDescriptor(diag.Id, diag.Title, diag.MessageFormat, diag.Category, (DiagnosticSeverity)diag.DefaultSeverity, diag.IsEnabledByDefault);
            var diagnostic = Diagnostic.Create(descriptor, location, diag.Args.ToArray());
            reportDiagnostic(diagnostic);
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                hasErrors = true;
            }
        }
        if (hasErrors)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        string sourceCode = CodeEmitter.GenerateSourceCode(typeMapping);
        // GEN-003 fix: Include namespace in file name to prevent collision when two mappers
        // share the same simple class name but live in different namespaces.
        string safeClassName = CodeEmitter.EscapeIdentifier(typeMapping.ClassName).Replace("@", "");
        string sourceFileName = string.IsNullOrWhiteSpace(typeMapping.Namespace)
            ? $"{safeClassName}.g.cs"
            : $"{CodeEmitter.EscapeIdentifier(typeMapping.Namespace).Replace("@", "").Replace(".", "_")}_{safeClassName}.g.cs";
        addSource(sourceFileName, SourceText.From(sourceCode, Encoding.UTF8));
    }
}
