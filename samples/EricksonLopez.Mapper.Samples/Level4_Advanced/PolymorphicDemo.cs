// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level4_Advanced;

// =============================================================================
// Level 4 - [MapDerivedType] Attribute (Polymorphic Mapping)
//
// Problem:
//   You have a type hierarchy in the domain (e.g., Vehicle -> Car, Truck) and
//   need to map to an equivalent DTO hierarchy (VehicleDto -> CarDto, TruckDto).
//   The generic Map(VehicleEntity) method must instantiate the correct DTO
//   based on the actual runtime type of the source object.
//
// Solution:
//   Declare multiple [MapDerivedType(sourceType, destinationType)] on the base
//   method so the generator emits a switch-case using pattern matching:
//
//     switch (source)
//     {
//         case CarEntity derived:   return MapCar(derived);
//         case TruckEntity derived: return MapTruck(derived);
//     }
//
// Diagnostic ELM011 (Warning):
//   If the destination class is abstract, the generator emits ELM011 because
//   it cannot create an instance of the base class if no derived type matches.
//   To avoid this warning, ensure all derived types are covered.
//
// When to use:
//   - Inheritance hierarchies in the domain (polymorphism).
//   - Polymorphic serialization that requires type discriminators.
//   - CQRS where commands derive from a common base class.
//
// When NOT to use:
//   - When types are completely independent (use separate mapping methods instead).
// =============================================================================

// --- Domain hierarchy (Source) ---

/// <summary>Represents the abstract base for all vehicle entities in the domain.</summary>
public abstract class VehicleEntity
{
    /// <summary>Gets or sets the unique identifier of the vehicle.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the vehicle brand name.</summary>
    public string Brand { get; set; } = string.Empty;
    /// <summary>Gets or sets the model year of the vehicle.</summary>
    public int Year { get; set; }
}

/// <summary>Represents a car entity with a door count.</summary>
public class CarEntity : VehicleEntity
{
    /// <summary>Gets or sets the number of doors on the car.</summary>
    public int NumberOfDoors { get; set; }
}

/// <summary>Represents a truck entity with a maximum load capacity.</summary>
public class TruckEntity : VehicleEntity
{
    /// <summary>Gets or sets the maximum load capacity of the truck in metric tons.</summary>
    public decimal MaxLoadTons { get; set; }
}

// --- DTO hierarchy (Destination) ---
// NOTE: VehicleDto is non-abstract so the generator can emit compilable code
// if it needs to instantiate the base type (avoids ELM011 warning).

/// <summary>Represents a data transfer object for a vehicle.</summary>
public class VehicleDto
{
    /// <summary>Gets or sets the unique identifier of the vehicle.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the vehicle brand name.</summary>
    public string Brand { get; set; } = string.Empty;
    /// <summary>Gets or sets the model year of the vehicle.</summary>
    public int Year { get; set; }
}

/// <summary>Represents a data transfer object for a car, including the number of doors.</summary>
public class CarDto : VehicleDto
{
    /// <summary>Gets or sets the number of doors on the car.</summary>
    public int NumberOfDoors { get; set; }
}

/// <summary>Represents a data transfer object for a truck, including the maximum load capacity.</summary>
public class TruckDto : VehicleDto
{
    /// <summary>Gets or sets the maximum load capacity of the truck in metric tons.</summary>
    public decimal MaxLoadTons { get; set; }
}

// --- Polymorphic mapper ---

/// <summary>
/// Provides compile-time-generated polymorphic mapping for the vehicle hierarchy,
/// dispatching to the correct derived mapping method based on the source object's runtime type.
/// </summary>
[Mapper]
public partial class VehicleMapper
{
    /// <summary>
    /// Maps a <see cref="VehicleEntity"/> to the correct <see cref="VehicleDto"/> subtype
    /// based on the runtime type of <paramref name="source"/>.
    /// </summary>
    /// <remarks>
    /// The generator emits a <see langword="switch"/> expression that pattern-matches
    /// the source and delegates to <see cref="MapCar"/> or <see cref="MapTruck"/>
    /// without using reflection.
    /// </remarks>
    /// <param name="source">The vehicle entity to map from.</param>
    /// <returns>
    /// A <see cref="CarDto"/> when <paramref name="source"/> is a <see cref="CarEntity"/>,
    /// or a <see cref="TruckDto"/> when it is a <see cref="TruckEntity"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    [MapDerivedType(typeof(CarEntity), typeof(CarDto))]
    [MapDerivedType(typeof(TruckEntity), typeof(TruckDto))]
    public partial VehicleDto Map(VehicleEntity source);

    /// <summary>Maps a <see cref="CarEntity"/> to a <see cref="CarDto"/>.</summary>
    /// <param name="source">The car entity to map from.</param>
    /// <returns>A new <see cref="CarDto"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial CarDto MapCar(CarEntity source);

    /// <summary>Maps a <see cref="TruckEntity"/> to a <see cref="TruckDto"/>.</summary>
    /// <param name="source">The truck entity to map from.</param>
    /// <returns>A new <see cref="TruckDto"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial TruckDto MapTruck(TruckEntity source);
}

/// <summary>Demonstrates polymorphic mapping using <see cref="MapDerivedTypeAttribute"/>.</summary>
public static class PolymorphicDemo
{
    /// <summary>Runs the polymorphic mapping demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 4: [MapDerivedType] - Polymorphic Mapping ===");
        Console.WriteLine();
        Console.WriteLine("Scenario: a mixed vehicle fleet is mapped to the correct DTO");
        Console.WriteLine("based on the actual runtime type, without reflection.");
        Console.WriteLine();

        var mapper = new VehicleMapper();

        // Mixed vehicle list (polymorphic)
        VehicleEntity[] fleet =
        {
            new CarEntity   { Id = Guid.NewGuid(), Brand = "Toyota",   Year = 2022, NumberOfDoors = 4 },
            new TruckEntity { Id = Guid.NewGuid(), Brand = "Volvo",    Year = 2021, MaxLoadTons = 20.5m },
            new CarEntity   { Id = Guid.NewGuid(), Brand = "Honda",    Year = 2023, NumberOfDoors = 2 },
            new TruckEntity { Id = Guid.NewGuid(), Brand = "Scania",   Year = 2020, MaxLoadTons = 35.0m },
        };

        foreach (var vehicle in fleet)
        {
            // Single call - the generator dispatches automatically
            var dto = mapper.Map(vehicle);

            switch (dto)
            {
                case CarDto car:
                    Console.WriteLine($"  Car   | {car.Brand} {car.Year} | Doors: {car.NumberOfDoors}");
                    break;
                case TruckDto truck:
                    Console.WriteLine($"  Truck | {truck.Brand} {truck.Year} | Load: {truck.MaxLoadTons}t");
                    break;
            }
        }

        Console.WriteLine();
        Console.WriteLine("  The generator emitted a pattern matching switch expression.");
        Console.WriteLine("  Each derived type was mapped to the corresponding DTO without reflection.");
        Console.WriteLine();
    }
}
