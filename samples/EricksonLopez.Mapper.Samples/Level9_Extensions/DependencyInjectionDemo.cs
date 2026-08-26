// Copyright © Erickson Lopez. MIT License.
// =============================================================================
// [GenerateMapperRegistration] - Assembly-level attribute
//
// When applied to the assembly, the Generator emits a static extension class:
//
//   public static class MapperServiceCollectionExtensions
//   {
//       public static IServiceCollection AddGeneratedMappers(this IServiceCollection services)
//       {
//           services.AddSingleton<ExtensionsMapper>();
//           // ... all non-static [Mapper] classes
//           return services;
//       }
//   }
//
// This removes the need to manually register each mapper in the DI container.
// The generated method name is: AddGeneratedMappers()
// =============================================================================
using System;
using EricksonLopez.Mapper;
using Microsoft.Extensions.DependencyInjection;

[assembly: GenerateMapperRegistration]

namespace EricksonLopez.Mapper.Sample.Level9_Extensions;

/// <summary>Demonstrates auto-registration of all mapper classes using the <see cref="GenerateMapperRegistrationAttribute"/>.</summary>
public static class DependencyInjectionDemo
{
    /// <summary>Runs the dependency injection auto-registration demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 9: [GenerateMapperRegistration] - Auto DI Registration ===");
        Console.WriteLine();
        Console.WriteLine("The [assembly: GenerateMapperRegistration] attribute triggers the generator");
        Console.WriteLine("to emit an AddGeneratedMappers() extension method for IServiceCollection.");
        Console.WriteLine("This auto-registers ALL non-static [Mapper] classes as singletons.");
        Console.WriteLine();

        var services = new ServiceCollection();

        // This call is generated automatically by EricksonLopez.Mapper.Generator
        // when [assembly: GenerateMapperRegistration] is present.
        // The method registers all non-static [Mapper] classes in this assembly.
        services.AddGeneratedMappers();

        var provider = services.BuildServiceProvider();

        // Resolve a mapper directly from DI - no manual registration needed
        var mapper = provider.GetRequiredService<ExtensionsMapper>();

        var result = mapper.Map(new OrderEntity { OrderId = 999 });
        Console.WriteLine($"  Resolved ExtensionsMapper from DI container.");
        Console.WriteLine($"  Mapped OrderId: {result.OrderId}");
        Console.WriteLine();
        Console.WriteLine("  Generated AddGeneratedMappers() registers:");
        Console.WriteLine("    services.AddSingleton<ExtensionsMapper>();");
        Console.WriteLine("    services.AddSingleton<...>();  // all other [Mapper] classes");
        Console.WriteLine();
        Console.WriteLine("  Note: static mapper classes are NOT registered (they require no instance).");
        Console.WriteLine();
    }
}


