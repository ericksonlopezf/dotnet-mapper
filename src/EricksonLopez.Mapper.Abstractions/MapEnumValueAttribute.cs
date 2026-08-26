// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Maps a specific source enum member value to an explicit destination enum member value.
/// </summary>
/// <remarks>
/// Multiple instances can be applied to a mapping method to override individual enum member pairings.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapEnumValueAttribute : Attribute
{
    /// <summary>
    /// Gets the source enum member value to map from.
    /// </summary>
    public object Source { get; }

    /// <summary>
    /// Gets the destination enum member value to map to.
    /// </summary>
    public object Target { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapEnumValueAttribute"/> class with explicit source and destination enum values.
    /// </summary>
    /// <param name="source">The source enum member value</param>
    /// <param name="target">The destination enum member value</param>
    public MapEnumValueAttribute(object source, object target)
    {
        Source = source;
        Target = target;
    }
}
