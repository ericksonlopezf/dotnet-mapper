// Copyright © Erickson Lopez. MIT License.
using AwesomeAssertions;
using EricksonLopez.Mapper.IntegrationTests.Fixtures;
using Xunit;

namespace EricksonLopez.Mapper.IntegrationTests;

/// <summary>
/// Integration tests verifying runtime polymorphic dispatch using <c>[MapDerivedType]</c>
/// and switch pattern matching generated at compile-time.
/// Adheres strictly to ADR-021 (UnitOfWork_StateUnderTest_ExpectedBehavior).
/// </summary>
public class PolymorphicMappingIntegrationTests
{
    [Fact]
    public void MapAnimal_WhenSourceIsDogEntity_ShouldPolymorphicallyReturnDogDto()
    {
        // Arrange
        var mapper = new AnimalMapper();
        AnimalEntity entity = new DogEntity
        {
            Name = "Rex",
            Breed = "German Shepherd"
        };

        // Act
        var result = mapper.MapAnimal(entity);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<DogDto>();
        var dogDto = (DogDto)result;
        dogDto.Name.Should().Be("Rex");
        dogDto.Breed.Should().Be("German Shepherd");
    }

    [Fact]
    public void MapAnimal_WhenSourceIsCatEntity_ShouldPolymorphicallyReturnCatDto()
    {
        // Arrange
        var mapper = new AnimalMapper();
        AnimalEntity entity = new CatEntity
        {
            Name = "Whiskers",
            IsIndoor = true
        };

        // Act
        var result = mapper.MapAnimal(entity);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CatDto>();
        var catDto = (CatDto)result;
        catDto.Name.Should().Be("Whiskers");
        catDto.IsIndoor.Should().BeTrue();
    }

    [Fact]
    public void MapAnimal_WhenSourceIsBaseAnimalEntity_ShouldReturnBaseAnimalDto()
    {
        // Arrange
        var mapper = new AnimalMapper();
        var entity = new AnimalEntity
        {
            Name = "Generic Animal"
        };

        // Act
        var result = mapper.MapAnimal(entity);

        // Assert
        result.Should().NotBeNull();
        result.GetType().Should().Be(typeof(AnimalDto));
        result.Name.Should().Be("Generic Animal");
    }
}


