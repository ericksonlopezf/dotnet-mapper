// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level1_QuickStart;

/// <summary>Represents a user entity with basic personal information.</summary>
public class SimpleUser
{
    /// <summary>Gets or sets the user's first name.</summary>
    public string FirstName { get; set; }
    /// <summary>Gets or sets the user's last name.</summary>
    public string LastName { get; set; }
    /// <summary>Gets or sets the user's age in years.</summary>
    public int Age { get; set; }
}

/// <summary>Represents a data transfer object for a user with basic personal information.</summary>
public class SimpleUserDto
{
    /// <summary>Gets or sets the user's first name.</summary>
    public string FirstName { get; set; }
    /// <summary>Gets or sets the user's last name.</summary>
    public string LastName { get; set; }
    /// <summary>Gets or sets the user's age in years.</summary>
    public int Age { get; set; }
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="SimpleUser"/> to <see cref="SimpleUserDto"/>.
/// </summary>
[Mapper]
public partial class SimpleUserMapper
{
    // By declaring the partial method, Roslyn implements the mapping code at compile time.
    /// <summary>Maps a <see cref="SimpleUser"/> to a <see cref="SimpleUserDto"/>.</summary>
    /// <param name="source">The source user entity to map from.</param>
    /// <returns>A new <see cref="SimpleUserDto"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial SimpleUserDto Map(SimpleUser source);
}

/// <summary>Demonstrates the minimal mapper setup required to perform object mapping.</summary>
public static class QuickStartDemo
{
    /// <summary>Runs the quick-start demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 1: Quick Start ===");

        var source = new SimpleUser { FirstName = "John", LastName = "Doe", Age = 30 };
        var mapper = new SimpleUserMapper();

        var target = mapper.Map(source);

        Console.WriteLine($"Source: {source.FirstName} {source.LastName}, Age: {source.Age}");
        Console.WriteLine($"Target: {target.FirstName} {target.LastName}, Age: {target.Age}");
        Console.WriteLine("Mapping successful using auto-generated SimpleUserMapper!");
        Console.WriteLine();
    }
}

