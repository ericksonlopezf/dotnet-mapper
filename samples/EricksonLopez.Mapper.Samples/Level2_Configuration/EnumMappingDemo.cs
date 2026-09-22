// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level2_Configuration;

// =============================================================================
// Level 2 - [EnumMappingStrategy] and [MapEnumValue] (Advanced Enum Strategies)
//
// Problem:
//   Enums in different layers often differ in casing (e.g. `pending` vs `Pending`),
//   underlying numeric values (e.g. `Active = 1` vs `Active = 10`), or member names
//   (e.g. `InProgress` vs `Processing`).
//
// Solution:
//   1. [EnumMappingStrategy(EnumMappingStrategy.ByName, IgnoreCase = true)]:
//      Matches enum members case-insensitively.
//   2. [EnumMappingStrategy(EnumMappingStrategy.ByValue)]:
//      Maps enum members directly by their underlying integer/numeric values.
//      Generated: Status = (TargetStatus)(int)source.Status
//   3. [MapEnumValue(SourceEnum.Val, TargetEnum.Val)]:
//      Explicitly remaps individual enum members when names completely differ.
// =============================================================================

// ------------------------------------------------------------------------------------
// Scenario A: ByName + IgnoreCase (casing mismatch between source and destination)
// ------------------------------------------------------------------------------------

/// <summary>Source priority enum in lowercase.</summary>
public enum SourcePriority
{
    /// <summary>Low priority.</summary>
    low,
    /// <summary>Medium priority.</summary>
    medium,
    /// <summary>High priority.</summary>
    high
}

/// <summary>Destination priority enum in PascalCase.</summary>
public enum TargetPriority
{
    /// <summary>Low priority.</summary>
    Low,
    /// <summary>Medium priority.</summary>
    Medium,
    /// <summary>High priority.</summary>
    High
}

/// <summary>Source status enum.</summary>
public enum PaymentStatus
{
    /// <summary>Pending processing.</summary>
    Pending,
    /// <summary>Payment captured.</summary>
    Captured,
    /// <summary>Payment rejected.</summary>
    Rejected
}

/// <summary>Destination status enum with different names.</summary>
public enum OrderProcessingStatus
{
    /// <summary>Queued for processing.</summary>
    Queued,
    /// <summary>Successfully processed.</summary>
    Completed,
    /// <summary>Failed during processing.</summary>
    Failed
}

/// <summary>Source ticket item.</summary>
public class SupportTicketEntity
{
    /// <summary>Gets or sets the ticket identifier.</summary>
    public int TicketId { get; set; }
    /// <summary>Gets or sets the priority.</summary>
    public SourcePriority Priority { get; set; }
    /// <summary>Gets or sets the payment status.</summary>
    public PaymentStatus Status { get; set; }
}

/// <summary>Destination ticket DTO.</summary>
public class SupportTicketDto
{
    /// <summary>Gets or sets the ticket identifier.</summary>
    public int TicketId { get; set; }
    /// <summary>Gets or sets the mapped priority.</summary>
    public TargetPriority Priority { get; set; }
    /// <summary>Gets or sets the remapped order processing status.</summary>
    public OrderProcessingStatus Status { get; set; }
}

/// <summary>
/// Provides compile-time-generated mapping demonstrating enum mapping strategies and explicit member remapping.
/// </summary>
[Mapper]
[EnumMappingStrategy(EnumMappingStrategy.ByName, IgnoreCase = true)]
public partial class SupportTicketMapper
{
    /// <summary>
    /// Maps a <see cref="SupportTicketEntity"/> to a <see cref="SupportTicketDto"/>,
    /// applying case-insensitive matching for Priority and explicit remapping for Status.
    /// </summary>
    /// <param name="source">The source entity.</param>
    /// <returns>A new <see cref="SupportTicketDto"/>.</returns>
    [MapEnumValue(PaymentStatus.Pending, OrderProcessingStatus.Queued)]
    [MapEnumValue(PaymentStatus.Captured, OrderProcessingStatus.Completed)]
    [MapEnumValue(PaymentStatus.Rejected, OrderProcessingStatus.Failed)]
    public partial SupportTicketDto Map(SupportTicketEntity source);
}

// ------------------------------------------------------------------------------------
// Scenario B: ByValue (numeric cast between enums with shared integer values)
// ------------------------------------------------------------------------------------
// Use case: two enums where the underlying int values represent the same concept
// and are numerically equivalent. The generator emits: (TargetEnum)(int)source.Prop
// ------------------------------------------------------------------------------------

/// <summary>Source HTTP-like status code enum with explicit integer values.</summary>
public enum SourceHttpStatus
{
    /// <summary>Success response (200).</summary>
    Ok = 200,
    /// <summary>Not found response (404).</summary>
    NotFound = 404,
    /// <summary>Internal server error response (500).</summary>
    InternalServerError = 500
}

/// <summary>Destination HTTP-like status code enum — same values, different type/namespace.</summary>
public enum TargetHttpStatus
{
    /// <summary>Success response (200).</summary>
    Ok = 200,
    /// <summary>Not found response (404).</summary>
    NotFound = 404,
    /// <summary>Internal server error response (500).</summary>
    InternalServerError = 500
}

/// <summary>Source response entity.</summary>
public class ApiResponseEntity
{
    /// <summary>Gets or sets the HTTP status code.</summary>
    public SourceHttpStatus StatusCode { get; set; }
    /// <summary>Gets or sets the response message.</summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>Destination response DTO.</summary>
public class ApiResponseDto
{
    /// <summary>Gets or sets the mapped HTTP status code.</summary>
    public TargetHttpStatus StatusCode { get; set; }
    /// <summary>Gets or sets the response message.</summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Demonstrates <see cref="EnumMappingStrategy.ByValue"/> on a specific method,
/// mapping enum members by their underlying integer values rather than by name.
/// </summary>
/// <remarks>
/// Generated code: <c>StatusCode = (TargetHttpStatus)(int)source.StatusCode,</c>
/// This is safe when source and destination enums share the same integer value semantics.
/// </remarks>
[Mapper]
public partial class ApiResponseMapper
{
    /// <summary>
    /// Maps an <see cref="ApiResponseEntity"/> to an <see cref="ApiResponseDto"/>,
    /// converting the <see cref="SourceHttpStatus"/> to <see cref="TargetHttpStatus"/> by value.
    /// </summary>
    /// <param name="source">The API response entity to map from.</param>
    /// <returns>A new <see cref="ApiResponseDto"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    [EnumMappingStrategy(EnumMappingStrategy.ByValue)]
    public partial ApiResponseDto Map(ApiResponseEntity source);
}

/// <summary>Demonstrates enum mapping strategies and member remapping.</summary>
public static class EnumMappingDemo
{
    /// <summary>Runs the enum mapping demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 2: [EnumMappingStrategy] & [MapEnumValue] - Advanced Enum Mapping ===");
        Console.WriteLine();

        // --- Scenario A: ByName + IgnoreCase ---
        Console.WriteLine("  Scenario A: EnumMappingStrategy.ByName (IgnoreCase = true) + [MapEnumValue]");
        var entity = new SupportTicketEntity
        {
            TicketId = 101,
            Priority = SourcePriority.high,
            Status = PaymentStatus.Captured
        };

        var mapper = new SupportTicketMapper();
        var dto = mapper.Map(entity);

        Console.WriteLine($"    Source: Priority={entity.Priority} (lowercase enum), Status={entity.Status}");
        Console.WriteLine($"    Target: Priority={dto.Priority} (PascalCase, matched via [EnumMappingStrategy(ByName, IgnoreCase = true)])");
        Console.WriteLine($"            Status={dto.Status} (remapped via [MapEnumValue(PaymentStatus.Captured, OrderProcessingStatus.Completed)])");
        Console.WriteLine();

        // --- Scenario B: ByValue ---
        Console.WriteLine("  Scenario B: EnumMappingStrategy.ByValue (numeric cast)");
        Console.WriteLine("    Generated code: StatusCode = (TargetHttpStatus)(int)source.StatusCode");
        var response = new ApiResponseEntity
        {
            StatusCode = SourceHttpStatus.NotFound,
            Message = "Resource not found."
        };

        var apiMapper = new ApiResponseMapper();
        var responseDto = apiMapper.Map(response);

        Console.WriteLine($"    Source: StatusCode={response.StatusCode} ({(int)response.StatusCode})");
        Console.WriteLine($"    Target: StatusCode={responseDto.StatusCode} ({(int)responseDto.StatusCode}) — mapped by numeric value");
        Console.WriteLine($"    Message: '{responseDto.Message}'");
        Console.WriteLine();
    }
}
