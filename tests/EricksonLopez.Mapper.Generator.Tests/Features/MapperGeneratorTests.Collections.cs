// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
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
    public async Task Generate_WhenNestedObjectsAndCollections_ShouldCreateCorrectMapping()
    {
        var source = @"
using EricksonLopez.Mapper;

namespace TestNamespace;

public class Address
{
    public string City { get; set; }
}
public class AddressDto
{
    public string City { get; set; }
}

public class User
{
    public string Name { get; set; }
    public Address PrimaryAddress { get; set; }
    public Address[] PastAddresses { get; set; }
    public List<Address> Aliases { get; set; }
}

public class UserDto
{
    public string Name { get; set; }
    public AddressDto PrimaryAddress { get; set; }
    public AddressDto[] PastAddresses { get; set; }
    public List<AddressDto> Aliases { get; set; }
}

[Mapper]
public partial class UserMapper
{
    public partial UserDto ToDto(User source);
    public partial AddressDto MapAddress(Address source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenCustomMappingAndDictionaries_ShouldCreateCorrectMapping()
    {
        var source = @"

namespace TestNamespace;

public class CustomUser
{
    public string SecretKey { get; set; }
    public string InternalName { get; set; }
    public Dictionary<string, int> Metadata { get; set; }
}

public class CustomUserDto
{
    public string Name { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}

[Mapper]
public partial class UserMapper
{
    [MapProperty(""InternalName"", ""Name"")]
    [MapIgnore(""SecretKey"")]
    public partial CustomUserDto ToDto(CustomUser source);
    
    public partial string MapIntToString(int value);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenDictionaryMapping_ShouldMapSuccessfully()
    {
        var source = @"
namespace TestNamespace;
public class Source { public Dictionary<int, string> Dict { get; set; } }
public class Dest { public Dictionary<int, string> Dict { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenArrayToHashSet_ShouldMapSuccessfully()
    {
        var source = @"
namespace TestNamespace;
public class Source { public int[] Tags { get; set; } }
public class Dest   { public HashSet<int> Tags { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenHashSetAsTarget_ShouldMapSuccessfully()
    {
        var source = @"
namespace TestNamespace;
public class Source { public List<string> Tags { get; set; } }
public class Dest   { public HashSet<string> Tags { get; set; } }
[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenImmutableArray_ShouldMapCorrectly()
    {
        var source = @"
using System.Collections.Immutable;

namespace TestNamespace;

public class Source { public ImmutableArray<int> Items { get; set; } }
public class Dest { public ImmutableArray<int> Items { get; set; } }

[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public void Map_WhenSourceIsImmutableArray_ShouldCheckForIsDefault()
    {
        string source = @"
using System.Collections.Immutable;

namespace TestNamespace;

public class Source { public ImmutableArray<int> Items { get; set; } }
public class Dest { public List<int> Items { get; set; } }

[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("!source.Items.IsDefault");
    }

    [Fact]
    public async Task Generate_WhenIReadOnlyListAndCollection_ShouldMapSuccessfully()
    {
        var source = @"

namespace TestNamespace;

public class Source { public List<int> Items { get; set; } = new(); }
public class Dest { public IReadOnlyList<int> Items { get; set; } = new List<int>(); }

[Mapper]
public partial class Mapper
{
    public partial Dest Map(Source source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public async Task Generate_WhenNestedCollections_ShouldCreateCorrectMapping()
    {
        var source = @"
using System.Collections.Generic;

namespace TestNamespace;

public class NestedSource
{
    public List<List<int>> Matrix { get; set; }
    public Dictionary<string, List<string>> GroupedNames { get; set; }
}

public class NestedDest
{
    public List<List<int>> Matrix { get; set; }
    public Dictionary<string, List<string>> GroupedNames { get; set; }
}

[Mapper]
public partial class Mapper
{
    public partial NestedDest Map(NestedSource source);
}
";
        await RunGeneratorAndVerify(source);
    }

    [Fact]
    public void Map_WhenTargetIsImmutableListOrFrozenSet_ShouldGenerateBuilders()
    {
        string source = @"
using System.Collections.Frozen;

public class Source { public List<int> Numbers { get; set; } = new(); }
public class Target { public ImmutableList<int> Numbers { get; set; } }

[Mapper]
public partial class CollectionMapper
{
    public partial Target Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("global::System.Collections.Immutable.ImmutableList.CreateBuilder<int>()");
        output.Should().Contain(".ToImmutable()");
    }

    [Fact]
    public void Map_WhenNestedGenericCollectionsAndWrappers_ShouldGenerateValidMappingAndCompile()
    {
        string source = @"
namespace TestNamespace;

public class ItemSource<T> { public T Value { get; set; } = default!; }
public class ItemDest<T> { public T Value { get; set; } = default!; }

public class ContainerSource<T>
{
    public string Name { get; set; } = string.Empty;
    public List<ItemSource<T>> Items { get; set; } = new();
}

public class ContainerDest<T>
{
    public string Name { get; set; } = string.Empty;
    public List<ItemDest<T>> Items { get; set; } = new();
}

[Mapper]
public partial class GenericContainerMapper
{
    public partial ItemDest<string> MapItem(ItemSource<string> source);
    public partial ContainerDest<string> MapContainer(ContainerSource<string> source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Name = source.Name");
        output.Should().Contain("this.MapItem(");
        output.Should().Contain("new global::System.Collections.Generic.List<global::TestNamespace.ItemDest<string>>(source.Items.Count);");
    }

    [Fact]
    public void Map_WhenMultiLevelGenericDictionaryAndList_ShouldGenerateValidMappingAndCompile()
    {
        string source = @"
namespace TestNamespace;

public class PayloadSource
{
    public int Id { get; set; }
    public Dictionary<string, List<int>> Matrix { get; set; } = new();
}

public class PayloadDest
{
    public int Id { get; set; }
    public Dictionary<string, List<int>> Matrix { get; set; } = new();
}

[Mapper]
public partial class MultiLevelGenericMapper
{
    public partial PayloadDest Map(PayloadSource source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);

        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Id = source.Id");
        output.Should().Contain("Matrix = source.Matrix");
    }
}




