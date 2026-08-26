// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using AwesomeAssertions;
using EricksonLopez.Mapper.Generator;
using EricksonLopez.Mapper.Generator.Models;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

[Trait("Category", "FastAst")]
public class ModelsTests
{
    [Fact]
    public void TypeMapping_WhenInstantiated_ShouldExposeExpectedProperties()
    {
        var typeMapping = new TypeMapping("MyNamespace", "MyClass", true, EquatableArray<MethodMapping>.Empty, EquatableArray<DiagnosticInfo>.Empty);
        typeMapping.Namespace.Should().Be("MyNamespace");
        typeMapping.ClassName.Should().Be("MyClass");
        typeMapping.IsStatic.Should().BeTrue();
        typeMapping.Methods.Should().BeEmpty();
        typeMapping.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void TypeMapping_WhenDeconstructed_ShouldReturnCorrectValues()
    {
        var typeMapping = new TypeMapping("MyNamespace", "MyClass", true, EquatableArray<MethodMapping>.Empty, EquatableArray<DiagnosticInfo>.Empty);
        var (ns, name, isStatic, methods, diags) = typeMapping;
        ns.Should().Be("MyNamespace");
        name.Should().Be("MyClass");
        isStatic.Should().BeTrue();
        methods.Should().BeEmpty();
        diags.Should().BeEmpty();
    }

    [Fact]
    public void DiagnosticInfo_WhenInstantiated_ShouldExposeExpectedProperties()
    {
        var diag = new DiagnosticInfo("ELM001", "Title", "Format", "Cat", 3, true, "File.cs", 10, 5, EquatableArray<string>.Empty);
        diag.Id.Should().Be("ELM001");
        diag.Title.Should().Be("Title");
        diag.MessageFormat.Should().Be("Format");
        diag.Category.Should().Be("Cat");
        diag.DefaultSeverity.Should().Be(3);
        diag.IsEnabledByDefault.Should().BeTrue();
        diag.FilePath.Should().Be("File.cs");
        diag.Line.Should().Be(10);
        diag.Column.Should().Be(5);
        diag.Args.Should().BeEmpty();
    }

    [Fact]
    public void DerivedTypeMapping_WhenInstantiated_ShouldExposeExpectedProperties()
    {
        var derived = new DerivedTypeMapping("SourceChild", "DestChild", "MapChild");
        derived.SourceType.Should().Be("SourceChild");
        derived.TargetType.Should().Be("DestChild");
        derived.MethodName.Should().Be("MapChild");
    }

    [Fact]
    public void MethodMapping_WhenInstantiated_ShouldExposeExpectedProperties()
    {
        var sourceRef = new TypeReference("Source", false, false, false);
        var targetRef = new TypeReference("Dest", false, false, false);
        var method = new MethodMapping(
            "Map",
            sourceRef,
            targetRef,
            new ConstructionStrategy.ObjectInitializer(),
            EquatableArray<MemberMapping>.Empty,
            true,
            EquatableArray<DerivedTypeMapping>.Empty,
            "CustomConv");

        method.MethodName.Should().Be("Map");
        method.SourceType.Should().Be(sourceRef);
        method.TargetType.Should().Be(targetRef);
        method.Construction.Should().BeOfType<ConstructionStrategy.ObjectInitializer>();
        method.IsStrict.Should().BeTrue();
        method.CustomConverter.Should().Be("CustomConv");
    }

    [Fact]
    public void ConstructionStrategy_WhenSubtypesInstantiated_ShouldExposeExpectedProperties()
    {
        var pCtor = new ConstructionStrategy.ParameterizedConstructor(EquatableArray<ParameterMapping>.Empty);
        pCtor.Parameters.Should().BeEmpty();

        var objInit = new ConstructionStrategy.ObjectInitializer();
        objInit.Should().NotBeNull();

        var factory = new ConstructionStrategy.FactoryMethod("Create", EquatableArray<ParameterMapping>.Empty);
        factory.MethodName.Should().Be("Create");
        factory.Parameters.Should().BeEmpty();

        var unsupp = new ConstructionStrategy.Unsupported("No constructor");
        unsupp.Reason.Should().Be("No constructor");
    }

    [Fact]
    public void TypeReference_WhenInstantiated_ShouldExposeExpectedProperties()
    {
        var typeRef = new TypeReference("System.String", true, false, false);
        typeRef.FullyQualifiedName.Should().Be("System.String");
        typeRef.IsNullable.Should().BeTrue();
        typeRef.IsAbstract.Should().BeFalse();
        typeRef.IsValueType.Should().BeFalse();
    }

    [Fact]
    public void MemberMapping_WhenInstantiated_ShouldExposeExpectedProperties()
    {
        var member = new MemberMapping("SrcProp", "TgtProp", new ConversionStrategy.DirectAssignment(), "defaultVal", true, true);
        member.SourceName.Should().Be("SrcProp");
        member.TargetName.Should().Be("TgtProp");
        member.Strategy.Should().BeOfType<ConversionStrategy.DirectAssignment>();
        member.Fallback.Should().Be("defaultVal");
        member.IsSourceNullable.Should().BeTrue();
        member.IsTargetNullable.Should().BeTrue();
    }

    [Fact]
    public void ParameterMapping_WhenInstantiated_ShouldExposeExpectedProperties()
    {
        var param = new ParameterMapping("srcArg", "tgtArg", new ConversionStrategy.DirectAssignment(), "null", false, false);
        param.SourceName.Should().Be("srcArg");
        param.TargetName.Should().Be("tgtArg");
        param.Strategy.Should().BeOfType<ConversionStrategy.DirectAssignment>();
        param.Fallback.Should().Be("null");
        param.IsSourceNullable.Should().BeFalse();
        param.IsTargetNullable.Should().BeFalse();
    }

    [Fact]
    public void ConversionStrategy_WhenAllSubtypesInstantiated_ShouldExposeExpectedProperties()
    {
        var direct = new ConversionStrategy.DirectAssignment();
        direct.Should().NotBeNull();

        var explicitCast = new ConversionStrategy.ExplicitCast();
        explicitCast.Should().NotBeNull();

        var custom = new ConversionStrategy.CustomMethod("ConvertCustom");
        custom.MethodName.Should().Be("ConvertCustom");

        var mapMethod = new ConversionStrategy.MapMethodInvocation("MapInner", true, true, "MapKey");
        mapMethod.MethodName.Should().Be("MapInner");
        mapMethod.IsSourceNullable.Should().BeTrue();
        mapMethod.IsTargetNullable.Should().BeTrue();
        mapMethod.MethodKey.Should().Be("MapKey");

        var enumMap = new ConversionStrategy.EnumerableMapping(direct, "int", "long", true, false, false, true, true, false, false, false);
        enumMap.ElementStrategy.Should().Be(direct);
        enumMap.SourceElementType.Should().Be("int");
        enumMap.TargetElementType.Should().Be("long");
        enumMap.IsArray.Should().BeTrue();
        enumMap.SourceIsArray.Should().BeTrue();
        enumMap.SourceHasCount.Should().BeTrue();

        var dictMap = new ConversionStrategy.DictionaryMapping(direct, direct, "string", "string", "int", "int", true);
        dictMap.KeyStrategy.Should().Be(direct);
        dictMap.ValueStrategy.Should().Be(direct);
        dictMap.SourceKeyType.Should().Be("string");
        dictMap.TargetKeyType.Should().Be("string");
        dictMap.SourceValueType.Should().Be("int");
        dictMap.TargetValueType.Should().Be("int");
        dictMap.SourceHasCount.Should().BeTrue();

        var voMap = new ConversionStrategy.ValueObjectMapping(0, direct, "int", "UserId");
        voMap.Kind.Should().Be(0);
        voMap.InnerStrategy.Should().Be(direct);
        voMap.SourceInnerType.Should().Be("int");
        voMap.TargetInnerType.Should().Be("UserId");

        var builtin = new ConversionStrategy.BuiltinConversion("System.Convert.ToInt64({0})");
        builtin.ExpressionTemplate.Should().Be("System.Convert.ToInt64({0})");

        var enumToEnum = new ConversionStrategy.EnumToEnumMapping("TargetEnum", new EquatableArray<string>(ImmutableArray.Create("UnmappedVal")));
        enumToEnum.TargetEnumType.Should().Be("TargetEnum");
        enumToEnum.UnmappedSourceMembers.Should().ContainSingle().Which.Should().Be("UnmappedVal");

        var unsupported = new ConversionStrategy.Unsupported();
        unsupported.Should().NotBeNull();
    }
}


