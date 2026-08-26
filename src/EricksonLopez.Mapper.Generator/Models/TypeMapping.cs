// Copyright © Erickson Lopez. MIT License.
using System.Diagnostics.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator.Models;

// Excluded from coverage: Records represent pure DTOs. Their auto-generated boilerplate (Equals, GetHashCode, PrintMembers) contains no custom business logic and is guaranteed by the C# compiler.
[ExcludeFromCodeCoverage]
internal record TypeMapping(
    string Namespace,
    string ClassName,
    bool IsStatic,
    EquatableArray<MethodMapping> Methods,
    EquatableArray<DiagnosticInfo> Diagnostics);
