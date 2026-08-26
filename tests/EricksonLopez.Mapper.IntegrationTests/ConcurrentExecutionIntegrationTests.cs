// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Mapper.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EricksonLopez.Mapper.IntegrationTests;

/// <summary>
/// Integration tests verifying thread-safety, statelessness, and concurrency resilience
/// of compile-time generated mappers and dependency injection resolutions under high concurrent load.
/// Adheres strictly to ADR-021 (UnitOfWork_StateUnderTest_ExpectedBehavior).
/// </summary>
public class ConcurrentExecutionIntegrationTests
{
    private static readonly int ConcurrencyIterations =
        int.TryParse(Environment.GetEnvironmentVariable("MAPPER_CONCURRENCY_ITERATIONS"), out var iters) && iters > 0 ? iters : 500;

    [Fact(Timeout = 15000)]
    public async Task MapOrder_WhenInvokedConcurrentlyFromMultipleThreads_ShouldBeThreadSafeAndProduceDeterministicResults()
    {
        // Arrange
        var mapper = new OrderMapper();
        var exceptions = new ConcurrentBag<Exception>();
        var results = new ConcurrentBag<OrderDto>();

        // Act
        await Task.Run(() =>
        {
            Parallel.For(0, ConcurrencyIterations, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                try
                {
                    var guid = Guid.NewGuid();
                    var entity = new OrderEntity
                    {
                        Id = guid,
                        Role = (SourceRole)(1 + (i % 3)),
                        ShippingAddress = new AddressEntity
                        {
                            Street = $"Street {i}",
                            City = $"City {i}"
                        },
                        ItemIds = new List<int> { i, i * 2, i * 3 },
                        Metadata = new Dictionary<string, string>
                        {
                            ["Key"] = $"Value_{i}"
                        }
                    };

                    var dto = mapper.MapOrder(entity);
                    results.Add(dto);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        });

        // Assert
        exceptions.Should().BeEmpty();
        results.Should().HaveCount(ConcurrencyIterations);
        results.Should().OnlyContain(r => r.ShippingAddress != null && r.ItemIds.Count == 3);
    }

    [Fact(Timeout = 15000)]
    public async Task DependencyInjection_WhenResolvedAndInvokedConcurrently_ShouldMaintainThreadSafetyAcrossScopes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGeneratedMappers();
        var provider = services.BuildServiceProvider();

        var exceptions = new ConcurrentBag<Exception>();
        var mappedSkus = new ConcurrentBag<string>();

        // Act: Run concurrent tasks resolving mappers from DI in parallel
        var tasks = Enumerable.Range(0, ConcurrencyIterations).Select(i => Task.Run(() =>
        {
            try
            {
                var productMapper = provider.GetRequiredService<ProductMapper>();
                var invoiceMapper = provider.GetRequiredService<InvoiceMapper>();

                var productDto = new ProductDto { Sku = $"SKU-{i}", Price = 19.99m + i };
                var entity = productMapper.MapToEntity(productDto);
                mappedSkus.Add(entity.Sku);

                var invoiceEntity = new InvoiceEntity { Id = Guid.NewGuid(), Amount = 100m + i };
                var invoiceDto = invoiceMapper.MapInvoice(invoiceEntity);
                invoiceDto.FormattedAmount.Should().Be($"${(100m + i):F2}");
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }));

        await Task.WhenAll(tasks);

        // Assert
        exceptions.Should().BeEmpty();
        mappedSkus.Should().HaveCount(ConcurrencyIterations);
        mappedSkus.Distinct().Should().HaveCount(ConcurrencyIterations);
    }

    [Fact(Timeout = 15000)]
    public async Task PolymorphicMap_WhenInvokedConcurrentlyWithVaryingDerivedTypes_ShouldCorrectlyDispatchWithoutInterference()
    {
        // Arrange
        var mapper = new AnimalMapper();
        var results = new ConcurrentBag<(int Index, string ExpectedType, string ActualType)>();
        var exceptions = new ConcurrentBag<Exception>();

        // Act
        await Task.Run(() =>
        {
            Parallel.For(0, ConcurrencyIterations, i =>
            {
                try
                {
                    AnimalEntity source = (i % 3) switch
                    {
                        0 => new DogEntity { Name = $"Dog_{i}", Breed = $"Breed_{i}" },
                        1 => new CatEntity { Name = $"Cat_{i}", IsIndoor = (i % 2 == 0) },
                        _ => new AnimalEntity { Name = $"Base_{i}" }
                    };

                    string expectedType = (i % 3) switch
                    {
                        0 => nameof(DogDto),
                        1 => nameof(CatDto),
                        _ => nameof(AnimalDto)
                    };

                    var dto = mapper.MapAnimal(source);
                    results.Add((i, expectedType, dto.GetType().Name));
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        });

        // Assert
        exceptions.Should().BeEmpty();
        results.Should().HaveCount(ConcurrencyIterations);
        results.Should().OnlyContain(r => r.ExpectedType == r.ActualType);
    }

    [Fact(Timeout = 15000)]
    public async Task BranchDirectoryMapper_WhenMappingNestedCollectionsConcurrently_ShouldProduceIndependentUncorruptedObjects()
    {
        // Arrange
        var mapper = new BranchDirectoryMapper();
        var exceptions = new ConcurrentBag<Exception>();
        var results = new ConcurrentBag<BranchDirectoryDto>();

        // Act
        await Task.Run(() =>
        {
            Parallel.For(0, ConcurrencyIterations, i =>
            {
                try
                {
                    var source = new BranchDirectoryEntity
                    {
                        BranchName = $"Branch_{i}",
                        Locations = new Dictionary<string, AddressEntity>
                        {
                            ["Main"] = new AddressEntity { Street = $"Main St {i}", City = $"City {i}" },
                            ["Aux"] = new AddressEntity { Street = $"Aux St {i}", City = $"City {i}" }
                        }
                    };

                    var dto = mapper.MapBranch(source);
                    results.Add(dto);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        });

        // Assert
        exceptions.Should().BeEmpty();
        results.Should().HaveCount(ConcurrencyIterations);
        results.Should().OnlyContain(r => r.Locations != null && r.Locations.Count == 2);
    }

    [Fact(Timeout = 15000)]
    public async Task CustomerProfileMapper_WhenInvokedConcurrentlyWithNullAndNonNullFallbacks_ShouldMaintainThreadSafety()
    {
        // Arrange
        var mapper = new CustomerProfileMapper();
        var exceptions = new ConcurrentBag<Exception>();
        var results = new ConcurrentBag<CustomerProfileDto>();

        // Act
        await Task.Run(() =>
        {
            Parallel.For(0, ConcurrencyIterations, i =>
            {
                try
                {
                    bool isEmailNull = i % 2 == 0;
                    var entity = new CustomerProfileEntity
                    {
                        Name = $"Customer_{i}",
                        Email = isEmailNull ? null : $"user_{i}@example.com"
                    };

                    var dto = mapper.MapProfile(entity);
                    results.Add(dto);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        });

        // Assert
        exceptions.Should().BeEmpty();
        results.Should().HaveCount(ConcurrencyIterations);
        results.Where(r => r.Email == "no-reply@domain.com").Should().HaveCount((ConcurrencyIterations + 1) / 2);
    }
}
