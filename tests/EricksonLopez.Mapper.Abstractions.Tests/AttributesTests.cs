// Copyright © Erickson Lopez. MIT License.
using System;
using AutoFixture.Xunit2;
using AwesomeAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace EricksonLopez.Mapper.Abstractions.Tests;

/// <summary>
/// Contains unit tests and property-based tests verifying default values, constructors,
/// properties, and invariants of the mapper configuration attributes in <see cref="EricksonLopez.Mapper"/>.
/// Adheres strictly to ADR-021 (Method_Scenario_Result).
/// </summary>
public class AttributesTests
{
    [Fact]
    public void MapperAttribute_WhenDefaultConstructorCalled_ShouldHaveStrictMappingTrue()
    {
        // Act
        var sut = new MapperAttribute();

        // Assert
        sut.StrictMapping.Should().BeTrue();
    }

    [Property]
    public bool MapperAttribute_WhenArbitraryStrictMappingSet_ShouldRoundtripProperty(bool value)
    {
        var sut = new MapperAttribute { StrictMapping = value };
        return sut.StrictMapping == value;
    }

    [Property]
    public bool MapPropertyAttribute_WhenArbitraryStringsProvided_ShouldSetProperties(NonNull<string> source, NonNull<string> destination)
    {
        var sut = new MapPropertyAttribute(source.Get, destination.Get);
        return sut.SourceName == source.Get && sut.DestinationName == destination.Get;
    }

    [Property]
    public bool MapIgnoreAttribute_WhenArbitraryStringProvided_ShouldSetDestinationName(NonNull<string> destination)
    {
        var sut = new MapIgnoreAttribute(destination.Get);
        return sut.DestinationName == destination.Get;
    }

    [Property]
    public bool MapIgnoreSourceAttribute_WhenArbitraryStringProvided_ShouldSetSourceName(NonNull<string> source)
    {
        var sut = new MapIgnoreSourceAttribute(source.Get);
        return sut.SourceName == source.Get;
    }

    [Theory]
    [AutoData]
    public void MapDerivedTypeAttribute_WhenConstructorCalled_ShouldSetTypes(
        Type sourceType,
        Type destinationType)
    {
        // Act
        var sut = new MapDerivedTypeAttribute(sourceType, destinationType);

        // Assert
        sut.SourceType.Should().Be(sourceType);
        sut.TargetType.Should().Be(destinationType);
    }

    [Property]
    public bool MapFactoryAttribute_WhenArbitraryStringProvided_ShouldSetMethodName(NonNull<string> methodName)
    {
        var sut = new MapFactoryAttribute(methodName.Get);
        return sut.MethodName == methodName.Get;
    }

    [Property]
    public bool MapNullFallbackAttribute_WhenArbitraryStringsProvided_ShouldSetProperties(
        NonNull<string> destination,
        NonNull<string> fallback)
    {
        var sut = new MapNullFallbackAttribute(destination.Get, fallback.Get);
        return sut.DestinationName == destination.Get && sut.FallbackExpression == fallback.Get;
    }

    [Theory]
    [AutoData]
    public void UseConverterAttribute_WhenConstructorWithConverterTypeCalled_ShouldSetConverterType(
        Type converterType)
    {
        // Act
        var sut = new UseConverterAttribute(converterType);

        // Assert
        sut.ConverterType.Should().Be(converterType);
        sut.ConverterFieldName.Should().BeNull();
    }

    [Property]
    public bool UseConverterAttribute_WhenArbitraryStringProvided_ShouldSetConverterFieldName(NonNull<string> fieldName)
    {
        var sut = new UseConverterAttribute(fieldName.Get);
        return sut.ConverterFieldName == fieldName.Get && sut.ConverterType == null;
    }

    [Fact]
    public void UseConverterAttribute_WhenConstructedWithFieldName_ShouldHaveNullConverterTypeAndExpectedFieldName()
    {
        // Act
        var sut = new UseConverterAttribute("ConverterInstance");

        // Assert
        sut.ConverterFieldName.Should().Be("ConverterInstance");
        sut.ConverterType.Should().BeNull();
    }

    [Fact]
    public void GenerateMapperRegistrationAttribute_WhenDefaultConstructorCalled_ShouldInstantiateWithExpectedMetadata()
    {
        // Act
        var sut = new GenerateMapperRegistrationAttribute();

        // Assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<GenerateMapperRegistrationAttribute>();
        sut.TypeId.Should().Be(typeof(GenerateMapperRegistrationAttribute));

        var usage = (AttributeUsageAttribute?)Attribute.GetCustomAttribute(typeof(GenerateMapperRegistrationAttribute), typeof(AttributeUsageAttribute));
        usage.Should().NotBeNull();
        usage!.ValidOn.Should().Be(AttributeTargets.Assembly);
        usage.Inherited.Should().BeFalse();
        usage.AllowMultiple.Should().BeFalse();
    }

    [Fact]
    public void ValueObjectAttribute_WhenDefaultConstructorCalled_ShouldInstantiateWithExpectedMetadata()
    {
        // Act
        var sut = new ValueObjectAttribute();

        // Assert
        sut.Should().NotBeNull();
        sut.Should().BeOfType<ValueObjectAttribute>();
        sut.TypeId.Should().Be(typeof(ValueObjectAttribute));

        var usage = (AttributeUsageAttribute?)Attribute.GetCustomAttribute(typeof(ValueObjectAttribute), typeof(AttributeUsageAttribute));
        usage.Should().NotBeNull();
        usage!.ValidOn.Should().Be(AttributeTargets.Class | AttributeTargets.Struct);
        usage.Inherited.Should().BeFalse();
        usage.AllowMultiple.Should().BeFalse();
    }

    [Fact]
    public void MapperDefaultsAttribute_WhenDefaultConstructorCalled_ShouldHaveExpectedDefaults()
    {
        // Act
        var sut = new MapperDefaultsAttribute();

        // Assert
        sut.EnumMappingStrategy.Should().Be(EnumMappingStrategy.ByName);
        sut.EnumIgnoreCase.Should().BeFalse();
        sut.StrictMapping.Should().BeTrue();
    }

    [Property]
    public bool MapperDefaultsAttribute_WhenPropertiesSet_ShouldRoundtripProperties(
        bool enumIgnoreCase,
        bool strictMapping,
        bool useByValue)
    {
        var strategy = useByValue ? EnumMappingStrategy.ByValue : EnumMappingStrategy.ByName;
        var sut = new MapperDefaultsAttribute
        {
            EnumMappingStrategy = strategy,
            EnumIgnoreCase = enumIgnoreCase,
            StrictMapping = strictMapping
        };

        return sut.EnumMappingStrategy == strategy &&
               sut.EnumIgnoreCase == enumIgnoreCase &&
               sut.StrictMapping == strictMapping;
    }

    [Theory]
    [InlineData(EnumMappingStrategy.ByName, false)]
    [InlineData(EnumMappingStrategy.ByName, true)]
    [InlineData(EnumMappingStrategy.ByValue, false)]
    [InlineData(EnumMappingStrategy.ByValue, true)]
    public void EnumMappingStrategyAttribute_WhenConstructorCalledAndPropertiesSet_ShouldHaveExpectedValues(
        EnumMappingStrategy strategy,
        bool ignoreCase)
    {
        // Act
        var sut = new EnumMappingStrategyAttribute(strategy)
        {
            IgnoreCase = ignoreCase
        };

        // Assert
        sut.Strategy.Should().Be(strategy);
        sut.IgnoreCase.Should().Be(ignoreCase);
    }

    [Fact]
    public void EnumMappingStrategyAttribute_WhenConstructedWithStrategy_ShouldDefaultIgnoreCaseToFalse()
    {
        // Act
        var sut = new EnumMappingStrategyAttribute(EnumMappingStrategy.ByValue);

        // Assert
        sut.Strategy.Should().Be(EnumMappingStrategy.ByValue);
        sut.IgnoreCase.Should().BeFalse();
    }

    [Theory]
    [InlineData("SourceA", "TargetA")]
    [InlineData(1, 10)]
    public void MapEnumValueAttribute_WhenConstructorCalled_ShouldSetSourceAndTarget(object source, object target)
    {
        // Act
        var sut = new MapEnumValueAttribute(source, target);

        // Assert
        sut.Source.Should().Be(source);
        sut.Target.Should().Be(target);
    }

    [Fact]
    public void MapperIgnoreAttribute_WhenDefaultConstructorCalled_ShouldInstantiate()
    {
        // Act
        var sut = new MapperIgnoreAttribute();

        // Assert
        sut.Should().NotBeNull();
    }

    [Property]
    public bool MapValueAttribute_WhenArbitraryStringsProvided_ShouldSetProperties(
        NonNull<string> destinationName,
        NonNull<string> valueExpression)
    {
        var sut = new MapValueAttribute(destinationName.Get, valueExpression.Get);
        return sut.DestinationName == destinationName.Get && sut.ValueExpression == valueExpression.Get;
    }

    [Theory]
    [InlineData(typeof(MapperAttribute), AttributeTargets.Class | AttributeTargets.Interface, false, false)]
    [InlineData(typeof(MapPropertyAttribute), AttributeTargets.Method, false, true)]
    [InlineData(typeof(MapIgnoreAttribute), AttributeTargets.Method, false, true)]
    [InlineData(typeof(MapIgnoreSourceAttribute), AttributeTargets.Method, false, true)]
    [InlineData(typeof(MapFactoryAttribute), AttributeTargets.Method, false, false)]
    [InlineData(typeof(MapDerivedTypeAttribute), AttributeTargets.Method, false, true)]
    [InlineData(typeof(UseConverterAttribute), AttributeTargets.Method, false, false)]
    [InlineData(typeof(MapNullFallbackAttribute), AttributeTargets.Method, false, true)]
    [InlineData(typeof(GenerateMapperRegistrationAttribute), AttributeTargets.Assembly, false, false)]
    [InlineData(typeof(ValueObjectAttribute), AttributeTargets.Class | AttributeTargets.Struct, false, false)]
    [InlineData(typeof(MapperDefaultsAttribute), AttributeTargets.Assembly, false, false)]
    [InlineData(typeof(EnumMappingStrategyAttribute), AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, false, false)]
    [InlineData(typeof(MapEnumValueAttribute), AttributeTargets.Method, false, true)]
    [InlineData(typeof(MapperIgnoreAttribute), AttributeTargets.Property | AttributeTargets.Field, false, false)]
    [InlineData(typeof(MapValueAttribute), AttributeTargets.Method, false, true)]
    public void AttributeUsage_WhenInspected_ShouldHaveExpectedTargetsAndFlags(
        Type attributeType,
        AttributeTargets expectedTargets,
        bool expectedInherited,
        bool expectedAllowMultiple)
    {
        var usage = (AttributeUsageAttribute?)Attribute.GetCustomAttribute(attributeType, typeof(AttributeUsageAttribute));
        usage.Should().NotBeNull();
        usage!.ValidOn.Should().Be(expectedTargets);
        usage.Inherited.Should().Be(expectedInherited);
        usage.AllowMultiple.Should().Be(expectedAllowMultiple);
    }
}
