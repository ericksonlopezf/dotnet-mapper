// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using static EricksonLopez.Mapper.Generator.Tests.Infrastructure.TestDataBuilders;
using static EricksonLopez.Mapper.Generator.Tests.Infrastructure.TestDataBuilders.CreateStrategies;

namespace EricksonLopez.Mapper.Generator.Tests.Engine;

[Trait("Category", "FastAst")]
public class CycleDetectorTests
{
    private static Location CreateMockLocation(string filePath = "Test.cs", int startOffset = 0, int length = 10)
    {
        var sourceText = SourceText.From("public class TestMapper {}\npublic class OtherMapper {}");
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, path: filePath);
        var textSpan = new TextSpan(startOffset, length);
        return Location.Create(syntaxTree, textSpan);
    }

    [Fact]
    public void DetectCycles_WhenNoMethods_ShouldNotAddDiagnostics()
    {
        var methods = new List<Models.MethodMapping>();
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void DetectCycles_WhenLinearMapping_ShouldNotAddDiagnostics()
    {
        var m1 = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("PropA", "PropA", Direct())
            .Build();

        var m2 = CreateMethod("MethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .AddMember("PropB", "PropB", Direct())
            .Build();

        var methods = new List<Models.MethodMapping> { m1, m2 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void DetectCycles_WhenDirectSelfCycle_ShouldEmitDiagnostic()
    {
        var m1 = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("PropA", "PropA", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { m1 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation("Mapper.cs");

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(1);
        var diag = diagnostics[0];
        diag.Id.Should().Be("ELM010");
        diag.FilePath.Should().Be("Mapper.cs");
        diag.DefaultSeverity.Should().Be((int)DiagnosticSeverity.Error);
        diag.IsEnabledByDefault.Should().BeTrue();
        diag.Category.Should().Be("EricksonLopez.Mapper");
        diag.Args.AsImmutableArray().Should().ContainSingle().Which.Should().Be("MethodA");
    }

    [Fact]
    public void DetectCycles_WhenDirectSelfCycle_WithLocationWithoutSourceTree_ShouldHandleGracefully()
    {
        var m1 = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("PropA", "PropA", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { m1 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = Location.None;

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(1);
        diagnostics[0].FilePath.Should().BeEmpty();
    }

    [Fact]
    public void DetectCycles_WhenMutualCycle_ShouldEmitDiagnosticsForBothMethods()
    {
        var m1 = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("B", "B", Invocation("MethodB"))
            .Build();

        var m2 = CreateMethod("MethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .AddMember("A", "A", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { m1, m2 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(2);
        diagnostics[0].Id.Should().Be("ELM010");
        diagnostics[1].Id.Should().Be("ELM010");
    }

    [Fact]
    public void DetectCycles_ParameterizedConstructor_AndFactoryMethod_ShouldDetectCycles()
    {
        var m1 = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .WithParameterizedConstructor(
                CreateParameter("ParamB", "ParamB").WithStrategy(Invocation("MethodB")))
            .Build();

        var m2 = CreateMethod("MethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .WithFactoryMethod("Create",
                CreateParameter("ParamA", "ParamA").WithStrategy(Invocation("MethodA")))
            .Build();

        var methods = new List<Models.MethodMapping> { m1, m2 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(2);
    }

    [Fact]
    public void DetectCycles_EnumerableMapping_ShouldDetectCycles()
    {
        var enumStrategy = Enumerable(Invocation("MethodB"), "SrcItem", "TgtItem");

        var m1 = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("Items", "Items", enumStrategy)
            .Build();

        var m2 = CreateMethod("MethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .AddMember("Parent", "Parent", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { m1, m2 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(2);
    }

    [Fact]
    public void DetectCycles_DictionaryMapping_KeyAndValue_ShouldDetectCycles()
    {
        var dictStrategy1 = Dictionary(
            Invocation("MethodB"),
            Direct(),
            "SrcKey", "TgtKey", "SrcVal", "TgtVal");

        var m1 = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("Dict", "Dict", dictStrategy1)
            .Build();

        var dictStrategy2 = Dictionary(
            Direct(),
            Invocation("MethodA"),
            "SrcKey", "TgtKey", "SrcVal", "TgtVal");

        var m2 = CreateMethod("MethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .AddMember("Dict", "Dict", dictStrategy2)
            .Build();

        var methods = new List<Models.MethodMapping> { m1, m2 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(2);
    }

    [Fact]
    public void DetectCycles_ValueObjectMapping_ShouldDetectCycles()
    {
        var voStrategy = ValueObject(0, Invocation("MethodB"), "SrcVo", "TgtVo");

        var m1 = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("Vo", "Vo", voStrategy)
            .Build();

        var m2 = CreateMethod("MethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .AddMember("A", "A", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { m1, m2 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(2);
    }

    [Fact]
    public void DetectCycles_DAG_DiamondDependency_ShouldNotReportCycle()
    {
        var mD = CreateMethod("MethodD")
            .WithSourceType("SrcD")
            .WithTargetType("TgtD")
            .AddMember("P", "P", Direct())
            .Build();

        var mB = CreateMethod("MethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .AddMember("D", "D", Invocation("MethodD"))
            .Build();

        var mC = CreateMethod("MethodC")
            .WithSourceType("SrcC")
            .WithTargetType("TgtC")
            .AddMember("D", "D", Invocation("MethodD"))
            .Build();

        var mA = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("B", "B", Invocation("MethodB"))
            .AddMember("C", "C", Invocation("MethodC"))
            .Build();

        var methods = new List<Models.MethodMapping> { mA, mB, mC, mD };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void DetectCycles_WhenInvocationPointsToNonExistentMethod_ShouldNotCrashOrReportCycle()
    {
        var m1 = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("P", "P", Invocation("ExternalOrMissingMethod"))
            .Build();

        var methods = new List<Models.MethodMapping> { m1 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void DetectCycles_WhenSourceTypeIsNull_ShouldUseMethodNameKey()
    {
        var m1 = new Models.MethodMapping(
            "MethodNullSrc",
            null!,
            CreateTypeRef("TgtA"),
            new Models.ConstructionStrategy.ObjectInitializer(),
            new[] { CreateMember("PropA", "PropA").WithStrategy(Invocation("MethodNullSrc")).Build() }.AsEquatableArray(),
            true,
            EquatableArray<Models.DerivedTypeMapping>.Empty);

        var methods = new List<Models.MethodMapping> { m1 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().Be("ELM010");
    }

    [Fact]
    public void DetectCycles_WhenSourceTypeFullyQualifiedNameIsEmpty_ShouldUseMethodNameKey()
    {
        var m1 = new Models.MethodMapping(
            "MethodEmptySrc",
            CreateTypeRef(""),
            CreateTypeRef("TgtA"),
            new Models.ConstructionStrategy.ObjectInitializer(),
            new[] { CreateMember("PropA", "PropA").WithStrategy(Invocation("MethodEmptySrc")).Build() }.AsEquatableArray(),
            true,
            EquatableArray<Models.DerivedTypeMapping>.Empty);


        var methods = new List<Models.MethodMapping> { m1 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().Be("ELM010");
    }

    [Fact]
    public void DetectCycles_WhenDiagnosticAlreadyReported_ShouldNotAddDuplicate()
    {
        var m1 = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("PropA", "PropA", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { m1 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation("Mapper.cs");

        // Run once
        CycleDetector.DetectCycles(methods, diagnostics, location);
        diagnostics.Should().HaveCount(1);

        // Run second time on same diagnostics list
        CycleDetector.DetectCycles(methods, diagnostics, location);
        diagnostics.Should().HaveCount(1);
    }

    [Fact]
    public void DetectCycles_ThreeStepCycle_ShouldEmitDiagnosticsAndClearStack()
    {
        var mA = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("B", "B", Invocation("MethodB", methodKey: "MethodB(SrcB)"))
            .Build();

        var mB = CreateMethod("MethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .AddMember("C", "C", Invocation("MethodC", methodKey: "MethodC(SrcC)"))
            .Build();

        var mC = CreateMethod("MethodC")
            .WithSourceType("SrcC")
            .WithTargetType("TgtC")
            .AddMember("A", "A", Invocation("MethodA", methodKey: "MethodA(SrcA)"))
            .Build();

        var methods = new List<Models.MethodMapping> { mA, mB, mC };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation("Mapper.cs");

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(3);
        diagnostics[0].Id.Should().Be("ELM010");
        diagnostics[1].Id.Should().Be("ELM010");
        diagnostics[2].Id.Should().Be("ELM010");
    }

    [Fact]
    public void DetectCycles_WhenLocationsDiffer_ShouldDeduplicateCorrectly()
    {
        var mA = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("A", "A", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { mA };
        var diagnostics = new List<Models.DiagnosticInfo>();

        var loc1 = CreateMockLocation("File1.cs");
        var loc2 = CreateMockLocation("File2.cs");

        CycleDetector.DetectCycles(methods, diagnostics, loc1);
        diagnostics.Should().HaveCount(1);

        // Different file location adds a diagnostic
        CycleDetector.DetectCycles(methods, diagnostics, loc2);
        diagnostics.Should().HaveCount(2);
    }

    [Fact]
    public void DetectCycles_WhenColumnsDifferOnSameLine_ShouldAddBothDiagnostics()
    {
        var mA = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("A", "A", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { mA };
        var diagnostics = new List<Models.DiagnosticInfo>();

        var loc1 = CreateMockLocation("File1.cs", startOffset: 0, length: 5);
        var loc2 = CreateMockLocation("File1.cs", startOffset: 6, length: 5);

        CycleDetector.DetectCycles(methods, diagnostics, loc1);
        diagnostics.Should().HaveCount(1);

        CycleDetector.DetectCycles(methods, diagnostics, loc2);
        diagnostics.Should().HaveCount(2);
    }

    [Fact]
    public void DetectCycles_WhenLinesDifferOnSameFile_ShouldAddBothDiagnostics()
    {
        var mA = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("A", "A", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { mA };
        var diagnostics = new List<Models.DiagnosticInfo>();

        var locLine1 = CreateMockLocation("File1.cs", startOffset: 0, length: 5);
        var locLine2 = CreateMockLocation("File1.cs", startOffset: 28, length: 5);

        CycleDetector.DetectCycles(methods, diagnostics, locLine1);
        diagnostics.Should().HaveCount(1);

        CycleDetector.DetectCycles(methods, diagnostics, locLine2);
        diagnostics.Should().HaveCount(2);
    }

    [Fact]
    public void DetectCycles_MultipleDisjointCycles_ShouldReportAllCycles()
    {
        // Cycle 1: A -> B -> A
        var mA = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("B", "B", Invocation("MethodB"))
            .Build();

        var mB = CreateMethod("MethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .AddMember("A", "A", Invocation("MethodA"))
            .Build();

        // Cycle 2: C -> D -> C
        var mC = CreateMethod("MethodC")
            .WithSourceType("SrcC")
            .WithTargetType("TgtC")
            .AddMember("D", "D", Invocation("MethodD"))
            .Build();

        var mD = CreateMethod("MethodD")
            .WithSourceType("SrcD")
            .WithTargetType("TgtD")
            .AddMember("C", "C", Invocation("MethodC"))
            .Build();

        var methods = new List<Models.MethodMapping> { mA, mB, mC, mD };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        // We expect 4 diagnostics (one for each method involved in a cycle)
        diagnostics.Should().HaveCount(4);
        diagnostics.Should().OnlyContain(d => d.Id == "ELM010");
    }

    [Fact]
    public void DetectCycles_WhenExistingDiagnosticHasDifferentId_ShouldAddNewDiagnostic()
    {
        var mA = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("A", "A", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { mA };
        var location = CreateMockLocation("File1.cs", startOffset: 0, length: 5);
        var lineSpan = location.GetLineSpan();

        // Pre-populate with ELM001 on the same location and same args
        var diagnostics = new List<Models.DiagnosticInfo>
        {
            new Models.DiagnosticInfo(
                "ELM001",
                "Unmapped destination member",
                "Message",
                "EricksonLopez.Mapper",
                (int)DiagnosticSeverity.Error,
                true,
                "File1.cs",
                lineSpan.StartLinePosition.Line,
                lineSpan.StartLinePosition.Character,
                new[] { "MethodA" }.AsEquatableArray())
        };

        CycleDetector.DetectCycles(methods, diagnostics, location);

        // Should contain both ELM001 and the newly added ELM010
        diagnostics.Should().HaveCount(2);
        diagnostics.Should().ContainSingle(d => d.Id == "ELM010");
        diagnostics.Should().ContainSingle(d => d.Id == "ELM001");
    }

    [Fact]
    public void DetectCycles_WhenExistingDiagnosticHasDifferentMethodArg_ShouldAddNewDiagnostic()
    {
        var mA = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("A", "A", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { mA };
        var location = CreateMockLocation("File1.cs", startOffset: 0, length: 5);
        var lineSpan = location.GetLineSpan();

        // Pre-populate with ELM010 but with different method argument ("OtherMethod")
        var diagnostics = new List<Models.DiagnosticInfo>
        {
            new Models.DiagnosticInfo(
                "ELM010",
                "Circular mapping reference",
                "Message",
                "EricksonLopez.Mapper",
                (int)DiagnosticSeverity.Error,
                true,
                "File1.cs",
                lineSpan.StartLinePosition.Line,
                lineSpan.StartLinePosition.Character,
                new[] { "OtherMethod" }.AsEquatableArray())
        };

        CycleDetector.DetectCycles(methods, diagnostics, location);

        // Should add ELM010 for MethodA because args differ
        diagnostics.Should().HaveCount(2);
        diagnostics.Should().Contain(d => d.Id == "ELM010" && d.Args.SequenceEqual(new[] { "MethodA" }));
        diagnostics.Should().Contain(d => d.Id == "ELM010" && d.Args.SequenceEqual(new[] { "OtherMethod" }));
    }

    [Fact]
    public void DetectCycles_WhenMethodKeyIsNullOrEmpty_ShouldFallbackToMethodNameKey()
    {
        // MethodInvocation with explicit null/empty MethodKey
        var mInvocationWithNullKey = Invocation("MethodB", methodKey: null!);
        var mInvocationWithEmptyKey = Invocation("MethodA", methodKey: "");

        var mA = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("B", "B", mInvocationWithNullKey)
            .Build();

        var mB = CreateMethod("MethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .AddMember("A", "A", mInvocationWithEmptyKey)
            .Build();

        var methods = new List<Models.MethodMapping> { mA, mB };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(2);
        diagnostics.Should().OnlyContain(d => d.Id == "ELM010");
    }

    [Fact]
    public void DetectCycles_FourLevelIndirectCycleWithMixedStrategies_ShouldDetectCyclesForAllFourMethods()
    {
        // Level 1: A -> B via ParameterizedConstructor
        var mA = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .WithParameterizedConstructor(
                CreateParameter("PropB", "PropB").WithStrategy(Invocation("MethodB")))
            .Build();

        // Level 2: B -> C via EnumerableMapping
        var enumStrategy = Enumerable(Invocation("MethodC"), "SrcItem", "TgtItem");

        var mB = CreateMethod("MethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .AddMember("ItemsC", "ItemsC", enumStrategy)
            .Build();

        // Level 3: C -> D via ValueObjectMapping
        var voStrategy = ValueObject(0, Invocation("MethodD"), "SrcVo", "TgtVo");

        var mC = CreateMethod("MethodC")
            .WithSourceType("SrcC")
            .WithTargetType("TgtC")
            .AddMember("VoD", "VoD", voStrategy)
            .Build();

        // Level 4: D -> A via MapMethodInvocation (closing cycle)
        var mD = CreateMethod("MethodD")
            .WithSourceType("SrcD")
            .WithTargetType("TgtD")
            .AddMember("RefA", "RefA", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { mA, mB, mC, mD };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation("Mapper.cs");

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(4);
        diagnostics.Should().OnlyContain(d => d.Id == "ELM010");
    }

    [Fact]
    public void DetectCycles_WhenLinearPrefixFollowedByCycle_ShouldDetectCycleAndClearStateCorrectly()
    {
        // Prefix: X -> Y (linear)
        var mX = CreateMethod("MethodX")
            .WithSourceType("SrcX")
            .WithTargetType("TgtX")
            .AddMember("Y", "Y", Invocation("MethodY"))
            .Build();

        var mY = CreateMethod("MethodY")
            .WithSourceType("SrcY")
            .WithTargetType("TgtY")
            .AddMember("Val", "Val", Direct())
            .Build();

        // Cycle: A -> B -> A
        var mA = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("B", "B", Invocation("MethodB"))
            .Build();

        var mB = CreateMethod("MethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .AddMember("A", "A", Invocation("MethodA"))
            .Build();

        var methods = new List<Models.MethodMapping> { mX, mY, mA, mB };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        // Only A and B should be reported, X and Y should not
        diagnostics.Should().HaveCount(2);
        diagnostics.Should().Contain(d => d.Args.Contains("MethodA"));
        diagnostics.Should().Contain(d => d.Args.Contains("MethodB"));
        diagnostics.Should().NotContain(d => d.Args.Contains("MethodX"));
        diagnostics.Should().NotContain(d => d.Args.Contains("MethodY"));
    }

    [Fact]
    public void DetectCycles_WhenComplexTenNodeGraphWithMultipleInterconnectedSubCycles_ShouldAccuratelyDetectCyclicNodesAndIsolateAcyclicNodes()
    {
        // Arrange 10 nodes topology using fluent graph builder:
        // Sub-cycle 1: N1 -> N2 -> N3 -> N1
        // Bridge: N3 also calls N4 (links sub-cycle 1 to sub-cycle 2)
        // Sub-cycle 2: N4 -> N5 -> N4
        // Acyclic chain: N6 -> N7 -> N8 (terminal)
        // Acyclic node calling into cycle: N9 -> N1 (N9 is not part of the cycle itself)
        // Isolated leaf: N10 (terminal)
        var methods = CreateGraph()
            .AddNode("Node1", "Src1", "Tgt1", "Node2")
            .AddNode("Node2", "Src2", "Tgt2", "Node3")
            .AddNode("Node3", "Src3", "Tgt3", "Node1", "Node4")
            .AddNode("Node4", "Src4", "Tgt4", "Node5")
            .AddNode("Node5", "Src5", "Tgt5", "Node4")
            .AddNode("Node6", "Src6", "Tgt6", "Node7")
            .AddNode("Node7", "Src7", "Tgt7", "Node8")
            .AddTerminal("Node8", "Src8", "Tgt8")
            .AddNode("Node9", "Src9", "Tgt9", "Node1")
            .AddTerminal("Node10", "Src10", "Tgt10")
            .Build();

        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation("GraphTopology.cs");

        // Act
        CycleDetector.DetectCycles(methods, diagnostics, location);

        // Assert
        // Cyclic nodes (Node1, Node2, Node3, Node4, Node5) and nodes leading into a cycle (Node9) MUST emit ELM010
        diagnostics.Should().HaveCount(6);
        diagnostics.Should().OnlyContain(d => d.Id == "ELM010");

        var reportedMethodNames = diagnostics.SelectMany(d => d.Args).ToList();
        reportedMethodNames.Should().Contain("Node1");
        reportedMethodNames.Should().Contain("Node2");
        reportedMethodNames.Should().Contain("Node3");
        reportedMethodNames.Should().Contain("Node4");
        reportedMethodNames.Should().Contain("Node5");
        reportedMethodNames.Should().Contain("Node9");

        // Independent acyclic nodes MUST NOT have diagnostics
        reportedMethodNames.Should().NotContain("Node6");
        reportedMethodNames.Should().NotContain("Node7");
        reportedMethodNames.Should().NotContain("Node8");
        reportedMethodNames.Should().NotContain("Node10");
    }

    [Fact]
    public void DetectCycles_WhenDeeplyNestedDictionaryAndValueObjectCycle_ShouldEmitELM010()
    {
        // Arrange
        // Root -> DictionaryMapping with Value mapped via InnerVoMethod
        // InnerVoMethod -> ValueObjectMapping mapped via LeafMethod
        // LeafMethod -> Maps back to Root

        var dictStrategy = Dictionary(
            Direct(),
            Invocation("InnerVoMethod"),
            "KeySrc", "KeyTgt", "ValSrc", "ValTgt");

        var mRoot = CreateMethod("RootMethod")
            .WithSourceType("SrcRoot").WithTargetType("TgtRoot")
            .AddMember("Dict", "Dict", dictStrategy)
            .Build();

        var voStrategy = ValueObject(0, Invocation("LeafMethod"), "VoSrc", "VoTgt");

        var mInner = CreateMethod("InnerVoMethod")
            .WithSourceType("SrcInner").WithTargetType("TgtInner")
            .AddMember("Vo", "Vo", voStrategy)
            .Build();

        var mLeaf = CreateMethod("LeafMethod")
            .WithSourceType("SrcLeaf").WithTargetType("TgtLeaf")
            .AddMember("BackToRoot", "BackToRoot", Invocation("RootMethod"))
            .Build();

        var methods = new List<Models.MethodMapping> { mRoot, mInner, mLeaf };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation("NestedCycle.cs");

        // Act
        CycleDetector.DetectCycles(methods, diagnostics, location);

        // Assert
        diagnostics.Should().HaveCount(3);
        diagnostics.Should().OnlyContain(d => d.Id == "ELM010");
        diagnostics.SelectMany(d => d.Args).Should().Contain(new[] { "RootMethod", "InnerVoMethod", "LeafMethod" });
    }

    [Fact]
    public void DetectCycles_WhenDuplicateMethodEntriesInList_ShouldSkipAlreadyReportedMethod()
    {
        var m1 = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("PropA", "PropA", Invocation("MethodA"))
            .Build();

        // Pass m1 twice in the methods list
        var methods = new List<Models.MethodMapping> { m1, m1 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation("DuplicateMethod.cs");

        CycleDetector.DetectCycles(methods, diagnostics, location);

        // Should only report 1 diagnostic despite being present twice
        diagnostics.Should().HaveCount(1);
    }

    [Fact]
    public void DetectCycles_WhenFirstMethodHasCycleAndSecondMethodIsAcyclicCallingFirst_ShouldOnlyEmitDiagnosticForFirstMethod()
    {
        var m1 = CreateMethod("CyclicMethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("A", "A", Invocation("CyclicMethodB"))
            .Build();

        var m2 = CreateMethod("CyclicMethodB")
            .WithSourceType("SrcB")
            .WithTargetType("TgtB")
            .AddMember("B", "B", Invocation("CyclicMethodA"))
            .Build();

        var mAcyclic = CreateMethod("AcyclicMethodC")
            .WithSourceType("SrcC")
            .WithTargetType("TgtC")
            .AddMember("C", "C", Direct())
            .Build();

        var methods = new List<Models.MethodMapping> { m1, m2, mAcyclic };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation("Isolation.cs");

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(2);
        diagnostics.SelectMany(d => d.Args).Should().NotContain("AcyclicMethodC");
    }

    [Fact]
    public void DetectCycles_WhenOverloadedMethodsUseSpecificMethodKey_ShouldResolveAccurately()
    {
        var mOverloadInt = CreateMethod("MapValue")
            .WithSourceType("int")
            .WithTargetType("long")
            .AddMember("Val", "Val", new Models.ConversionStrategy.MapMethodInvocation("MapHelper", false, false, "MapHelper(int)"))
            .Build();

        var mOverloadString = CreateMethod("MapValue")
            .WithSourceType("string")
            .WithTargetType("string")
            .AddMember("Val", "Val", Direct())
            .Build();

        var mHelper = CreateMethod("MapHelper")
            .WithSourceType("int")
            .WithTargetType("int")
            .AddMember("H", "H", new Models.ConversionStrategy.MapMethodInvocation("MapValue", false, false, "MapValue(int)"))
            .Build();

        var methods = new List<Models.MethodMapping> { mOverloadInt, mOverloadString, mHelper };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation("Overloads.cs");

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(2);
        diagnostics.SelectMany(d => d.Args).Should().Contain("MapValue");
        diagnostics.SelectMany(d => d.Args).Should().Contain("MapHelper");
    }

    [Fact]
    public void DetectCycles_WhenMethodKeyDistinguishesOverload_ShouldNotTriggerFalsePositiveCycle()
    {
        // Map(int) calls MapHelper
        // Map(string) is terminal (direct)
        // MapHelper calls Map(string) using specific MethodKey "Map(string)"
        // If MethodKey was ignored and MethodName "Map" was used, it would check both overloads,
        // finding Map(int), which would falsely report a cycle Map(int) -> MapHelper -> Map(int).
        var mMapInt = CreateMethod("Map")
            .WithSourceType("int")
            .WithTargetType("long")
            .AddMember("Val", "Val", new Models.ConversionStrategy.MapMethodInvocation("MapHelper", false, false, "MapHelper(int)"))
            .Build();

        var mMapString = CreateMethod("Map")
            .WithSourceType("string")
            .WithTargetType("string")
            .AddMember("Val", "Val", Direct())
            .Build();

        var mMapHelper = CreateMethod("MapHelper")
            .WithSourceType("int")
            .WithTargetType("int")
            .AddMember("Val", "Val", new Models.ConversionStrategy.MapMethodInvocation("Map", false, false, "Map(string)"))
            .Build();

        var methods = new List<Models.MethodMapping> { mMapInt, mMapString, mMapHelper };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation("OverloadAcyclic.cs");

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void DetectCycles_WhenMethodKeyIsEmptyString_ShouldFallbackToMethodName()
    {
        var m1 = CreateMethod("MethodA")
            .WithSourceType("SrcA")
            .WithTargetType("TgtA")
            .AddMember("Prop", "Prop", new Models.ConversionStrategy.MapMethodInvocation("MethodA", false, false, ""))
            .Build();

        var methods = new List<Models.MethodMapping> { m1 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().Be("ELM010");
    }

    [Fact]
    public void DetectCycles_WhenAcyclicMethodEvaluatedAfterCyclicMethod_ShouldClearRecursionStackAndNotReportFalseCycle()
    {
        // Method1 is cyclic: M1 -> M1
        var m1 = CreateMethod("M1")
            .WithSourceType("Src1")
            .WithTargetType("Tgt1")
            .AddMember("Self", "Self", Invocation("M1"))
            .Build();

        // Method2 calls M1, but is not in a cycle back to M2: M2 -> M1
        var m2 = CreateMethod("M2")
            .WithSourceType("Src2")
            .WithTargetType("Tgt2")
            .AddMember("CallM1", "CallM1", Invocation("M1"))
            .Build();

        // Method3 is completely independent and acyclic: M3 -> Terminal
        var m3 = CreateMethod("M3")
            .WithSourceType("Src3")
            .WithTargetType("Tgt3")
            .AddMember("Val", "Val", Direct())
            .Build();

        var methods = new List<Models.MethodMapping> { m1, m2, m3 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        // M1 and M2 are reported (M1 has self-cycle, M2 leads into M1)
        // M3 MUST NOT be reported
        diagnostics.SelectMany(d => d.Args).Should().NotContain("M3");
    }

    [Fact]
    public void DetectCycles_WhenDiamondDagSharesAcyclicNode_ShouldUtilizeVisitedCache()
    {
        // Root calls Left and Right. Both Left and Right call Leaf (terminal).
        var mLeaf = CreateMethod("Leaf")
            .WithSourceType("SrcLeaf")
            .WithTargetType("TgtLeaf")
            .AddMember("Val", "Val", Direct())
            .Build();

        var mLeft = CreateMethod("Left")
            .WithSourceType("SrcLeft")
            .WithTargetType("TgtLeft")
            .AddMember("L", "L", Invocation("Leaf"))
            .Build();

        var mRight = CreateMethod("Right")
            .WithSourceType("SrcRight")
            .WithTargetType("TgtRight")
            .AddMember("R", "R", Invocation("Leaf"))
            .Build();

        var mRoot = CreateMethod("Root")
            .WithSourceType("SrcRoot")
            .WithTargetType("TgtRoot")
            .AddMember("Left", "Left", Invocation("Left"))
            .AddMember("Right", "Right", Invocation("Right"))
            .Build();

        var methods = new List<Models.MethodMapping> { mRoot, mLeft, mRight, mLeaf };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation();

        CycleDetector.DetectCycles(methods, diagnostics, location);

        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void DetectCycles_WhenCyclicMethodAlsoCallsAcyclicBranch_RecursionStackMustBeClearedToPreventFalsePositive()
    {
        // M1 has a member calling M2, then a member calling M1 (cycle)
        var m1 = CreateMethod("M1")
            .WithSourceType("Src1")
            .WithTargetType("Tgt1")
            .AddMember("CallM2", "CallM2", Invocation("M2"))
            .AddMember("Self", "Self", Invocation("M1"))
            .Build();

        // M2 is completely acyclic (terminal)
        var m2 = CreateMethod("M2")
            .WithSourceType("Src2")
            .WithTargetType("Tgt2")
            .AddMember("Val", "Val", Direct())
            .Build();

        var methods = new List<Models.MethodMapping> { m1, m2 };
        var diagnostics = new List<Models.DiagnosticInfo>();
        var location = CreateMockLocation("ClearingStack.cs");

        CycleDetector.DetectCycles(methods, diagnostics, location);

        // M1 MUST be reported
        diagnostics.Should().HaveCount(1);
        diagnostics[0].Args.AsImmutableArray().Should().ContainSingle().Which.Should().Be("M1");
        // M2 MUST NOT be reported
        diagnostics.SelectMany(d => d.Args).Should().NotContain("M2");
    }
}




