// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Mapper.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    EricksonLopez.Mapper.Analyzers.MapperAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace EricksonLopez.Mapper.Analyzers.Tests
{
    /// <summary>Contains unit tests that verify the diagnostic reporting behavior of <see cref="EricksonLopez.Mapper.Analyzers.MapperAnalyzer"/>.</summary>
    public class MapperAnalyzerTests
    {
        private static async Task VerifyMapperAnalyzerAsync(string testCode, params DiagnosticResult[] expected)
        {
            var test = new CSharpAnalyzerTest<MapperAnalyzer, DefaultVerifier>
            {
                TestCode = testCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            };
            test.TestState.Sources.Add(("MapperAttribute.cs", TestConstants.MapperAttributeCode));
            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }

        [Fact]
        public async Task Analyze_WhenUsingReflection_ShouldEmitELM008()
        {
            var testCode = @"
using System.Reflection;
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class MyMapper
    {
        public void Map()
        {
            var props = typeof(string).GetProperties();
        }
    }
}";

            var expected = VerifyCS.Diagnostic("ELM008").WithLocation(12, 25).WithMessage("Mapper uses reflection which violates AOT-First principles");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task Analyze_WhenUsingReflectionNamespaceMember_ShouldEmitELM008()
        {
            var testCode = @"
using System.Reflection;
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class MyMapper
    {
        public void Map()
        {
            var asm = Assembly.GetExecutingAssembly();
        }
    }
}";

            var expected = VerifyCS.Diagnostic("ELM008").WithLocation(12, 23).WithMessage("Mapper uses reflection which violates AOT-First principles");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task Analyze_WhenUsingActivator_ShouldEmitELM008()
        {
            var testCode = @"
using System;
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class MyMapper
    {
        public void Map()
        {
            var obj = Activator.CreateInstance(typeof(string));
        }
    }
}";

            var expected = VerifyCS.Diagnostic("ELM008").WithLocation(12, 23).WithMessage("Mapper uses reflection which violates AOT-First principles");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task Analyze_WhenUsingDynamic_ShouldEmitELM009()
        {
            var testCode = @"
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class MyMapper
    {
        public void Map()
        {
            dynamic obj = null;
        }
    }
}";

            var expected = VerifyCS.Diagnostic("ELM009").WithLocation(11, 13).WithMessage("Mapper uses dynamic which violates AOT-First principles");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task Analyze_WhenClassIsNotAMapper_ShouldNotEmitDiagnostics()
        {
            var testCode = @"
using System;
namespace TestNamespace
{
    public class NormalClass
    {
        public void Map()
        {
            dynamic obj = null;
            var props = typeof(string).GetProperties();
            var inst = Activator.CreateInstance(typeof(string));
        }
    }
}";
            // Should be empty
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Analyze_WhenClassIsNotPartial_ShouldEmitELM012()
        {
            var testCode = @"
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public class MyMapper
    {
    }
}";

            var expected = VerifyCS.Diagnostic("ELM012").WithLocation(7, 18).WithMessage("Mapper class 'MyMapper' must be declared as partial");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task Analyze_WhenInterfaceIsNotPartial_ShouldEmitELM012()
        {
            var testCode = @"
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public interface IMyMapper
    {
    }
}";

            var expected = VerifyCS.Diagnostic("ELM012").WithLocation(7, 22).WithMessage("Mapper class 'IMyMapper' must be declared as partial");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task Analyze_WhenInterfaceIsPartial_ShouldNotEmitDiagnostics()
        {
            var testCode = @"
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial interface IMyMapper
    {
    }
}";

            await VerifyMapperAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Analyze_WhenInterfaceIsNotMapper_ShouldNotEmitDiagnostics()
        {
            var testCode = @"
namespace TestNamespace
{
    public interface INormalInterface
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Analyze_WhenInterfaceHasMultipleAttributesIncludingMapper_ShouldEmitELM012()
        {
            var testCode = @"
namespace TestNamespace
{
    using System;
    using EricksonLopez.Mapper;

    [Obsolete, Mapper]
    public interface IMyMapper
    {
    }
}";

            var expected = VerifyCS.Diagnostic("ELM012").WithLocation(8, 22).WithMessage("Mapper class 'IMyMapper' must be declared as partial");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task Analyze_WhenInterfaceHasDifferentAttribute_ShouldNotEmitDiagnostics()
        {
            var testCode = @"
namespace TestNamespace
{
    using System;

    [Obsolete]
    public interface INonMapper
    {
    }
}";

            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Analyze_WhenTopLevelStatement_ShouldIgnore()
        {
            var testCode = @"
using System;
dynamic obj = null;
var props = typeof(string).GetProperties();
System.Console.WriteLine(obj);
";
            await VerifyAnalyzerIgnoringCompilerDiagnosticsAsync(testCode);
        }

        [Fact]
        public async Task Analyze_WhenSymbolIsUnresolved_ShouldIgnore()
        {
            var testCode = @"
using System;
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class MyMapper
    {
        public void Map()
        {
            typeof(string).UnknownMethod();
        }
    }
}";
            await VerifyAnalyzerIgnoringCompilerDiagnosticsAsync(testCode);
        }

        private static async Task VerifyAnalyzerIgnoringCompilerDiagnosticsAsync(string testCode)
        {
            var test = new CSharpAnalyzerTest<MapperAnalyzer, DefaultVerifier>
            {
                TestCode = testCode,
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
                CompilerDiagnostics = CompilerDiagnostics.None
            };
            test.TestState.Sources.Add(("MapperAttribute.cs", TestConstants.MapperAttributeCode));
            await test.RunAsync();
        }

        [Fact]
        public async Task Analyze_WhenUsingMarshal_ShouldEmitELM008()
        {
            var testCode = @"
using System.Runtime.InteropServices;
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class MyMapper
    {
        public void Map()
        {
            var size = Marshal.SizeOf<int>();
        }
    }
}";
            var expected = VerifyCS.Diagnostic("ELM008").WithLocation(12, 24).WithMessage("Mapper uses reflection which violates AOT-First principles");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task Analyze_WhenUsingRuntimeHelpers_ShouldEmitELM008()
        {
            var testCode = @"
using System.Runtime.CompilerServices;
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class MyMapper
    {
        public void Map()
        {
            var hash = RuntimeHelpers.GetHashCode(new object());
        }
    }
}";
            var expected = VerifyCS.Diagnostic("ELM008").WithLocation(12, 24).WithMessage("Mapper uses reflection which violates AOT-First principles");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task Analyze_WhenUsingFormatterServices_ShouldEmitELM008()
        {
            var testCode = @"
using System.Runtime.Serialization;
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class MyMapper
    {
        public void Map()
        {
            var obj = FormatterServices.GetUninitializedObject(typeof(string));
        }
    }
}";
            var expected = VerifyCS.Diagnostic("ELM008").WithLocation(12, 23).WithMessage("Mapper uses reflection which violates AOT-First principles");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public void Descriptors_WhenInspected_ShouldContainExpectedProperties()
        {
            var analyzer = new MapperAnalyzer();
            analyzer.SupportedDiagnostics.Length.Should().Be(3);
            analyzer.SupportedDiagnostics.Should().Contain(MapperAnalyzer.UsesReflection);
            analyzer.SupportedDiagnostics.Should().Contain(MapperAnalyzer.UsesDynamic);
            analyzer.SupportedDiagnostics.Should().Contain(MapperAnalyzer.MustBePartial);

            // MustBePartial (ELM012)
            MapperAnalyzer.MustBePartial.Id.Should().Be("ELM012");
            MapperAnalyzer.MustBePartial.Title.ToString().Should().Be("Mapper must be partial");
            MapperAnalyzer.MustBePartial.MessageFormat.ToString().Should().Be("Mapper class '{0}' must be declared as partial");
            MapperAnalyzer.MustBePartial.Category.Should().Be("EricksonLopez.Mapper");
            MapperAnalyzer.MustBePartial.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
            MapperAnalyzer.MustBePartial.IsEnabledByDefault.Should().BeTrue();

            // UsesReflection (ELM008)
            MapperAnalyzer.UsesReflection.Id.Should().Be("ELM008");
            MapperAnalyzer.UsesReflection.Title.ToString().Should().Be("Mapper uses reflection");
            MapperAnalyzer.UsesReflection.MessageFormat.ToString().Should().Be("Mapper uses reflection which violates AOT-First principles");
            MapperAnalyzer.UsesReflection.Category.Should().Be("EricksonLopez.Mapper");
            MapperAnalyzer.UsesReflection.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
            MapperAnalyzer.UsesReflection.IsEnabledByDefault.Should().BeTrue();

            // UsesDynamic (ELM009)
            MapperAnalyzer.UsesDynamic.Id.Should().Be("ELM009");
            MapperAnalyzer.UsesDynamic.Title.ToString().Should().Be("Mapper uses dynamic");
            MapperAnalyzer.UsesDynamic.MessageFormat.ToString().Should().Be("Mapper uses dynamic which violates AOT-First principles");
            MapperAnalyzer.UsesDynamic.Category.Should().Be("EricksonLopez.Mapper");
            MapperAnalyzer.UsesDynamic.DefaultSeverity.Should().Be(DiagnosticSeverity.Error);
            MapperAnalyzer.UsesDynamic.IsEnabledByDefault.Should().BeTrue();
        }

        [Fact]
        public async Task Analyze_WhenMemberAccessIsValid_ShouldIgnore()
        {
            var testCode = @"
using System;
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class MyMapper
    {
        public void Map()
        {
            int number = 42;
            int copy = number;
            var text = string.Concat(""Hello "", copy.ToString());
            var t = typeof(string).ToString();
            var len = global::System.Math.Abs(-5);
        }
    }
}";
            await VerifyMapperAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Analyze_WhenClassHasNoMapperAttribute_ShouldNotEmitAnyDiagnostics()
        {
            var testCode = @"
using System;
namespace TestNamespace
{
    public class RegularClass
    {
        public void DoSomething()
        {
            dynamic d = 1;
            var t = typeof(string).GetMethods();
        }
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public void DiagnosticDescriptors_WhenInspected_ShouldHaveUniqueIdsAndValidSeverities()
        {
            var analyzer = new MapperAnalyzer();
            var descriptors = analyzer.SupportedDiagnostics;

            var ids = descriptors.Select(d => d.Id).ToList();
            ids.Should().OnlyHaveUniqueItems();

            MapperAnalyzer.MustBePartial.Id.Should().Be("ELM012");
            MapperAnalyzer.UsesReflection.Id.Should().Be("ELM008");
            MapperAnalyzer.UsesDynamic.Id.Should().Be("ELM009");
        }

        [Fact]
        public async Task Analyze_WhenUsingTypeGetMethods_ShouldEmitELM008()
        {
            var testCode = @"
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class MyMapper
    {
        public void Map()
        {
            var methods = typeof(string).GetMethods();
        }
    }
}";
            var expected = VerifyCS.Diagnostic("ELM008").WithLocation(11, 27).WithMessage("Mapper uses reflection which violates AOT-First principles");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task Analyze_WhenUsingTypeGetField_ShouldEmitELM008()
        {
            var testCode = @"
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class MyMapper
    {
        public void Map()
        {
            var field = typeof(string).GetField(""Empty"");
        }
    }
}";
            var expected = VerifyCS.Diagnostic("ELM008").WithLocation(11, 25).WithMessage("Mapper uses reflection which violates AOT-First principles");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task Analyze_WhenUsingTypeGetConstructor_ShouldEmitELM008()
        {
            var testCode = @"
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class MyMapper
    {
        public void Map()
        {
            var ctor = typeof(string).GetConstructor(global::System.Type.EmptyTypes);
        }
    }
}";
            var expected = VerifyCS.Diagnostic("ELM008").WithLocation(11, 24).WithMessage("Mapper uses reflection which violates AOT-First principles");
            await VerifyMapperAnalyzerAsync(testCode, expected);
        }

        [Fact]
        public async Task Analyze_WhenClassIsAlreadyPartialWithMapper_ShouldNotEmitELM012()
        {
            var testCode = @"
namespace TestNamespace
{
    using EricksonLopez.Mapper;

    [Mapper]
    public partial class ValidPartialMapper
    {
        public void Map()
        {
        }
    }
}";
            await VerifyMapperAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task Analyze_WhenGlobalStatementsWithoutMapper_ShouldNotEmitDiagnostics()
        {
            var testCode = @"
using System;
public class StandaloneClass
{
    public void Run()
    {
        var x = 10;
        System.Console.WriteLine(x);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(testCode);
        }
    }
}








