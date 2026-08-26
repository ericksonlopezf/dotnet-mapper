// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Configures assembly-level default mapping conventions and behavior for all mappers in the compilation.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public sealed class MapperDefaultsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the default strategy used for enum conversions across the assembly.
    /// </summary>
    public EnumMappingStrategy EnumMappingStrategy { get; set; } = EnumMappingStrategy.ByName;

    /// <summary>
    /// Gets or sets a value indicating whether enum member names are matched case-insensitively by default.
    /// </summary>
    public bool EnumIgnoreCase { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether strict mapping validation is enabled by default across all mappers in the assembly.
    /// </summary>
    public bool StrictMapping { get; set; } = true;
}
