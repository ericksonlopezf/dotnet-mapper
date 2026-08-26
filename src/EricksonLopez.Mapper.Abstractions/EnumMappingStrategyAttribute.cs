// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Configures the enum mapping strategy and case-sensitivity for an annotated mapper class, interface, or method.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class EnumMappingStrategyAttribute : Attribute
{
    /// <summary>
    /// Gets the enum mapping strategy applied to the annotated target.
    /// </summary>
    public EnumMappingStrategy Strategy { get; }

    /// <summary>
    /// Gets or sets a value indicating whether enum member names are matched case-insensitively.
    /// </summary>
    public bool IgnoreCase { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumMappingStrategyAttribute"/> class with the specified enum mapping strategy.
    /// </summary>
    /// <param name="strategy">The enum mapping strategy to apply</param>
    public EnumMappingStrategyAttribute(EnumMappingStrategy strategy)
    {
        Strategy = strategy;
    }
}
