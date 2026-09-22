// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics.CodeAnalysis;
using EricksonLopez.Mapper;
using Mapster;

namespace EricksonLopez.Mapper.Mapster;

/// <summary>
/// Provides extension methods for integrating Mapster configuration with <see cref="IConverter{TSource, TDestination}"/>.
/// </summary>
public static class MapsterMapperExtensions
{
    private const string AotWarningMessage = "Mapster uses reflection internally and is not compatible with AOT or trimmed builds.";

    /// <summary>
    /// Configures a <see cref="TypeAdapterConfig"/> instance to use the specified <see cref="IConverter{TSource, TDestination}"/> for mapping.
    /// </summary>
    /// <typeparam name="TSource">The source type to convert from.</typeparam>
    /// <typeparam name="TDestination">The destination type to convert to.</typeparam>
    /// <param name="config">The Mapster configuration instance to configure</param>
    /// <param name="converter">The converter instance handling the mapping</param>
    /// <returns>The configured <see cref="TypeAdapterConfig"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="config"/> or <paramref name="converter"/> is <see langword="null"/></exception>
    [RequiresUnreferencedCode(AotWarningMessage)]
    [RequiresDynamicCode(AotWarningMessage)]
    public static TypeAdapterConfig UseConverter<TSource, TDestination>(
        this TypeAdapterConfig config,
        IConverter<TSource, TDestination> converter)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(converter);

        config.NewConfig<TSource, TDestination>()
            .MapWith(src => converter.Convert(src));

        return config;
    }
}
