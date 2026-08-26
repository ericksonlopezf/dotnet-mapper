// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level3_RealWorld;

// =============================================================================
// Level 3 - Built-in Type Conversions
//
// The generator automatically supports (without additional configuration):
//
// enum -> string            (emits: source.EnumProp.ToString())
// string -> enum            (emits: Enum.Parse<TEnum>(source.StringProp))
// Guid -> string            (emits: source.GuidProp.ToString())
// string -> Guid            (emits: Guid.Parse(source.StringProp))
// DateTime -> DateOnly      (emits: DateOnly.FromDateTime(source.DateTimeProp))
// DateTime -> DateTimeOffset (emits: new DateTimeOffset(source.DateTimeProp))
// Numeric widening          (byte->int, int->long, float->double, etc.)
//
// Numeric narrowing (int -> byte, etc.) = ELM003 (compile-time error)
//   Solution: use [UseConverter] with an explicit IConverter<int, byte>.
//
// IMPORTANT: Source and destination property names must match (case-insensitive).
// If names differ, use [MapProperty("Src", "Dst")].
// =============================================================================

/// <summary>Specifies the fulfillment state of a sales order.</summary>
public enum OrderStatus
{
    /// <summary>The order has been placed but not yet processed.</summary>
    Pending,
    /// <summary>The order is currently being processed.</summary>
    Processing,
    /// <summary>The order has been shipped to the customer.</summary>
    Shipped,
    /// <summary>The order has been delivered to the customer.</summary>
    Delivered,
    /// <summary>The order has been cancelled.</summary>
    Cancelled
}

/// <summary>Specifies the authorization state of a payment transaction.</summary>
public enum PaymentStatus
{
    /// <summary>The payment is awaiting authorization.</summary>
    Pending,
    /// <summary>The payment has been authorized but not yet captured.</summary>
    Authorized,
    /// <summary>The payment has been captured and funds collected.</summary>
    Captured,
    /// <summary>The payment has been refunded to the customer.</summary>
    Refunded,
    /// <summary>The payment authorization or capture failed.</summary>
    Failed
}

/// <summary>Represents a sales order entity with typed scalar fields.</summary>
public class SalesOrderEntity
{
    /// <summary>Gets or sets the unique identifier of the order.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the current fulfillment status of the order.</summary>
    public OrderStatus Status { get; set; }
    /// <summary>Gets or sets the current payment status of the order.</summary>
    public PaymentStatus PaymentStatus { get; set; }
    /// <summary>Gets or sets the UTC date and time when the order was created.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Gets or sets the external reference identifier for the order.</summary>
    public string ExternalReference { get; set; } = string.Empty;
    /// <summary>Gets or sets the total number of line items in the order.</summary>
    public int TotalItems { get; set; }
    /// <summary>Gets or sets the total weight of the order in grams.</summary>
    public long TotalWeightGrams { get; set; }
}

/// <summary>Represents an API response DTO for a sales order with string-typed identifiers and status fields.</summary>
public class SalesOrderApiDto
{
    /// <summary>Gets or sets the order identifier as a string (mapped from <see cref="Guid"/> via <c>.ToString()</c>).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the fulfillment status as a string (mapped from <see cref="OrderStatus"/> via <c>.ToString()</c>).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the payment status as a string (mapped from <see cref="PaymentStatus"/> via <c>.ToString()</c>).</summary>
    public string PaymentStatus { get; set; } = string.Empty;

    /// <summary>Gets or sets the creation date (mapped from <see cref="DateTime"/> via <c>DateOnly.FromDateTime</c>).</summary>
    public DateOnly CreatedDate { get; set; }

    /// <summary>Gets or sets the external reference identifier for the order.</summary>
    public string ExternalReference { get; set; } = string.Empty;

    /// <summary>Gets or sets the total number of items (widened from <c>int</c> to <c>long</c> automatically).</summary>
    public long TotalItems { get; set; }

    /// <summary>Gets or sets the total weight of the order in grams.</summary>
    public long TotalWeightGrams { get; set; }
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="SalesOrderEntity"/> to <see cref="SalesOrderApiDto"/>,
/// demonstrating built-in type conversions.
/// </summary>
[Mapper]
public partial class SalesOrderMapper
{
    /// <summary>
    /// Maps a <see cref="SalesOrderEntity"/> to a <see cref="SalesOrderApiDto"/>,
    /// remapping <c>CreatedAt</c> (DateTime) to <c>CreatedDate</c> (DateOnly).
    /// </summary>
    /// <param name="source">The sales order entity to map from.</param>
    /// <returns>A new <see cref="SalesOrderApiDto"/> with all fields mapped using built-in conversions.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    [MapProperty("CreatedAt", "CreatedDate")]
    public partial SalesOrderApiDto MapToApi(SalesOrderEntity source);
}

/// <summary>Represents an incoming order request with string-typed identifiers and status fields.</summary>
public class IncomingOrderRequest
{
    /// <summary>Gets or sets the order identifier as a string (will be parsed to <see cref="Guid"/>).</summary>
    public string OrderId { get; set; } = Guid.NewGuid().ToString();
    /// <summary>Gets or sets the fulfillment status as a string (will be parsed to <see cref="OrderStatus"/>).</summary>
    public string Status { get; set; } = "Processing";
    /// <summary>Gets or sets the payment status as a string (will be parsed to <see cref="PaymentStatus"/>).</summary>
    public string PaymentStatus { get; set; } = "Authorized";
}

/// <summary>Represents a command to create or update an order with strongly typed identifiers and enumerations.</summary>
public class OrderCommand
{
    /// <summary>Gets or sets the order identifier.</summary>
    public Guid OrderId { get; set; }
    /// <summary>Gets or sets the current fulfillment status of the order.</summary>
    public OrderStatus Status { get; set; }
    /// <summary>Gets or sets the current payment status of the order.</summary>
    public PaymentStatus PaymentStatus { get; set; }
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="IncomingOrderRequest"/> to <see cref="OrderCommand"/>,
/// converting string identifiers and status values to their strongly typed equivalents.
/// </summary>
[Mapper]
public partial class IncomingOrderMapper
{
    /// <summary>Maps an <see cref="IncomingOrderRequest"/> to an <see cref="OrderCommand"/>.</summary>
    /// <param name="source">The incoming request to map from.</param>
    /// <returns>A new <see cref="OrderCommand"/> with parsed identifiers and enum values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public partial OrderCommand MapToCommand(IncomingOrderRequest source);
}

/// <summary>Represents a domain event entity.</summary>
public class EventEntity
{
    /// <summary>Gets or sets the unique identifier of the event.</summary>
    public Guid EventId { get; set; }
    /// <summary>Gets or sets the name of the event.</summary>
    public string EventName { get; set; } = string.Empty;
    /// <summary>Gets or sets the UTC date and time when the event occurred.</summary>
    public DateTime OccurredAt { get; set; }
}

/// <summary>Represents a data transfer object for a domain event.</summary>
public class EventDto
{
    /// <summary>Gets or sets the unique identifier of the event.</summary>
    public Guid EventId { get; set; }
    /// <summary>Gets or sets the name of the event.</summary>
    public string EventName { get; set; } = string.Empty;
    /// <summary>Gets or sets the date and time when the event occurred, with timezone offset information.</summary>
    public DateTimeOffset OccurredAt { get; set; }
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="EventEntity"/> to <see cref="EventDto"/>,
/// converting <see cref="DateTime"/> to <see cref="DateTimeOffset"/> via <c>new DateTimeOffset(value)</c>.
/// </summary>
[Mapper]
public partial class EventMapper
{
    /// <summary>Maps an <see cref="EventEntity"/> to an <see cref="EventDto"/>.</summary>
    /// <param name="source">The event entity to map from.</param>
    /// <returns>A new <see cref="EventDto"/> with the occurrence time converted to <see cref="DateTimeOffset"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public partial EventDto MapEvent(EventEntity source);
}

/// <summary>Demonstrates the built-in scalar type conversions supported by the source generator.</summary>
public static class BuiltinConversionsDemo
{
    /// <summary>Runs the built-in type conversions demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 3: Built-in Type Conversions ===");
        Console.WriteLine();
        Console.WriteLine("The generator converts these type pairs automatically,");
        Console.WriteLine("without any additional converter:");
        Console.WriteLine();

        var entity = new SalesOrderEntity
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Shipped,
            PaymentStatus = PaymentStatus.Captured,
            CreatedAt = new DateTime(2024, 3, 15, 10, 30, 0, DateTimeKind.Utc),
            ExternalReference = "EXT-2024-0042",
            TotalItems = 5,
            TotalWeightGrams = 2500L
        };

        var mapper = new SalesOrderMapper();
        var dto = mapper.MapToApi(entity);

        Console.WriteLine("  Entity -> ApiDto:");
        Console.WriteLine($"    Guid      -> string  : '{dto.Id}'");
        Console.WriteLine($"    enum      -> string  : '{dto.Status}' (OrderStatus.Shipped)");
        Console.WriteLine($"    enum      -> string  : '{dto.PaymentStatus}' (PaymentStatus.Captured)");
        Console.WriteLine($"    DateTime  -> DateOnly: {dto.CreatedDate}  [via MapProperty + built-in]");
        Console.WriteLine($"    int       -> long    : {dto.TotalItems}  (numeric widening)");
        Console.WriteLine();

        var request = new IncomingOrderRequest
        {
            OrderId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
            Status = "Delivered",
            PaymentStatus = "Captured"
        };

        var incomingMapper = new IncomingOrderMapper();
        var command = incomingMapper.MapToCommand(request);

        Console.WriteLine("  Request -> Command (inverse):");
        Console.WriteLine($"    string -> Guid: {command.OrderId}");
        Console.WriteLine($"    string -> enum: {command.Status}  (OrderStatus)");
        Console.WriteLine($"    string -> enum: {command.PaymentStatus}  (PaymentStatus)");
        Console.WriteLine();

        var eventEntity = new EventEntity
        {
            EventId = Guid.NewGuid(),
            EventName = "UserRegistered",
            OccurredAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
        };

        var eventMapper = new EventMapper();
        var eventDto = eventMapper.MapEvent(eventEntity);

        Console.WriteLine("  EventEntity -> EventDto:");
        Console.WriteLine($"    DateTime -> DateTimeOffset: {eventDto.OccurredAt:O}");
        Console.WriteLine();
    }
}
