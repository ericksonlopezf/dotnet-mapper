// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Mapper.Sample.Level0_Conceptual;

/// <summary>
/// Provides a conceptual overview of the EricksonLopez.Mapper library: its purpose,
/// design constraints, and trade-offs compared to reflection-based mappers.
/// </summary>
/// <remarks>
/// EricksonLopez.Mapper is an object mapper for .NET implemented entirely as a
/// Roslyn Incremental Source Generator. Unlike reflection-based mappers such as AutoMapper,
/// it generates explicit, verifiable C# mapping code at compile time, making it fully
/// compatible with Native AOT and code trimming.
/// <para>
/// Advantages: full Native AOT support, zero startup overhead, and compile-time detection
/// of unmapped members when <c>StrictMapping = true</c>.
/// </para>
/// <para>
/// Trade-offs: mapping APIs must be explicit (partial methods annotated with <c>[Mapper]</c>);
/// dynamic types and global runtime-configurable profiles are not supported.
/// </para>
/// </remarks>
public static class ConceptualOverview
{
    /// <summary>Runs the conceptual overview demonstration.</summary>
    public static void Run()
    {
        Console.WriteLine("=== Level 0: Conceptual ===");
        Console.WriteLine("EricksonLopez.Mapper is a Roslyn Source Generator that transforms object mapping into pure, verifiable, AOT-compatible C# code at compile-time.");
        Console.WriteLine();
    }
}
