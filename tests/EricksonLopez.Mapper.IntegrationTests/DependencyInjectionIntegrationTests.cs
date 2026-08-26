// Copyright © Erickson Lopez. MIT License.
using AwesomeAssertions;
using EricksonLopez.Mapper.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Mapper.IntegrationTests;

/// <summary>
/// Integration tests verifying the Dependency Injection registration emitted by <c>AddGeneratedMappers()</c>.
/// Adheres strictly to ADR-021 (UnitOfWork_StateUnderTest_ExpectedBehavior).
/// </summary>
public class DependencyInjectionIntegrationTests
{
    [Fact]
    public void AddGeneratedMappers_WhenRegisteredInServiceCollection_ShouldRegisterMappersAndConvertersWithCorrectLifetimes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGeneratedMappers();
        var provider = services.BuildServiceProvider();

        // Act
        var orderMapper = provider.GetService<OrderMapper>();
        var productMapper = provider.GetService<ProductMapper>();
        var invoiceMapper = provider.GetService<InvoiceMapper>();
        var animalMapper = provider.GetService<AnimalMapper>();
        var profileMapper = provider.GetService<CustomerProfileMapper>();
        var converter = provider.GetService<CurrencyConverter>();

        // Assert
        orderMapper.Should().NotBeNull();
        productMapper.Should().NotBeNull();
        invoiceMapper.Should().NotBeNull();
        animalMapper.Should().NotBeNull();
        profileMapper.Should().NotBeNull();
        converter.Should().NotBeNull();

        // Validate that mappers resolve as singletons and converters as transient
        var orderMapper2 = provider.GetService<OrderMapper>();
        var animalMapper2 = provider.GetService<AnimalMapper>();
        var converter2 = provider.GetService<CurrencyConverter>();

        orderMapper.Should().BeSameAs(orderMapper2);
        animalMapper.Should().BeSameAs(animalMapper2);
        converter.Should().NotBeSameAs(converter2);
    }
}
