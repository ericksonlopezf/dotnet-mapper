// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Assigns a constant value or custom C# expression directly to a destination member during mapping.
/// </summary>
/// <remarks>
/// The expression is emitted verbatim in the generated assignment or constructor argument.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapValueAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the destination member receiving the assigned value.
    /// </summary>
    public string DestinationName { get; }

    /// <summary>
    /// Gets the C# expression string emitted for the destination assignment.
    /// </summary>
    public string ValueExpression { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapValueAttribute"/> class with the destination member name and expression.
    /// </summary>
    /// <param name="destinationName">The name of the destination property or constructor parameter</param>
    /// <param name="valueExpression">The valid C# expression string assigned to the destination member</param>
    public MapValueAttribute(string destinationName, string valueExpression)
    {
        DestinationName = destinationName;
        ValueExpression = valueExpression;
    }
}
