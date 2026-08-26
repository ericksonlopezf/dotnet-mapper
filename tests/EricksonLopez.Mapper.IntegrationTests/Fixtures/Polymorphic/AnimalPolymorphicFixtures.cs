// Copyright © Erickson Lopez. MIT License.
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.IntegrationTests.Fixtures;

public class AnimalEntity
{
    public string Name { get; set; } = string.Empty;
}

public class DogEntity : AnimalEntity
{
    public string Breed { get; set; } = string.Empty;
}

public class CatEntity : AnimalEntity
{
    public bool IsIndoor { get; set; }
}

public class AnimalDto
{
    public string Name { get; set; } = string.Empty;
}

public class DogDto : AnimalDto
{
    public string Breed { get; set; } = string.Empty;
}

public class CatDto : AnimalDto
{
    public bool IsIndoor { get; set; }
}

[Mapper]
public partial class AnimalMapper
{
    [MapDerivedType(typeof(DogEntity), typeof(DogDto))]
    [MapDerivedType(typeof(CatEntity), typeof(CatDto))]
    public partial AnimalDto MapAnimal(AnimalEntity source);

    public partial DogDto MapDog(DogEntity source);
    public partial CatDto MapCat(CatEntity source);
}
