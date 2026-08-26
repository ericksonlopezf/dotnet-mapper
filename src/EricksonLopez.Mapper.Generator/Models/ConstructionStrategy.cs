// Copyright © Erickson Lopez. MIT License.
using System.Diagnostics.CodeAnalysis;

namespace EricksonLopez.Mapper.Generator.Models;

// Excluded from coverage: Pure DTO record boilerplate.
[ExcludeFromCodeCoverage]
internal abstract record ConstructionStrategy
{
    public record ParameterizedConstructor(EquatableArray<ParameterMapping> Parameters) : ConstructionStrategy;
    public record ObjectInitializer() : ConstructionStrategy;
    public record FactoryMethod(string MethodName, EquatableArray<ParameterMapping> Parameters) : ConstructionStrategy;
    public record Unsupported(string Reason) : ConstructionStrategy;
}
