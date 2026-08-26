// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EricksonLopez.Mapper.Generator;

/// <summary>
/// Provides an immutable array implementation that implements value equality.
/// This is required for Roslyn incremental source generators to properly cache models.
/// </summary>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
{
    private readonly ImmutableArray<T> _underlyingArray;

    public EquatableArray(ImmutableArray<T> underlyingArray)
    {
        _underlyingArray = underlyingArray;
    }

    public static EquatableArray<T> Empty => new(ImmutableArray<T>.Empty);

    public T this[int index] => _underlyingArray[index];

    public int Count => _underlyingArray.IsDefault ? 0 : _underlyingArray.Length;

    public bool IsDefault => _underlyingArray.IsDefault;

    public ImmutableArray<T> AsImmutableArray() => _underlyingArray;

    public bool Equals(EquatableArray<T> other)
    {
        if (IsDefault && other.IsDefault) return true;
        if (IsDefault || other.IsDefault) return false;

        return _underlyingArray.SequenceEqual(other._underlyingArray);
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        if (IsDefault) return 0;

        unchecked
        {
            int hash = 17;
            foreach (var item in _underlyingArray)
            {
                hash = hash * 31 + (item == null ? 0 : item.GetHashCode());
            }
            return hash;
        }
    }

    public IEnumerator<T> GetEnumerator() => (_underlyingArray.IsDefault ? ImmutableArray<T>.Empty : _underlyingArray).AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator EquatableArray<T>(ImmutableArray<T> array) => new(array);
    public static implicit operator ImmutableArray<T>(EquatableArray<T> array) => array.AsImmutableArray();
}

