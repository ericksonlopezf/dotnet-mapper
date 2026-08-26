// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using EricksonLopez.Mapper;
using Mapster;

namespace Benchmarks;

// ── Value Object / Strongly Typed ID types ──────────────────────────────────
/// <summary>Represents a strongly typed user identifier for benchmark scenarios.</summary>
public readonly record struct UserId(Guid Value);
/// <summary>Represents a monetary amount value object for benchmark scenarios.</summary>
public readonly record struct Money(decimal Amount);
/// <summary>Represents an email address value object for benchmark scenarios.</summary>
public readonly record struct Email(string Value);

// ── Polymorphic types ───────────────────────────────────────────────────────
/// <summary>Represents the abstract base for all animal types in polymorphic mapping benchmarks.</summary>
public abstract class Animal
{
    /// <summary>Gets or sets the name of the animal.</summary>
    public string Name { get; set; } = "";
}
/// <summary>Represents a dog entity in polymorphic mapping benchmarks.</summary>
public class Dog : Animal
{
    /// <summary>Gets or sets the breed of the dog.</summary>
    public string Breed { get; set; } = "";
}
/// <summary>Represents a cat entity in polymorphic mapping benchmarks.</summary>
public class Cat : Animal
{
    /// <summary>Gets a value indicating whether the cat is kept indoors.</summary>
    public bool IsIndoor { get; set; }
}
/// <summary>Represents a fish entity in polymorphic mapping benchmarks.</summary>
public class Fish : Animal
{
    /// <summary>Gets or sets the species name of the fish.</summary>
    public string Species { get; set; } = "";
}

/// <summary>Represents a data transfer object for an animal.</summary>
public class AnimalDto
{
    /// <summary>Gets or sets the name of the animal.</summary>
    public string Name { get; set; } = "";
}
/// <summary>Represents a data transfer object for a dog.</summary>
public class DogDto : AnimalDto
{
    /// <summary>Gets or sets the breed of the dog.</summary>
    public string Breed { get; set; } = "";
}
/// <summary>Represents a data transfer object for a cat.</summary>
public class CatDto : AnimalDto
{
    /// <summary>Gets a value indicating whether the cat is kept indoors.</summary>
    public bool IsIndoor { get; set; }
}
/// <summary>Represents a data transfer object for a fish.</summary>
public class FishDto : AnimalDto
{
    /// <summary>Gets or sets the species name of the fish.</summary>
    public string Species { get; set; } = "";
}

// ── Simple POCO ─────────────────────────────────────────────────────────────
/// <summary>Represents a simple POCO source model for mapping benchmarks.</summary>
public class SimplePocoSource
{
    /// <summary>Gets or sets the name field.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the age field.</summary>
    public int Age { get; set; }
}
/// <summary>Represents a simple POCO destination model for mapping benchmarks.</summary>
public class SimplePocoTarget
{
    /// <summary>Gets or sets the name field.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the age field.</summary>
    public int Age { get; set; }
}

// ── Record Type ─────────────────────────────────────────────────────────────
/// <summary>Represents an immutable record source type for mapping benchmarks.</summary>
public record RecordSource(string Name, int Age);
/// <summary>Represents an immutable record destination type for mapping benchmarks.</summary>
public record RecordTarget(string Name, int Age);

// ── Collection ──────────────────────────────────────────────────────────────
/// <summary>Represents a collection source model for mapping benchmarks.</summary>
public class CollectionSource
{
    /// <summary>Gets or sets the list of items to map.</summary>
    public List<string> Items { get; set; } = new();
}
/// <summary>Represents a collection destination model for mapping benchmarks.</summary>
public class CollectionTarget
{
    /// <summary>Gets or sets the mapped list of items.</summary>
    public List<string> Items { get; set; } = new();
}

// ── Deep Nested ─────────────────────────────────────────────────────────────
/// <summary>Represents the deepest nesting level (5) of the benchmark source model.</summary>
public class Nested5
{
    /// <summary>Gets or sets the leaf string value.</summary>
    public string Val { get; set; } = string.Empty;
}
/// <summary>Represents nesting level 4 of the benchmark source model.</summary>
public class Nested4
{
    /// <summary>Gets or sets the next nested level.</summary>
    public Nested5 Level5 { get; set; } = null!;
}
/// <summary>Represents nesting level 3 of the benchmark source model.</summary>
public class Nested3
{
    /// <summary>Gets or sets the next nested level.</summary>
    public Nested4 Level4 { get; set; } = null!;
}
/// <summary>Represents nesting level 2 of the benchmark source model.</summary>
public class Nested2
{
    /// <summary>Gets or sets the next nested level.</summary>
    public Nested3 Level3 { get; set; } = null!;
}
/// <summary>Represents nesting level 1 of the benchmark source model.</summary>
public class Nested1
{
    /// <summary>Gets or sets the next nested level.</summary>
    public Nested2 Level2 { get; set; } = null!;
}

/// <summary>Represents the deepest nesting level (5) of the benchmark destination DTO.</summary>
public class Dto5
{
    /// <summary>Gets or sets the leaf string value.</summary>
    public string Val { get; set; } = string.Empty;
}
/// <summary>Represents nesting level 4 of the benchmark destination DTO.</summary>
public class Dto4
{
    /// <summary>Gets or sets the next nested level.</summary>
    public Dto5 Level5 { get; set; } = null!;
}
/// <summary>Represents nesting level 3 of the benchmark destination DTO.</summary>
public class Dto3
{
    /// <summary>Gets or sets the next nested level.</summary>
    public Dto4 Level4 { get; set; } = null!;
}
/// <summary>Represents nesting level 2 of the benchmark destination DTO.</summary>
public class Dto2
{
    /// <summary>Gets or sets the next nested level.</summary>
    public Dto3 Level3 { get; set; } = null!;
}
/// <summary>Represents nesting level 1 of the benchmark destination DTO.</summary>
public class Dto1
{
    /// <summary>Gets or sets the next nested level.</summary>
    public Dto2 Level2 { get; set; } = null!;
}

/// <summary>Represents a complex source model with deep nesting and large collections for benchmark scenarios.</summary>
public class SourceModel
{
    /// <summary>Gets or sets the unique identifier of the model.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the deeply nested object structure for the benchmark.</summary>
    public Nested1 Nested { get; set; } = null!;
    /// <summary>Gets or sets the large collection of string items for the benchmark.</summary>
    public List<string> LargeCollection { get; set; } = new();
}

/// <summary>Represents a complex destination model with value objects and nested DTOs for benchmark scenarios.</summary>
public class TargetModel
{
    /// <summary>Gets or sets the strongly typed user identifier.</summary>
    public UserId Id { get; set; } = default;
    /// <summary>Gets or sets the deeply nested DTO structure for the benchmark.</summary>
    public Dto1 Nested { get; set; } = null!;
    /// <summary>Gets or sets the large collection of string items for the benchmark.</summary>
    public List<string> LargeCollection { get; set; } = new();
}

// ── Value Object source/dest ─────────────────────────────────────────────────
/// <summary>Represents an order source with primitive types for value object mapping benchmarks.</summary>
public class OrderSource
{
    /// <summary>Gets or sets the unique identifier of the order.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the order amount as a raw decimal.</summary>
    public decimal Amount { get; set; }
    /// <summary>Gets or sets the customer email address as a raw string.</summary>
    public string Email { get; set; } = "";
}

/// <summary>Represents an order destination DTO with value object types for mapping benchmarks.</summary>
public class OrderDto
{
    /// <summary>Gets or sets the strongly typed user identifier.</summary>
    public UserId Id { get; set; } = default;
    /// <summary>Gets or sets the monetary amount as a value object.</summary>
    public Money Amount { get; set; } = default;
    /// <summary>Gets or sets the customer email address as a value object.</summary>
    public Email Email { get; set; } = default;
}

// ── Mapper definitions ────────────────────────────────────────────────────────
/// <summary>Provides compile-time-generated mapping for all benchmark scenarios using the EricksonLopez.Mapper source generator.</summary>
[EricksonLopez.Mapper.Mapper(StrictMapping = false)]
public partial class BenchmarkMapper
{
    /// <summary>Maps a <see cref="SourceModel"/> to a <see cref="TargetModel"/>.</summary>
    public partial TargetModel ToTarget(SourceModel source);
    /// <summary>Maps a <see cref="Nested1"/> to a <see cref="Dto1"/>.</summary>
    public partial Dto1 Map(Nested1 source);
    /// <summary>Maps a <see cref="Nested2"/> to a <see cref="Dto2"/>.</summary>
    public partial Dto2 Map(Nested2 source);
    /// <summary>Maps a <see cref="Nested3"/> to a <see cref="Dto3"/>.</summary>
    public partial Dto3 Map(Nested3 source);
    /// <summary>Maps a <see cref="Nested4"/> to a <see cref="Dto4"/>.</summary>
    public partial Dto4 Map(Nested4 source);
    /// <summary>Maps a <see cref="Nested5"/> to a <see cref="Dto5"/>.</summary>
    public partial Dto5 Map(Nested5 source);

    /// <summary>Maps a <see cref="SimplePocoSource"/> to a <see cref="SimplePocoTarget"/>.</summary>
    public partial SimplePocoTarget Map(SimplePocoSource source);
    /// <summary>Maps a <see cref="RecordSource"/> to a <see cref="RecordTarget"/>.</summary>
    public partial RecordTarget Map(RecordSource source);
    /// <summary>Maps a <see cref="CollectionSource"/> to a <see cref="CollectionTarget"/>.</summary>
    public partial CollectionTarget Map(CollectionSource source);

    // Value Objects / Strongly Typed IDs
    /// <summary>Maps an <see cref="OrderSource"/> to an <see cref="OrderDto"/>, wrapping primitive fields in value objects.</summary>
    public partial OrderDto MapOrder(OrderSource source);

    // Polymorphism
    /// <summary>Maps an <see cref="Animal"/> to the correct <see cref="AnimalDto"/> subtype based on the runtime type.</summary>
    [MapDerivedType(typeof(Dog), typeof(DogDto))]
    [MapDerivedType(typeof(Cat), typeof(CatDto))]
    [MapDerivedType(typeof(Fish), typeof(FishDto))]
    public partial AnimalDto MapAnimal(Animal source);
    /// <summary>Maps a <see cref="Dog"/> to a <see cref="DogDto"/>.</summary>
    public partial DogDto Map(Dog source);
    /// <summary>Maps a <see cref="Cat"/> to a <see cref="CatDto"/>.</summary>
    public partial CatDto Map(Cat source);
    /// <summary>Maps a <see cref="Fish"/> to a <see cref="FishDto"/>.</summary>
    public partial FishDto Map(Fish source);
}

/// <summary>Provides Mapperly-generated mapping for all benchmark scenarios, used as a competitive baseline.</summary>
[Riok.Mapperly.Abstractions.Mapper]
public partial class MapperlyBenchmarkMapper
{
    /// <summary>Maps a <see cref="SourceModel"/> to a <see cref="TargetModel"/>.</summary>
    public partial TargetModel ToTarget(SourceModel source);
    /// <summary>Maps a <see cref="SimplePocoSource"/> to a <see cref="SimplePocoTarget"/>.</summary>
    public partial SimplePocoTarget Map(SimplePocoSource source);
    /// <summary>Maps a <see cref="RecordSource"/> to a <see cref="RecordTarget"/>.</summary>
    public partial RecordTarget Map(RecordSource source);
    /// <summary>Maps a <see cref="CollectionSource"/> to a <see cref="CollectionTarget"/>.</summary>
    public partial CollectionTarget Map(CollectionSource source);

    /// <summary>Maps an <see cref="OrderSource"/> to an <see cref="OrderDto"/>.</summary>
    public partial OrderDto MapOrder(OrderSource source);

    /// <summary>Converts a <see cref="Guid"/> to a <see cref="UserId"/> value object.</summary>
    public UserId MapUserId(Guid id) => new UserId(id);
    /// <summary>Converts a <see cref="decimal"/> to a <see cref="Money"/> value object.</summary>
    public Money MapMoney(decimal amount) => new Money(amount);
    /// <summary>Converts a <see cref="string"/> to an <see cref="Email"/> value object.</summary>
    public Email MapEmail(string email) => new Email(email);
    /// <summary>Extracts the underlying <see cref="Guid"/> from a <see cref="UserId"/> value object.</summary>
    public Guid MapGuid(UserId id) => id.Value;
    /// <summary>Extracts the underlying <see cref="decimal"/> amount from a <see cref="Money"/> value object.</summary>
    public decimal MapDecimal(Money money) => money.Amount;
    /// <summary>Extracts the underlying <see cref="string"/> from an <see cref="Email"/> value object.</summary>
    public string MapString(Email email) => email.Value;

    /// <summary>Maps an <see cref="Animal"/> to the correct <see cref="AnimalDto"/> subtype based on the runtime type.</summary>
    [Riok.Mapperly.Abstractions.MapDerivedType(typeof(Dog), typeof(DogDto))]
    [Riok.Mapperly.Abstractions.MapDerivedType(typeof(Cat), typeof(CatDto))]
    [Riok.Mapperly.Abstractions.MapDerivedType(typeof(Fish), typeof(FishDto))]
    public partial AnimalDto MapAnimal(Animal source);
}

// ── Benchmark classes ─────────────────────────────────────────────────────────

/// <summary>Benchmarks comparing mapping performance for complex, POCO, record, and collection scenarios across multiple mapper libraries.</summary>
[MemoryDiagnoser]
[BenchmarkDotNet.Attributes.ShortRunJob]
public class MappingBenchmarks
{
    private SourceModel _source = null!;
    private SimplePocoSource _poco = null!;
    private RecordSource _record = null!;
    private CollectionSource _collection = null!;

    private BenchmarkMapper _mapper = null!;
    private AutoMapper.IMapper _autoMapper = null!;
    private MapperlyBenchmarkMapper _mapperly = null!;

    [GlobalSetup]
    public void Setup()
    {
        _source = new SourceModel
        {
            Id = Guid.NewGuid(),
            Nested = new Nested1 { Level2 = new Nested2 { Level3 = new Nested3 { Level4 = new Nested4 { Level5 = new Nested5 { Val = "Deep" } } } } },
            LargeCollection = Enumerable.Range(0, 1000).Select(i => "Item" + i.ToString()).ToList()
        };
        _poco = new SimplePocoSource { Name = "John", Age = 30 };
        _record = new RecordSource("Jane", 25);
        _collection = new CollectionSource { Items = Enumerable.Range(0, 1000).Select(i => "Item" + i.ToString()).ToList() };

        _mapper = new BenchmarkMapper();
        _mapperly = new MapperlyBenchmarkMapper();

        var config = new AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Guid, UserId>().ConstructUsing(g => new UserId(g));
            cfg.CreateMap<Nested5, Dto5>();
            cfg.CreateMap<Nested4, Dto4>();
            cfg.CreateMap<Nested3, Dto3>();
            cfg.CreateMap<Nested2, Dto2>();
            cfg.CreateMap<Nested1, Dto1>();
            cfg.CreateMap<SourceModel, TargetModel>();

            cfg.CreateMap<SimplePocoSource, SimplePocoTarget>();
            cfg.CreateMap<RecordSource, RecordTarget>();
            cfg.CreateMap<CollectionSource, CollectionTarget>();
        }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        _autoMapper = config.CreateMapper();

        TypeAdapterConfig<Guid, UserId>.NewConfig().MapWith(g => new UserId(g));
    }

    [Benchmark(Baseline = true)]
    public TargetModel Manual_Complex()
    {
        var target = new TargetModel
        {
            Id = new UserId(_source.Id),
            Nested = new Dto1
            {
                Level2 = new Dto2
                {
                    Level3 = new Dto3
                    {
                        Level4 = new Dto4
                        {
                            Level5 = new Dto5
                            {
                                Val = _source.Nested.Level2.Level3.Level4.Level5.Val
                            }
                        }
                    }
                }
            }
        };

        if (_source.LargeCollection != null)
        {
            target.LargeCollection = new List<string>(_source.LargeCollection.Count);
            foreach (var item in _source.LargeCollection)
            {
                target.LargeCollection.Add(item);
            }
        }

        return target;
    }

    [Benchmark]
    public TargetModel Generator_Complex() => _mapper.ToTarget(_source);

    [Benchmark]
    public TargetModel Mapperly_Complex() => _mapperly.ToTarget(_source);

    [Benchmark]
    public TargetModel Mapster_Complex() => _source.Adapt<TargetModel>();

    [Benchmark]
    public TargetModel AutoMapper_Complex() => _autoMapper.Map<TargetModel>(_source);

    [Benchmark]
    public SimplePocoTarget Manual_Poco() => new SimplePocoTarget { Name = _poco.Name, Age = _poco.Age };

    [Benchmark]
    public SimplePocoTarget Generator_Poco() => _mapper.Map(_poco);

    [Benchmark]
    public SimplePocoTarget Mapperly_Poco() => _mapperly.Map(_poco);

    [Benchmark]
    public SimplePocoTarget Mapster_Poco() => _poco.Adapt<SimplePocoTarget>();

    [Benchmark]
    public SimplePocoTarget AutoMapper_Poco() => _autoMapper.Map<SimplePocoTarget>(_poco);
}

/// <summary>Benchmarks comparing Value Object wrapping and unwrapping mapping performance across multiple mapper libraries.</summary>
[MemoryDiagnoser]
[BenchmarkDotNet.Attributes.ShortRunJob]
public class ValueObjectBenchmarks
{
    private OrderSource _order = null!;
    private BenchmarkMapper _mapper = null!;
    private MapperlyBenchmarkMapper _mapperly = null!;

    [GlobalSetup]
    public void Setup()
    {
        _order = new OrderSource
        {
            Id = Guid.NewGuid(),
            Amount = 99.99m,
            Email = "user@example.com"
        };
        _mapper = new BenchmarkMapper();
        _mapperly = new MapperlyBenchmarkMapper();

        TypeAdapterConfig<decimal, Money>.NewConfig().MapWith(g => new Money(g));
        TypeAdapterConfig<string, Email>.NewConfig().MapWith(g => new Email(g));
    }

    [Benchmark(Baseline = true)]
    public OrderDto Manual_ValueObjects() => new OrderDto
    {
        Id = new UserId(_order.Id),
        Amount = new Money(_order.Amount),
        Email = new Email(_order.Email)
    };

    [Benchmark]
    public OrderDto Generator_ValueObjects() => _mapper.MapOrder(_order);

    [Benchmark]
    public OrderDto Mapperly_ValueObjects() => _mapperly.MapOrder(_order);

    [Benchmark]
    public OrderDto Mapster_ValueObjects() => _order.Adapt<OrderDto>();
}

/// <summary>Benchmarks comparing polymorphic dispatch mapping performance across multiple mapper libraries.</summary>
[MemoryDiagnoser]
[BenchmarkDotNet.Attributes.ShortRunJob]
public class PolymorphicBenchmarks
{
    private Animal[] _animals = null!;
    private BenchmarkMapper _mapper = null!;
    private MapperlyBenchmarkMapper _mapperly = null!;

    [GlobalSetup]
    public void Setup()
    {
        _animals = new Animal[]
        {
            new Dog  { Name = "Rex",  Breed = "Labrador" },
            new Cat  { Name = "Luna", IsIndoor = true },
            new Fish { Name = "Nemo", Species = "Clownfish" },
            new Dog  { Name = "Max",  Breed = "Beagle" }
        };
        _mapper = new BenchmarkMapper();
        _mapperly = new MapperlyBenchmarkMapper();
    }

    [Benchmark(Baseline = true)]
    public AnimalDto[] Manual_Polymorphic()
    {
        var result = new AnimalDto[_animals.Length];
        for (int i = 0; i < _animals.Length; i++)
        {
            result[i] = _animals[i] switch
            {
                Dog d => new DogDto { Name = d.Name, Breed = d.Breed },
                Cat c => new CatDto { Name = c.Name, IsIndoor = c.IsIndoor },
                Fish f => new FishDto { Name = f.Name, Species = f.Species },
                _ => throw new InvalidOperationException()
            };
        }
        return result;
    }

    [Benchmark]
    public AnimalDto[] Generator_Polymorphic()
    {
        var result = new AnimalDto[_animals.Length];
        for (int i = 0; i < _animals.Length; i++)
            result[i] = _mapper.MapAnimal(_animals[i]);
        return result;
    }

    [Benchmark]
    public AnimalDto[] Mapperly_Polymorphic()
    {
        var result = new AnimalDto[_animals.Length];
        for (int i = 0; i < _animals.Length; i++)
            result[i] = _mapperly.MapAnimal(_animals[i]);
        return result;
    }
}

/// <summary>Entry point for the EricksonLopez.Mapper benchmark runner.</summary>
public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<MappingBenchmarks>();
        BenchmarkRunner.Run<ValueObjectBenchmarks>();
        BenchmarkRunner.Run<PolymorphicBenchmarks>();
    }
}



