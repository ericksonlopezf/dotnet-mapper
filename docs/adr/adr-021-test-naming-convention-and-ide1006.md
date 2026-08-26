# ADR-021: Institutional Test Naming Convention (Roy Osherove) and Local IDE1006 Diagnostic Policy

## Status
Accepted

## Context
In enterprise-grade .NET software engineering and Continuous Integration / Continuous Deployment (CI/CD) environments, automated test suites serve two foundational, interdependent objectives:

1. **Regression Safety Net**: Verifying functional correctness, invariant enforcement, and preventing behavioral regressions across runtime versions (.NET 8, .NET 9, .NET 10) and compilation targets.
2. **Living Executable Specification**: Serving as unambiguous, living documentation of domain requirements, compiler contracts, AST transformation rules, edge cases, and diagnostic behaviors that are immediately readable by engineers without inspecting the test implementation details.

### The Problem with Strict PascalCase in Test Explorers and CI Logs
The standard Microsoft .NET C# coding style rules enforced by Roslyn analyzer diagnostic `IDE1006` (`Naming Styles`) mandate `PascalCase` without underscores for all method identifiers.

While `PascalCase` is appropriate for production code APIs in `src/`, enforcing strict `PascalCase` on test method identifiers degrades cognitive readability in:
- **CI/CD Console Logs and Dashboards** (e.g., GitHub Actions step summaries, Azure DevOps pipelines).
- **TRX and JUnit XML Test Reports** generated during automated test execution.
- **IDE Test Explorers** (Visual Studio, JetBrains Rider, VS Code Test Runner).

For instance, compare:
- **Unformatted `PascalCase`**: `GivenDirectSelfCycleWhenDetectingGraphCyclesShouldEmitDiagnosticELM010`
- **Structured Osherove 3-Part Pattern**: `DetectCycles_WhenDirectSelfCycle_ShouldEmitDiagnosticELM010`

The structured pattern instantly isolates:
1. What unit or method is being tested (**UnitOfWork**).
2. Under what specific conditions or input state (**Scenario / StateUnderTest**).
3. What the verifiable contract or outcome must be (**ExpectedBehavior / Result**).

---

## Decision

### 1. Institutional Adoption of Roy Osherove's Three-Part Pattern
All test methods across all test projects (`tests/**`) MUST strictly adhere to the Roy Osherove naming standard:

$$\textbf{UnitOfWork\_Scenario\_ExpectedBehavior}$$
$$\text{or}$$
$$\textbf{MethodName\_StateUnderTest\_ExpectedResult}$$

#### Structural Rules:
1. **Unit of Work**: The method, attribute, algorithm, or class under test (e.g., `DetectCycles`, `RegisterCodeFixesAsync`, `MapPropertyAttribute`, `Resolve`, `Map`).
2. **Scenario / State Under Test**: Preceded by `When` or describing the input condition (e.g., `WhenDirectSelfCycle`, `WhenUnmappedMember`, `WhenPrimitiveNonNullableToNullable`, `WithFieldName`).
3. **Expected Behavior**: Preceded by `Should` or defining the exact expected result (e.g., `ShouldEmitDiagnostic`, `ShouldAddMapPropertyAttribute`, `ShouldEmitDirectAssignment`, `ShouldSetConverterFieldName`).

#### Standard Examples across Test Projects:
- **Roslyn AST & Graph Algorithms**:
  - `DetectCycles_WhenDirectSelfCycle_ShouldEmitDiagnostic`
  - `DetectCycles_WhenLinearMapping_ShouldNotAddDiagnostics`
- **Roslyn Analyzers & CodeFixes**:
  - `Analyze_WhenUsingReflection_ShouldEmitELM008`
  - `RegisterCodeFixesAsync_WhenUnmappedMember_ShouldAddMapPropertyAttribute`
  - `FixableDiagnosticIds_WhenQueried_ShouldContainELM001`
- **Model & Attribute Invariants (Property-Based)**:
  - `MapperAttribute_DefaultConstructor_ShouldHaveStrictMappingTrue`
  - `MapPropertyAttribute_Constructor_ShouldSetSourceAndDestinationNames`
  - `AttributeUsage_WhenInspected_ShouldHaveExpectedTargetsAndFlags`
- **Conversion Strategy Resolution**:
  - `Resolve_WhenNumericWideningToDecimal_ShouldGenerateDirectAssignments`
  - `Resolve_WhenCollectionMapping_ShouldGenerateCapacitySizing`
- **End-to-End & Integration Mappings**:
  - `OrderMapper_WhenMappingDifferentEnumValues_ShouldEmitSwitchExpression`
  - `DependencyInjection_AddGeneratedMappers_ShouldRegisterMappersAndConvertersWithCorrectLifetimes`

---

### 2. Local Suppression of Analyzer Diagnostic `IDE1006` for Test Projects
To ensure strict zero-warning policy (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`) across the entire repository while enabling living specification test names:

1. **MSBuild Configuration (`tests/Directory.Build.props`)**:
   ```xml
   <Project>
     <Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../'))" />
     <PropertyGroup>
       <!-- ADR-021: Living specifications using Roy Osherove naming pattern (Method_Scenario_Result) -->
       <NoWarn>$(NoWarn);IDE1006</NoWarn>
     </PropertyGroup>
   </Project>
   ```

2. **EditorConfig Configuration (`.editorconfig`)**:
   ```ini
   # Test files: Living specifications under Roy Osherove naming pattern (ADR-021)
   # Allow underscores in test method names (UnitOfWork_StateUnderTest_ExpectedBehavior)
   [tests/**.cs]
   dotnet_diagnostic.IDE1006.severity = none
   ```

3. **Production Isolation**:
   - Production source code (`src/**`) remains strictly bound to `PascalCase` method styling with zero analyzer suppressions and `TreatWarningsAsErrors = true`.

---

## Consequences

### Positive
- **Living Executable Specification**: Test failure summaries in CI/CD logs pinpoint the exact failure context without requiring manual source inspection.
- **Triage and Mean Time to Resolution (MTTR)**: Fast diagnostic triage during code reviews and continuous delivery pipelines.
- **Unified Consistency**: Elimination of arbitrary naming patterns (`Test1`, `CheckMapping`, `MyTest`) across the entire team and repository.
- **Zero Production Compromise**: Production assemblies maintain 100% compliance with Microsoft standard API naming guidelines.

### Negative / Trade-offs
- Requires local analyzer suppression (`IDE1006`) scoped strictly to test directories via MSBuild and EditorConfig.
