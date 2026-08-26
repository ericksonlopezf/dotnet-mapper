// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Defines a custom converter that maps a source instance to a destination instance.
/// </summary>
/// <remarks>
/// Implement this contract to supply custom transformation logic for a specific source and destination type pair.
/// </remarks>
/// <typeparam name="TSource">The source type to convert from.</typeparam>
/// <typeparam name="TDestination">The destination type to convert to.</typeparam>
public interface IConverter<TSource, TDestination>
{
    /// <summary>
    /// Converts the specified source instance to an instance of <typeparamref name="TDestination"/>.
    /// </summary>
    /// <param name="source">The source instance to convert</param>
    /// <returns>The converted instance of <typeparamref name="TDestination"/>.</returns>
    TDestination Convert(TSource source);
}





