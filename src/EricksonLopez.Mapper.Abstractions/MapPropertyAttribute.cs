// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Configures an explicit member-to-member mapping between source and destination properties with differing names.
/// </summary>
/// <remarks>
/// Multiple instances can be applied to a single mapping method to configure multiple remapped properties.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the source property or constructor parameter to read from.
    /// </summary>
    public string SourceName { get; }

    /// <summary>
    /// Gets the name of the destination property or constructor parameter to populate.
    /// </summary>
    public string DestinationName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapPropertyAttribute"/> class with the specified source and destination names.
    /// </summary>
    /// <param name="sourceName">The name of the source property or parameter to map from</param>
    /// <param name="destinationName">The name of the destination property or parameter to map to</param>
    public MapPropertyAttribute(string sourceName, string destinationName)
    {
        SourceName = sourceName;
        DestinationName = destinationName;
    }
}
