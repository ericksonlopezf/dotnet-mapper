// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.DomainPrimitives;

namespace EricksonLopez.Mapper.DomainPrimitives;

#if NET7_0_OR_GREATER
/// <summary>
/// Provides conversion from a primitive value to an <see cref="IDomainPrimitive{TSelf, TValue}"/> instance.
/// </summary>
/// <typeparam name="TValue">The primitive value type.</typeparam>
/// <typeparam name="TPrimitive">The domain primitive type.</typeparam>
public sealed class ValueToDomainPrimitiveConverter<TValue, TPrimitive> : IConverter<TValue, TPrimitive>
    where TPrimitive : IDomainPrimitive<TPrimitive, TValue>
    where TValue : notnull, IComparable<TValue>, IEquatable<TValue>
{
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
    public TPrimitive Convert(TValue source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return TPrimitive.Create(source);
    }
}
#endif
