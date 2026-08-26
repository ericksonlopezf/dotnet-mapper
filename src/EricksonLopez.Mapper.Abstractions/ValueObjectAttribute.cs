// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Marks a class or struct as a single-value wrapper (Value Object) to be mapped by wrapping or unwrapping its underlying value.
/// </summary>
/// <remarks>
/// When mapping between a primitive and a type annotated with this attribute, the generator extracts or passes the inner value directly.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class ValueObjectAttribute : Attribute
{
}
