// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using AwesomeAssertions;
using EricksonLopez.Mapper.Generator;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Core;

/// <summary>Contains unit tests that verify the value equality and collection semantics of <see cref="EricksonLopez.Mapper.Generator.EquatableArray{T}"/>.</summary>
[Trait("Category", "FastAst")]
public class EquatableArrayTests
{
    [Fact]
    public void Constructor_WithDefaultArray_ShouldCreateEmptyArray()
    {
        // Arrange & Act
        var array = new EquatableArray<int>(default);

        // Assert
        array.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithArray_ShouldWrapArray()
    {
        // Arrange
        var source = ImmutableArray.Create(1, 2, 3);

        // Act
        var array = new EquatableArray<int>(source);

        // Assert
        array.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void IsDefault_WhenDefault_ShouldBeTrue()
    {
        var array = new EquatableArray<int>(default);
        array.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void IsDefault_WhenNotDefault_ShouldBeFalse()
    {
        var array = new EquatableArray<int>(ImmutableArray.Create(1));
        array.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void Count_WhenDefault_ShouldBeZero()
    {
        var array = new EquatableArray<int>(default);
        array.Count.Should().Be(0);
    }

    [Fact]
    public void Count_WhenNotDefault_ShouldReturnLength()
    {
        var array = new EquatableArray<int>(ImmutableArray.Create(1, 2));
        array.Count.Should().Be(2);
    }

    [Fact]
    public void Indexer_ShouldReturnElement()
    {
        var array = new EquatableArray<int>(ImmutableArray.Create(10, 20));
        array[0].Should().Be(10);
        array[1].Should().Be(20);
    }

    [Fact]
    public void AsImmutableArray_ShouldReturnUnderlyingArray()
    {
        var source = ImmutableArray.Create(1, 2, 3);
        var array = new EquatableArray<int>(source);
        array.AsImmutableArray().Should().BeEquivalentTo(source);
    }

    [Fact]
    public void Equals_WithSameInstance_ShouldReturnTrue()
    {
        // Arrange
        var array = new EquatableArray<int>(ImmutableArray.Create(1, 2, 3));

        // Act & Assert
        array.Equals(array).Should().BeTrue();
    }

    [Fact]
    public void Equals_BothDefault_ShouldReturnTrue()
    {
        var array1 = new EquatableArray<int>(default);
        var array2 = new EquatableArray<int>(default);
        array1.Equals(array2).Should().BeTrue();
    }

    [Fact]
    public void Equals_OneDefaultOtherNot_ShouldReturnFalse()
    {
        var array1 = new EquatableArray<int>(default);
        var array2 = new EquatableArray<int>(ImmutableArray.Create(1));
        array1.Equals(array2).Should().BeFalse();
        array2.Equals(array1).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameContent_ShouldReturnTrue()
    {
        // Arrange
        var array1 = new EquatableArray<int>(ImmutableArray.Create(1, 2, 3));
        var array2 = new EquatableArray<int>(ImmutableArray.Create(1, 2, 3));

        // Act & Assert
        array1.Equals(array2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentContent_ShouldReturnFalse()
    {
        // Arrange
        var array1 = new EquatableArray<int>(ImmutableArray.Create(1, 2, 3));
        var array2 = new EquatableArray<int>(ImmutableArray.Create(1, 2, 4));

        // Act & Assert
        array1.Equals(array2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameContent_ShouldBeEqual()
    {
        // Arrange
        var array1 = new EquatableArray<int>(ImmutableArray.Create(1, 2, 3));
        var array2 = new EquatableArray<int>(ImmutableArray.Create(1, 2, 3));

        // Act & Assert
        array1.GetHashCode().Should().Be(array2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDefaultArray_ShouldBeZero()
    {
        // Arrange
        var array = new EquatableArray<int>(default);

        // Act & Assert
        array.GetHashCode().Should().Be(0);
    }

    [Fact]
    public void GetHashCode_WithNullElements_ShouldBeDeterministicAndDistinguishDifferentArrays()
    {
        var array1 = new EquatableArray<int?>(ImmutableArray.Create<int?>(1, null, 2));
        var array2 = new EquatableArray<int?>(ImmutableArray.Create<int?>(1, null, 2));
        var array3 = new EquatableArray<int?>(ImmutableArray.Create<int?>(1, 2, null));

        var hash1 = array1.GetHashCode();
        var hash2 = array2.GetHashCode();
        var hash3 = array3.GetHashCode();

        hash1.Should().Be(hash2);
        hash1.Should().NotBe(hash3);
    }

    [Fact]
    public void Equals_Object_WithValidType_ShouldCompare()
    {
        // Arrange
        object array1 = new EquatableArray<int>(ImmutableArray.Create(1, 2, 3));
        var array2 = new EquatableArray<int>(ImmutableArray.Create(1, 2, 3));

        // Act & Assert
        array1.Equals(array2).Should().BeTrue();
    }

    [Fact]
    public void Equals_Object_WithInvalidType_ShouldReturnFalse()
    {
        // Arrange
        object array1 = new EquatableArray<int>(ImmutableArray.Create(1, 2, 3));
        var array2 = new object();

        // Act & Assert
        array1.Equals(array2).Should().BeFalse();
    }

    [Fact]
    public void Equals_Object_Null_ShouldReturnFalse()
    {
        var array = new EquatableArray<int>(ImmutableArray.Create(1));
        array.Equals((object?)null).Should().BeFalse();
    }

    [Fact]
    public void Equals_Object_WithInvalidTypeAndDefaultInstance_ShouldReturnFalse()
    {
        var array = new EquatableArray<int>(default);
        object other = "string";
        array.Equals(other).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WhenArraysHaveDifferentContent_ShouldReturnDistinctHashes()
    {
        var array1 = new EquatableArray<int>(ImmutableArray.Create(1, 2));
        var array2 = new EquatableArray<int>(ImmutableArray.Create(2, 1));
        var array3 = new EquatableArray<int>(ImmutableArray.Create(1, 2));

        array1.GetHashCode().Should().Be(array3.GetHashCode());
        array1.GetHashCode().Should().NotBe(array2.GetHashCode());

        // Verify that prefix difference is preserved across iterations (kills hash / 31 mutant)
        var arrayPrefix0 = new EquatableArray<int>(ImmutableArray.Create(0, 1));
        var arrayPrefix5 = new EquatableArray<int>(ImmutableArray.Create(5, 1));
        arrayPrefix0.GetHashCode().Should().NotBe(arrayPrefix5.GetHashCode());

        // Verify arithmetic addition vs subtraction (kills hash * 31 - item mutant)
        unchecked
        {
            int expected = 17;
            expected = expected * 31 + 1;
            expected = expected * 31 + 2;
            array1.GetHashCode().Should().Be(expected);
        }
    }

    [Fact]
    public void Enumerator_ShouldEnumerateElements()
    {
        // Arrange
        var source = ImmutableArray.Create(1, 2, 3);
        var array = new EquatableArray<int>(source);

        // Act & Assert
        var result = new List<int>();
        foreach (var item in array)
        {
            result.Add(item);
        }

        result.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void IEnumerable_GetEnumerator_ShouldEnumerateElements()
    {
        var source = ImmutableArray.Create(1, 2, 3);
        IEnumerable array = new EquatableArray<int>(source);

        var result = new List<int>();
        var enumerator = array.GetEnumerator();
        while (enumerator.MoveNext())
        {
            result.Add((int)enumerator.Current);
        }

        result.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void Enumerator_WhenDefault_ShouldBeEmpty()
    {
        var array = new EquatableArray<int>(default);
        var enumerator = array.GetEnumerator();
        enumerator.MoveNext().Should().BeFalse();
    }

    [Fact]
    public void ImplicitOperator_ImmutableArrayToEquatableArray_ShouldWrap()
    {
        var source = ImmutableArray.Create(1, 2, 3);
        EquatableArray<int> array = source;
        array.AsImmutableArray().Should().BeEquivalentTo(source);
    }

    [Fact]
    public void ImplicitOperator_EquatableArrayToImmutableArray_ShouldUnwrap()
    {
        var array = new EquatableArray<int>(ImmutableArray.Create(1, 2, 3));
        ImmutableArray<int> source = array;
        source.Should().BeEquivalentTo(array.AsImmutableArray());
    }

    [Fact]
    public void EquatableArray_Empty_ShouldReturnEmptyInstance()
    {
        var empty = EquatableArray<int>.Empty;
        empty.Count.Should().Be(0);
        empty.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void EquatableArrayExtensions_AsEquatableArray_ShouldCreateArray()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act
        var array = source.AsEquatableArray();

        // Assert
        array.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void NonGeneric_GetEnumerator_ShouldWork()
    {
        var array = new EquatableArray<int>(ImmutableArray.Create(1, 2));
        var enumerator = ((System.Collections.IEnumerable)array).GetEnumerator();
        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Should().Be(1);
    }

    [Fact]
    public void GetHashCode_WhenArrayContainsNullElement_ShouldComputeHashCodeWithoutThrowing()
    {
        // Arrange
        var array1 = new EquatableArray<string?>(ImmutableArray.Create<string?>("A", null, "B"));
        var array2 = new EquatableArray<string?>(ImmutableArray.Create<string?>("A", null, "B"));
        var array3 = new EquatableArray<string?>(ImmutableArray.Create<string?>("A", "C", "B"));

        // Act
        int hash1 = array1.GetHashCode();
        int hash2 = array2.GetHashCode();
        int hash3 = array3.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
        hash1.Should().NotBe(hash3);
    }
}






