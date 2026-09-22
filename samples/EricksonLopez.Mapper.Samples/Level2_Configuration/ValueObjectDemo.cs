// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level2_Configuration;

// =============================================================================
// Level 2 - [ValueObject] Attribute (Value Object / Strongly Typed ID)
//
// Problem:
//   In DDD (Domain-Driven Design), Value Objects and Strongly Typed IDs are used
//   instead of primitives to prevent type-confusion errors.
//   Example: using `CustomerId(Guid)` instead of a raw `Guid`.
//   When mapping to DTOs, the mapping needs to convert `CustomerId -> Guid` and vice versa.
//
// Solution:
//   Mark the Value Object with [ValueObject]. The generator detects this and generates:
//     - Source ValueObject -> Destination primitive: reads source.Value
//     - Source primitive -> Destination ValueObject: calls new ValueObject(value)
//
//   Alternatively, without [ValueObject], the generator heuristically detects:
//     - Classes/structs with exactly ONE public property named 'Value' of the primitive type.
//     - Readonly record structs with a single parameter of the primitive type.
//
// When to use:
//   - Always in DDD when you have strongly typed IDs.
//   - When mapping between layers (Domain -> DTO, DTO -> Command, etc.).
//
// When NOT to use:
//   - When types are completely different and conversion is non-trivial.
//     -> Use IConverter<T,T> in that case.
// =============================================================================

// --- Strongly Typed IDs (Value Objects) ---

/// <summary>
/// Represents a strongly typed customer identifier.
/// </summary>
/// <remarks>
/// The <see cref="ValueObjectAttribute"/> instructs the generator to treat this as a
/// wrapper over <see cref="Guid"/> for unwrapping and wrapping conversions.
/// </remarks>
/// <param name="Value">The underlying <see cref="Guid"/> identifier value.</param>
[ValueObject]
public readonly record struct CustomerId(Guid Value);

/// <summary>
/// Represents a strongly typed order identifier, auto-detected as a Value Object
/// by the generator heuristic for <see langword="readonly"/> record structs with a single parameter.
/// </summary>
/// <param name="Value">The underlying <see cref="Guid"/> identifier value.</param>
public readonly record struct OrderId(Guid Value);

/// <summary>
/// Represents a monetary amount as a Value Object.
/// </summary>
/// <param name="Value">The decimal monetary amount.</param>
[ValueObject]
public record Money(decimal Value);

// --- Domain Entity ---

/// <summary>Represents a customer order entity in the domain layer, using strongly typed identifiers and value objects.</summary>
public class CustomerOrderEntity
{
    /// <summary>Gets or sets the strongly typed customer identifier.</summary>
    public CustomerId CustomerId { get; set; }
    /// <summary>Gets or sets the strongly typed order identifier.</summary>
    public OrderId OrderId { get; set; }
    /// <summary>Gets or sets the name of the ordered product.</summary>
    public string ProductName { get; set; } = string.Empty;
    /// <summary>Gets or sets the total amount charged for the order.</summary>
    public Money TotalAmount { get; set; } = new Money(0m);
}

// --- Flat DTO (primitives) ---

/// <summary>Represents a flat data transfer object for a customer order, using primitive types for identifiers and amounts.</summary>
public class CustomerOrderDto
{
    /// <summary>Gets or sets the customer identifier as a raw <see cref="Guid"/>.</summary>
    public Guid CustomerId { get; set; }
    /// <summary>Gets or sets the order identifier as a raw <see cref="Guid"/>.</summary>
    public Guid OrderId { get; set; }
    /// <summary>Gets or sets the name of the ordered product.</summary>
    public string ProductName { get; set; } = string.Empty;
    /// <summary>Gets or sets the total amount charged for the order as a raw <see cref="decimal"/>.</summary>
    public decimal TotalAmount { get; set; }
}

// --- Reverse DTO (for wrapping) ---

/// <summary>Represents a command to create or update a customer order using primitive types that will be wrapped into Value Objects.</summary>
public class CustomerOrderCommand
{
    /// <summary>Gets or sets the customer identifier.</summary>
    public Guid CustomerId { get; set; }
    /// <summary>Gets or sets the order identifier.</summary>
    public Guid OrderId { get; set; }
    /// <summary>Gets or sets the name of the ordered product.</summary>
    public string ProductName { get; set; } = string.Empty;
    /// <summary>Gets or sets the total amount charged for the order.</summary>
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Provides compile-time-generated bidirectional mapping between domain entities using Value Objects
/// and flat DTOs using primitive types.
/// </summary>
[Mapper]
public partial class ValueObjectMapper
{
    /// <summary>
    /// Maps a <see cref="CustomerOrderEntity"/> to a <see cref="CustomerOrderDto"/>,
    /// unwrapping each Value Object to its underlying primitive.
    /// </summary>
    /// <param name="source">The domain entity to map from.</param>
    /// <returns>A new <see cref="CustomerOrderDto"/> with all identifiers and amounts as primitives.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial CustomerOrderDto MapToDto(CustomerOrderEntity source);

    /// <summary>
    /// Maps a <see cref="CustomerOrderCommand"/> to a <see cref="CustomerOrderEntity"/>,
    /// wrapping each primitive identifier and amount into the corresponding Value Object.
    /// </summary>
    /// <param name="source">The command to map from.</param>
    /// <returns>A new <see cref="CustomerOrderEntity"/> with all identifiers and amounts as Value Objects.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial CustomerOrderEntity MapToEntity(CustomerOrderCommand source);
}

/// <summary>Demonstrates bidirectional mapping between domain Value Objects and primitive DTOs.</summary>
public static class ValueObjectDemo
{
    /// <summary>Runs the Value Object mapping demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 2: [ValueObject] - Strongly Typed IDs and Value Objects ===");
        Console.WriteLine();
        Console.WriteLine("Scenario: Bidirectional mapping between DDD entities with Value Objects and flat DTOs.");
        Console.WriteLine();

        var mapper = new ValueObjectMapper();

        // Unwrapping (Entity -> DTO)
        var entity = new CustomerOrderEntity
        {
            CustomerId = new CustomerId(Guid.Parse("11111111-0000-0000-0000-000000000001")),
            OrderId = new OrderId(Guid.Parse("22222222-0000-0000-0000-000000000002")),
            ProductName = "Enterprise Software License",
            TotalAmount = new Money(4999.00m)
        };

        var dto = mapper.MapToDto(entity);

        Console.WriteLine("  Entity -> DTO (Unwrapping Value Objects):");
        Console.WriteLine($"    entity.CustomerId  = CustomerId({entity.CustomerId.Value})");
        Console.WriteLine($"    dto.CustomerId     = {dto.CustomerId}  (raw Guid)");
        Console.WriteLine($"    entity.TotalAmount = Money({entity.TotalAmount.Value})");
        Console.WriteLine($"    dto.TotalAmount    = {dto.TotalAmount}  (raw decimal)");
        Console.WriteLine();

        // Wrapping (Command -> Entity)
        var command = new CustomerOrderCommand
        {
            CustomerId = Guid.Parse("33333333-0000-0000-0000-000000000003"),
            OrderId = Guid.Parse("44444444-0000-0000-0000-000000000004"),
            ProductName = "Cloud Subscription",
            TotalAmount = 299.99m
        };

        var newEntity = mapper.MapToEntity(command);

        Console.WriteLine("  Command -> Entity (Wrapping primitives into Value Objects):");
        Console.WriteLine($"    command.CustomerId      = {command.CustomerId}");
        Console.WriteLine($"    entity.CustomerId       = CustomerId({newEntity.CustomerId.Value})");
        Console.WriteLine($"    command.TotalAmount     = {command.TotalAmount}");
        Console.WriteLine($"    entity.TotalAmount      = Money({newEntity.TotalAmount.Value})");
        Console.WriteLine();
        Console.WriteLine("  Equivalent generated code:");
        Console.WriteLine("    CustomerId  = new CustomerId(source.CustomerId),");
        Console.WriteLine("    TotalAmount = new Money(source.TotalAmount)");
        Console.WriteLine();
    }
}
