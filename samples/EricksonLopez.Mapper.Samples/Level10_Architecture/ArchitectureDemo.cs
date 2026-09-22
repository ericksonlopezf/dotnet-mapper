// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level10_Architecture;

// Domain Layer
/// <summary>Represents a user entity in the domain layer, including sensitive data not exposed externally.</summary>
public class UserEntity
{
    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the user's email address.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Gets or sets the hashed password. Never exposed in the presentation layer.</summary>
    public string PasswordHash { get; set; } = string.Empty;
}

// Application Layer (DTOs)
/// <summary>Represents a read-only response containing only the publicly safe user profile fields.</summary>
public class UserResponse
{
    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the user's email address.</summary>
    public string Email { get; set; } = string.Empty;
}

// Infrastructure / Presentation Layer Mapper
/// <summary>
/// Provides compile-time-generated mapping from <see cref="UserEntity"/> to <see cref="UserResponse"/>,
/// explicitly excluding sensitive fields such as <c>PasswordHash</c>.
/// </summary>
[Mapper]
public partial class UserProfileMapper
{
    /// <summary>
    /// Maps a <see cref="UserEntity"/> to a <see cref="UserResponse"/>,
    /// explicitly excluding the sensitive source <c>PasswordHash</c> field via <see cref="MapIgnoreSourceAttribute"/>.
    /// </summary>
    /// <param name="source">The user entity to map from.</param>
    /// <returns>A new <see cref="UserResponse"/> containing only publicly safe fields.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    [MapIgnoreSource(nameof(UserEntity.PasswordHash))]
    public partial UserResponse Map(UserEntity source);
}

/// <summary>Demonstrates the mapper in a Clean Architecture context, mapping domain entities to response DTOs across layer boundaries.</summary>
public static class ArchitectureDemo
{
    /// <summary>Runs the architecture demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 10: Architecture ===");
        Console.WriteLine("Demonstrating how the mapper fits into Clean Architecture boundaries.");
        Console.WriteLine("Mappers are typically defined in the Application or Presentation layer to map Domain Entities to DTOs/ViewModels.\n");

        var mapper = new UserProfileMapper();
        var domainEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "architect@example.com",
            PasswordHash = "super_secret_hash"
        };

        var dto = mapper.Map(domainEntity);

        Console.WriteLine($"Mapped Domain Entity to DTO:");
        Console.WriteLine($"  ID: {dto.Id}");
        Console.WriteLine($"  Email: {dto.Email}");
        Console.WriteLine($"  (PasswordHash was safely excluded from the DTO structure)");
        Console.WriteLine();
    }
}
