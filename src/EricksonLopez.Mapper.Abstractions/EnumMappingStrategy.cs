// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.Mapper;

/// <summary>
/// Specifies the strategy used to map enumeration values between source and destination enum types.
/// </summary>
public enum EnumMappingStrategy
{
    /// <summary>
    /// Maps enumeration members by matching their identifier names.
    /// </summary>
    ByName = 0,

    /// <summary>
    /// Maps enumeration members by their underlying integer or numeric values.
    /// </summary>
    ByValue = 1
}
