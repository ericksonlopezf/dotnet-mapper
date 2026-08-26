// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Specifies that a source member is excluded from member resolution during mapping generation.
/// </summary>
/// <remarks>
/// Multiple instances can be applied to a single mapping method to ignore several source members.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapIgnoreSourceAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the source member to exclude from mapping resolution.
    /// </summary>
    public string SourceName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapIgnoreSourceAttribute"/> class with the specified source member name.
    /// </summary>
    /// <param name="sourceName">The name of the source property to exclude from mapping</param>
    public MapIgnoreSourceAttribute(string sourceName)
    {
        SourceName = sourceName;
    }
}
