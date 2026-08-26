// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;
using EricksonLopez.Mapper.Mapster;
using Mapster;

namespace EricksonLopez.Mapper.Sample.Level9_Extensions;

// =============================================================================
// Level 9 - EricksonLopez.Mapper.Mapster Integration
//
// Package: EricksonLopez.Mapper.Mapster
//
// Purpose:
//   Provides bi-directional interoperability between EricksonLopez.Mapper (IConverter)
//   and Mapster's TypeAdapterConfig engine.
//
// Public API (MapsterConverter):
//   - MapsterConverter<TSource, TDestination>()
//       Uses TypeAdapterConfig.GlobalSettings (default constructor).
//   - MapsterConverter<TSource, TDestination>(TypeAdapterConfig config)
//       Uses an explicitly provided Mapster configuration.
//
// Public API (extension):
//   - TypeAdapterConfig.UseConverter<TSource, TDest>(IConverter<TSource,TDest> converter)
//       Registers an IConverter<T,D> into Mapster's TypeAdapterConfig.
// =============================================================================

/// <summary>Sample legacy data entity.</summary>
public sealed record LegacyCustomer(int CustomerNumber, string Name, string City);

/// <summary>Sample modern view model.</summary>
public sealed record ModernCustomerView(int CustomerNumber, string Name, string City);

/// <summary>Custom converter from legacy customer to modern view.</summary>
public sealed class LegacyCustomerCustomConverter : IConverter<LegacyCustomer, ModernCustomerView>
{
    /// <inheritdoc/>
    public ModernCustomerView Convert(LegacyCustomer source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ModernCustomerView(source.CustomerNumber, source.Name.Trim(), source.City.ToUpperInvariant());
    }
}

/// <summary>Demonstrates the Mapster adapter converter and extension methods.</summary>
public static class MapsterBridgeDemo
{
    /// <summary>Runs the Mapster interoperability demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 9: EricksonLopez.Mapper.Mapster Interoperability ===");
        Console.WriteLine("Demonstrating bidirectional integration with Mapster via IConverter and TypeAdapterConfig.\n");

        var source = new LegacyCustomer(777, "  Acme Corp  ", "madrid");

        // 1. MapsterConverter<TSource, TDestination>() — default constructor
        //    Uses TypeAdapterConfig.GlobalSettings. Equivalent to: new MapsterConverter<T,D>(TypeAdapterConfig.GlobalSettings)
        var mapsterConverter = new MapsterConverter<LegacyCustomer, ModernCustomerView>();
        var mappedWithMapster = mapsterConverter.Convert(source);

        Console.WriteLine($"  1. MapsterConverter<TSource, TDestination>() — default ctor (global Mapster config):");
        Console.WriteLine($"     Source: Name='{source.Name}', City='{source.City}'");
        Console.WriteLine($"     Target: Name='{mappedWithMapster.Name}', City='{mappedWithMapster.City}'");

        // 2. MapsterConverter<TSource, TDestination>(TypeAdapterConfig config) — explicit config constructor
        //    Use when you need an isolated Mapster configuration instance (e.g., multi-tenant, testing).
        var explicitConfig = new TypeAdapterConfig();
        explicitConfig.NewConfig<LegacyCustomer, ModernCustomerView>()
            .Map(dest => dest.Name, src => src.Name.Trim().ToUpperInvariant());

        var mapsterConverterWithConfig = new MapsterConverter<LegacyCustomer, ModernCustomerView>(explicitConfig);
        var mappedWithExplicitConfig = mapsterConverterWithConfig.Convert(source);

        Console.WriteLine($"\n  2. MapsterConverter<TSource, TDestination>(TypeAdapterConfig config) — explicit config ctor:");
        Console.WriteLine($"     Source: Name='{source.Name}', City='{source.City}'");
        Console.WriteLine($"     Target: Name='{mappedWithExplicitConfig.Name}', City='{mappedWithExplicitConfig.City}' (custom rule applied)");

        // 3. TypeAdapterConfig.UseConverter<TSource,TDest>(IConverter) extension method
        //    Registers an IConverter<T,D> implementation into Mapster's config pipeline.
        var config = new TypeAdapterConfig();
        var customConverter = new LegacyCustomerCustomConverter();
        config.UseConverter(customConverter);

        var adaptedViaConfig = source.Adapt<ModernCustomerView>(config);

        Console.WriteLine($"\n  3. TypeAdapterConfig.UseConverter(IConverter) extension:");
        Console.WriteLine($"     Target: Name='{adaptedViaConfig.Name}', City='{adaptedViaConfig.City}' (normalized to uppercase by custom converter)");
        Console.WriteLine();
    }
}
