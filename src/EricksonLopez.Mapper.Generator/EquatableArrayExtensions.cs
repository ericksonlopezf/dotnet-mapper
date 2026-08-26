// Copyright © Erickson Lopez. MIT License.
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EricksonLopez.Mapper.Generator;

internal static class EquatableArrayExtensions
{
    public static EquatableArray<T> AsEquatableArray<T>(this IEnumerable<T> source)
    {
        return new EquatableArray<T>(source.ToImmutableArray());
    }
}
