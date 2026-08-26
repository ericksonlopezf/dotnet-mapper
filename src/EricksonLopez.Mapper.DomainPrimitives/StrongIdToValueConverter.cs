// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.DomainPrimitives;

namespace EricksonLopez.Mapper.DomainPrimitives;

/// <summary>
/// Provides conversion from an <see cref="IStrongId{TSelf, TValue}"/> instance to its underlying value.
/// </summary>
/// <typeparam name="TStrongId">The strongly-typed identifier type.</typeparam>
/// <typeparam name="TValue">The underlying value type.</typeparam>
public sealed class StrongIdToValueConverter<TStrongId, TValue> : IConverter<TStrongId, TValue>
    where TStrongId : IStrongId<TStrongId, TValue>
    where TValue : notnull, IComparable<TValue>, IEquatable<TValue>
{
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public TValue Convert(TStrongId source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Value;
    }
}
