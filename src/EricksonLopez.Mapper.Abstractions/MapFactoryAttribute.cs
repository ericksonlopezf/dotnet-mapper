// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Instructs the source generator to instantiate the destination type using a static factory method instead of a constructor.
/// </summary>
/// <remarks>
/// The referenced method must be static, public, return the destination type, and declare parameters compatible with source properties.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class MapFactoryAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the static factory method declared on the destination type.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapFactoryAttribute"/> class with the specified static factory method name.
    /// </summary>
    /// <param name="methodName">The name of the static factory method to invoke on the destination type</param>
    public MapFactoryAttribute(string methodName)
    {
        MethodName = methodName;
    }
}
