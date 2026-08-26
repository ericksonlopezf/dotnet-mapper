// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using EricksonLopez.Mapper.Generator.Models;

namespace EricksonLopez.Mapper.Generator.Tests.Infrastructure;

/// <summary>
/// Fluent test data builders for synthesizing AST model instances in unit tests.
/// Decouples test code from positional constructor signatures of internal records.
/// </summary>
internal static class TestDataBuilders
{
    public static MethodMappingBuilder CreateMethod(string methodName = "Map") => new(methodName);
    public static TypeMappingBuilder CreateType(string className = "MyMapper") => new(className);
    public static MemberMappingBuilder CreateMember(string sourceName, string targetName) => new(sourceName, targetName);
    public static ParameterMappingBuilder CreateParameter(string sourceName, string targetName) => new(sourceName, targetName);
    public static MappingGraphBuilder CreateGraph() => new();
    public static TypeReference CreateTypeRef(string name, bool isNullable = false, bool isAbstract = false, bool isValueType = false)
        => new(name, isNullable, isAbstract, isValueType);

    public static class CreateStrategies
    {
        public static ConversionStrategy Direct() => new ConversionStrategy.DirectAssignment();

        public static ConversionStrategy Invocation(string methodName, bool isSourceNullable = false, bool isTargetNullable = false, string? methodKey = null)
            => new ConversionStrategy.MapMethodInvocation(methodName, isSourceNullable, isTargetNullable, methodKey ?? "");

        public static ConversionStrategy Enumerable(
            ConversionStrategy elementStrategy,
            string sourceItem = "SrcItem",
            string targetItem = "TgtItem",
            bool isArray = false,
            bool isList = true,
            bool isImmutableArray = false,
            bool sourceIsSpan = false,
            bool sourceHasCount = false,
            bool isHashSet = false,
            bool isImmutableList = false,
            bool isFrozenSet = false,
            bool sourceIsValueType = false)
            => new ConversionStrategy.EnumerableMapping(
                elementStrategy,
                sourceItem,
                targetItem,
                isArray,
                isList,
                isImmutableArray,
                sourceIsSpan,
                sourceHasCount,
                isHashSet,
                isImmutableList,
                isFrozenSet,
                sourceIsValueType);

        public static ConversionStrategy Dictionary(
            ConversionStrategy keyStrategy,
            ConversionStrategy valueStrategy,
            string srcKey = "SrcKey",
            string tgtKey = "TgtKey",
            string srcVal = "SrcVal",
            string tgtVal = "TgtVal",
            bool sourceHasCount = false)
            => new ConversionStrategy.DictionaryMapping(keyStrategy, valueStrategy, srcKey, tgtKey, srcVal, tgtVal, sourceHasCount);

        public static ConversionStrategy ValueObject(
            int pattern,
            ConversionStrategy innerStrategy,
            string srcVo = "SrcVo",
            string tgtVo = "TgtVo")
            => new ConversionStrategy.ValueObjectMapping(pattern, innerStrategy, srcVo, tgtVo);
    }
}



internal sealed class MethodMappingBuilder
{
    private string _methodName;
    private TypeReference _sourceType = new("Source", false, false, false);
    private TypeReference _targetType = new("Dest", false, false, false);
    private ConstructionStrategy _construction = new ConstructionStrategy.ObjectInitializer();
    private readonly List<MemberMapping> _members = new();
    private bool _isStrict = true;
    private readonly List<DerivedTypeMapping> _derivedTypes = new();
    private string? _customConverter;
    private string? _customConverterField;

    public MethodMappingBuilder(string methodName = "Map")
    {
        _methodName = methodName;
    }

    public MethodMappingBuilder WithName(string name) { _methodName = name; return this; }
    public MethodMappingBuilder WithSourceType(string name, bool isNullable = false, bool isAbstract = false, bool isValueType = false)
    {
        _sourceType = new TypeReference(name, isNullable, isAbstract, isValueType);
        return this;
    }
    public MethodMappingBuilder WithSourceType(TypeReference typeRef) { _sourceType = typeRef; return this; }

    public MethodMappingBuilder WithTargetType(string name, bool isNullable = false, bool isAbstract = false, bool isValueType = false)
    {
        _targetType = new TypeReference(name, isNullable, isAbstract, isValueType);
        return this;
    }
    public MethodMappingBuilder WithTargetType(TypeReference typeRef) { _targetType = typeRef; return this; }

    public MethodMappingBuilder WithConstruction(ConstructionStrategy construction) { _construction = construction; return this; }
    public MethodMappingBuilder WithParameterizedConstructor(params ParameterMapping[] parameters)
    {
        _construction = new ConstructionStrategy.ParameterizedConstructor(parameters.AsEquatableArray());
        return this;
    }
    public MethodMappingBuilder WithFactoryMethod(string factoryMethodName, params ParameterMapping[] parameters)
    {
        _construction = new ConstructionStrategy.FactoryMethod(factoryMethodName, parameters.AsEquatableArray());
        return this;
    }

    public MethodMappingBuilder AddMember(MemberMapping member) { _members.Add(member); return this; }
    public MethodMappingBuilder AddMember(string sourceName, string targetName, ConversionStrategy strategy, string? fallback = null, bool isSourceNullable = false, bool isTargetNullable = false)
    {
        _members.Add(new MemberMapping(sourceName, targetName, strategy, fallback, isSourceNullable, isTargetNullable));
        return this;
    }
    public MethodMappingBuilder WithMembers(IEnumerable<MemberMapping> members)
    {
        _members.Clear();
        _members.AddRange(members);
        return this;
    }

    public MethodMappingBuilder WithStrict(bool isStrict) { _isStrict = isStrict; return this; }
    public MethodMappingBuilder AddDerivedType(string sourceType, string targetType, string? methodName = null)
    {
        _derivedTypes.Add(new DerivedTypeMapping(sourceType, targetType, methodName));
        return this;
    }

    public MethodMappingBuilder WithCustomConverter(string converterName) { _customConverter = converterName; return this; }
    public MethodMappingBuilder WithCustomConverterField(string fieldName) { _customConverterField = fieldName; return this; }

    public MethodMapping Build()
    {
        return new MethodMapping(
            _methodName,
            _sourceType,
            _targetType,
            _construction,
            _members.AsEquatableArray(),
            _isStrict,
            _derivedTypes.AsEquatableArray(),
            _customConverter,
            _customConverterField);
    }

    public static implicit operator MethodMapping(MethodMappingBuilder builder) => builder.Build();
}

internal sealed class TypeMappingBuilder
{
    private string _namespace = "TestNamespace";
    private string _className;
    private bool _isStatic;
    private readonly List<MethodMapping> _methods = new();
    private readonly List<DiagnosticInfo> _diagnostics = new();

    public TypeMappingBuilder(string className = "MyMapper")
    {
        _className = className;
    }

    public TypeMappingBuilder WithNamespace(string ns) { _namespace = ns; return this; }
    public TypeMappingBuilder WithClassName(string name) { _className = name; return this; }
    public TypeMappingBuilder WithStatic(bool isStatic = true) { _isStatic = isStatic; return this; }

    public TypeMappingBuilder AddMethod(MethodMapping method) { _methods.Add(method); return this; }
    public TypeMappingBuilder WithMethods(IEnumerable<MethodMapping> methods)
    {
        _methods.Clear();
        _methods.AddRange(methods);
        return this;
    }

    public TypeMappingBuilder AddDiagnostic(DiagnosticInfo diagnostic) { _diagnostics.Add(diagnostic); return this; }

    public TypeMapping Build()
    {
        return new TypeMapping(
            _namespace,
            _className,
            _isStatic,
            _methods.AsEquatableArray(),
            _diagnostics.AsEquatableArray());
    }

    public static implicit operator TypeMapping(TypeMappingBuilder builder) => builder.Build();
}

internal abstract class BaseMemberMappingBuilder<TBuilder, TResult>
    where TBuilder : BaseMemberMappingBuilder<TBuilder, TResult>
{
    protected readonly string _sourceName;
    protected readonly string _targetName;
    protected ConversionStrategy _strategy = new ConversionStrategy.DirectAssignment();
    protected string? _fallback;
    protected bool _isSourceNullable;
    protected bool _isTargetNullable;

    protected BaseMemberMappingBuilder(string sourceName, string targetName)
    {
        _sourceName = sourceName;
        _targetName = targetName;
    }

    public TBuilder WithStrategy(ConversionStrategy strategy) { _strategy = strategy; return (TBuilder)this; }
    public TBuilder WithFallback(string fallback) { _fallback = fallback; return (TBuilder)this; }
    public TBuilder WithSourceNullable(bool isNullable = true) { _isSourceNullable = isNullable; return (TBuilder)this; }
    public TBuilder WithTargetNullable(bool isNullable = true) { _isTargetNullable = isNullable; return (TBuilder)this; }

    public abstract TResult Build();
}

internal sealed class MemberMappingBuilder : BaseMemberMappingBuilder<MemberMappingBuilder, MemberMapping>
{
    public MemberMappingBuilder(string sourceName, string targetName) : base(sourceName, targetName) { }

    public override MemberMapping Build() =>
        new(_sourceName, _targetName, _strategy, _fallback, _isSourceNullable, _isTargetNullable);

    public static implicit operator MemberMapping(MemberMappingBuilder builder) => builder.Build();
}

internal sealed class ParameterMappingBuilder : BaseMemberMappingBuilder<ParameterMappingBuilder, ParameterMapping>
{
    public ParameterMappingBuilder(string sourceName, string targetName) : base(sourceName, targetName) { }

    public override ParameterMapping Build() =>
        new(_sourceName, _targetName, _strategy, _fallback, _isSourceNullable, _isTargetNullable);

    public static implicit operator ParameterMapping(ParameterMappingBuilder builder) => builder.Build();
}

internal sealed class MappingGraphBuilder
{
    private readonly List<MethodMapping> _methods = new();

    public MappingGraphBuilder AddNode(string methodName, string sourceTypeName, string targetTypeName, params string[] targetsInvoked)
    {
        var builder = new MethodMappingBuilder(methodName)
            .WithSourceType(sourceTypeName)
            .WithTargetType(targetTypeName);

        if (targetsInvoked == null || targetsInvoked.Length == 0)
        {
            builder.AddMember("Direct", "Direct", new ConversionStrategy.DirectAssignment());
        }
        else
        {
            for (int i = 0; i < targetsInvoked.Length; i++)
            {
                builder.AddMember($"Prop{i}", $"Prop{i}", new ConversionStrategy.MapMethodInvocation(targetsInvoked[i], false, false));
            }
        }

        _methods.Add(builder.Build());
        return this;
    }

    public MappingGraphBuilder AddTerminal(string methodName, string sourceTypeName = "Src", string targetTypeName = "Tgt")
    {
        return AddNode(methodName, sourceTypeName, targetTypeName);
    }

    public List<MethodMapping> Build() => _methods;

    public static implicit operator List<MethodMapping>(MappingGraphBuilder builder) => builder.Build();
}


