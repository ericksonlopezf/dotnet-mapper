// Copyright © Erickson Lopez. MIT License.
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

public class MemberIgnoreTests
{
    [Fact]
    public void MapperIgnore_WhenOnTargetProperty_ShouldSkipMappingAndAvoidELM001()
    {
        string source = @"
public class Source { public int Id { get; set; } }
public class Target
{
    public int Id { get; set; }
    [MapperIgnore]
    public string InternalData { get; set; }
}

[Mapper]
public partial class UserMapper
{
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Id = source.Id");
        output.Should().NotContain("InternalData");
    }

    [Fact]
    public void MapperIgnore_WhenOnSourceProperty_ShouldBeExcludedFromMatching()
    {
        string source = @"
public class Source
{
    public int Id { get; set; }
    [MapperIgnore]
    public string Name { get; set; }
}
public class Target
{
    public int Id { get; set; }
    public string Name { get; set; }
}

[Mapper(StrictMapping = false)]
public partial class UserMapper
{
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().NotContain("Name = source.Name");
    }
}

