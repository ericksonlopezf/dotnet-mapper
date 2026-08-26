// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using AwesomeAssertions;
using EricksonLopez.Mapper.IntegrationTests.Fixtures;
using Xunit;

namespace EricksonLopez.Mapper.IntegrationTests;

/// <summary>
/// Integration tests verifying runtime behavior of enum mapping with switch expressions,
/// collections, dictionaries, and ternary null propagation.
/// Adheres strictly to ADR-021 (UnitOfWork_StateUnderTest_ExpectedBehavior).
/// </summary>
public class EnumAndCollectionIntegrationTests
{
    [Fact]
    public void MapOrder_WhenSourceHasDifferentEnumValues_ShouldEmitSwitchExpressionAndMapDto()
    {
        // Arrange
        var mapper = new OrderMapper();
        var entity = new OrderEntity
        {
            Id = Guid.NewGuid(),
            Role = SourceRole.Admin,
            ShippingAddress = new AddressEntity { Street = "Main St 123", City = "Metropolis" },
            ItemIds = new List<int> { 101, 102, 103 },
            Metadata = new Dictionary<string, string> { ["priority"] = "high", ["source"] = "web" }
        };

        // Act
        var dto = mapper.MapOrder(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Value.Should().Be(entity.Id);
        dto.Role.Should().Be(TargetRole.Admin); // Enum value translated from 2 -> 200 via switch
        dto.ShippingAddress.Should().NotBeNull();
        dto.ShippingAddress!.Street.Should().Be("Main St 123");
        dto.ShippingAddress.City.Should().Be("Metropolis");
        dto.ItemIds.Should().HaveCount(3);
        dto.Metadata.Should().HaveCount(2);
        dto.Metadata["priority"].Should().Be("high");
    }

    [Fact]
    public void MapOrder_WhenNestedObjectIsNull_ShouldMapToNullSafely()
    {
        // Arrange
        var mapper = new OrderMapper();
        var entity = new OrderEntity
        {
            Id = Guid.NewGuid(),
            Role = SourceRole.User,
            ShippingAddress = null,
            ItemIds = new List<int>(),
            Metadata = new Dictionary<string, string>()
        };

        // Act
        var dto = mapper.MapOrder(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.ShippingAddress.Should().BeNull(); // Safe ternary null propagation
        dto.Role.Should().Be(TargetRole.User);
    }

    [Fact]
    public void MapBranch_WhenDictionaryHasNestedEntityValues_ShouldMapAllDictionaryEntries()
    {
        // Arrange
        var mapper = new BranchDirectoryMapper();
        var entity = new BranchDirectoryEntity
        {
            BranchName = "HQ",
            Locations = new Dictionary<string, AddressEntity>
            {
                ["Primary"] = new AddressEntity { Street = "100 Broadway", City = "New York" },
                ["Secondary"] = new AddressEntity { Street = "200 Michigan Ave", City = "Chicago" }
            }
        };

        // Act
        var dto = mapper.MapBranch(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.BranchName.Should().Be("HQ");
        dto.Locations.Should().HaveCount(2);
        dto.Locations["Primary"].Street.Should().Be("100 Broadway");
        dto.Locations["Primary"].City.Should().Be("New York");
        dto.Locations["Secondary"].Street.Should().Be("200 Michigan Ave");
        dto.Locations["Secondary"].City.Should().Be("Chicago");
    }
}

