// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using Xunit;

namespace EricksonLopez.Mapper.Abstractions.Tests;

/// <summary>
/// Contains unit tests verifying the contracts and implementation behavior of <see cref="IConverter{TSource, TDestination}"/>.
/// Adheres strictly to ADR-021 (Method_Scenario_Result).
/// </summary>
public class ConverterTests
{
    private sealed class SampleConverter : IConverter<int, string>
    {
        public string Convert(int source) => source.ToString();
    }

    private sealed class ReverseStringConverter : IConverter<string, string>
    {
        public string Convert(string source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            char[] chars = source.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }
    }

    [Fact]
    public void IConverter_WhenDirectImplementationCalled_ShouldConvertSourceToDestination()
    {
        // Arrange
        IConverter<int, string> converter = new SampleConverter();

        // Act
        var result = converter.Convert(42);

        // Assert
        result.Should().Be("42");
    }

    [Fact]
    public void IConverter_WhenCustomImplementationExecuted_ShouldTransformPayloadAccordingToContract()
    {
        // Arrange
        IConverter<string, string> converter = new ReverseStringConverter();

        // Act
        var result = converter.Convert("hello");

        // Assert
        result.Should().Be("olleh");

        Action act = () => converter.Convert(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }
}
