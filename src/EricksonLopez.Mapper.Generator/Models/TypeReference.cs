// Copyright © Erickson Lopez. MIT License.
using System.Diagnostics.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator.Models;

// Excluded from coverage: Pure DTO record boilerplate.
[ExcludeFromCodeCoverage]
internal record TypeReference(
    string FullyQualifiedName,
    bool IsNullable,
    bool IsAbstract = false,
    bool IsValueType = false);
