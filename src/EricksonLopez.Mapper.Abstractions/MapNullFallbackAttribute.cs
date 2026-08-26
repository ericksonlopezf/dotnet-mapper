// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Specifies a fallback expression to substitute when a nullable source member evaluates to <see langword="null"/> for a non-nullable destination member.
/// </summary>
/// <remarks>
/// The expression is emitted verbatim as the right-hand operand of a null-coalescing expression in the generated mapping code.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapNullFallbackAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the destination member to which the fallback expression applies.
    /// </summary>
    public string DestinationName { get; }

    /// <summary>
    /// Gets the C# expression emitted when the source member is <see langword="null"/>.
    /// </summary>
    public string FallbackExpression { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapNullFallbackAttribute"/> class with the destination member name and fallback expression.
    /// </summary>
    /// <param name="destinationName">The name of the destination member receiving the fallback</param>
    /// <param name="fallbackExpression">The valid C# expression emitted when the source value is <see langword="null"/></param>
    public MapNullFallbackAttribute(string destinationName, string fallbackExpression)
    {
        DestinationName = destinationName;
        FallbackExpression = fallbackExpression;
    }
}
