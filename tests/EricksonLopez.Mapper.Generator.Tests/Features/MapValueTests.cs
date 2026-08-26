// Copyright © Erickson Lopez. MIT License.
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

public class MapValueTests
{
    [Fact]
    public void MapValue_WithConstantExpression_ShouldEmitDirectValue()
    {
        string source = @"
public class Source { public int Id { get; set; } }
public class Target
{
    public int Id { get; set; }
    public string Status { get; set; }
}

[Mapper]
public partial class UserMapper
{
    [MapValue(""Status"", ""\""ACTIVE\"""")]
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Status = \"ACTIVE\"");
    }

    [Fact]
    public void MapValue_WithComputedExpression_ShouldEmitExpressionDirectly()
    {
        string source = @"
public class Source { public int Id { get; set; } }
public class Target
{
    public int Id { get; set; }
    public System.DateTime CreatedAt { get; set; }
}

[Mapper]
public partial class UserMapper
{
    [MapValue(""CreatedAt"", ""System.DateTime.UtcNow"")]
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("CreatedAt = System.DateTime.UtcNow");
    }

    [Fact]
    public void MapValue_IntoConstructorParameter_ShouldPassExpressionToConstructor()
    {
        string source = @"
public class Source { public int Id { get; set; } }
public class Target
{
    public int Id { get; }
    public string Role { get; }
    public Target(int id, string role) { Id = id; Role = role; }
}

[Mapper]
public partial class UserMapper
{
    [MapValue(""role"", ""\""Admin\"""")]
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::TestNamespace.Target(source.Id, \"Admin\")");
    }
}
