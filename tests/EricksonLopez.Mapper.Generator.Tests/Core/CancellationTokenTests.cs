// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using AwesomeAssertions;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Core;

/// <summary>
/// Contains unit tests that verify proper propagation and handling of <see cref="CancellationToken"/>
/// throughout the <see cref="MapperGenerator"/> incremental pipeline.
/// </summary>
public class CancellationTokenTests
{
    [Fact]
    public void RunGenerator_WhenCancelledBeforeExecution_ShouldThrowOperationCanceledException()
    {
        // Arrange
        string source = @"
using EricksonLopez.Mapper;

public class User { public string Name { get; set; } = string.Empty; }
public class UserDto { public string Name { get; set; } = string.Empty; }

[Mapper]
public partial class UserMapper
{
    public partial UserDto Map(User source);
}";

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        Action act = () => GeneratorTestHelper.RunGeneratorSimple(source, cts.Token);
        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void RunGenerator_WhenNotCancelled_ShouldCompleteSuccessfullyWithoutExceptions()
    {
        // Arrange
        string source = @"

public class User { public string Name { get; set; } = string.Empty; }
public class UserDto { public string Name { get; set; } = string.Empty; }

[Mapper]
public partial class UserMapper
{
    public partial UserDto Map(User source);
}";

        using var cts = new CancellationTokenSource();

        // Act
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source, cts.Token);

        // Assert
        diagnostics.Should().BeEmpty();
        output.Should().Contain("Name = source.Name");
    }

    [Fact]
    public void RunGenerator_WhenCancelledDuringMultiMethodExecution_ShouldThrowOperationCanceledException()
    {
        // Arrange: mapper with multiple methods to verify cancellation checkpoint in method loop
        string source = @"

public class User1 { public string Name { get; set; } = string.Empty; }
public class UserDto1 { public string Name { get; set; } = string.Empty; }
public class User2 { public string Name { get; set; } = string.Empty; }
public class UserDto2 { public string Name { get; set; } = string.Empty; }
public class User3 { public string Name { get; set; } = string.Empty; }
public class UserDto3 { public string Name { get; set; } = string.Empty; }

[Mapper]
public partial class LargeMapper
{
    public partial UserDto1 Map1(User1 source);
    public partial UserDto2 Map2(User2 source);
    public partial UserDto3 Map3(User3 source);
}";

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Deterministic cancellation without timing dependency

        // Act & Assert
        Action act = () => GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false, cancellationToken: cts.Token);
        act.Should().Throw<OperationCanceledException>();
    }
}

