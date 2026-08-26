# Level 08: Fluent Unit Testing & Contract Verification

## 1. Zero-Allocation Contract Assertions

Using the shared test fixture `MapperContractExtensions.AssertZeroAllocations`, test suites can continuously enforce zero heap allocations in CI/CD:

```csharp
using AwesomeAssertions;
using EricksonLopez.Mapper.Tests.Common;
using Xunit;

public sealed class UserMapperTests
{
    [Fact]
    public void ToResponse_WhenInvoked_AllocatesZeroBytesOnHeap()
    {
        var entity = new UserEntity(
            Guid.NewGuid(),
            "John",
            "Doe",
            "john.doe@example.com",
            DateTime.UtcNow);

        // Asserts delta GC allocated bytes equals zero across 20 iterations
        MapperContractExtensions.AssertZeroAllocations(() =>
        {
            var response = UserMapper.ToResponse(entity);
            response.Should().NotBeNull();
        });
    }

    [Fact]
    public void ToResponse_ValidEntity_MapsAllPropertiesAccurately()
    {
        var id = Guid.NewGuid();
        var entity = new UserEntity(id, "Alice", "Smith", "alice@example.com", DateTime.UtcNow);

        var dto = UserMapper.ToResponse(entity);

        dto.Id.Should().Be(id);
        dto.FirstName.Should().Be("Alice");
        dto.LastName.Should().Be("Smith");
        dto.Email.Should().Be("alice@example.com");
    }
}
```
