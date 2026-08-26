// Copyright © Erickson Lopez. MIT License.
using System.Diagnostics.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator.Models;

// Excluded from coverage: Pure DTO record boilerplate.
[ExcludeFromCodeCoverage]
internal record MethodMapping(
    string MethodName,
    TypeReference SourceType,
    TypeReference TargetType,
    ConstructionStrategy Construction,
    EquatableArray<MemberMapping> Members,
    bool IsStrict,
    EquatableArray<DerivedTypeMapping> DerivedTypes,
    string? CustomConverter = null,
    string? CustomConverterField = null);
