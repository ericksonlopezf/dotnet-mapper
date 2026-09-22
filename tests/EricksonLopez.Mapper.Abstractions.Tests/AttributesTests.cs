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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MapperAttribute_WhenStrictMappingSet_ShouldReturnAssignedValue(bool strict)
    {
        // Act
        var sut = new MapperAttribute { StrictMapping = strict };

        // Assert
        sut.StrictMapping.Should().Be(strict);
    }

    [Property]
    public bool MapperAttribute_WhenArbitraryStrictMappingSet_ShouldRoundtripProperty(bool value)
    {
        var sut = new MapperAttribute { StrictMapping = value };
        return sut.StrictMapping == value;
    }

    [Fact]
    public void MapPropertyAttribute_WhenConstructorCalled_ShouldSetSourceAndDestinationNames()
    {
        // Act
        var sut = new MapPropertyAttribute("SourceProp", "DestProp");

        // Assert
        sut.SourceName.Should().Be("SourceProp");
        sut.DestinationName.Should().Be("DestProp");
    }

    [Property]
    public bool MapPropertyAttribute_WhenArbitraryStringsProvided_ShouldSetProperties(NonNull<string> source, NonNull<string> destination)
    {
        var sut = new MapPropertyAttribute(source.Get, destination.Get);
        return sut.SourceName == source.Get && sut.DestinationName == destination.Get;
    }

    [Fact]
    public void MapIgnoreAttribute_WhenConstructorCalled_ShouldSetDestinationName()
    {
        // Act
        var sut = new MapIgnoreAttribute("IgnoredProperty");

        // Assert
        sut.DestinationName.Should().Be("IgnoredProperty");
    }

    [Property]
    public bool MapIgnoreAttribute_WhenArbitraryStringProvided_ShouldSetDestinationName(NonNull<string> destination)
    {
        var sut = new MapIgnoreAttribute(destination.Get);
        return sut.DestinationName == destination.Get;
    }

    [Fact]
    public void MapIgnoreSourceAttribute_WhenConstructorCalled_ShouldSetSourceName()
    {
        // Act
        var sut = new MapIgnoreSourceAttribute("IgnoredSourceProperty");

        // Assert
        sut.SourceName.Should().Be("IgnoredSourceProperty");
    }

    [Property]
    public bool MapIgnoreSourceAttribute_WhenArbitraryStringProvided_ShouldSetSourceName(NonNull<string> source)
    {
        var sut = new MapIgnoreSourceAttribute(source.Get);
        return sut.SourceName == source.Get;
    }

    [Fact]
    public void MapDerivedTypeAttribute_WhenConstructorCalled_ShouldSetTypes()
    {
        // Arrange
        var sourceType = typeof(string);
        var targetType = typeof(object);

        // Act
        var sut = new MapDerivedTypeAttribute(sourceType, targetType);

        // Assert
        sut.SourceType.Should().Be(sourceType);
        sut.TargetType.Should().Be(targetType);
    }

    [Fact]
    public void MapFactoryAttribute_WhenConstructorCalled_ShouldSetMethodName()
    {
        // Act
        var sut = new MapFactoryAttribute("CreateInstance");

        // Assert
        sut.MethodName.Should().Be("CreateInstance");
    }

    [Property]
    public bool MapFactoryAttribute_WhenArbitraryStringProvided_ShouldSetMethodName(NonNull<string> methodName)
    {
        var sut = new MapFactoryAttribute(methodName.Get);
        return sut.MethodName == methodName.Get;
    }

    [Fact]
    public void MapNullFallbackAttribute_WhenConstructorCalled_ShouldSetProperties()
    {
        // Act
        var sut = new MapNullFallbackAttribute("DisplayName", "\"N/A\"");

        // Assert
        sut.DestinationName.Should().Be("DisplayName");
        sut.FallbackExpression.Should().Be("\"N/A\"");
    }

    [Property]
    public bool MapNullFallbackAttribute_WhenArbitraryStringsProvided_ShouldSetProperties(
        NonNull<string> destination,
        NonNull<string> fallback)
    {
        var sut = new MapNullFallbackAttribute(destination.Get, fallback.Get);
        return sut.DestinationName == destination.Get && sut.FallbackExpression == fallback.Get;
    }

    [Fact]
    public void UseConverterAttribute_WhenConstructorWithConverterTypeCalled_ShouldSetConverterType()
    {
        // Act
        var sut = new UseConverterAttribute(typeof(IConverter<int, string>));

        // Assert
        sut.ConverterType.Should().Be(typeof(IConverter<int, string>));
        sut.ConverterFieldName.Should().BeNull();
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(IConverter<int, string>))]
    public void UseConverterAttribute_WhenConstructedWithType_ShouldSetConverterType(Type converterType)
    {
        // Act
        var sut = new UseConverterAttribute(converterType);

        // Assert
        sut.ConverterType.Should().Be(converterType);
        sut.ConverterFieldName.Should().BeNull();
    }

    [Fact]
    public void MapValueAttribute_WhenConstructorCalled_ShouldSetProperties()
    {
        // Act
        var sut = new MapValueAttribute("CreatedAt", "DateTime.UtcNow");

        // Assert
        sut.DestinationName.Should().Be("CreatedAt");
        sut.ValueExpression.Should().Be("DateTime.UtcNow");
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
