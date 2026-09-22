// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level2_Configuration;

/// <summary>Represents a user entity with internal implementation details.</summary>
public class UserEntity
{
    /// <summary>Gets or sets the user's display name.</summary>
    public string FirstName { get; set; }
    /// <summary>Gets or sets the internal system key, not exposed in the public contract.</summary>
    public string InternalKey { get; set; }
    /// <summary>Gets or sets the hashed password. Never exposed in mapped output.</summary>
    public string PasswordHash { get; set; }
    /// <summary>Gets or sets an internal diagnostic token excluded directly via <see cref="MapperIgnoreAttribute"/>.</summary>
    [MapperIgnore]
    public string InternalSessionToken { get; set; } = "session-secret";
    /// <summary>Gets or sets a value indicating whether the user account is active.</summary>
    public bool IsActive { get; set; }
}

/// <summary>Represents the public-facing data transfer object for a user.</summary>
public class UserContract
{
    /// <summary>Gets or sets the user's display name.</summary>
    public string FirstName { get; set; }
    /// <summary>Gets or sets the external-facing identifier derived from the internal key.</summary>
    public string ExternalId { get; set; }
    /// <summary>Gets or sets a property that is explicitly excluded from the mapping.</summary>
    public string UnmappedProperty { get; set; }
    /// <summary>Gets or sets a value indicating whether the user account is active.</summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="UserEntity"/> to <see cref="UserContract"/>,
/// demonstrating member renaming with <see cref="MapPropertyAttribute"/>, destination exclusion with <see cref="MapIgnoreAttribute"/>,
/// and source exclusion with <see cref="MapIgnoreSourceAttribute"/>.
/// </summary>
[Mapper(StrictMapping = false)]
public partial class ConfiguredUserMapper
{
    /// <summary>
    /// Maps a <see cref="UserEntity"/> to a <see cref="UserContract"/>,
    /// renaming <c>InternalKey</c> to <c>ExternalId</c>, ignoring source <c>PasswordHash</c>,
    /// and ignoring destination <c>UnmappedProperty</c>.
    /// </summary>
    /// <param name="source">The user entity to map from.</param>
    /// <returns>A new <see cref="UserContract"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    [MapProperty("InternalKey", "ExternalId")]
    [MapIgnoreSource("PasswordHash")]
    [MapIgnore("UnmappedProperty")]
    public partial UserContract MapToContract(UserEntity source);
}

/// <summary>Demonstrates member renaming and exclusion using <see cref="MapPropertyAttribute"/>, <see cref="MapIgnoreAttribute"/>, <see cref="MapIgnoreSourceAttribute"/>, and <see cref="MapperIgnoreAttribute"/>.</summary>
public static class ConfigurationDemo
{
    /// <summary>Runs the configuration demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 2: Configuration ([MapProperty], [MapIgnore], [MapIgnoreSource], [MapperIgnore]) ===");

        var source = new UserEntity
        {
            FirstName = "Alice",
            InternalKey = "USR123",
            PasswordHash = "xxx-secret-hash",
            InternalSessionToken = "token-xyz",
            IsActive = true
        };
        var mapper = new ConfiguredUserMapper();

        var target = mapper.MapToContract(source);

        Console.WriteLine($"Source: FirstName={source.FirstName}, InternalKey={source.InternalKey}, PasswordHash={source.PasswordHash}, IsActive={source.IsActive}");
        Console.WriteLine($"Target: FirstName={target.FirstName}, ExternalId={target.ExternalId}, IsActive={target.IsActive}, UnmappedProperty='{target.UnmappedProperty}'");
        Console.WriteLine("  [MapProperty]: Re-mapped 'InternalKey' -> 'ExternalId'");
        Console.WriteLine("  [MapIgnoreSource]: Ignored source 'PasswordHash'");
        Console.WriteLine("  [MapperIgnore]: Ignored source property 'InternalSessionToken'");
        Console.WriteLine("  [MapIgnore]: Ignored destination 'UnmappedProperty'");
        Console.WriteLine();
    }
}
