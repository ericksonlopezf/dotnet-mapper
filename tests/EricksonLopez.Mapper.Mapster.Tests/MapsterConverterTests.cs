// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using Mapster;
using Xunit;

namespace EricksonLopez.Mapper.Mapster.Tests;

/// <summary>
/// Contains unit tests verifying the Mapster adapter converter and fluent configuration extensions.
/// Adheres strictly to ADR-021 (UnitOfWork_StateUnderTest_ExpectedBehavior).
/// </summary>
public sealed class MapsterConverterTests
{
    public sealed record SourceDto(int Id, string Name, decimal Price);
    public sealed record DestViewModel(int Id, string FullName, decimal Price);

    [Fact]
    public void Constructor_WhenDefaultConstructorCalled_ShouldInitializeWithGlobalSettings()
    {
        // Arrange & Act
        var converter = new MapsterConverter<SourceDto, DestViewModel>();

        // Assert
        converter.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WhenNullConfigProvided_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        var act = () => new MapsterConverter<SourceDto, DestViewModel>(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Convert_WhenGlobalSettingsProvided_ShouldMapProperties()
    {
        // Arrange
        var converter = new MapsterConverter<SourceDto, DestViewModel>();
        var source = new SourceDto(42, "Item-A", 19.99m);

        // Act
        var result = converter.Convert(source);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(42);
        result.Price.Should().Be(19.99m);
    }

    [Fact]
    public void Convert_WhenCustomConfigProvided_ShouldMapConfiguredProperties()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        config.NewConfig<SourceDto, DestViewModel>()
            .Map(dest => dest.FullName, src => "Transformed: " + src.Name);

        var converter = new MapsterConverter<SourceDto, DestViewModel>(config);
        var source = new SourceDto(100, "Keyboard", 49.99m);

        // Act
        var result = converter.Convert(source);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(100);
        result.FullName.Should().Be("Transformed: Keyboard");
    }

    [Fact]
    public void Convert_WhenNullSourceProvided_ShouldReturnDefault()
    {
        // Arrange
        var converter = new MapsterConverter<SourceDto, DestViewModel>();

        // Act
        var result = converter.Convert(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Convert_WhenNullSourceAndCustomMappingConfigured_ShouldReturnDefaultWithoutInvokingMapsterEngine()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        config.NewConfig<SourceDto, DestViewModel>()
            .MapWith(src => SentinelMapping());

        var converter = new MapsterConverter<SourceDto, DestViewModel>(config);

        // Act
        var result = converter.Convert(null!);

        // Assert
        result.Should().BeNull();
    }

    private static DestViewModel SentinelMapping() => new(-999, "Sentinel", -999m);

    private sealed record UnmappedSource(string Value);
    private sealed record UnmappedDest(string Value);

    [Fact]
    public void Convert_WhenNullSourceAndUnmappedTypeWithExplicitMappingRequired_ShouldReturnDefault()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        config.RequireExplicitMapping = true;

        var converter = new MapsterConverter<UnmappedSource, UnmappedDest>(config);

        // Act
        var result = converter.Convert(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Convert_WhenNullableValueTypeIsNull_ShouldReturnDefault()
    {
        // Arrange
        var converter = new MapsterConverter<int?, int?>();

        // Act
        var result = converter.Convert(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Convert_WhenCustomConfigWithFailingMapping_ShouldPropagateException()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        config.NewConfig<SourceDto, DestViewModel>()
            .Map(dest => dest.FullName, src => ThrowingHelper(src.Name));

        var converter = new MapsterConverter<SourceDto, DestViewModel>(config);
        var source = new SourceDto(1, "Test", 10m);

        // Act & Assert
        var act = () => converter.Convert(source);
        act.Should().Throw<InvalidOperationException>().WithMessage("Mapster mapping failed internally");
    }

    private static string ThrowingHelper(string _) => throw new InvalidOperationException("Mapster mapping failed internally");

    [Fact]
    public void UseConverter_WhenConfigIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        TypeAdapterConfig config = null!;
        var customConverter = new CustomUpperConverter();

        // Act & Assert
        var act = () => config.UseConverter(customConverter);
        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void UseConverter_WhenConverterIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new TypeAdapterConfig();

        // Act & Assert
        var act = () => config.UseConverter<SourceDto, DestViewModel>(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("converter");
    }

    [Fact]
    public void UseConverter_WhenConfiguredOnMapster_ShouldAdaptSourceSuccessfully()
    {
        // Arrange
        var customConverter = new CustomUpperConverter();
        var config = new TypeAdapterConfig();
        config.UseConverter(customConverter);

        var source = new SourceDto(1, "abc", 10m);

        // Act
        var result = source.Adapt<DestViewModel>(config);

        // Assert
        result.FullName.Should().Be("ABC");
        result.Id.Should().Be(1);
        result.Price.Should().Be(10m);
    }

    [Fact]
    public void UseConverter_WhenConfiguredOnMapsterAndConverterThrows_ShouldPropagateException()
    {
        // Arrange
        var failingConverter = new FailingConverter();
        var config = new TypeAdapterConfig();
        config.UseConverter(failingConverter);

        var source = new SourceDto(1, "abc", 10m);

        // Act & Assert
        var act = () => source.Adapt<DestViewModel>(config);
        act.Should().Throw<InvalidOperationException>().WithMessage("Custom converter error");
    }

    [Fact]
    public void UseConverter_WhenInvoked_ShouldReturnSameConfigInstanceForChaining()
    {
        // Arrange
        var customConverter = new CustomUpperConverter();
        var config = new TypeAdapterConfig();

        // Act
        var returnedConfig = config.UseConverter(customConverter);

        // Assert
        returnedConfig.Should().BeSameAs(config);
    }

    private sealed class CustomUpperConverter : IConverter<SourceDto, DestViewModel>
    {
        public DestViewModel Convert(SourceDto source)
            => new(source.Id, source.Name.ToUpperInvariant(), source.Price);
    }

    private sealed class FailingConverter : IConverter<SourceDto, DestViewModel>
    {
        public DestViewModel Convert(SourceDto source)
            => throw new InvalidOperationException("Custom converter error");
    }
}
