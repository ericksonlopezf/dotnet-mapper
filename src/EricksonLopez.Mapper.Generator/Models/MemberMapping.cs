// Copyright © Erickson Lopez. MIT License.
using System.Diagnostics.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator.Models;

// Excluded from coverage: Pure DTO record boilerplate.
[ExcludeFromCodeCoverage]
internal record MemberMapping(
    string SourceName,
    string TargetName,
    ConversionStrategy Strategy,
    string? Fallback = null,
    bool IsSourceNullable = false,
    bool IsTargetNullable = false,
    string? CustomValueExpression = null,
    string? CustomSourceExpression = null);
