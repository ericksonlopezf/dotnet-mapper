// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Specifies that a destination member is excluded from automatic mapping, suppressing strict mapping diagnostics.
/// </summary>
/// <remarks>
/// Multiple instances can be applied to a single mapping method to ignore several destination members.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapIgnoreAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the destination member to exclude from mapping.
    /// </summary>
    public string DestinationName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapIgnoreAttribute"/> class with the specified destination member name.
    /// </summary>
    /// <param name="destinationName">The name of the destination member to exclude from mapping</param>
    public MapIgnoreAttribute(string destinationName)
    {
        DestinationName = destinationName;
    }
}
