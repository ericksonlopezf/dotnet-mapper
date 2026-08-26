// Copyright © Erickson Lopez. MIT License.
using System;
using AwesomeAssertions;
using EricksonLopez.Mapper.IntegrationTests.Fixtures;
using Xunit;

namespace EricksonLopez.Mapper.IntegrationTests;

/// <summary>
/// Integration tests verifying domain mappings, static domain factories ([MapFactory]),
/// Value Objects, and custom converters ([UseConverter]) at runtime.
/// Adheres strictly to ADR-021 (UnitOfWork_StateUnderTest_ExpectedBehavior).
/// </summary>
public class DomainMappingIntegrationTests
{
    [Fact]
    public void MapToEntity_WhenMappedWithFactoryMethod_ShouldInvokeStaticDomainFactory()
    {
        // Arrange
        var mapper = new ProductMapper();
        var dto = new ProductDto { Sku = "PROD-999", Price = 49.99m };

        // Act
        var entity = mapper.MapToEntity(dto);

        // Assert
        entity.Should().NotBeNull();
        entity.Sku.Should().Be("PROD-999");
        entity.Price.Should().Be(49.99m);
    }

    [Fact]
    public void MapInvoice_WhenUseConverterApplied_ShouldTransformCurrencyToString()
    {
        // Arrange
        var mapper = new InvoiceMapper();
        var entity = new InvoiceEntity { Id = Guid.NewGuid(), Amount = 149.95m };

        // Act
        var dto = mapper.MapInvoice(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(entity.Id);
        dto.FormattedAmount.Should().Be("$149.95");
    }

    [Fact]
    public void MapProfile_WhenEmailIsNull_ShouldApplyConfiguredFallbackValue()
    {
        // Arrange
        var mapper = new CustomerProfileMapper();
        var entity = new CustomerProfileEntity
        {
            Name = "John Doe",
            Email = null
        };

        // Act
        var dto = mapper.MapProfile(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Name.Should().Be("John Doe");
        dto.Email.Should().Be("no-reply@domain.com");
    }

    [Fact]
    public void MapProfile_WhenEmailIsProvided_ShouldPreserveOriginalEmail()
    {
        // Arrange
        var mapper = new CustomerProfileMapper();
        var entity = new CustomerProfileEntity
        {
            Name = "Jane Doe",
            Email = "jane@example.com"
        };

        // Act
        var dto = mapper.MapProfile(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Name.Should().Be("Jane Doe");
        dto.Email.Should().Be("jane@example.com");
    }
}
