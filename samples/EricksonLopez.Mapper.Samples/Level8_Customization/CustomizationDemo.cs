// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level8_Customization;

/// <summary>Represents a legacy user record where the full name is stored as a single string.</summary>
public class LegacyUser
{
    /// <summary>Gets or sets the full name of the user as a single combined string.</summary>
    public string FullName { get; set; } = string.Empty;
}

/// <summary>Represents a modernized user record where first and last names are stored separately.</summary>
public class ModernUser
{
    /// <summary>Gets or sets the user's first name.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Gets or sets the user's last name.</summary>
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Converts a <see cref="LegacyUser"/> to a <see cref="ModernUser"/> by splitting
/// the combined full name into separate first and last name fields.
/// </summary>
public class LegacyToModernUserConverter : IConverter<LegacyUser, ModernUser>
{
    /// <inheritdoc/>
    public ModernUser Convert(LegacyUser source)
    {
        if (source == null || string.IsNullOrWhiteSpace(source.FullName))
            return new ModernUser();

        var parts = source.FullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            1 => new ModernUser { FirstName = parts[0], LastName = string.Empty },
            2 => new ModernUser { FirstName = parts[0], LastName = parts[1] },
            _ => new ModernUser()
        };
    }
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="LegacyUser"/> to <see cref="ModernUser"/>
/// using a static type-referenced <see cref="LegacyToModernUserConverter"/>.
/// </summary>
[Mapper]
public partial class CustomizationMapper
{
    /// <summary>Converts a <see cref="LegacyUser"/> to a <see cref="ModernUser"/> by splitting the full name.</summary>
    /// <param name="source">The legacy user to convert.</param>
    /// <returns>A new <see cref="ModernUser"/> with the first and last names separated.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    [UseConverter(typeof(LegacyToModernUserConverter))]
    public partial ModernUser Map(LegacyUser source);
}

/// <summary>
/// Demonstrates an instance mapper using an injected converter field referenced by <see cref="UseConverterAttribute(string)"/>.
/// </summary>
[Mapper]
public partial class InjectedConverterMapper
{
    private readonly IConverter<LegacyUser, ModernUser> _userConverter;

    /// <summary>Initializes a new instance of <see cref="InjectedConverterMapper"/> with an injected converter.</summary>
    /// <param name="userConverter">The injected converter instance.</param>
    public InjectedConverterMapper(IConverter<LegacyUser, ModernUser> userConverter)
    {
        _userConverter = userConverter ?? throw new ArgumentNullException(nameof(userConverter));
    }

    /// <summary>Delegates mapping to the injected converter instance held in <c>_userConverter</c>.</summary>
    /// <param name="source">The legacy user to convert.</param>
    /// <returns>The mapped modern user.</returns>
    [UseConverter(nameof(_userConverter))]
    public partial ModernUser MapWithInjected(LegacyUser source);
}

/// <summary>
/// Demonstrates fully custom mapping logic using <see cref="IConverter{TSource,TDestination}"/>
/// and both <see cref="UseConverterAttribute"/> overloads (Type and Field name).
/// </summary>
public static class CustomizationDemo
{
    /// <summary>Runs the customization demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 8: Customization ([UseConverter(Type)] & [UseConverter(FieldName)]) ===");
        Console.WriteLine("Demonstrating custom implementations using IConverter<TSource, TDestination> with [UseConverter].\n");

        // 1. Type-based converter
        var typeMapper = new CustomizationMapper();
        var source = new LegacyUser { FullName = "Erickson Lopez" };
        var target1 = typeMapper.Map(source);

        Console.WriteLine("  1. Type-based Converter [UseConverter(typeof(LegacyToModernUserConverter))]:");
        Console.WriteLine($"     Source FullName: '{source.FullName}'");
        Console.WriteLine($"     Target FirstName: '{target1.FirstName}', LastName: '{target1.LastName}'");
        Console.WriteLine();

        // 2. Field-based converter (DI injected instance)
        var injectedConverter = new LegacyToModernUserConverter();
        var fieldMapper = new InjectedConverterMapper(injectedConverter);
        var target2 = fieldMapper.MapWithInjected(source);

        Console.WriteLine("  2. Field-based Converter [UseConverter(nameof(_userConverter))]:");
        Console.WriteLine($"     Source FullName: '{source.FullName}'");
        Console.WriteLine($"     Target FirstName: '{target2.FirstName}', LastName: '{target2.LastName}'");
        Console.WriteLine();
    }
}
