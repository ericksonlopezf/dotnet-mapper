// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;
using Mapster;

namespace EricksonLopez.Mapper.Mapster;

/// <summary>
/// Provides an adapter that delegates object conversion to Mapster's mapping engine.
/// </summary>
/// <typeparam name="TSource">The source object type to convert from.</typeparam>
/// <typeparam name="TDestination">The destination object type to convert to.</typeparam>
public sealed class MapsterConverter<TSource, TDestination> : IConverter<TSource, TDestination>
{
    private readonly TypeAdapterConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapsterConverter{TSource, TDestination}"/> class using global Mapster configuration.
    /// </summary>
    public MapsterConverter() : this(TypeAdapterConfig.GlobalSettings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapsterConverter{TSource, TDestination}"/> class with the specified Mapster configuration.
    /// </summary>
    /// <param name="config">The Mapster configuration instance to use for mapping</param>
    /// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/></exception>
    public MapsterConverter(TypeAdapterConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc/>
    public TDestination Convert(TSource source)
        => source.Adapt<TDestination>(_config)!;
}
