// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper;

/// <summary>
/// Instructs the source generator to emit Microsoft.Extensions.DependencyInjection extension methods registering all non-static mappers.
/// </summary>
/// <remarks>
/// Applied at assembly level to generate an <c>AddGeneratedMappers</c> extension method on <c>IServiceCollection</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public sealed class GenerateMapperRegistrationAttribute : Attribute
{
}
