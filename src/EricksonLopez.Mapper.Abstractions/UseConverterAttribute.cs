// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Specifies a custom <see cref="IConverter{TSource,TDestination}"/> implementation or mapper member to handle the entire mapping operation.
/// </summary>
/// <remarks>
/// When applied to a mapping method, the generator delegates execution directly to the converter rather than synthesizing member assignments.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class UseConverterAttribute : Attribute
{
    /// <summary>
    /// Gets the type of the converter implementing <see cref="IConverter{TSource,TDestination}"/>.
    /// </summary>
    public Type ConverterType { get; } = null!;

    /// <summary>
    /// Gets the name of the instance field or property supplying the converter instance.
    /// </summary>
    public string? ConverterFieldName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UseConverterAttribute"/> class with a converter type instantiated via its parameterless constructor.
    /// </summary>
    /// <param name="converterType">The type implementing <see cref="IConverter{TSource,TDestination}"/> for the mapping method signatures</param>
    public UseConverterAttribute(Type converterType)
    {
        ConverterType = converterType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UseConverterAttribute"/> class with the name of an existing converter field or property.
    /// </summary>
    /// <param name="converterFieldName">The name of the instance field or property holding the converter</param>
    public UseConverterAttribute(string converterFieldName)
    {
        ConverterFieldName = converterFieldName;
    }
}
