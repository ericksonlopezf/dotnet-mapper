// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.Sample.Level4_Advanced;

// =============================================================================
// Level 4 - [MapFactory] Attribute (Factory Method Mapping)
//
// Problem:
//   The destination DTO has no public parameterless constructor and must be
//   instantiated via a static factory method that enforces invariants,
//   such as normalizing a currency code to uppercase.
//
// Solution:
//   [MapFactory("Create")] instructs the generator to call
//   PaymentTransactionDto.Create(...) instead of new PaymentTransactionDto().
//   The parameters are resolved by name (case-insensitive) from source properties.
//
// When to use:
//   - When the destination type enforces invariants via a factory method.
//   - When the destination constructor is private or non-public.
//
// When NOT to use:
//   - When the destination has a public parameterless constructor and no invariants.
// =============================================================================

/// <summary>Represents an immutable payment transaction data transfer object.</summary>
/// <remarks>
/// Instances must be created via the <see cref="Create"/> factory method to ensure
/// that the currency code is normalized.
/// </remarks>
/// <param name="TransactionId">The unique identifier of the transaction.</param>
/// <param name="Currency">The ISO currency code for the transaction.</param>
/// <param name="Amount">The transaction amount.</param>
/// <param name="Status">The current status of the transaction.</param>
public record PaymentTransactionDto(Guid TransactionId, string Currency, decimal Amount, string Status)
{
    /// <summary>
    /// Creates a new <see cref="PaymentTransactionDto"/> with the currency code normalized to uppercase.
    /// </summary>
    /// <param name="transactionId">The unique identifier of the transaction.</param>
    /// <param name="currency">The ISO currency code. Defaults to <c>"USD"</c> when <see langword="null"/> or empty.</param>
    /// <param name="amount">The transaction amount.</param>
    /// <param name="status">The current status of the transaction.</param>
    /// <returns>A new <see cref="PaymentTransactionDto"/> with the normalized currency code.</returns>
    public static PaymentTransactionDto Create(Guid transactionId, string currency, decimal amount, string status)
    {
        // Normalization and domain validation can be performed here.
        var normalizedCurrency = (currency ?? "USD").ToUpperInvariant();
        return new PaymentTransactionDto(transactionId, normalizedCurrency, amount, status);
    }
}

/// <summary>Represents a payment transaction entity in the data layer.</summary>
public class PaymentTransactionEntity
{
    /// <summary>Gets or sets the unique identifier of the transaction.</summary>
    public Guid TransactionId { get; set; }
    /// <summary>Gets or sets the ISO currency code (e.g., <c>"usd"</c>).</summary>
    public string Currency { get; set; } = "usd";
    /// <summary>Gets or sets the transaction amount.</summary>
    public decimal Amount { get; set; }
    /// <summary>Gets or sets the current status of the transaction.</summary>
    public string Status { get; set; } = "pending";
}

/// <summary>
/// Provides compile-time-generated mapping from <see cref="PaymentTransactionEntity"/> to <see cref="PaymentTransactionDto"/>
/// via the <see cref="PaymentTransactionDto.Create"/> factory method.
/// </summary>
[Mapper]
public partial class PaymentMapper
{
    /// <summary>
    /// Maps a <see cref="PaymentTransactionEntity"/> to a <see cref="PaymentTransactionDto"/>
    /// by invoking <see cref="PaymentTransactionDto.Create"/> instead of calling <see langword="new"/> directly.
    /// </summary>
    /// <param name="source">The payment transaction entity to map from.</param>
    /// <returns>A new <see cref="PaymentTransactionDto"/> with the currency code normalized to uppercase.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    [MapFactory("Create")]
    public partial PaymentTransactionDto MapTransaction(PaymentTransactionEntity source);
}

/// <summary>Demonstrates factory method mapping using <see cref="MapFactoryAttribute"/>.</summary>
public static class FactoryMethodDemo
{
    /// <summary>Runs the factory method mapping demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 4: [MapFactory] - Factory Method Mapping ===");
        Console.WriteLine();
        Console.WriteLine("Scenario: The DTO must be instantiated through a static factory method");
        Console.WriteLine("that normalizes the currency to uppercase (e.g., 'usd' -> 'USD').");
        Console.WriteLine();

        var entity = new PaymentTransactionEntity
        {
            TransactionId = Guid.NewGuid(),
            Currency = "eur",   // intentionally lowercase
            Amount = 1500.00m,
            Status = "completed"
        };

        var mapper = new PaymentMapper();
        var dto = mapper.MapTransaction(entity);

        Console.WriteLine($"  Source.Currency   : '{entity.Currency}' (lowercase)");
        Console.WriteLine($"  Target.Currency   : '{dto.Currency}' (normalized by factory)");
        Console.WriteLine($"  Target.Amount     : {dto.Amount:C}");
        Console.WriteLine($"  Target.Status     : {dto.Status}");
        Console.WriteLine();
        Console.WriteLine("  The generator emitted: PaymentTransactionDto.Create(source.TransactionId, ...)");
        Console.WriteLine("  instead of: new PaymentTransactionDto { ... }");
        Console.WriteLine();
    }
}
