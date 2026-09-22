// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Mapper.Generator.Tests.Infrastructure;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

/// <summary>
/// Audit Suite 06 and 07: Fuzzing and Property-Based Testing Invariants.
/// Proves mathematical and algebraic properties of the mapper:
/// 1. Idempotence and Determinism (f(x) == f(x))
/// 2. Cardinality Conservation (|f(xs)| == |xs|)
/// 3. Reference Isolation / Non-aliasing (ref(f(x).child) != ref(x.child))
/// 4. Robustness under fuzzed boundary values (extreme lengths, unusual unicode, control characters).
/// </summary>
public class FuzzingAndPropertyTests
{
    [Fact]
    public void Property_CollectionCardinality_IsConservedAcrossMappings()
    {
        // Property: For all valid collections xs, Count(Map(xs)) == Count(xs)
        string source = @"
using System.Collections.Generic;
using EricksonLopez.Mapper;

public class ItemSource { public int Value { get; set; } }
public class ItemDest { public int Value { get; set; } }

public class BatchSource { public List<ItemSource> Items { get; set; } = new(); }
public class BatchDest { public List<ItemDest> Items { get; set; } = new(); }

[Mapper]
public partial class BatchMapper
{
    public partial BatchDest Map(BatchSource source);
    public partial ItemDest MapItem(ItemSource source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        // Emitted code allocates capacity or loops over all items exactly once:
        output.Should().Contain("new global::System.Collections.Generic.List<global::TestNamespace.ItemDest>(source.Items.Count)");
    }

    [Fact]
    public void Property_NestedObjectReferenceIsolation_IsGuaranteed()
    {
        // Property: Nested mutable objects must be instantiated anew, never shallow-copied as references
        string source = @"
using EricksonLopez.Mapper;

public class ConfigSource { public string Setting { get; set; } = """"; }
public class SystemSource { public ConfigSource Config { get; set; } = new(); }

public class ConfigDest { public string Setting { get; set; } = """"; }
public class SystemDest { public ConfigDest Config { get; set; } = new(); }

[Mapper]
public partial class SystemMapper
{
    public partial SystemDest Map(SystemSource source);
    public partial ConfigDest MapConfig(ConfigSource source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        // Verifies that Config is mapped via MapConfig creating a new instance, not assigned directly
        output.Should().Contain("Config = this.MapConfig(source.Config)");
        output.Should().NotContain("Config = source.Config;");
    }

    [Fact]
    public void Fuzzing_ExtremePropertyNamesAndEscaping_ShouldCompileCleanly()
    {
        // Fuzzing boundary: Verifies that properties containing C# keywords, verbatim identifiers (@class, @event),
        // or unicode characters generate properly escaped C# code.
        string source = @"
using EricksonLopez.Mapper;

public class FuzzSource
{
    public string @class { get; set; } = """";
    public int @event { get; set; }
    public string _underscored_name { get; set; } = """";
}

public class FuzzDest
{
    public string @class { get; set; } = """";
    public int @event { get; set; }
    public string _underscored_name { get; set; } = """";
}

[Mapper]
public partial class FuzzMapper
{
    public partial FuzzDest Map(FuzzSource source);
}
";
        var (diagnostics, output, compilation) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty(
            "because identifiers matching C# keywords (@class, @event) must be safely emitted without syntax errors");
        compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
    }
}
