// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Strategies;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

public partial class ConversionStrategyExhaustiveTests
{
    [Fact]
    public void Resolve_WhenCollectionMapping_ShouldGenerateCapacitySizing()
    {
        string source = @"
using System.Collections.ObjectModel;

namespace TestNamespace;

public class Source
{
    public ICollection<int> Col1 { get; set; } = new List<int>();
    public IReadOnlyCollection<int> Col2 { get; set; } = new List<int>();
    public IList<int> Col3 { get; set; } = new List<int>();
    public IReadOnlyList<int> Col4 { get; set; } = new List<int>();
    public List<int> Col5 { get; set; } = new List<int>();
    public HashSet<int> Col6 { get; set; } = new HashSet<int>();
    public Collection<int> Col7 { get; set; } = new Collection<int>();
}

public class Dest
{
    public List<int> Col1 { get; set; } = new();
    public List<int> Col2 { get; set; } = new();
    public List<int> Col3 { get; set; } = new();
    public List<int> Col4 { get; set; } = new();
    public List<int> Col5 { get; set; } = new();
    public List<int> Col6 { get; set; } = new();
    public List<int> Col7 { get; set; } = new();
}

[Mapper]
public partial class ColTypesMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("TryGetNonEnumeratedCount(source.Col1, out int _c2)");
        output.Should().Contain("TryGetNonEnumeratedCount(source.Col2, out int _c3)");
        output.Should().Contain("new global::System.Collections.Generic.List<int>(source.Col3.Count)");
        output.Should().Contain("new global::System.Collections.Generic.List<int>(source.Col4.Count)");
        output.Should().Contain("new global::System.Collections.Generic.List<int>(source.Col6.Count)");
        output.Should().Contain("new global::System.Collections.Generic.List<int>(source.Col7.Count)");
    }

    [Fact]
    public void Resolve_WhenClassHasValueObjectAttribute_ShouldWrapAndUnwrap()
    {
        string source = @"

namespace TestNamespace;

[ValueObject]
public class CustomerId
{
    public Guid Value { get; }
    public CustomerId(Guid value) => Value = value;
}

public class Source { public Guid Id { get; set; } }
public class Dest   { public CustomerId Id { get; set; } }

public class SourceBack { public CustomerId Id { get; set; } }
public class DestBack   { public Guid Id { get; set; } }

[Mapper]
public partial class VoAttrMapper
{
    public partial Dest Map(Source source);
    public partial DestBack MapBack(SourceBack source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("CustomerId(source.Id)");
        output.Should().Contain("Id = source.Id.Value");
    }

    [Fact]
    public void Resolve_WhenUseConverterHasNonMatchingTypeArgs_ShouldEmitELM003()
    {
        string source = @"

namespace TestNamespace;

public class WrongConverter : IConverter<int, string>
{
    public string Convert(int source) => source.ToString();
}

public class Source { public Guid Id { get; set; } }
public class Dest   { public DateTime Id { get; set; } }

[Mapper]
public partial class MismatchConvMapper
{
    [UseConverter(typeof(WrongConverter))]
    public partial Dest Map(Source source);
}
";
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Should().Contain(d => d.Id == "ELM003");
    }

    [Fact]
    public void Resolve_WhenIncompatibleSourceToCollectionTargets_ShouldFallThroughToUnsupported()
    {
        string source = @"

namespace TestNamespace;

public class Source
{
    public int ToHashSet { get; set; }
    public int ToDict { get; set; }
    public int ToList { get; set; }
    public int ToVoNoCtor { get; set; }
}

public class VoNo1ParamCtor
{
    public int Value { get; }
    public VoNo1ParamCtor(int a, int b) { Value = a + b; }
}

public class Dest
{
    public HashSet<string> ToHashSet { get; set; } = new();
    public Dictionary<string, string> ToDict { get; set; } = new();
    public List<string> ToList { get; set; } = new();
    public VoNo1ParamCtor ToVoNoCtor { get; set; } = null!;
}

[Mapper(StrictMapping = false)]
public partial class FallthroughMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorSimple(source);
        var elm003Diags = diagnostics.Where(d => d.Id == "ELM003").ToList();
        elm003Diags.Should().HaveCount(4);
        elm003Diags.Should().Contain(d => d.GetMessage().Contains("ToHashSet"));
        elm003Diags.Should().Contain(d => d.GetMessage().Contains("ToDict"));
        elm003Diags.Should().Contain(d => d.GetMessage().Contains("ToList"));
        elm003Diags.Should().Contain(d => d.GetMessage().Contains("ToVoNoCtor"));
    }

    [Fact]
    public void Resolve_WhenHashSetFromReadOnlyCollection_ShouldEmitCapacitySizing()
    {
        string source = @"

namespace TestNamespace;

public class Source { public IReadOnlyCollection<int> Items { get; set; } = new List<int>(); }
public class Dest   { public HashSet<int> Items { get; set; } = new(); }

[Mapper]
public partial class HashSetMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::System.Collections.Generic.HashSet<int>()");
        output.Should().Contain("_col1.Add(");
    }

    [Fact]
    public void Resolve_WhenDictionaryWithComplexKeyAndValue_ShouldMapBoth()
    {
        string source = @"

namespace TestNamespace;

public enum PrioritySource { Low = 1, High = 2 }
public enum PriorityDest { Low = 10, High = 20 }

public class Source { public Dictionary<PrioritySource, DateTime> Events { get; set; } = new(); }
public class Dest   { public IDictionary<PriorityDest, DateOnly> Events { get; set; } = new Dictionary<PriorityDest, DateOnly>(); }

[Mapper]
public partial class ComplexDictMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::System.Collections.Generic.Dictionary<global::TestNamespace.PriorityDest, global::System.DateOnly>(source.Events.Count)");
        output.Should().Contain("global::System.DateOnly.FromDateTime");
        output.Should().Contain("PrioritySource.Low => global::TestNamespace.PriorityDest.Low");
    }

    [Fact]
    public void Resolve_WhenValueObjectExplicitAttributeAndCustomProperty_ShouldUnwrapAndWrap()
    {
        string source = @"

namespace TestNamespace;

[ValueObject]
public class CustomVo
{
    public int Value { get; }
    public CustomVo(int value) => Value = value;
}

public class Source { public CustomVo Vo { get; set; } }
public class Dest   { public int Vo { get; set; } }

[Mapper]
public partial class VoUnwrapMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Vo = source.Vo.Value");
    }

    [Fact]
    public void Resolve_WhenValueObjectExplicitAttributeWrap_ShouldInvokeSingleParamConstructor()
    {
        string source = @"

namespace TestNamespace;

[ValueObject]
public class CustomWrapVo
{
    public int Value { get; }
    public CustomWrapVo(int value) => Value = value;
}

public class Source { public int Vo { get; set; } }
public class Dest   { public CustomWrapVo Vo { get; set; } }

[Mapper]
public partial class VoWrapMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("Vo = new global::TestNamespace.CustomWrapVo(source.Vo)");
    }

    [Fact]
    public void Resolve_WhenSourceIsReadOnlyCollection_ShouldPresizeWithCount()
    {
        string source = @"

namespace TestNamespace;

public class Source { public IReadOnlyCollection<int> Items { get; set; } = new List<int>(); }
public class Dest   { public List<int> Items { get; set; } = new(); }

[Mapper]
public partial class ReadOnlyColMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("TryGetNonEnumeratedCount(source.Items");
    }

    [Fact]
    public void Resolve_WhenSourceIsEnumerable_ShouldUseDefaultCapacity()
    {
        string source = @"

namespace TestNamespace;

public class Source { public IEnumerable<int> Items { get; set; } = new List<int>(); }
public class Dest   { public List<int> Items { get; set; } = new(); }

[Mapper]
public partial class EnumerableMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::System.Collections.Generic.List<int>()");
    }

    [Fact]
    public void Resolve_WhenTargetIsHashSet_ShouldInstantiateHashSet()
    {
        string source = @"

namespace TestNamespace;

public class Source { public int[] Items { get; set; } = System.Array.Empty<int>(); }
public class Dest   { public HashSet<int> Items { get; set; } = new(); }

[Mapper]
public partial class HashSetMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::System.Collections.Generic.HashSet<int>(source.Items.Length)");
    }

    [Fact]
    public void Resolve_WhenCustomPureIEnumerableSource_ShouldEmitTryGetNonEnumeratedCount()
    {
        string source = @"
using System.Collections;

namespace TestNamespace;

public class CustomPureSequence<T> : IEnumerable<T>
{
    private readonly List<T> _inner = new();
    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class Source { public CustomPureSequence<int> Items { get; set; } = new(); }
public class Dest { public List<int> Items { get; set; } = new(); }

[Mapper]
public partial class PureEnumerableMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("TryGetNonEnumeratedCount(source.Items, out int _c");
    }

    [Fact]
    public void Resolve_WhenPureICollectionSource_ShouldEmitDirectCountAccess()
    {
        string source = @"

namespace TestNamespace;

public class PureCollection<T> : ICollection<T>
{
    private readonly List<T> _inner = new();
    public int Count => _inner.Count;
    public bool IsReadOnly => false;
    public void Add(T item) => _inner.Add(item);
    public void Clear() => _inner.Clear();
    public bool Contains(T item) => _inner.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);
    public bool Remove(T item) => _inner.Remove(item);
    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class Source { public PureCollection<int> Items { get; set; } = new(); }
public class Dest { public List<int> Items { get; set; } = new(); }

[Mapper]
public partial class PureCollectionMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::System.Collections.Generic.List<int>(source.Items.Count)");
    }

    [Fact]
    public void Resolve_WhenPureIReadOnlyCollectionSource_ShouldEmitDirectCountAccess()
    {
        string source = @"

namespace TestNamespace;

public class PureReadOnlyCollection<T> : IReadOnlyCollection<T>
{
    private readonly List<T> _inner = new();
    public int Count => _inner.Count;
    public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class Source { public PureReadOnlyCollection<int> Items { get; set; } = new(); }
public class Dest { public List<int> Items { get; set; } = new(); }

[Mapper]
public partial class PureReadOnlyCollectionMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::System.Collections.Generic.List<int>(source.Items.Count)");
    }

    [Fact]
    public void Resolve_WhenTargetIsImmutableArray_ShouldEmitBuilderWithCapacity()
    {
        string source = @"
using System.Collections.Immutable;

namespace TestNamespace;

public class Source { public List<int> Numbers { get; set; } = new(); }
public class Dest { public ImmutableArray<int> Numbers { get; set; } }

[Mapper]
public partial class ImmutableArrayMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("CreateBuilder<int>(source.Numbers.Count)");
    }

    [Fact]
    public void Resolve_WhenTargetIsImmutableList_ShouldEmitImmutableListBuilder()
    {
        string source = @"

namespace TestNamespace;

public class Source { public List<int> Numbers { get; set; } = new(); }
public class Dest { public ImmutableList<int> Numbers { get; set; } = ImmutableList<int>.Empty; }

[Mapper]
public partial class ImmutableListMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("global::System.Collections.Immutable.ImmutableList.CreateBuilder<int>()");
        output.Should().Contain(".ToImmutable()");
    }

    [Fact]
    public void Resolve_WhenTargetIsFrozenSet_ShouldEmitToFrozenSet()
    {
        string source = @"
using System.Collections.Frozen;

namespace TestNamespace;

public class Source { public List<string> Items { get; set; } = new(); }
public class Dest { public FrozenSet<string> Items { get; set; } = FrozenSet<string>.Empty; }

[Mapper]
public partial class FrozenSetMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("global::System.Collections.Frozen.FrozenSet.ToFrozenSet");
    }

    [Fact]
    public void Resolve_WhenDictionarySourceIsIReadOnlyDictionary_ShouldEmitCountPresizing()
    {
        string source = @"

namespace TestNamespace;

public class Source { public IReadOnlyDictionary<string, int> Dict { get; set; } = new Dictionary<string, int>(); }
public class Dest { public Dictionary<string, int> Dict { get; set; } = new(); }

[Mapper]
public partial class ReadOnlyDictMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::System.Collections.Generic.Dictionary<string, int>(source.Dict.Count)");
    }

    [Fact]
    public void Resolve_WhenDictionarySourceIsIDictionary_ShouldEmitCountPresizing()
    {
        string source = @"

namespace TestNamespace;

public class Source { public IDictionary<string, int> Dict { get; set; } = new Dictionary<string, int>(); }
public class Dest { public Dictionary<string, int> Dict { get; set; } = new(); }

[Mapper]
public partial class IDictMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::System.Collections.Generic.Dictionary<string, int>(source.Dict.Count)");
    }

    [Fact]
    public void Resolve_WhenNestedCollectionInDictionary_ShouldEmitCapacitySizingAndCorrectMapping()
    {
        string source = @"
using System.Collections.Generic;

namespace TestNamespace;

public class ItemSource { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
public class ItemDest   { public int Id { get; set; } public string Name { get; set; } = string.Empty; }

public class Source { public Dictionary<string, List<ItemSource>> Categories { get; set; } = new(); }
public class Dest   { public Dictionary<string, List<ItemDest>> Categories { get; set; } = new(); }

[Mapper]
public partial class NestedCollectionMapper
{
    public partial Dest Map(Source source);
    public partial ItemDest MapItem(ItemSource source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.List<global::TestNamespace.ItemDest>>(source.Categories.Count)");
        output.Should().Contain("new global::System.Collections.Generic.List<global::TestNamespace.ItemDest>(");
    }

    [Fact]
    public void Resolve_WhenNestedListOfLists_ShouldEmitCapacitySizingAndCompileCleanly()
    {
        string source = @"
using System.Collections.Generic;

namespace TestNamespace;

public class ItemSource { public int Value { get; set; } }
public class ItemDest   { public int Value { get; set; } }

public class Source { public List<List<ItemSource>> Matrix { get; set; } = new(); }
public class Dest   { public List<List<ItemDest>> Matrix { get; set; } = new(); }

[Mapper]
public partial class MatrixMapper
{
    public partial Dest Map(Source source);
    public partial ItemDest MapItem(ItemSource source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::TestNamespace.ItemDest>>(source.Matrix.Count)");
    }

    [Fact]
    public void Resolve_WhenArrayToArrayWithElementWidening_ShouldEmitArrayMapping()
    {
        string source = @"
namespace TestNamespace;

public class Source { public byte[] Items { get; set; } = new byte[0]; }
public class Dest   { public long[] Items { get; set; } = new long[0]; }

[Mapper]
public partial class ArrayWideningMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new long[source.Items.Length]");
    }

    [Fact]
    public void Resolve_WhenHashSetTargetFromVariousSources_ShouldEmitPresizedHashSetInstantiation()
    {
        string source = @"
using System.Collections.Generic;

namespace TestNamespace;

public class Source
{
    public List<int> FromList { get; set; } = new();
    public int[] FromArray { get; set; } = new int[0];
    public HashSet<int> FromHashSet { get; set; } = new();
}

public class Dest
{
    public HashSet<int> FromList { get; set; } = new();
    public HashSet<int> FromArray { get; set; } = new();
    public HashSet<int> FromHashSet { get; set; } = new();
}

[Mapper]
public partial class HashSetMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("new global::System.Collections.Generic.HashSet<int>(source.FromList.Count)");
        output.Should().Contain("new global::System.Collections.Generic.HashSet<int>(source.FromArray.Length)");
        output.Should().Contain("FromHashSet = source.FromHashSet");
    }

    [Fact]
    public void Resolve_WhenTargetIsImmutableList_ShouldEmitBuilderAndToImmutable()
    {
        string source = @"
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TestNamespace;

public class Source { public List<int> Items { get; set; } = new(); }
public class Dest   { public ImmutableList<int> Items { get; set; } = ImmutableList<int>.Empty; }

[Mapper]
public partial class ImmutableListMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output, _) = GeneratorTestHelper.RunGenerator(source, verifyEmittedCodeCompiles: true);
        diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        output.Should().Contain("CreateBuilder<int>()");
        output.Should().Contain(".ToImmutable()");
    }

    [Fact]
    public void Resolve_WhenValueObjectHasMultipleProperties_ShouldNotUseValueObjectWrapping()
    {
        string source = @"
namespace TestNamespace;

public class ComplexVO
{
    public int Id { get; }
    public string Name { get; }
    public ComplexVO(int id, string name) { Id = id; Name = name; }
}

public class Source { public int Id { get; set; } }
public class Dest   { public ComplexVO Id { get; set; } = null!; }

[Mapper(StrictMapping = false)]
public partial class ComplexVOMapper
{
    public partial Dest Map(Source source);
}
";
        var (diagnostics, output) = GeneratorTestHelper.RunGeneratorSimple(source);
        diagnostics.Should().Contain(d => d.Id == "ELM003");
        output.Should().NotContain("new global::TestNamespace.ComplexVO(source.Id)");
    }
}




