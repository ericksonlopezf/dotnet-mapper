// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Registers a runtime derived type pair for polymorphic dispatch in the generated mapping method.
/// </summary>
/// <remarks>
/// When applied to a mapping method, the source generator emits a type pattern-matching switch expression
/// that delegates derived source instances to their corresponding derived destination mappings.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class MapDerivedTypeAttribute : Attribute
{
    /// <summary>
    /// Gets the derived source runtime type to match during polymorphic dispatch.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// Gets the derived destination type to instantiate when the source instance matches <see cref="SourceType"/>.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapDerivedTypeAttribute"/> class with the specified source and target types.
    /// </summary>
    /// <param name="sourceType">The derived source type to match at runtime</param>
    /// <param name="targetType">The derived destination type to instantiate</param>
    public MapDerivedTypeAttribute(Type sourceType, Type targetType)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }
}
