// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Mapper.Generator.Tests.Infrastructure;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

/// <summary>
/// Audit Suite 20: Silent Data Corruption Attack and Verification Tests.
/// Validates that the compiler and generator detect, warn, or reject conditions
/// that could cause silent data loss, precision truncation, enum misalignment,
/// or accidental reference sharing.
/// </summary>
public class SilentDataCorruptionTests
{
    [Fact]
    public void NarrowingNumericConversion_LongToInt_ShouldEmitELM015Warning()
    {
        // Vector: Mapping a 64-bit integer into a 32-bit integer silently truncates bits if unverified.
        // Generator must emit ELM015 (Warning) alerting developers of possible silent data corruption.
        string source = @"
using EricksonLopez.Mapper;

public class Source { public long BigNumber { get; set; } }
public class Dest { public int BigNumber { get; set; } }

[Mapper]
public partial class NarrowingMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Should().Contain(d => d.Id == "ELM015",
            "because narrowing numeric conversion from long to int risks silent data corruption and must emit ELM015");
    }

    [Fact]
    public void EnumMismatch_WhenTargetEnumMissingValue_ShouldEmitELM014()
    {
        // Vector: Source enum has values not present in Destination enum.
        // Under strict enum mapping, unmapped enum values must trigger compile error ELM014 rather than silently mapping to default(0).
        string source = @"
using EricksonLopez.Mapper;

public enum SourceStatus { Active, Inactive, Archived }
public enum TargetStatus { Active, Inactive }

public class SourceModel { public SourceStatus Status { get; set; } }
public class TargetModel { public TargetStatus Status { get; set; } }

[Mapper]
public partial class StatusMapper
{
    public partial TargetModel Map(SourceModel source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Should().Contain(d => d.Id == "ELM014",
            "because TargetStatus is missing 'Archived' and must emit ELM014 to prevent silent runtime fallback to 0");
    }

    [Fact]
    public void StrictMapping_WhenDestinationHasUnmappedProperty_ShouldEmitELM001()
    {
        // Vector: Destination has critical business property that is unmapped from Source.
        // In Strict mode (default), this must cause a compiler error ELM001 to prevent silent drop of data.
        string source = @"
using EricksonLopez.Mapper;

public class SourceDto { public string Name { get; set; } = """"; }
public class DestEntity { public string Name { get; set; } = """"; public decimal Balance { get; set; } }

[Mapper(StrictMapping = true)]
public partial class StrictOrderMapper
{
    public partial DestEntity Map(SourceDto source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Should().Contain(d => d.Id == "ELM001",
            "because unmapped destination property 'Balance' under StrictMapping=true must prevent silent omission");
    }

    [Fact]
    public void NestedObjectMapping_WhenSourceNestedIsNull_ShouldEmitSafeNullPropagationNotThrow()
    {
        // Vector: Deep graph mapping where intermediate node is null.
        // Emitted code must use null-safe navigation ('?') to prevent NullReferenceException.
        string source = @"
using EricksonLopez.Mapper;

public class AddressSource { public string City { get; set; } = """"; }
public class CustomerSource { public AddressSource? Address { get; set; } }

public class AddressDest { public string City { get; set; } = """"; }
public class CustomerDest { public AddressDest? Address { get; set; } }

[Mapper]
public partial class CustomerMapper
{
    public partial CustomerDest Map(CustomerSource source);
    public partial AddressDest MapAddress(AddressSource source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Address = (source.Address != null ? this.MapAddress(source.Address) : null)",
            "because mapping of nullable nested object must preserve null without throwing NullReferenceException");
    }

    [Fact]
    public void DateOnlyToDateTimeOffset_ShouldEmitExplicitConversionWithUtcOffset()
    {
        // Vector: Temporal conversions can cause timezone drift if not strictly UTC or explicitly converted.
        string source = @"
using System;
using EricksonLopez.Mapper;

public class EventSource { public DateOnly Date { get; set; } }
public class EventDest { public DateTimeOffset Date { get; set; } }

[Mapper]
public partial class EventMapper
{
    public partial EventDest Map(EventSource source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::System.DateTimeOffset((source.Date).ToDateTime(global::System.TimeOnly.MinValue, global::System.DateTimeKind.Utc))",
            "because DateOnly to DateTimeOffset conversion must explicitly use UTC offset to prevent timezone drift");
    }
}
