// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Specifies that the annotated class or interface is a mapper whose partial methods
/// are implemented by the source generator at compile time.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public sealed class MapperAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether strict mapping mode is enforced.
    /// </summary>
    /// <remarks>
    /// When set to <see langword="true"/>, every destination member must have a matching source member,
    /// an explicit property mapping, a fallback value, or an explicit ignore directive; unmapped members produce a compile-time diagnostic.
    /// </remarks>
    public bool StrictMapping { get; set; } = true;
}
