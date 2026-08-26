// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Reflection;
using AwesomeAssertions;
using EricksonLopez.Mapper.Generator;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Core;

[Trait("Category", "FastAst")]
public class DiagnosticsTests
{
    [Fact]
    public void DiagnosticDescriptors_WhenInspected_ShouldBeValid()
    {
        var fields = typeof(DiagnosticDescriptors)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(DiagnosticDescriptor));

        var descriptors = fields.Select(f => (DiagnosticDescriptor)f.GetValue(null)!).ToList();

        descriptors.Should().NotBeEmpty();

        foreach (var descriptor in descriptors)
        {
            descriptor.Id.Should().StartWith("ELM");
            descriptor.Category.Should().Be("EricksonLopez.Mapper");
            descriptor.MessageFormat.ToString().Should().NotBeNullOrWhiteSpace();
            descriptor.Title.ToString().Should().NotBeNullOrWhiteSpace();
            descriptor.DefaultSeverity.Should().BeOneOf(DiagnosticSeverity.Error, DiagnosticSeverity.Warning, DiagnosticSeverity.Info);
            descriptor.IsEnabledByDefault.Should().BeTrue();
        }
    }

    /// <summary>
    /// T003 — Regression guard: no two DiagnosticDescriptor IDs in the generator may collide,
    /// and they must not use the reserved IDs allocated to Roslyn Analyzers (ELM008, ELM009, ELM012).
    /// Introduced to prevent recurrence of the ELM012 duplication bug (Bug #1 P0).
    /// </summary>
    [Fact]
    public void DiagnosticDescriptors_WhenAllIdsInspected_ShouldBeUniqueAndNotCollideWithReservedAnalyzerIds()
    {
        var generatorFields = typeof(DiagnosticDescriptors)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(DiagnosticDescriptor))
            .Select(f => (DiagnosticDescriptor)f.GetValue(null)!)
            .ToList();

        var generatorIds = generatorFields.Select(d => d.Id).ToList();

        // Generator IDs must be unique
        generatorIds.Should().OnlyHaveUniqueItems(
            because: "diagnostic ID collisions break IDE tooling and confuse developers");

        // Reserved Analyzer IDs: ELM008 (Reflection), ELM009 (Dynamic), ELM012 (MustBePartial)
        var reservedAnalyzerIds = new[] { "ELM008", "ELM009", "ELM012" };
        foreach (var reservedId in reservedAnalyzerIds)
        {
            generatorIds.Should().NotContain(reservedId,
                because: $"diagnostic ID '{reservedId}' is reserved for Roslyn Analyzers (Bug #1 — ELM012 collision)");
        }
    }

    /// <summary>
    /// T003 — Explicit guard: InvalidConverterType must be ELM013 and not collide with MustBePartial (ELM012).
    /// </summary>
    [Fact]
    public void InvalidConverterType_WhenIdInspected_ShouldBeELM013()
    {
        var invalidConverterTypeId = DiagnosticDescriptors.InvalidConverterType.Id;

        invalidConverterTypeId.Should().Be("ELM013",
            because: "ELM012 collision bug was fixed by reassigning InvalidConverterType to ELM013");
    }

    public static readonly TheoryData<DiagnosticDescriptor, string, DiagnosticSeverity> DescriptorsData = new()
    {
        { DiagnosticDescriptors.UnmappedDestinationMember, "ELM001", DiagnosticSeverity.Error },
        { DiagnosticDescriptors.MissingFactoryOrConstructor, "ELM002", DiagnosticSeverity.Error },
        { DiagnosticDescriptors.UnsupportedConversion, "ELM003", DiagnosticSeverity.Error },
        { DiagnosticDescriptors.NullabilityMismatch, "ELM004", DiagnosticSeverity.Error },
        { DiagnosticDescriptors.AmbiguousPropertyMatch, "ELM005", DiagnosticSeverity.Error },
        { DiagnosticDescriptors.MissingConstructorMapping, "ELM006", DiagnosticSeverity.Error },
        { DiagnosticDescriptors.AmbiguousConstructor, "ELM007", DiagnosticSeverity.Error },
        { DiagnosticDescriptors.CircularReference, "ELM010", DiagnosticSeverity.Error },
        { DiagnosticDescriptors.AbstractBaseIncompletePolymorphism, "ELM011", DiagnosticSeverity.Warning },
        { DiagnosticDescriptors.InvalidConverterType, "ELM013", DiagnosticSeverity.Error },
        { DiagnosticDescriptors.EnumMappingMissingDestinationMember, "ELM014", DiagnosticSeverity.Error },
        { DiagnosticDescriptors.NarrowingConversionPotentialDataLoss, "ELM015", DiagnosticSeverity.Warning },
        { DiagnosticDescriptors.StringToEnumRuntimeRisk, "ELM016", DiagnosticSeverity.Warning }
    };

    /// <summary>
    /// Verifies all individual diagnostic descriptors match their expected IDs and default severities in isolation.
    /// </summary>
    [Theory]
    [MemberData(nameof(DescriptorsData))]
    public void DiagnosticDescriptor_WhenInspected_ShouldMatchExpectedIdAndSeverity(
        DiagnosticDescriptor descriptor,
        string expectedId,
        DiagnosticSeverity expectedSeverity)
    {
        descriptor.Id.Should().Be(expectedId);
        descriptor.DefaultSeverity.Should().Be(expectedSeverity);
    }
}


