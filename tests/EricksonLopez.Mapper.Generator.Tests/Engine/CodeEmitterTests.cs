// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AwesomeAssertions;
using EricksonLopez.Mapper.Generator;
using EricksonLopez.Mapper.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace EricksonLopez.Mapper.Generator.Tests.Engine;

using EricksonLopez.Mapper.Generator.Tests.Infrastructure;

[Trait("Category", "FastAst")]
public class CodeEmitterTests
{
    [Theory]
    [InlineData(null, "_")]
    [InlineData("", "_")]
    [InlineData("   ", "_")]
    [InlineData("ValidName", "ValidName")]
    [InlineData("123Number", "_123Number")]
    [InlineData("Foo-Bar", "Foo_Bar")]
    [InlineData("class", "@class")]
    [InlineData("namespace", "@namespace")]
    [InlineData("record", "@record")]
    [InlineData("My.Namespace.class", "My.Namespace.@class")]
    [InlineData("A..B", "A.@_.B")]
    [InlineData("...", "@_.@_.@_.@_")]
    [InlineData("123.456", "_123._456")]
    public void EscapeIdentifier_WhenInputProvided_ShouldEscapeProperly(string? input, string expected)
    {
        var result = CodeEmitter.EscapeIdentifier(input!);
        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateSourceCode_WhenCustomConverterField_ShouldEmitThisCall()
    {
        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .WithCustomConverterField("_convField")
            .Build();

        var typeMapping = TestDataBuilders.CreateType("MyMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(typeMapping);
        code.Should().Contain("return this._convField.Convert(source);");
    }

    [Fact]
    public void GenerateSourceCode_WhenSingleRootCollectionMethod_ShouldEmitPreStatementsAndReturn()
    {
        var listStrategy = new ConversionStrategy.EnumerableMapping(new ConversionStrategy.DirectAssignment(), "int", "long", IsArray: false, IsList: true, IsImmutableArray: false, SourceIsArray: false, SourceHasCount: true);
        var method1 = TestDataBuilders.CreateMethod("MapCollection1")
            .WithSourceType("List<int>")
            .WithTargetType("List<long>")
            .AddMember("", "", listStrategy)
            .Build();

        var method2 = TestDataBuilders.CreateMethod("MapCollection2")
            .WithSourceType("List<int>")
            .WithTargetType("List<long>")
            .AddMember("", "", listStrategy)
            .Build();

        var typeMapping = TestDataBuilders.CreateType("ColMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(method1)
            .AddMethod(method2)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(typeMapping);
        code.Should().Contain("global::System.Collections.Generic.List<long>? _col1 = null;");
        code.Should().Contain("return _col1;");
        code.Should().Contain("        }" + Environment.NewLine + Environment.NewLine + "        public partial List<long> MapCollection2");
    }

    [Fact]
    public void GenerateSourceCode_WhenStaticMapper_ShouldEmitStaticKeywords()
    {
        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .AddMember("Name", "Name", new ConversionStrategy.DirectAssignment())
            .Build();

        var typeMapping = TestDataBuilders.CreateType("MyStaticMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(true)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(typeMapping);
        code.Should().Contain("public static partial class MyStaticMapper");
        code.Should().Contain("public static partial Dest Map(Source source)");
    }

    [Fact]
    public void GenerateSourceCode_WhenValueTypeSource_ShouldOmitNullCheck()
    {
        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("int", isValueType: true)
            .WithTargetType("long", isValueType: true)
            .Build();

        var typeMapping = TestDataBuilders.CreateType("MyMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(typeMapping);
        code.Should().NotContain("ArgumentNullException");
    }

    [Fact]
    public void GenerateSourceCode_WhenCustomConverter_ShouldEmitInstantiationAndCall()
    {
        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .WithCustomConverter("MyConverter")
            .Build();

        var typeMapping = TestDataBuilders.CreateType("MyMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(typeMapping);
        code.Should().Contain("var converter = new MyConverter();");
        code.Should().Contain("return converter.Convert(source);");
    }

    [Fact]
    public void GenerateSourceCode_WhenDerivedTypesWithoutMethodName_ShouldEmitExceptionThrow()
    {
        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("BaseSource", isAbstract: true)
            .WithTargetType("BaseDest", isAbstract: true)
            .AddDerivedType("ChildSource", "ChildDest", null)
            .Build();

        var typeMapping = TestDataBuilders.CreateType("MyMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(typeMapping);
        code.Should().Contain("throw new global::System.InvalidOperationException(\"No mapping method defined for ChildSource to ChildDest\")");
        code.Should().Contain("Cannot instantiate abstract base type BaseDest");
    }

    [Fact]
    public void GenerateSourceCode_WhenParameterizedConstructorAndFactoryMethod_ShouldEmitInstantiation()
    {
        var param = new ParameterMapping("Arg", "Arg", new ConversionStrategy.DirectAssignment(), null, false, false);
        var member = new MemberMapping("Prop", "Prop", new ConversionStrategy.DirectAssignment(), null, false, false);

        var methodCtor = TestDataBuilders.CreateMethod("MapCtor")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .WithParameterizedConstructor(param)
            .AddMember(member)
            .Build();

        var methodFactory = TestDataBuilders.CreateMethod("MapFactory")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .WithFactoryMethod("Create", param)
            .AddMember(member)
            .Build();

        var typeMapping = TestDataBuilders.CreateType("MyMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(methodCtor)
            .AddMethod(methodFactory)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(typeMapping);
        code.Should().Contain("var target = new Dest(source.Arg)");
        code.Should().Contain("var target = Dest.Create(source.Arg)");
        code.Should().Contain("Prop = source.Prop");
    }

    [Fact]
    public void GenerateSourceCode_WhenValueObjectStrategiesUsed_ShouldEmitExpectedAccessors()
    {
        var voCtor = new ConversionStrategy.ValueObjectMapping(0, new ConversionStrategy.DirectAssignment(), "int", "UserId");
        var voProp = new ConversionStrategy.ValueObjectMapping(1, new ConversionStrategy.DirectAssignment(), "UserId", "int");
        var voCast = new ConversionStrategy.ValueObjectMapping(2, new ConversionStrategy.DirectAssignment(), "int", "CustomId");

        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .AddMember("A", "A", voCtor)
            .AddMember("B", "B", voProp)
            .AddMember("C", "C", voCast)
            .Build();

        var typeMapping = TestDataBuilders.CreateType("MyMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(typeMapping);
        code.Should().Contain("new UserId(source.A)");
        code.Should().Contain("source.B.Value");
        code.Should().Contain("(CustomId)source.C");
    }

    [Fact]
    public void GenerateSourceCode_WhenCollectionsAndDictionariesMapped_ShouldEmitExpectedVariables()
    {
        var direct = new ConversionStrategy.DirectAssignment();
        var mapMethod = new ConversionStrategy.MapMethodInvocation("MapItem", true, true);

        var arrayMap = new ConversionStrategy.EnumerableMapping(mapMethod, "SourceItem", "DestItem", true, false, false, true, true);
        var immArrayNoCount = new ConversionStrategy.EnumerableMapping(direct, "int", "int", false, false, true, false, false);
        var immList = new ConversionStrategy.EnumerableMapping(direct, "int", "int", false, false, false, false, false, false, true, false);
        var frozenSet = new ConversionStrategy.EnumerableMapping(direct, "int", "int", false, false, false, false, false, false, false, true);
        var hashSet = new ConversionStrategy.EnumerableMapping(direct, "int", "int", false, false, false, false, false, true, false, false);
        var listNoCount = new ConversionStrategy.EnumerableMapping(direct, "int", "int", false, true, false, false, false);
        var dictMap = new ConversionStrategy.DictionaryMapping(direct, direct, "string", "string", "int", "int", false);

        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .AddMember("Arr", "Arr", arrayMap)
            .AddMember("ImmArr", "ImmArr", immArrayNoCount)
            .AddMember("ImmList", "ImmList", immList)
            .AddMember("Frozen", "Frozen", frozenSet)
            .AddMember("Hash", "Hash", hashSet)
            .AddMember("List", "List", listNoCount)
            .AddMember("Dict", "Dict", dictMap)
            .Build();

        var typeMapping = TestDataBuilders.CreateType("MyMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(typeMapping);
        code.Should().Contain("DestItem[] _col1 = global::System.Array.Empty<DestItem>();");
        code.Should().Contain("global::System.Collections.Immutable.ImmutableArray<int> _col2 = global::System.Collections.Immutable.ImmutableArray<int>.Empty;");
        code.Should().Contain("global::System.Collections.Immutable.ImmutableList<int> _col3 = global::System.Collections.Immutable.ImmutableList<int>.Empty;");
        code.Should().Contain("global::System.Collections.Frozen.FrozenSet<int>? _col4 = null;");
        code.Should().Contain("global::System.Collections.Generic.HashSet<int>? _col5 = null;");
        code.Should().Contain("global::System.Collections.Generic.List<int>? _col6 = null;");
        code.Should().Contain("global::System.Collections.Generic.Dictionary<string, int>? _dict7 = null;");
    }

    [Fact]
    public void GenerateSourceCode_WhenUnsupportedStrategy_ShouldReturnSourceAccessor()
    {
        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .AddMember("Prop", "Prop", new ConversionStrategy.Unsupported())
            .Build();

        var typeMapping = TestDataBuilders.CreateType("MyMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(typeMapping);
        code.Should().Contain("Prop = source.Prop");
    }

    [Fact]
    public void GenerateSourceCode_WhenMapMethodInvocationUsed_ShouldEmitStaticAndInstanceVariants()
    {
        var methodNullable = new ConversionStrategy.MapMethodInvocation("MapItem", true, true);
        var methodNonNullable = new ConversionStrategy.MapMethodInvocation("MapItem", false, false);
        var methodOneNull = new ConversionStrategy.MapMethodInvocation("MapItem", true, false);

        var staticMethod = TestDataBuilders.CreateMethod("MapStatic")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .AddMember("A", "A", methodNullable)
            .AddMember("B", "B", methodNonNullable)
            .AddMember("C", "C", methodOneNull)
            .Build();

        var instanceMethod = TestDataBuilders.CreateMethod("MapInstance")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .AddMember("A", "A", methodNullable)
            .AddMember("B", "B", methodNonNullable)
            .AddMember("C", "C", methodOneNull)
            .Build();

        var staticMapper = TestDataBuilders.CreateType("StaticMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(true)
            .AddMethod(staticMethod)
            .Build();

        var instanceMapper = TestDataBuilders.CreateType("InstanceMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(instanceMethod)
            .Build();

        var staticCode = CodeEmitter.GenerateSourceCode(staticMapper);
        staticCode.Should().Contain("(source.A != null ? MapItem(source.A) : null)");
        staticCode.Should().Contain("B = MapItem(source.B)");
        staticCode.Should().Contain("C = MapItem(source.C)");

        var instanceCode = CodeEmitter.GenerateSourceCode(instanceMapper);
        instanceCode.Should().Contain("(source.A != null ? this.MapItem(source.A) : null)");
        instanceCode.Should().Contain("B = this.MapItem(source.B)");
        instanceCode.Should().Contain("C = this.MapItem(source.C)");
    }

    [Fact]
    public void GenerateSourceCode_WhenFactoryMethodWithCustomValueExpression_ShouldEmitCustomExpression()
    {
        var param = new ParameterMapping("Arg", "Arg", new ConversionStrategy.DirectAssignment(), CustomValueExpression: "42");
        var method = TestDataBuilders.CreateMethod("MapFactory")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .WithFactoryMethod("Create", param)
            .Build();

        var typeMapping = TestDataBuilders.CreateType("MyMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(typeMapping);
        code.Should().Contain("var target = Dest.Create(42)");
    }

    [Fact]
    public void GenerateSourceCode_WhenEnumerableMappingUsed_ShouldHandleCountAndUncountedVariants()
    {
        var direct = new ConversionStrategy.DirectAssignment();

        // 1. ImmutableArray without count (TryGetNonEnumeratedCount)
        var immArrayNoCount = new ConversionStrategy.EnumerableMapping(direct, "int", "int", IsArray: false, IsList: false, IsImmutableArray: true, SourceIsArray: false, SourceHasCount: false);
        // 2. ImmutableArray with array source length
        var immArrayWithArray = new ConversionStrategy.EnumerableMapping(direct, "long", "long", IsArray: false, IsList: false, IsImmutableArray: true, SourceIsArray: true, SourceHasCount: false);
        // 3. HashSet with count
        var hashSetWithCount = new ConversionStrategy.EnumerableMapping(direct, "string", "string", IsArray: false, IsList: false, IsImmutableArray: false, SourceIsArray: false, SourceHasCount: true, IsHashSet: true);
        // 4. HashSet without count
        var hashSetNoCount = new ConversionStrategy.EnumerableMapping(direct, "bool", "bool", IsArray: false, IsList: false, IsImmutableArray: false, SourceIsArray: false, SourceHasCount: false, IsHashSet: true);
        // 5. FrozenSet with count
        var frozenWithCount = new ConversionStrategy.EnumerableMapping(direct, "double", "double", IsArray: false, IsList: false, IsImmutableArray: false, SourceIsArray: false, SourceHasCount: true, IsFrozenSet: true);
        // 6. FrozenSet without count
        var frozenNoCount = new ConversionStrategy.EnumerableMapping(direct, "decimal", "decimal", IsArray: false, IsList: false, IsImmutableArray: false, SourceIsArray: false, SourceHasCount: false, IsFrozenSet: true);
        // 7. List without count (TryGetNonEnumeratedCount)
        var listNoCount = new ConversionStrategy.EnumerableMapping(direct, "short", "short", IsArray: false, IsList: true, IsImmutableArray: false, SourceIsArray: false, SourceHasCount: false);
        // 8. ValueType source enumerable (no null check)
        var valTypeEnum = new ConversionStrategy.EnumerableMapping(direct, "byte", "byte", IsArray: false, IsList: true, IsImmutableArray: false, SourceIsArray: false, SourceHasCount: true, SourceIsValueType: true);


        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .AddMember("P1", "P1", immArrayNoCount)
            .AddMember("P2", "P2", immArrayWithArray)
            .AddMember("P3", "P3", hashSetWithCount)
            .AddMember("P4", "P4", hashSetNoCount)
            .AddMember("P5", "P5", frozenWithCount)
            .AddMember("P6", "P6", frozenNoCount)
            .AddMember("P7", "P7", listNoCount)
            .AddMember("P8", "P8", valTypeEnum)
            .Build();

        var mapper = TestDataBuilders.CreateType("ColMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(mapper);
        code.Should().Contain("TryGetNonEnumeratedCount(source.P1, out int _c");
        code.Should().Contain("CreateBuilder<long>(source.P2.Length)");
        code.Should().Contain("new global::System.Collections.Generic.HashSet<string>(source.P3.Count)");
        code.Should().Contain("new global::System.Collections.Generic.HashSet<bool>()");
        code.Should().Contain("new global::System.Collections.Generic.HashSet<double>(source.P5.Count)");
        code.Should().Contain("new global::System.Collections.Generic.HashSet<decimal>()");
        code.Should().Contain("_frozenBuilder6.Add(");
        code.Should().Contain("_frozenBuilder7.Add(");
        code.Should().Contain("global::System.Collections.Frozen.FrozenSet.ToFrozenSet(_frozenBuilder");
        code.Should().Contain("TryGetNonEnumeratedCount(source.P7, out int _c");
        code.Should().NotContain("if (source.P8 != null)");
    }

    [Fact]
    public void GenerateSourceCode_WhenDictionaryMappingUsed_ShouldHandleCountAndUncountedVariants()
    {
        var direct = new ConversionStrategy.DirectAssignment();
        var dictWithCount = new ConversionStrategy.DictionaryMapping(direct, direct, "string", "string", "int", "int", true);
        var dictNoCount = new ConversionStrategy.DictionaryMapping(direct, direct, "string", "string", "int", "int", false);

        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .AddMember("D1", "D1", dictWithCount)
            .AddMember("D2", "D2", dictNoCount)
            .Build();

        var mapper = TestDataBuilders.CreateType("DictMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(mapper);
        code.Should().Contain("new global::System.Collections.Generic.Dictionary<string, int>(source.D1.Count)");
        code.Should().Contain("TryGetNonEnumeratedCount(source.D2, out int _dc");
        AssertValidSyntax(code);
    }

    [Fact]
    public void GenerateSourceCode_WhenEmptyNamespace_ShouldFallbackToUnderscoreNamespace()
    {
        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .AddMember("Name", "Name", new ConversionStrategy.DirectAssignment())
            .Build();

        var typeMapping = TestDataBuilders.CreateType("GlobalMapper")
            .WithNamespace("")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(typeMapping);
        code.Should().Contain("namespace _");
        code.Should().Contain("public partial class GlobalMapper");
        AssertValidSyntax(code);
    }

    [Fact]
    public void GenerateSourceCode_WhenMethodMappingWithPreStatements_ShouldEmitIndentedCode()
    {
        var direct = new ConversionStrategy.DirectAssignment();
        var listStrategy = new ConversionStrategy.EnumerableMapping(direct, "string", "string", IsArray: false, IsList: true, IsImmutableArray: false, SourceIsArray: false, SourceHasCount: true);

        var method = TestDataBuilders.CreateMethod("Map")
            .WithSourceType("Source")
            .WithTargetType("Dest")
            .AddMember("Items", "Items", listStrategy)
            .Build();

        var mapper = TestDataBuilders.CreateType("ListMapper")
            .WithNamespace("TestNamespace")
            .WithStatic(false)
            .AddMethod(method)
            .Build();

        var code = CodeEmitter.GenerateSourceCode(mapper);
        code.Should().Contain("var target = new Dest()");
        code.Should().Contain("Items = _col1");
        AssertValidSyntax(code);
    }

    private static void AssertValidSyntax(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var errors = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        errors.Should().BeEmpty(because: "generated C# code must be syntactically valid");
    }
}







