// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EricksonLopez.Mapper;

namespace EricksonLopez.Mapper.AotTest;

/// <summary>Represents a strongly typed user identifier as a Value Object.</summary>
/// <param name="Value">The underlying <see cref="Guid"/> identifier value.</param>
public readonly record struct UserId(Guid Value);

/// <summary>Represents the flat source data for a user, used as the mapping origin.</summary>
public class UserSource
{
    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the full name of the user.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the list of current addresses associated with the user.</summary>
    public List<AddressSource> Addresses { get; set; } = new();
    /// <summary>Gets or sets the metadata key-value pairs associated with the user.</summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
    /// <summary>Gets or sets the past addresses associated with the user.</summary>
    public AddressSource[] PastAddresses { get; set; } = Array.Empty<AddressSource>();
}

/// <summary>Represents a source address record to be mapped into an <see cref="AddressValueObject"/>.</summary>
public class AddressSource
{
    /// <summary>Gets or sets the street name and number.</summary>
    public string Street { get; set; } = string.Empty;
    /// <summary>Gets or sets the city name.</summary>
    public string City { get; set; } = string.Empty;
}

/// <summary>
/// Represents an immutable user entity in the domain layer, constructed via the <see cref="Create"/> factory method.
/// </summary>
public class UserEntity
{
    /// <summary>Gets the strongly typed user identifier.</summary>
    public UserId Id { get; private set; }
    /// <summary>Gets the full name of the user.</summary>
    public string Name { get; private set; }
    /// <summary>Gets the immutable list of current addresses.</summary>
    public ImmutableArray<AddressValueObject> Addresses { get; private set; }
    /// <summary>Gets the read-only metadata key-value pairs, or <see langword="null"/> when no metadata is available.</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; private set; }
    /// <summary>Gets the past addresses associated with the user.</summary>
    public AddressValueObject[] PastAddresses { get; private set; }

    private UserEntity(
        UserId id,
        string name,
        ImmutableArray<AddressValueObject> addresses,
        IReadOnlyDictionary<string, string>? metadata,
        AddressValueObject[] pastAddresses)
    {
        Id = id;
        Name = name;
        Addresses = addresses;
        Metadata = metadata;
        PastAddresses = pastAddresses;
    }

    /// <summary>
    /// Creates a new <see cref="UserEntity"/> with the specified identity, addresses, and metadata.
    /// </summary>
    /// <param name="id">The strongly typed user identifier.</param>
    /// <param name="name">The full name of the user.</param>
    /// <param name="addresses">The immutable list of current addresses.</param>
    /// <param name="metadata">The metadata key-value pairs, or <see langword="null"/>.</param>
    /// <param name="pastAddresses">The past addresses associated with the user.</param>
    /// <returns>A new <see cref="UserEntity"/> instance.</returns>
    public static UserEntity Create(
        UserId id,
        string name,
        ImmutableArray<AddressValueObject> addresses,
        IReadOnlyDictionary<string, string>? metadata,
        AddressValueObject[] pastAddresses)
        => new UserEntity(id, name, addresses, metadata, pastAddresses);
}

/// <summary>Represents an immutable address value object.</summary>
/// <param name="Street">The street name and number.</param>
/// <param name="City">The city name.</param>
public readonly record struct AddressValueObject(string Street, string City);

/// <summary>
/// Provides compile-time-generated mapping from <see cref="UserSource"/> to <see cref="UserEntity"/>
/// and from <see cref="AddressSource"/> to <see cref="AddressValueObject"/>.
/// </summary>
[Mapper(StrictMapping = false)]
public partial class UserMapper
{
    /// <summary>
    /// Maps a <see cref="UserSource"/> to a <see cref="UserEntity"/> via the <see cref="UserEntity.Create"/> factory method.
    /// </summary>
    /// <param name="source">The user source to map from.</param>
    /// <returns>A new <see cref="UserEntity"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    [MapFactory("Create")]
    public partial UserEntity MapToEntity(UserSource source);

    /// <summary>Maps an <see cref="AddressSource"/> to an <see cref="AddressValueObject"/>.</summary>
    /// <param name="source">The address source to map from.</param>
    /// <returns>A new <see cref="AddressValueObject"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial AddressValueObject MapAddress(AddressSource source);
}

/// <summary>Represents an animal entity in the source domain.</summary>
public class AnimalSource
{
    /// <summary>Gets or sets the name of the animal.</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>Represents a dog entity, deriving from <see cref="AnimalSource"/>.</summary>
public class DogSource : AnimalSource
{
    /// <summary>Gets or sets the breed of the dog.</summary>
    public string Breed { get; set; } = string.Empty;
}

/// <summary>Represents an animal data transfer object.</summary>
public class AnimalDest
{
    /// <summary>Gets or sets the name of the animal.</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>Represents a dog data transfer object, deriving from <see cref="AnimalDest"/>.</summary>
public class DogDest : AnimalDest
{
    /// <summary>Gets or sets the breed of the dog.</summary>
    public string Breed { get; set; } = string.Empty;
}

/// <summary>Represents a custom date type with separate year, month, and day fields.</summary>
public class CustomDate
{
    /// <summary>Gets or sets the year component.</summary>
    public int Year { get; set; }
    /// <summary>Gets or sets the month component (1-12).</summary>
    public int Month { get; set; }
    /// <summary>Gets or sets the day component (1-31).</summary>
    public int Day { get; set; }
    /// <summary>Initializes a new instance of <see cref="CustomDate"/> with the specified date components.</summary>
    /// <param name="year">The year component.</param>
    /// <param name="month">The month component (1-12).</param>
    /// <param name="day">The day component (1-31).</param>
    public CustomDate(int year, int month, int day) { Year = year; Month = month; Day = day; }
}

/// <summary>Represents a string wrapper for a formatted date.</summary>
public class CustomString
{
    /// <summary>Gets or sets the formatted date string.</summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Converts a <see cref="CustomDate"/> to a <see cref="CustomString"/> formatted as <c>YYYY-MM-DD</c>.
/// </summary>
public class DateStringConverter : IConverter<CustomDate, CustomString>
{
    /// <inheritdoc/>
    public CustomString Convert(CustomDate source) => new CustomString { Value = $"{source.Year}-{source.Month:D2}-{source.Day:D2}" };
}

/// <summary>
/// Provides compile-time-generated mapping for animal types and date-to-string conversion.
/// </summary>
[Mapper]
public partial class ComplexMapper
{
    /// <summary>
    /// Maps an <see cref="AnimalSource"/> to the correct <see cref="AnimalDest"/> subtype
    /// based on the runtime type of <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The animal source to map from.</param>
    /// <returns>A <see cref="DogDest"/> when <paramref name="source"/> is a <see cref="DogSource"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    [MapDerivedType(typeof(DogSource), typeof(DogDest))]
    public partial AnimalDest MapAnimal(AnimalSource source);

    /// <summary>Maps a <see cref="DogSource"/> to a <see cref="DogDest"/>.</summary>
    /// <param name="source">The dog source to map from.</param>
    /// <returns>A new <see cref="DogDest"/> containing the mapped values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public partial DogDest MapDog(DogSource source);

    /// <summary>Converts a <see cref="CustomDate"/> to a <see cref="CustomString"/> formatted as <c>YYYY-MM-DD</c>.</summary>
    /// <param name="source">The date to convert.</param>
    /// <returns>A <see cref="CustomString"/> with the ISO-formatted date.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    [UseConverter(typeof(DateStringConverter))]
    public partial CustomString MapDate(CustomDate source);
}

internal sealed class Program
{
    static void Main()
    {
        var mapper = new UserMapper();

        var source = new UserSource
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Addresses = new List<AddressSource>
            {
                new AddressSource { Street = "123 Main St", City = "New York" }
            },
            Metadata = new Dictionary<string, string>
            {
                { "Role", "Admin" }
            },
            PastAddresses = new AddressSource[]
            {
                new AddressSource { Street = "456 Old St", City = "Boston" }
            }
        };

        var entity = mapper.MapToEntity(source);

        Console.WriteLine($"Mapped User: {entity.Name}");
        Console.WriteLine($"ID: {entity.Id.Value}");
        Console.WriteLine($"Addresses: {entity.Addresses.Length}");
        if (entity.Addresses.Length > 0)
        {
            Console.WriteLine($"First Address City: {entity.Addresses[0].City}");
        }
        Console.WriteLine($"Past Addresses: {entity.PastAddresses.Length}");
        Console.WriteLine($"Metadata Role: {entity.Metadata?["Role"]}");

        var complexMapper = new ComplexMapper();
        AnimalDest animal = complexMapper.MapAnimal(new DogSource { Name = "Rex", Breed = "German Shepherd" });
        Console.WriteLine($"Polymorphic Animal is Dog: {animal is DogDest}");

        var dateStr = complexMapper.MapDate(new CustomDate(2025, 1, 1));
        Console.WriteLine($"Custom Date Converter: {dateStr.Value}");

        Console.WriteLine("AOT mapping test completed successfully!");
    }
}
