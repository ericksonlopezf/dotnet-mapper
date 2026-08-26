// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Excludes a property or field from participating in mapping operations when declared directly on the model member.
/// </summary>
/// <remarks>
/// Can be placed directly on source or destination model properties to exclude them across all mappers.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class MapperIgnoreAttribute : Attribute
{
}
