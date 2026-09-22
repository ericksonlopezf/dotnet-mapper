// Copyright © Erickson Lopez. MIT License.
using System.Diagnostics.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator.Models;

// Excluded from coverage: Pure DTO record boilerplate.
[ExcludeFromCodeCoverage]
internal abstract record ConversionStrategy
{
    public record DirectAssignment() : ConversionStrategy;
    public record ExplicitCast() : ConversionStrategy;
    public record CustomMethod(string MethodName) : ConversionStrategy;
    public record MapMethodInvocation(string MethodName, bool IsSourceNullable = false, bool IsTargetNullable = false, string MethodKey = "") : ConversionStrategy;
    public record EnumerableMapping(ConversionStrategy ElementStrategy, string SourceElementType, string TargetElementType, bool IsArray, bool IsList, bool IsImmutableArray, bool SourceIsArray, bool SourceHasCount, bool IsHashSet = false, bool IsImmutableList = false, bool IsFrozenSet = false, bool SourceIsValueType = false, bool SourceIsImmutableArray = false, bool IsSourceNullable = false, bool IsTargetNullable = false) : ConversionStrategy;
    public record DictionaryMapping(ConversionStrategy KeyStrategy, ConversionStrategy ValueStrategy, string SourceKeyType, string TargetKeyType, string SourceValueType, string TargetValueType, bool SourceHasCount = false) : ConversionStrategy;

    // ValueObjectKind: 0 = constructor, 1 = property "Value", 2 = direct cast
    public record ValueObjectMapping(int Kind, ConversionStrategy InnerStrategy, string SourceInnerType, string TargetInnerType, bool IsSourceNullable = false, bool IsTargetNullable = false, bool SourceIsValueType = false) : ConversionStrategy;

    /// <summary>
    /// Represents a built-in type conversion that emits a specific expression template.
    /// The template uses {0} as placeholder for the source expression.
    /// Examples: "(long){0}", "{0}.ToString()", "Enum.Parse&lt;TargetEnum&gt;({0})", "Guid.Parse({0})"
    /// </summary>
    public record BuiltinConversion(string ExpressionTemplate) : ConversionStrategy;

    public record EnumToEnumMapping(string TargetEnumType, EquatableArray<string> UnmappedSourceMembers) : ConversionStrategy;

    public record Unsupported() : ConversionStrategy;
}
