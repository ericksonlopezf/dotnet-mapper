// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.Mapper.Sample.Level0_Conceptual;
using EricksonLopez.Mapper.Sample.Level1_QuickStart;
using EricksonLopez.Mapper.Sample.Level10_Architecture;
using EricksonLopez.Mapper.Sample.Level2_Configuration;
using EricksonLopez.Mapper.Sample.Level3_RealWorld;
using EricksonLopez.Mapper.Sample.Level4_Advanced;
using EricksonLopez.Mapper.Sample.Level5_Processing;
using EricksonLopez.Mapper.Sample.Level6_ErrorHandling;
using EricksonLopez.Mapper.Sample.Level7_Scalability;
using EricksonLopez.Mapper.Sample.Level8_Customization;
using EricksonLopez.Mapper.Sample.Level9_Extensions;

namespace EricksonLopez.Mapper.Sample;

/// <summary>Entry point for the EricksonLopez.Mapper showcase application.</summary>
public static class Program
{
    /// <summary>Runs all demonstration levels in sequence.</summary>
    /// <param name="args">Command-line arguments (not used).</param>
    public static void Main(string[] args)
    {
        Console.WriteLine("=================================================");
        Console.WriteLine("  EricksonLopez.Mapper - AOT Source Generator    ");
        Console.WriteLine("  Executable Showcase & Reference Implementation ");
        Console.WriteLine("=================================================");
        Console.WriteLine();

        // --- Level 0: Conceptual ---
        ConceptualOverview.Run();

        // --- Level 1: Quick Start ---
        QuickStartDemo.Run();

        // --- Level 2: Configuration ---
        ConfigurationDemo.Run();
        NullFallbackDemo.Run();        // [MapNullFallback]
        ValueObjectDemo.Run();         // [ValueObject]
        ValueAssignmentDemo.Run();     // [MapValue]
        EnumMappingDemo.Run();         // [EnumMappingStrategy] ByName+IgnoreCase AND ByValue, [MapEnumValue]
        MapperDefaultsDemo.Run();      // [assembly: MapperDefaults]
        StaticMapperDemo.Run();        // static partial class mapper pattern

        // --- Level 3: Real World ---
        RealWorldDemo.Run();
        CollectionsDemo.Run();
        BuiltinConversionsDemo.Run();  // enum/Guid/DateTime/numeric
        CollectionTypesDemo.Run();     // Array/ImmutableArray/HashSet

        // --- Level 4: Advanced Integration ---
        AdvancedDemo.Run();
        FactoryMethodDemo.Run();       // [MapFactory]
        PolymorphicDemo.Run();         // [MapDerivedType]

        // --- Level 5: Processing ---
        ProcessingDemo.Run();

        // --- Level 6: Error Handling ---
        ErrorHandlingDemo.Run();

        // --- Level 7: Scalability ---
        ScalabilityDemo.Run();

        // --- Level 8: Customization ---
        CustomizationDemo.Run();       // [UseConverter(Type)] & [UseConverter(FieldName)]

        // --- Level 9: Extensions / DI & Ecosystem ---
        ExtensionsDemo.Run();
        DependencyInjectionDemo.Run(); // [GenerateMapperRegistration]
        DomainPrimitivesDemo.Run();    // EricksonLopez.Mapper.DomainPrimitives
        ResultIntegrationDemo.Run();   // EricksonLopez.Mapper.Result (all 4 overloads incl. ValueTask)
        MapsterBridgeDemo.Run();       // EricksonLopez.Mapper.Mapster (both ctors + UseConverter ext)

        // --- Level 10: Enterprise Architecture ---
        ArchitectureDemo.Run();        // Clean Architecture & [MapIgnoreSource]

        Console.WriteLine("=================================================");
        Console.WriteLine("  Showcase completed successfully!               ");
        Console.WriteLine("=================================================");
    }
}
