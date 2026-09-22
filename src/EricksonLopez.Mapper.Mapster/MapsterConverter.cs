// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics.CodeAnalysis;
using EricksonLopez.Mapper;
using Mapster;

namespace EricksonLopez.Mapper.Mapster;

/// <summary>
/// Provides an adapter that delegates object conversion to Mapster's mapping engine.
/// </summary>
/// <remarks>
/// <strong>Native AOT / Trimming Warning:</strong> Mapster uses reflection and dynamic code generation internally.
/// This adapter is <em>not</em> compatible with Native AOT or trimmed builds.
/// The package declares <c>IsAotCompatible=false</c> and <c>IsTrimmable=false</c> to document this constraint.
/// Use a source-generated <see cref="IConverter{TSource,TDestination}"/> for AOT scenarios.
/// </remarks>
/// <typeparam name="TSource">The source object type to convert from.</typeparam>
/// <typeparam name="TDestination">The destination object type to convert to.</typeparam>
public sealed class MapsterConverter<TSource, TDestination> : IConverter<TSource, TDestination>
{
    private const string AotWarningMessage = "Mapster uses reflection internally and is not compatible with AOT or trimmed builds.";
    private readonly TypeAdapterConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapsterConverter{TSource, TDestination}"/> class using global Mapster configuration.
    /// </summary>
    [RequiresUnreferencedCode(AotWarningMessage)]
    [RequiresDynamicCode(AotWarningMessage)]
    public MapsterConverter() : this(TypeAdapterConfig.GlobalSettings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapsterConverter{TSource, TDestination}"/> class with the specified Mapster configuration.
    /// </summary>
    /// <param name="config">The Mapster configuration instance to use for mapping</param>
    /// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/></exception>
    [RequiresUnreferencedCode(AotWarningMessage)]
    [RequiresDynamicCode(AotWarningMessage)]
    public MapsterConverter(TypeAdapterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;
    }

    /// <inheritdoc/>
    public TDestination Convert(TSource source)
        => source.Adapt<TDestination>(_config)!;
}
