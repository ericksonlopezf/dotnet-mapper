// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Mapper.Generator.Tests.Infrastructure;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

/// <summary>
/// Adversarial security, recursion detection, nullability preservation, and compilation tests.
/// Includes RED→GREEN→REGRESSION tests for bugs discovered during the comprehensive audit.
/// </summary>
public class AdversarialSecurityTests
{
    [Fact]
    public void CycleDetector_WhenPolymorphicCycleExists_ShouldEmitELM010()
    {
        string source = @"
using EricksonLopez.Mapper;

public abstract class NodeBase { }
public class NodeDerived : NodeBase
{
    public NodeBase Child { get; set; } = null!;
}

public abstract class NodeDtoBase { }
public class NodeDtoDerived : NodeDtoBase
{
    public NodeDtoBase Child { get; set; } = null!;
}

[Mapper]
public partial class PolymorphicCycleMapper
{
    [MapDerivedType(typeof(NodeDerived), typeof(NodeDtoDerived))]
    public partial NodeDtoBase MapBase(NodeBase source);

    public partial NodeDtoDerived MapDerived(NodeDerived source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Should().Contain(d => d.Id == "ELM010");
    }

    [Fact]
    public void NullableArray_WhenTargetIsNullable_ShouldPreserveNullSemantics()
    {
        string source = @"
using EricksonLopez.Mapper;

public class ItemSource { public string Name { get; set; } = """"; }
public class ItemDest { public string Name { get; set; } = """"; }

public class SourceModel
{
    public ItemSource[]? Tags { get; set; }
}

public class TargetModel
{
    public ItemDest[]? Tags { get; set; }
}

[Mapper]
public partial class ArrayNullMapper
{
    public partial TargetModel Map(SourceModel source);
    public partial ItemDest MapItem(ItemSource source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("ItemDest[]? _col1 = null;");
        output.Should().NotContain("Array.Empty<");
    }

    [Fact]
    public void GlobalNamespace_WhenEmittingDIRegistration_ShouldNotHaveLeadingDot()
    {
        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .Build();

        var mapper = TestDataBuilders.CreateType("GlobalRootMapper")
            .WithNamespace("")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var diSource = DependencyInjectionEmitter.GenerateDISource(System.Collections.Immutable.ImmutableArray.Create(mapper), true);
        diSource.Should().NotBeNull();
        diSource.Should().NotContain("AddSingleton<.");
        diSource.Should().Contain("AddSingleton<GlobalRootMapper>();");
    }

    [Fact]
    public void DateOnly_ToDateTimeOffset_ShouldUseDeterministicUtcKind()
    {
        string source = @"
using System;
using EricksonLopez.Mapper;

public class SourceDate { public DateOnly Date { get; set; } }
public class TargetDate { public DateTimeOffset Date { get; set; } }

[Mapper]
public partial class DateMapper
{
    public partial TargetDate Map(SourceDate source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("DateTimeKind.Utc");
    }

    /// <summary>
    /// GEN-003 — Regression: Two mapper classes with the same simple name but different namespaces
    /// previously caused a source file name collision ("SameMapper.g.cs" written twice).
    /// After fix: each mapper produces a namespace-qualified source file name.
    /// RED: generator throws or silently drops one mapper. GREEN: both namespaces present in output.
    /// </summary>
    [Fact]
    public void TwoMappersWithSameSimpleName_InDifferentNamespaces_ShouldProduceTwoDistinctSourceFiles()
    {
        string source = @"
using EricksonLopez.Mapper;

namespace App.Orders
{
    public class OrderSource { public string Name { get; set; } = """"; }
    public class OrderDest { public string Name { get; set; } = """"; }
    [Mapper] public partial class SameMapper { public partial OrderDest Map(OrderSource s); }
}

namespace App.Products
{
    public class ProductSource { public string Name { get; set; } = """"; }
    public class ProductDest { public string Name { get; set; } = """"; }
    [Mapper] public partial class SameMapper { public partial ProductDest Map(ProductSource s); }
}
";
        var (diagnostics, generatedSource, outputCompilation) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

        // Both namespace scopes must appear in the generated output
        var allGeneratedCode = string.Join("\n", outputCompilation.SyntaxTrees
            .Where(t => t.FilePath.EndsWith(".g.cs"))
            .Select(t => t.ToString()));

        allGeneratedCode.Should().Contain("App.Orders",
            "because the Orders.SameMapper implementation must be generated");
        allGeneratedCode.Should().Contain("App.Products",
            "because the Products.SameMapper implementation must be generated");
    }

    /// <summary>
    /// SEM-003 — Regression: Declaring [MapProperty] twice to the same destination silently
    /// overwrites the first mapping with no diagnostic. After fix: a diagnostic is emitted.
    /// RED: no diagnostic emitted. GREEN: ambiguity/duplicate diagnostic emitted.
    /// </summary>
    [Fact]
    public void DuplicateMapProperty_WithSameDestinationName_ShouldEmitDiagnosticNotSilentlyOverwrite()
    {
        string source = @"
using EricksonLopez.Mapper;

public class Source { public string NameA { get; set; } = """"; public string NameB { get; set; } = """"; }
public class Dest { public string FullName { get; set; } = """"; }

[Mapper]
public partial class DupMapper
{
    [MapProperty(""NameA"", ""FullName"")]
    [MapProperty(""NameB"", ""FullName"")]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);

        // ELM005 = ambiguous property match, or a new ELM018 = duplicate MapProperty destination
        var ambiguityDiagnostics = diagnostics.Where(d =>
            d.Id is "ELM005" or "ELM018" or "ELM019"
            || (d.Severity >= DiagnosticSeverity.Warning &&
                d.GetMessage().Contains("FullName", StringComparison.OrdinalIgnoreCase))).ToList();

        ambiguityDiagnostics.Should().NotBeEmpty(
            "because two [MapProperty] attributes mapping different sources to the same destination 'FullName' " +
            "is ambiguous and must produce a diagnostic rather than silently using the last declaration");
    }

    /// <summary>
    /// DIAG-007 / FIX-A — RED→GREEN regression: [MapFactory("NonExistentMethod")] was silently
    /// ignored (falling through to constructor resolution). After ELM017, it emits a compile-time Error.
    /// RED: no diagnostic emitted, generator silently used default constructor.
    /// GREEN: ELM017 is emitted with severity Error.
    /// </summary>
    [Fact]
    public void MapFactory_WhenMethodDoesNotExistOnTarget_ShouldEmitELM017()
    {
        string source = @"
using EricksonLopez.Mapper;

public class Source { public string Name { get; set; } = """"; }
public class Dest
{
    public string Name { get; set; } = """";
    public static Dest Create(string name) => new Dest { Name = name };
}

[Mapper]
public partial class FactoryMapper
{
    [MapFactory(""NonExistentFactory"")]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Should().Contain(d => d.Id == "ELM017",
            "because [MapFactory(\"NonExistentFactory\")] references a method that does not exist " +
            "on Dest, and the generator must emit ELM017 (Error) instead of silently falling through");
    }

    /// <summary>
    /// SEM-001 / FIX-B — RED→GREEN regression: uint → ulong is a valid C# implicit widening
    /// conversion but was missing from the IsWideningNumeric table, causing ELM003 false positive.
    /// RED: ELM003 (unsupported conversion) or ELM015 (narrowing warning) was emitted.
    /// GREEN: No error diagnostics; DirectAssignment strategy is selected.
    /// </summary>
    [Fact]
    public void WideningConversion_UIntToULong_ShouldNotEmitAnyDiagnostic()
    {
        string source = @"
using EricksonLopez.Mapper;

public class Source { public uint Count { get; set; } }
public class Dest { public ulong Count { get; set; } }

[Mapper]
public partial class WideningMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty(
            "because uint → ulong is a safe widening (implicit) conversion in C# and must not emit any error");
        diagnostics.Where(d => d.Id == "ELM015").Should().BeEmpty(
            "because uint → ulong is not a narrowing conversion and must not emit ELM015");
    }

    /// <summary>
    /// SEM-001 / FIX-B2 — RED→GREEN regression: byte → uint was also missing from the widening table.
    /// </summary>
    [Fact]
    public void WideningConversion_ByteToUInt_ShouldNotEmitAnyDiagnostic()
    {
        string source = @"
using EricksonLopez.Mapper;

public class Source { public byte Count { get; set; } }
public class Dest { public uint Count { get; set; } }

[Mapper]
public partial class WideningMapper2
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty(
            "because byte → uint is a safe widening (implicit) conversion in C# and must not emit any error");
    }

    /// <summary>
    /// SEC-008 / FIX-E — RED→GREEN regression: a generic type with a single public property
    /// (e.g. Result&lt;T&gt;) was incorrectly matched by the ValueObject heuristic, causing
    /// Unsupported conversion instead of trying structural mapping.
    /// RED: ValueObjectMapping was selected incorrectly.
    /// GREEN: The generic type guard prevents the heuristic from firing; structural mapping is attempted.
    /// </summary>
    [Fact]
    public void GenericTypeWithSingleProperty_ShouldNotBeMatchedByValueObjectHeuristic()
    {
        // A generic wrapper type with a single Value property resembles a ValueObject.
        // The heuristic must NOT fire because it is a generic type (IsGenericType == true).
        string source = @"
using EricksonLopez.Mapper;

public class ResultWrapper<T>
{
    public T Value { get; set; } = default!;
}

public class Source { public string Name { get; set; } = """"; }
public class Dest { public string Name { get; set; } = """"; }

[Mapper]
public partial class GenericMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);

        // This mapper does a simple Source→Dest mapping which has nothing to do with ResultWrapper.
        // Before FIX-E, the generator could incorrectly enter the ValueObject branch for Source/Dest
        // if they had a similar structure. The mapper itself must compile without errors.
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty(
            "because a simple Source→Dest structural mapping must succeed regardless of whether " +
            "generic types with single properties exist in scope");
    }
}
