// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.DomainPrimitives.Validation;
using Xunit;

namespace EricksonLopez.Mapper.DomainPrimitives.Tests;

/// <summary>
/// Contains unit tests verifying the domain primitive converters.
/// Adheres strictly to ADR-021 (UnitOfWork_StateUnderTest_ExpectedBehavior).
/// </summary>
public sealed class DomainPrimitiveConvertersTests
{
    private sealed record CustomerId(Guid Value) : IStrongId<CustomerId, Guid>
    {
        public bool IsDefault => Value == Guid.Empty;
        public static string PrimitiveName => "CustomerId";
        public static CustomerId Create() => new(Guid.NewGuid());
        public static CustomerId Empty => new(Guid.Empty);
        public static CustomerId Create(Guid value) => new(value);
        public static bool TryCreate(Guid value, out CustomerId result, out PrimitiveError validationError)
        {
            result = new CustomerId(value);
            validationError = default;
            return true;
        }

        public int CompareTo(CustomerId? other) => other is null ? 1 : Value.CompareTo(other.Value);
    }

    private sealed record EmailAddress(string Value) : IDomainPrimitive<EmailAddress, string>
    {
        public bool IsDefault => string.IsNullOrEmpty(Value);
        public static string PrimitiveName => "EmailAddress";
        public static EmailAddress Create(string value) => new(value);
        public static bool TryCreate(string value, out EmailAddress result, out PrimitiveError validationError)
        {
            result = new EmailAddress(value);
            validationError = default;
            return true;
        }

        public int CompareTo(EmailAddress? other) => string.Compare(Value, other?.Value, StringComparison.Ordinal);
    }

    [Fact]
    public void StrongIdToValueConverter_WhenValidStrongIdProvided_ShouldExtractUnderlyingGuid()
    {
        var converter = new StrongIdToValueConverter<CustomerId, Guid>();
        var guid = Guid.NewGuid();
        var strongId = CustomerId.Create(guid);

        var result = converter.Convert(strongId);

        result.Should().Be(guid);
    }

    [Fact]
    public void StrongIdToValueConverter_WhenSourceNull_ShouldThrowArgumentNullException()
    {
        var converter = new StrongIdToValueConverter<CustomerId, Guid>();
        var act = () => converter.Convert(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }

    [Fact]
    public void DomainPrimitiveToValueConverter_WhenValidPrimitiveProvided_ShouldExtractUnderlyingGuid()
    {
        var converter = new DomainPrimitiveToValueConverter<CustomerId, Guid>();
        var guid = Guid.NewGuid();
        var strongId = CustomerId.Create(guid);

        var result = converter.Convert(strongId);

        result.Should().Be(guid);
    }

    [Fact]
    public void DomainPrimitiveToValueConverter_WhenValidStringPrimitiveProvided_ShouldExtractUnderlyingString()
    {
        var converter = new DomainPrimitiveToValueConverter<EmailAddress, string>();
        var email = EmailAddress.Create("test@example.com");

        var result = converter.Convert(email);

        result.Should().Be("test@example.com");
    }

    [Fact]
    public void DomainPrimitiveToValueConverter_WhenSourceNull_ShouldThrowArgumentNullException()
    {
        var converter = new DomainPrimitiveToValueConverter<CustomerId, Guid>();
        var act = () => converter.Convert(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }

    [Fact]
    public void DomainPrimitiveToValueConverter_WhenEmailSourceNull_ShouldThrowArgumentNullException()
    {
        var converter = new DomainPrimitiveToValueConverter<EmailAddress, string>();
        var act = () => converter.Convert(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }

    private sealed record ValidatedEmail(string Value) : IDomainPrimitive<ValidatedEmail, string>
    {
        public bool IsDefault => string.IsNullOrEmpty(Value);
        public static string PrimitiveName => "ValidatedEmail";
        public static ValidatedEmail Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
            {
                throw new ArgumentException("Invalid email format", nameof(value));
            }
            return new(value);
        }
        public static bool TryCreate(string value, out ValidatedEmail result, out PrimitiveError validationError)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
            {
                result = default!;
                validationError = new PrimitiveError("Email.Invalid", "Invalid email format");
                return false;
            }
            result = new ValidatedEmail(value);
            validationError = default;
            return true;
        }

        public int CompareTo(ValidatedEmail? other) => string.Compare(Value, other?.Value, StringComparison.Ordinal);
    }

#if NET7_0_OR_GREATER
    /// <summary>
    /// <see cref="ValueToDomainPrimitiveConverter{TValue, TPrimitive}"/> leverages C# 11 static abstract interface
    /// members (<see cref="IDomainPrimitive{TSelf, TValue}.Create(TValue)"/>) introduced in .NET 7+.
    /// In modern runtimes (.NET 8/9/10), this enables compile-time generic instantiation without reflection or Activator.
    /// </summary>
    [Fact]
    public void ValueToDomainPrimitiveConverter_WhenValidGuidProvided_ShouldInstantiatePrimitive()
    {
        var converter = new ValueToDomainPrimitiveConverter<Guid, CustomerId>();
        var guid = Guid.NewGuid();

        var result = converter.Convert(guid);

        result.Should().NotBeNull();
        result.Value.Should().Be(guid);
    }

    [Fact]
    public void ValueToDomainPrimitiveConverter_WhenReferenceTypeSourceNull_ShouldThrowArgumentNullException()
    {
        var converter = new ValueToDomainPrimitiveConverter<string, EmailAddress>();
        var act = () => converter.Convert(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("source");
    }

    [Fact]
    public void ValueToDomainPrimitiveConverter_WhenInvalidValueProvided_ShouldThrowArgumentExceptionFromFactory()
    {
        var converter = new ValueToDomainPrimitiveConverter<string, ValidatedEmail>();
        var act = () => converter.Convert("invalid-email-address");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid email format*");
    }

    [Fact]
    public void ValueToDomainPrimitiveConverter_WhenValidValueProvided_ShouldInstantiateValidatedPrimitive()
    {
        var converter = new ValueToDomainPrimitiveConverter<string, ValidatedEmail>();
        var result = converter.Convert("user@example.com");

        result.Should().NotBeNull();
        result.Value.Should().Be("user@example.com");
    }

    private sealed record OrderNumber(long Value) : IStrongId<OrderNumber, long>
    {
        public bool IsDefault => Value == 0;
        public static string PrimitiveName => "OrderNumber";
        public static OrderNumber Create() => new(0L);
        public static OrderNumber Empty => new(0L);
        public static OrderNumber Create(long value) => new(value);
        public static bool TryCreate(long value, out OrderNumber result, out PrimitiveError validationError)
        {
            result = new OrderNumber(value);
            validationError = default;
            return true;
        }
        public int CompareTo(OrderNumber? other) => other is null ? 1 : Value.CompareTo(other.Value);
    }

    [Fact]
    public void ValueToDomainPrimitiveConverter_WhenNumericLongProvided_ShouldInstantiateStrongId()
    {
        var converter = new ValueToDomainPrimitiveConverter<long, OrderNumber>();
        var result = converter.Convert(9876543210L);

        result.Should().NotBeNull();
        result.Value.Should().Be(9876543210L);
    }

    [Fact]
    public void StrongIdToValueConverter_WhenNumericLongProvided_ShouldExtractUnderlyingValue()
    {
        var converter = new StrongIdToValueConverter<OrderNumber, long>();
        var strongId = OrderNumber.Create(9876543210L);
        var result = converter.Convert(strongId);

        result.Should().Be(9876543210L);
    }

    [Fact]
    public void StrongIdToValueConverter_WhenInvokedViaIConverterInterface_ShouldPolymorphicallyExecute()
    {
        IConverter<OrderNumber, long> converter = new StrongIdToValueConverter<OrderNumber, long>();
        var strongId = OrderNumber.Create(12345L);
        var result = converter.Convert(strongId);

        result.Should().Be(12345L);
    }

    [Fact]
    public void DomainPrimitiveToValueConverter_WhenInvokedViaIConverterInterface_ShouldPolymorphicallyExecute()
    {
        IConverter<EmailAddress, string> converter = new DomainPrimitiveToValueConverter<EmailAddress, string>();
        var email = EmailAddress.Create("info@domain.com");
        var result = converter.Convert(email);

        result.Should().Be("info@domain.com");
    }

    [Fact]
    public void ValueToDomainPrimitiveConverter_WhenInvokedViaIConverterInterface_ShouldPolymorphicallyExecute()
    {
        IConverter<string, EmailAddress> converter = new ValueToDomainPrimitiveConverter<string, EmailAddress>();
        var result = converter.Convert("admin@domain.com");

        result.Should().NotBeNull();
        result.Value.Should().Be("admin@domain.com");
    }
#endif
}
