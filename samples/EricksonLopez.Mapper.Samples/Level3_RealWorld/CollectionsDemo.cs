// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level3_RealWorld;

/// <summary>Represents a configuration entity whose settings are stored as name-integer pairs.</summary>
public class ConfigurationEntity
{
    /// <summary>Gets or sets the configuration settings as a map of setting names to integer values.</summary>
    public Dictionary<string, int> Settings { get; set; }
}

/// <summary>Represents a data transfer object for configuration settings serialized as name-string pairs.</summary>
public class ConfigurationDto
{
    /// <summary>Gets or sets the configuration settings as a map of setting names to string values.</summary>
    public Dictionary<string, string> Settings { get; set; }
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="ConfigurationEntity"/> to <see cref="ConfigurationDto"/>,
/// converting integer values to strings within the dictionary.
/// </summary>
[Mapper]
public partial class ConfigurationMapper
{
    /// <summary>Maps a <see cref="ConfigurationEntity"/> to a <see cref="ConfigurationDto"/>.</summary>
    /// <param name="source">The configuration entity to map from.</param>
    /// <returns>A new <see cref="ConfigurationDto"/> with all integer values converted to strings.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial ConfigurationDto Map(ConfigurationEntity source);

    /// <summary>Converts an integer configuration value to its string representation.</summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The string representation of <paramref name="value"/>.</returns>
    public partial string MapIntToString(int value);
}

// Partial implementation of the mapping helper
/// <summary>Provides the implementation of <see cref="MapIntToString(int)"/> for <see cref="ConfigurationMapper"/>.</summary>
public partial class ConfigurationMapper
{
    /// <inheritdoc/>
    public partial string MapIntToString(int value) => value.ToString();
}

/// <summary>Demonstrates dictionary mapping with element-level type conversion using the source generator.</summary>
public static class CollectionsDemo
{
    /// <summary>Runs the collections and dictionary mapping demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 3: Collections & Dictionaries ===");

        var source = new ConfigurationEntity
        {
            Settings = new Dictionary<string, int>
            {
                { "MaxUsers", 100 },
                { "TimeoutSeconds", 30 }
            }
        };

        var mapper = new ConfigurationMapper();
        var target = mapper.Map(source);

        Console.WriteLine("Mapped Dictionary<string, int> to Dictionary<string, string>:");
        foreach (var kvp in target.Settings)
        {
            Console.WriteLine($"  - {kvp.Key}: \"{kvp.Value}\" (Type: {kvp.Value.GetType().Name})");
        }
        Console.WriteLine();
    }
}
