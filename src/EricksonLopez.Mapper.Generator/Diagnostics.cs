// Copyright © Erickson Lopez. MIT License.
using System;
using Microsoft.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnmappedDestinationMember = new(
        id: "ELM001",
        title: "Unmapped destination member",
        messageFormat: "Destination member '{0}' has no source mapping",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingFactoryOrConstructor = new(
        id: "ELM002",
        title: "Missing factory or constructor",
        messageFormat: "Destination type '{0}' has no public constructors or factory methods",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedConversion = new(
        id: "ELM003",
        title: "Unsupported conversion",
        messageFormat: "Cannot convert from '{0}' to '{1}' for member '{2}'",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NullabilityMismatch = new(
        id: "ELM004",
        title: "Nullability mismatch",
        messageFormat: "Possible null reference assignment for member '{0}' from '{1}' to '{2}'",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AmbiguousPropertyMatch = new(
        id: "ELM005",
        title: "Ambiguous property match",
        messageFormat: "Multiple source properties match destination member '{0}' case-insensitively",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingConstructorMapping = new(
        id: "ELM006",
        title: "Missing constructor mapping",
        messageFormat: "Destination type '{0}' does not have a supported constructor or public setters",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AmbiguousConstructor = new(
        id: "ELM007",
        title: "Ambiguous constructor",
        messageFormat: "Destination type '{0}' has multiple parameterized constructors, declare explicitly which constructor to use",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CircularReference = new(
        id: "ELM010",
        title: "Circular mapping reference",
        messageFormat: "Circular mapping detected involving method '{0}'. This will cause a StackOverflow at runtime.",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AbstractBaseIncompletePolymorphism = new(
        id: "ELM011",
        title: "Incomplete polymorphism on abstract base type",
        messageFormat: "Base type '{0}' is abstract and may cause a runtime error if a derived type is not matched. Ensure all derived types are mapped.",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidConverterType = new(
        id: "ELM013",
        title: "Invalid converter type",
        messageFormat: "Type '{0}' does not implement 'IConverter<{1}, {2}>'",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EnumMappingMissingDestinationMember = new(
        id: "ELM014",
        title: "Enum member has no destination equivalent",
        messageFormat: "Enum member '{0}' in source enum '{1}' has no corresponding member in destination enum '{2}'",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NarrowingConversionPotentialDataLoss = new(
        id: "ELM015",
        title: "Narrowing numeric conversion potential data loss",
        messageFormat: "Narrowing numeric conversion from '{0}' to '{1}' for member '{2}' may result in overflow or precision loss",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor StringToEnumRuntimeRisk = new(
        id: "ELM016",
        title: "String to enum parsing runtime exception risk",
        messageFormat: "Direct string-to-enum parsing for member '{0}' can fail at runtime if source string does not match any member of '{1}'",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// ELM017 — DIAG-007 fix: [MapFactory("MethodName")] referenced a factory method that does not
    /// exist on the target type. Previously this was silently ignored (falling through to constructors).
    /// Now emits an Error so the user is informed of the invalid configuration at compile time.
    /// </summary>
    public static readonly DiagnosticDescriptor MapFactoryMethodNotFound = new(
        id: "ELM017",
        title: "MapFactory method not found",
        messageFormat: "Factory method '{0}' was not found as a public static method returning '{1}' on the target type. Verify the method name and accessibility.",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// ELM018 — SEM-003 fix: Duplicate [MapProperty] destination declarations silently overwrote
    /// each other. Now emits a Warning so the developer knows the second declaration takes precedence.
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicateMapPropertyDestination = new(
        id: "ELM018",
        title: "Duplicate MapProperty destination member",
        messageFormat: "Destination member '{0}' is targeted by multiple [MapProperty] attributes. Only the last declaration (source: '{1}') will be used. Remove the redundant attribute to resolve the ambiguity.",
        category: "EricksonLopez.Mapper",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}





