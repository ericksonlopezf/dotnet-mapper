// Copyright © Erickson Lopez. MIT License.
using System.Diagnostics.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator.Models;

// Excluded from coverage: Pure DTO record boilerplate.
[ExcludeFromCodeCoverage]
internal record DerivedTypeMapping(
    string SourceType,
    string TargetType,
    string? MethodName = null);
