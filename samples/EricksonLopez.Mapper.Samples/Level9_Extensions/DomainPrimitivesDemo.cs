// Copyright © Erickson Lopez. MIT License.
#nullable enable
using System;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.DomainPrimitives.Validation;
using EricksonLopez.Mapper.DomainPrimitives;

namespace EricksonLopez.Mapper.Sample.Level9_Extensions;

// =============================================================================
// Level 9 - EricksonLopez.Mapper.DomainPrimitives Integration
//
// Package: EricksonLopez.Mapper.DomainPrimitives
//
// Purpose:
//   Provides out-of-the-box converters for domain primitives implementing
//   IDomainPrimitive<TSelf, TValue> and IStrongId<TSelf, TValue>.
//
// Types:
//   - DomainPrimitiveToValueConverter<TPrimitive, TValue>: Extracts .Value from primitive
//   - StrongIdToValueConverter<TStrongId, TValue>: Extracts .Value from strongly-typed ID
//   - ValueToDomainPrimitiveConverter<TValue, TPrimitive>: Calls TPrimitive.Create(value)
// =============================================================================

/// <summary>Sample strongly-typed customer ID domain primitive.</summary>
public sealed record SampleCustomerId(Guid Value) : IStrongId<SampleCustomerId, Guid>
{
    /// <inheritdoc/>
    public bool IsDefault => Value == Guid.Empty;
    /// <inheritdoc/>
    public static string PrimitiveName => "CustomerId";
    /// <inheritdoc/>
    public static SampleCustomerId Create() => new(Guid.NewGuid());
    /// <inheritdoc/>
    public static SampleCustomerId Empty => new(Guid.Empty);
    /// <inheritdoc/>
    public static SampleCustomerId Create(Guid value) => new(value);
    /// <inheritdoc/>
    public static bool TryCreate(Guid value, out SampleCustomerId result, out PrimitiveError validationError)
    {
        result = new SampleCustomerId(value);
        validationError = default;
        return true;
    }
    /// <inheritdoc/>
    public int CompareTo(SampleCustomerId? other) => other is null ? 1 : Value.CompareTo(other.Value);
}

/// <summary>Sample email domain primitive.</summary>
public sealed record SampleEmailAddress(string Value) : IDomainPrimitive<SampleEmailAddress, string>
{
    /// <inheritdoc/>
    public bool IsDefault => string.IsNullOrEmpty(Value);
    /// <inheritdoc/>
    public static string PrimitiveName => "EmailAddress";
    /// <inheritdoc/>
    public static SampleEmailAddress Create(string value) => new(value);
    /// <inheritdoc/>
    public static bool TryCreate(string value, out SampleEmailAddress result, out PrimitiveError validationError)
    {
        result = new SampleEmailAddress(value);
        validationError = default;
        return true;
    }
    /// <inheritdoc/>
    public int CompareTo(SampleEmailAddress? other) => string.Compare(Value, other?.Value, StringComparison.Ordinal);
}

/// <summary>Demonstrates the official EricksonLopez.Mapper.DomainPrimitives converters.</summary>
public static class DomainPrimitivesDemo
{
    /// <summary>Runs the domain primitives extension demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 9: EricksonLopez.Mapper.DomainPrimitives Extension ===");
        Console.WriteLine("Demonstrating official converters for IDomainPrimitive<TSelf, TValue> and IStrongId<TSelf, TValue>.\n");

        var customerId = SampleCustomerId.Create();
        var email = SampleEmailAddress.Create("domain.expert@cleanarchitecture.dev");

        // 1. StrongIdToValueConverter
        var strongIdConverter = new StrongIdToValueConverter<SampleCustomerId, Guid>();
        Guid rawId = strongIdConverter.Convert(customerId);

        // 2. DomainPrimitiveToValueConverter
        var primitiveConverter = new DomainPrimitiveToValueConverter<SampleEmailAddress, string>();
        string rawEmail = primitiveConverter.Convert(email);

        // 3. ValueToDomainPrimitiveConverter
        var valueToPrimitiveConverter = new ValueToDomainPrimitiveConverter<string, SampleEmailAddress>();
        var restoredEmail = valueToPrimitiveConverter.Convert("restored@cleanarchitecture.dev");

        Console.WriteLine($"  StrongIdToValueConverter       : CustomerId({customerId.Value}) -> raw Guid: '{rawId}'");
        Console.WriteLine($"  DomainPrimitiveToValueConverter: EmailAddress('{email.Value}') -> raw string: '{rawEmail}'");
        Console.WriteLine($"  ValueToDomainPrimitiveConverter: raw string: 'restored@...' -> EmailAddress('{restoredEmail.Value}')");
        Console.WriteLine();
    }
}
