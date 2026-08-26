// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.DomainPrimitives;

namespace EricksonLopez.Mapper.DomainPrimitives;

/// <summary>
/// Provides conversion from an <see cref="IDomainPrimitive{TSelf, TValue}"/> instance to its underlying primitive value.
/// </summary>
/// <typeparam name="TPrimitive">The domain primitive type.</typeparam>
/// <typeparam name="TValue">The underlying primitive value type.</typeparam>
public sealed class DomainPrimitiveToValueConverter<TPrimitive, TValue> : IConverter<TPrimitive, TValue>
    where TPrimitive : IDomainPrimitive<TPrimitive, TValue>
    where TValue : notnull, IComparable<TValue>, IEquatable<TValue>
{
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public TValue Convert(TPrimitive source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return source.Value;
    }
}
