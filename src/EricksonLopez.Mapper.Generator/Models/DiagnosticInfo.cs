// Copyright © Erickson Lopez. MIT License.
using System.Diagnostics.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator.Models;

/// <summary>
/// Stores diagnostic information using only value-equatable types so that
/// the incremental pipeline can cache this model correctly.
/// <see cref="Microsoft.CodeAnalysis.Location"/> is a Roslyn reference type
/// without value equality and MUST NOT be stored here.
/// </summary>
// Excluded from coverage: Pure DTO record boilerplate.
[ExcludeFromCodeCoverage]
internal record DiagnosticInfo(
    string Id,
    string Title,
    string MessageFormat,
    string Category,
    int DefaultSeverity,
    bool IsEnabledByDefault,
    string FilePath,
    int Line,
    int Column,
    EquatableArray<string> Args);
