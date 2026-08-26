// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

#pragma warning disable RS1019
#pragma warning disable RS1036
#pragma warning disable RS2008
namespace EricksonLopez.Mapper.Analyzers.Tests;

/// <summary>
/// Common constants, test syntax snippets, and centralized mock diagnostic descriptors
/// used across analyzer and code fix tests.
/// </summary>
public static class TestConstants
{
    /// <summary>
    /// Source code definition of <c>[Mapper]</c> attribute for standalone Roslyn test compilations.
    /// </summary>
    public const string MapperAttributeCode = @"
namespace EricksonLopez.Mapper
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class MapperAttribute : System.Attribute
    {
        public bool StrictMapping { get; set; } = true;
    }
}";

    public static readonly DiagnosticDescriptor UnmappedMemberDescriptor = new(
        id: "ELM001",
        title: "Unmapped member",
        messageFormat: "Unmapped destination member '{0}'",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MalformedUnmappedMemberDescriptor = new(
        id: "ELM001_MALFORMED",
        title: "Malformed unmapped member",
        messageFormat: "Unmapped destination member without single quotes",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedConversionDescriptor = new(
        id: "ELM003",
        title: "Unsupported conversion",
        messageFormat: "Cannot convert from '{0}' to '{1}' for member '{2}'",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MalformedUnsupportedConversionDescriptor = new(
        id: "ELM003",
        title: "Malformed conversion",
        messageFormat: "Invalid message format without expected patterns",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NullabilityMismatchDescriptor = new(
        id: "ELM004",
        title: "Nullability mismatch",
        messageFormat: "Possible null reference assignment for member '{0}' from '{1}' to '{2}'",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MalformedNullabilityMismatchDescriptor = new(
        id: "ELM004_MALFORMED",
        title: "Malformed nullability mismatch",
        messageFormat: "Nullability mismatch without quotes",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AmbiguousConstructorDescriptor = new(
        id: "ELM007",
        title: "Ambiguous constructor",
        messageFormat: "Destination type '{0}' has multiple parameterized constructors, declare explicitly which constructor to use",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}




