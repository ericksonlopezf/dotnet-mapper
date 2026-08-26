// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level2_Configuration;

// =============================================================================
// Level 2 - [MapValue] Attribute (Constant and Computed Value Injection)
//
// Problem:
//   A destination DTO requires properties that do not exist on the source entity,
//   such as an audit timestamp (DateTime.UtcNow), an environment tag ("Production"),
//   or a fixed status flag ("ACTIVE"), without requiring a full IConverter.
//
// Solution:
//   [MapValue("DestinationPropertyName", "literalCSharpExpression")]
//   Embeds the C# expression directly into the destination assignment or constructor.
//
// When to use:
//   - Injecting runtime constants (e.g. environment names, API version tags).
//   - Emitting timestamp calls (e.g. DateTime.UtcNow).
//   - Supplying fixed values to required constructor parameters.
// =============================================================================

/// <summary>Represents incoming payload data.</summary>
public class IncomingRegistrationRequest
{
    /// <summary>Gets or sets the username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address.</summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>Represents an account entity with system-assigned metadata.</summary>
public class AccountEntity
{
    /// <summary>Gets or sets the username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Gets or sets the email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the initial account status assigned via <see cref="MapValueAttribute"/>.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC creation timestamp assigned via <see cref="MapValueAttribute"/>.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the deployment environment assigned via <see cref="MapValueAttribute"/>.</summary>
    public string Environment { get; set; } = string.Empty;
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="IncomingRegistrationRequest"/> to <see cref="AccountEntity"/>,
/// using <see cref="MapValueAttribute"/> to inject literal values.
/// </summary>
[Mapper]
public partial class AccountRegistrationMapper
{
    /// <summary>
    /// Maps an <see cref="IncomingRegistrationRequest"/> to an <see cref="AccountEntity"/>,
    /// populating metadata via <see cref="MapValueAttribute"/>.
    /// </summary>
    /// <param name="source">The registration request.</param>
    /// <returns>A new <see cref="AccountEntity"/> with injected values.</returns>
    [MapValue("Status", "\"ACTIVE\"")]
    [MapValue("CreatedAt", "System.DateTime.UtcNow")]
    [MapValue("Environment", "\"Production\"")]
    public partial AccountEntity Map(IncomingRegistrationRequest source);
}

/// <summary>Demonstrates constant and computed expression assignment using <see cref="MapValueAttribute"/>.</summary>
public static class ValueAssignmentDemo
{
    /// <summary>Runs the value assignment demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 2: [MapValue] - Constant & Computed Value Injection ===");
        Console.WriteLine();

        var request = new IncomingRegistrationRequest
        {
            Username = "jdoe",
            Email = "john.doe@example.com"
        };

        var mapper = new AccountRegistrationMapper();
        var entity = mapper.Map(request);

        Console.WriteLine($"  Source: Username='{request.Username}', Email='{request.Email}'");
        Console.WriteLine($"  Target: Username='{entity.Username}', Email='{entity.Email}'");
        Console.WriteLine($"          Status='{entity.Status}'           <- [MapValue(\"Status\", \"\\\"ACTIVE\\\"\")]");
        Console.WriteLine($"          CreatedAt={entity.CreatedAt:O} <- [MapValue(\"CreatedAt\", \"DateTime.UtcNow\")]");
        Console.WriteLine($"          Environment='{entity.Environment}'   <- [MapValue(\"Environment\", \"\\\"Production\\\"\")]");
        Console.WriteLine();
    }
}
