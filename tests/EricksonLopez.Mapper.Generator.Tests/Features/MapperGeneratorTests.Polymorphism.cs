// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Features;

public partial class MapperGeneratorTests
{
    [Fact]
    public async Task Generate_WhenPolymorphicMap_ShouldGenerateSwitchExpression()
    {
        var source = @"#nullable enable
using EricksonLopez.Mapper;

namespace TestNamespace;

public class Animal { public string Name { get; set; } = """"; }
public class Dog : Animal { public string Breed { get; set; } = """"; }
public class Cat : Animal { public bool IsIndoor { get; set; } }

public class Pet { public string Name { get; set; } = """"; }
public class DogPet : Pet { public string Breed { get; set; } = """"; }
public class CatPet : Pet { public bool IsIndoor { get; set; } }

[Mapper]
public partial class Mapper
{
    [MapDerivedType(typeof(DogPet), typeof(Dog))]
    [MapDerivedType(typeof(CatPet), typeof(Cat))]
    public partial Animal Map(Pet pet);

    public partial Dog Map(DogPet dog);
    public partial Cat Map(CatPet cat);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenAbstractBaseIncompletePolymorphism_ShouldGenerateWarningELM011()
    {
        var source = @"
namespace TestNamespace;
public class BaseSource { }
public class BaseDest { }
public abstract class AbstractDest { }
[Mapper]
public partial class Mapper
{
    [MapDerivedType(typeof(BaseSource), typeof(BaseDest))]
    public partial AbstractDest Map(BaseSource source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenAbstractBaseWithFactory_ShouldGenerateExceptionOnMissingDerived()
    {
        var source = @"
namespace TestNamespace;
public class BaseSource { }
public class BaseDest { }
public abstract class AbstractDest 
{ 
    public static AbstractDest Create() => null!; 
}
[Mapper]
public partial class Mapper
{
    [MapDerivedType(typeof(BaseSource), typeof(BaseDest))]
    [MapFactory(""Create"")]
    public partial AbstractDest Map(BaseSource source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public void Generate_WhenPolymorphicMappingWithPrivateConstructorsAndFactoryMethods_ShouldCompileAndMap()
    {
        var source = @"#nullable enable

namespace TestNamespace;

public abstract class ShapeSource { public string Name { get; set; } = """"; }
public class CircleSource : ShapeSource { public double Radius { get; set; } }
public class SquareSource : ShapeSource { public double Side { get; set; } }

public abstract class ShapeDest { public string Name { get; set; } = """"; }

public class CircleDest : ShapeDest
{
    public double Radius { get; private set; }
    private CircleDest(string name, double radius) { Name = name; Radius = radius; }
    public static CircleDest Create(string name, double radius) => new CircleDest(name, radius);
}

public class SquareDest : ShapeDest
{
    public double Side { get; }
    public SquareDest(string name, double side) { Name = name; Side = side; }
}

[Mapper]
public partial class ShapeMapper
{
    [MapDerivedType(typeof(CircleSource), typeof(CircleDest))]
    [MapDerivedType(typeof(SquareSource), typeof(SquareDest))]
    public partial ShapeDest Map(ShapeSource source);

    [MapFactory(nameof(CircleDest.Create))]
    public partial CircleDest MapCircle(CircleSource source);

    public partial SquareDest MapSquare(SquareSource source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("case global::TestNamespace.CircleSource derivedSource: return MapCircle(derivedSource);");
        output.Should().Contain("case global::TestNamespace.SquareSource derivedSource: return MapSquare(derivedSource);");
        output.Should().Contain("CircleDest.Create(source.Name, source.Radius)");
        output.Should().Contain("new global::TestNamespace.SquareDest(source.Name, source.Side)");
    }

    [Fact]
    public void Generate_WhenPolymorphicMappingNullableSource_ShouldEmitSafeNullTernaryAndSwitch()
    {
        var source = @"#nullable enable

namespace TestNamespace;

public class VehicleSource { public string Brand { get; set; } = """"; }
public class CarSource : VehicleSource { public int Doors { get; set; } }

public class VehicleDest { public string Brand { get; set; } = """"; }
public class CarDest : VehicleDest { public int Doors { get; set; } }

public class FleetSource { public VehicleSource? Vehicle { get; set; } }
public class FleetDest { public VehicleDest? Vehicle { get; set; } }

[Mapper]
public partial class FleetMapper
{
    public partial FleetDest MapFleet(FleetSource source);

    [MapDerivedType(typeof(CarSource), typeof(CarDest))]
    public partial VehicleDest MapVehicle(VehicleSource source);

    public partial CarDest MapCar(CarSource source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Vehicle = (source.Vehicle != null ? this.MapVehicle(source.Vehicle) : null)");
        output.Should().Contain("case global::TestNamespace.CarSource derivedSource: return MapCar(derivedSource);");
    }

    [Fact]
    public void Generate_WhenThreeLevelPolymorphicHierarchyWithAbstractIntermediate_ShouldEmitExhaustiveSwitch()
    {
        var source = @"#nullable enable

namespace TestNamespace;

public abstract class BaseMessageSource { public string Id { get; set; } = """"; }
public abstract class DocumentMessageSource : BaseMessageSource { public string FileName { get; set; } = """"; }
public class PdfMessageSource : DocumentMessageSource { public int PageCount { get; set; } }
public class WordMessageSource : DocumentMessageSource { public bool TrackChanges { get; set; } }
public class TextMessageSource : BaseMessageSource { public string Body { get; set; } = """"; }

public abstract class BaseMessageDest { public string Id { get; set; } = """"; }
public abstract class DocumentMessageDest : BaseMessageDest { public string FileName { get; set; } = """"; }
public class PdfMessageDest : DocumentMessageDest { public int PageCount { get; set; } }
public class WordMessageDest : DocumentMessageDest { public bool TrackChanges { get; set; } }
public class TextMessageDest : BaseMessageDest { public string Body { get; set; } = """"; }

[Mapper]
public partial class HierarchyMapper
{
    [MapDerivedType(typeof(PdfMessageSource), typeof(PdfMessageDest))]
    [MapDerivedType(typeof(WordMessageSource), typeof(WordMessageDest))]
    [MapDerivedType(typeof(TextMessageSource), typeof(TextMessageDest))]
    public partial BaseMessageDest MapMessage(BaseMessageSource source);

    public partial PdfMessageDest MapPdf(PdfMessageSource source);
    public partial WordMessageDest MapWord(WordMessageSource source);
    public partial TextMessageDest MapText(TextMessageSource source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("case global::TestNamespace.PdfMessageSource derivedSource: return MapPdf(derivedSource);");
        output.Should().Contain("case global::TestNamespace.WordMessageSource derivedSource: return MapWord(derivedSource);");
        output.Should().Contain("case global::TestNamespace.TextMessageSource derivedSource: return MapText(derivedSource);");
        output.Should().Contain("FileName = source.FileName");
        output.Should().Contain("PageCount = source.PageCount");
        output.Should().Contain("TrackChanges = source.TrackChanges");
    }
}




