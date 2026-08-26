// Copyright © Erickson Lopez. MIT License.
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

public class MapperDefaultsTests
{
    [Fact]
    public void MapperDefaults_WhenAssemblyLevelStrictFalse_ShouldInheritNonStrictByDefault()
    {
        string source = @"
[assembly: MapperDefaults(StrictMapping = false)]

public class Source { public int Id { get; set; } }
public class Target { public int Id { get; set; } public string Extra { get; set; } }

[Mapper]
public partial class UserMapper
{
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Id = source.Id");
    }

    [Fact]
    public void MapperDefaults_WhenClassLevelOverridesAssembly_ShouldApplyClassLevelSetting()
    {
        string source = @"
[assembly: MapperDefaults(StrictMapping = false)]

public class Source { public int Id { get; set; } }
public class Target { public int Id { get; set; } public string Extra { get; set; } }

[Mapper(StrictMapping = true)]
public partial class UserMapper
{
    public partial Target Map(Source source);
}
";
        var (diagnostics, _, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);

        diagnostics.Should().Contain(d => d.Id == "ELM001" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void MapperDefaults_WhenAssemblyLevelEnumMappingStrategyByValue_ShouldEmitCast()
    {
        string source = @"
[assembly: MapperDefaults(EnumMappingStrategy = EnumMappingStrategy.ByValue)]

public enum SourceEnum { A = 1, B = 2 }
public enum TargetEnum { A = 1, B = 2 }

[Mapper]
public partial class EnumMapper
{
    public partial TargetEnum Map(SourceEnum source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("(global::TestNamespace.TargetEnum)(source)");
    }

    [Fact]
    public void MapperDefaults_WhenAssemblyLevelEnumIgnoreCaseTrue_ShouldMatchCaseInsensitive()
    {
        string source = @"
[assembly: MapperDefaults(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumIgnoreCase = true)]

public enum SourceEnum { First = 1, Second = 2 }
public enum TargetEnum { FIRST = 1, SECOND = 2 }

[Mapper]
public partial class EnumMapper
{
    public partial TargetEnum Map(SourceEnum source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("SourceEnum.First => global::TestNamespace.TargetEnum.FIRST");
    }

    [Fact]
    public void MapperDefaults_WhenAssemblyLevelEnumIgnoreCaseOmitted_ShouldBeCaseSensitiveByDefault()
    {
        string source = @"
[assembly: MapperDefaults(EnumMappingStrategy = EnumMappingStrategy.ByName)]

public enum SourceEnum { First = 1, Second = 2 }
public enum TargetEnum { FIRST = 1, SECOND = 2 }

[Mapper]
public partial class EnumMapper
{
    public partial TargetEnum Map(SourceEnum source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: false);
        diagnostics.Should().Contain(d => d.Id == "ELM014");
    }
}

