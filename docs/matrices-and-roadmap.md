# EricksonLopez.Mapper — Complementary Matrices and Detailed Roadmap

---

## SECTION A: CONVERSION MATRIX

> **Automatic**: Generated without additional configuration  
> **Warning**: Emits ELM-xxx diagnostic but generates mapping code  
> **Error**: ELM-xxx blocks compilation  
> **Custom Converter Required**: Requires [UseConverter] or partial method

| Source | Destination | Automatic | Warning | Error | Custom Converter | Notes |
|---|---|---|---|---|---|---|
| string | string | ✅ | — | — | No | DirectAssignment |
| string | string? | ✅ | — | — | No | DirectAssignment |
| string? | string | ❌ | — | ELM004 | [MapNullFallback] | Nullability mismatch |
| string? | string? | ✅ | — | — | No | DirectAssignment |
| int | int | ✅ | — | — | No | DirectAssignment |
| byte | int | ✅ | — | — | No | Widening — implicit |
| byte | long | ✅ | — | — | No | Widening — implicit |
| byte | double | ✅ | — | — | No | Widening — implicit |
| short | int | ✅ | — | — | No | Widening — implicit |
| short | long | ✅ | — | — | No | Widening — implicit |
| int | long | ✅ | — | — | No | Widening — implicit |
| int | double | ✅ | — | — | No | Widening — implicit |
| int | decimal | ✅ | — | — | No | Widening — implicit |
| float | double | ✅ | — | — | No | Widening — implicit |
| long | int | ✅ | ELM015 | — | No | Narrowing — cast (int)x |
| long | short | ✅ | ELM015 | — | No | Narrowing — cast |
| double | float | ✅ | ELM015 | — | No | Narrowing — cast |
| double | decimal | ❌ | — | ELM003 | Yes | Precision loss |
| decimal | double | ✅ | ELM015 | — | No | Narrowing |
| Guid | string | ✅ | — | — | No | .ToString() |
| string | Guid | ✅ | ELM016 | — | No | Guid.Parse — can throw |
| Guid? | string | ✅ | — | — | No | .ToString() |
| string? | Guid | ❌ | — | ELM004 | [MapNullFallback] | Null + parse risk |
| DateTime | DateOnly | ✅ | — | — | No | DateOnly.FromDateTime() |
| DateTime | DateTimeOffset | ✅ | — | — | No | new DateTimeOffset() |
| DateTime | DateTime | ✅ | — | — | No | DirectAssignment |
| DateOnly | DateTime | ✅ | — | — | No | .ToDateTime(TimeOnly.MinValue) |
| DateOnly | DateTimeOffset | ✅ | — | — | No | new DateTimeOffset(.ToDateTime()) |
| DateTimeOffset | DateTime | ✅ | — | — | No | .DateTime |
| DateTimeOffset | DateOnly | ✅ | — | — | No | DateOnly.FromDateTime(.DateTime) |
| TimeSpan | string | ❌ | — | ELM003 | Yes | No builtin |
| Enum | string | ✅ | — | — | No | .ToString() |
| string | Enum | ✅ | ELM016 | — | No | Enum.Parse — can throw |
| Enum | Enum (same) | ✅ | — | — | No | DirectAssignment |
| Enum | Enum (different, name match) | ✅ | — | — | No | Member name match + numeric cast |
| Enum | Enum (no name match) | ❌ | ELM014 (permissive) | ELM014 (strict) | Yes | Unmatched enum value |
| Enum | int | ✅ | — | — | No | (int)source.Enum |
| int | Enum | ✅ | — | — | No | (DestEnum)source.Int |
| T | T? | ✅ | — | — | No | DirectAssignment |
| T? | T | ❌ | — | ELM004 | [MapNullFallback] | Null guard required |
| T? | T? | ✅ | — | — | No | DirectAssignment |
| bool | string | ❌ | — | ELM003 | Yes | No builtin |
| string | bool | ❌ | — | ELM003 | Yes | No builtin |
| ValueObject | Primitive | ✅ | — | — | No | Heuristic unwrap (value.Value) |
| Primitive | ValueObject (record) | ✅ | — | — | No | Heuristic wrap new VO(primitive) |
| ClassA | ClassB (different) | ❌ | — | ELM003 | partial method or [UseConverter] | No structural inference |
| ClassA | ClassA | ✅ | — | — | No | Property mapping |
| List<T> | List<T> | ✅ | — | — | No | Foreach + capacity |
| T[] | List<T> | ✅ | — | — | No | for loop |
| List<T> | T[] | ✅ | — | — | No | Array allocation |
| IEnumerable<T> | List<T> | ✅ | — | — | No | TryGetNonEnumeratedCount |
| List<T> | ImmutableArray<T> | ✅ | — | — | No | CreateBuilder |
| List<T> | HashSet<T> | ✅ | — | — | No | new HashSet with capacity |
| Dictionary<K,V> | Dictionary<K,V> | ✅ | — | — | No | foreach KVP |
| Dictionary<K,V1> | Dictionary<K,V2> | Partial | — | ELM003 | partial method for V1→V2 | If V1/V2 have matching method |

> **Note**: All planned native conversions have been completed and implemented in the generator.

---

## SECTION B: AOT MATRIX

| Feature | Uses Reflection | Uses Dynamic Code | Trim Safe | Native AOT | Source Generated | Status |
|---|---|---|---|---|---|---|
| Property → Property | No | No | Yes | Yes | Yes | ✅ Safe |
| Nested object mapping | No | No | Yes | Yes | Yes | ✅ Safe |
| [MapFactory] | No | No | Yes | Yes | Yes | ✅ Safe |
| [MapDerivedType] | No | No | Yes | Yes | Yes | ✅ Safe |
| Value Object wrap/unwrap | No | No | Yes | Yes | Yes | ✅ Safe |
| IConverter<T,T> instantiation | No | No | Yes | Yes | Yes (new T()) | ✅ Safe |
| Enum → string | No | No | Yes | Yes | Yes (.ToString()) | ✅ Safe |
| string → Enum (Enum.Parse) | No | No | Yes | Yes | Yes | ⚠️ AOT-safe but runtime throw risk (ELM016) |
| string → Enum (Enum.TryParse) | No | No | Yes | Yes | Yes | ✅ Recommended |
| Guid → string / string → Guid | No | No | Yes | Yes | Yes | ✅ Safe |
| DateTime → DateOnly | No | No | Yes | Yes | Yes | ✅ Safe |
| Collection loops (for/foreach) | No | No | Yes | Yes | Yes | ✅ Safe |
| ImmutableArray.CreateBuilder | No | No | Yes | Yes | Yes | ✅ Safe |
| TryGetNonEnumeratedCount | No | No | Yes | Yes | Yes | ✅ Safe |
| AddGeneratedMappers() | No | No | Yes | Yes | Yes | ✅ Safe |
| Explicit/Implicit cast operators | No | No | Yes | Yes | Yes | ✅ Safe |
| MapperGenerator pipeline | Build-time only | Build-time only | N/A | N/A | Yes | ✅ Build-time only |
| Roslyn SemanticModel usage | Build-time only | No | N/A | N/A | N/A | ✅ Not in output |
| ELM008/009 Analyzer | Build-time only | No | N/A | N/A | N/A | ✅ Not in output |
| Activator.CreateInstance | N/A | N/A | No | No | N/A | ❌ REJECTED permanently |
| PropertyInfo.GetValue/SetValue | N/A | N/A | No | No | N/A | ❌ REJECTED permanently |
| Expression.Compile() | N/A | N/A | No | No | N/A | ❌ REJECTED permanently |
| DynamicMethod / IL Emit | N/A | N/A | No | No | N/A | ❌ REJECTED permanently |
| dynamic keyword | N/A | Yes | No | No | N/A | ❌ REJECTED permanently |
| MakeGenericType/Method | N/A | N/A | No | No | N/A | ❌ REJECTED permanently |
| FormatterServices.GetUninitializedObject | N/A | N/A | No | No | N/A | ❌ REJECTED permanently |
| Assembly.GetTypes() | N/A | N/A | No | No | N/A | ❌ REJECTED permanently |
| LINQ .Select().ToList() in generated | No | No | Yes | Yes | No | ❌ REJECTED (allocation budget) |

---

## SECTION C: ANALYZER MATRIX

| Rule ID | Title | Severity | False Positive Risk | Code Fix | AOT Impact | Recommended | Status |
|---|---|---|---|---|---|---|---|
| ELM001 | Unmapped destination member | Error | Low | ✅ (MapIgnore / MapProperty) | None | ✅ Yes | Implemented (Code fixes available) |
| ELM002 | Missing factory or constructor | Error | Low | No | None | ✅ Yes | Implemented |
| ELM003 | Unsupported conversion | Error | Low | ✅ (Add partial method) | None | ✅ Yes | Implemented (Code fix available) |
| ELM004 | Nullability mismatch | Error | Low | ✅ (Add MapNullFallback) | None | ✅ Yes | Implemented (Code fix available) |
| ELM005 | Ambiguous property match | Error | Low | No | None | ✅ Yes | Implemented |
| ELM006 | Missing constructor mapping | Error | Medium (overlaps ELM002) | No | None | ⚠️ Clarify vs ELM002 | Implemented |
| ELM007 | Ambiguous constructor | Error | Low | ✅ (Add MapFactory) | None | ✅ Yes | Implemented (Code fix available) |
| ELM008 | Reflection usage in mapper | Error | Low | No | High — prevents AOT | ✅ Yes | Implemented (Expanded: Reflection, Activator, Marshal, RuntimeHelpers, FormatterServices) |
| ELM009 | dynamic keyword in mapper | Error | Low | No | High — prevents AOT | ✅ Yes | Implemented |
| ELM010 | Circular mapping reference | Error | Low | No | None | ✅ Yes | Implemented (Reports all cycles) |
| ELM011 | Incomplete polymorphism | Warning | Medium | No | Low | ✅ Yes | Implemented |
| ELM012 | Mapper must be partial (Analyzer) | Error | Low | ✅ (Make partial) | None | ✅ Yes | Implemented (Code fix available) |
| ELM013 | Invalid converter type (Generator) | Error | Low | No | None | ✅ Yes | Implemented (Assigned unique ID ELM013) |
| ELM014 | Enum source value without destination match | Error/Warning | Low | No | None | ✅ Yes | Implemented (Strict=Error, Permissive=Warning) |
| ELM015 | Narrowing numeric conversion | Warning | Low | No | Low | ✅ Yes | Implemented (Warning on potential data loss) |
| ELM016 | string→Enum may throw at runtime | Warning | Low | No | None | ✅ Yes | Implemented (Warning on Enum.Parse risk) |

---

## SECTION D: DETAILED ROADMAP — COMPLETE SPECIFICATION

### Phase 0 — Correctness & Debt

---

#### T001 — Fix ELM012 Diagnostic ID Collision

```
ID:           T001
Title:        Fix ELM012 Diagnostic ID Collision
Status:       DONE (Implemented & Verified)
Problem:      InvalidConverterType in Generator uses ELM012. MustBePartial in Analyzer
              also uses ELM012. Two distinct diagnostics share the same ID, corrupting
              diagnostic routing in IDEs and in test assertions.
Current State: Resolved. Generator uses ELM013 for InvalidConverterType.
               Analyzer uses ELM012 for MustBePartial.
Target State: InvalidConverterType is assigned id "ELM013". All tests, docs, and
              documentation reference ELM013 for converter validation.
Why:          Same ID in two different subsystems breaks IDE diagnostic filtering,
              suppression, and test assertions. A developer adding #pragma warning disable
              ELM012 for partial-class issues would also suppress converter errors.
Affected Package: EricksonLopez.Mapper.Generator
Affected API: DiagnosticDescriptors.InvalidConverterType
Dependencies: None
Breaking Change: No (diagnostic IDs not in PublicAPI.Shipped.txt)
AOT Impact:   None
Performance Impact: None
Generated Code Impact: None
Tests:        Update ConverterValidationTests.cs to assert ELM013.
              Add regression test that ELM012 and ELM013 are never the same ID.
Benchmarks:   None
Documentation: Update api-reference.md diagnostics table. Update ADR-013.
Acceptance Criteria:
  - DiagnosticDescriptors.InvalidConverterType.Id == "ELM013" (Met)
  - MapperAnalyzer MustBePartial remains ELM012 (Met)
  - All existing tests pass (Met)
  - New test: cannot find ELM012 when converter type is wrong (finds ELM013) (Met)
Priority:     P0
Complexity:   Low (3-line change + test update)
```

---

#### T002 — Define and Implement Narrowing Numeric Conversion Policy

```
ID:           T002
Title:        Define and Implement Narrowing Numeric Conversion Policy
Status:       DONE (Implemented & Verified)
Problem:      Comment at MapperGenerator.cs:1045 says narrowing is "intentionally NOT handled
              here". Yet test Generate_NarrowingNumericConversion_Int64ToInt32_EmitsCast
              exists and has a snapshot — if narrowing emits ELM003, the test name is
              misleading. Code, comment, test name, and documentation are misaligned.
Current State: Resolved. Narrowing generates explicit cast `(int)source.X` and emits
               ELM015 Warning "Potential data loss: narrowing conversion from {0} to {1}".
Target State: DECISION: Narrowing generates explicit cast `(int)source.X` with
              ELM015 Warning "Potential data loss: narrowing conversion from {0} to {1}".
              Comment updated. Test renamed to match. Snapshot verified.
Why:          Rejecting narrowing completely forces UseConverter for trivial cases
              (e.g., long→int for IDs stored as long in DB). The explicit cast in
              generated code is visible and debuggable. The warning informs the developer.
              This matches C# language behavior: narrowing is allowed, the compiler warns.
Affected Package: EricksonLopez.Mapper.Generator
Affected API: GetBuiltinConversionStrategy, DiagnosticDescriptors (add ELM015)
Dependencies: T001 (ID namespace established)
Breaking Change: Yes if narrowing was ELM003 (now allows it). No if it was already
                 generating casts (just adds warning).
AOT Impact:   None
Performance Impact: None
Generated Code Impact: Adds explicit cast: `(int)source.Count` instead of `source.Count`
Tests:        Verify existing snapshot for narrowing test.
              Add test that ELM015 is emitted (warning, not error).
              Add test that narrowing cast compiles correctly.
Benchmarks:   None (cast is zero-overhead)
Documentation: Update api-reference.md conversion table. Create ADR-020.
Acceptance Criteria:
  - long → int generates `(int)source.X` in output (Met)
  - ELM015 is emitted as Warning (not Error) for every narrowing conversion (Met)
  - Generated code compiles without CS warnings (Met)
  - Test name reflects actual behavior (Met)
Priority:     P0
Complexity:   Low-Medium
```

---

#### T003 — Null Propagation for Nullable Nested Objects

```
ID:           T003
Title:        Null Propagation for Nullable Nested Objects
Status:       DONE (Implemented & Verified)
Problem:      When a nested property type is nullable on the destination (AddressDto?),
              the generator emits MapAddress(source.Address) directly. If source.Address
              is null, the sub-method throws ArgumentNullException — surprising behavior
              for a mapping that is explicitly nullable.
Current State: Resolved. Generator emits:
               `Address = source.Address != null ? MapAddress(source.Address) : null`
Target State: When destination property type is `T?` (nullable reference type) AND
              the source property type is also nullable:
              `Address = source.Address != null ? MapAddress(source.Address) : null`
              For non-nullable destination, behavior is unchanged (ArgumentNullException
              is semantically correct when the source is null and dest is non-nullable).
Why:          In DDD, optional relationships are common. A User may optionally have
              a billing address. The mapper must handle this naturally, not throw.
              This is the behavior developers expect and what Mapperly already does.
Affected Package: EricksonLopez.Mapper.Generator
Affected API: GenerateConversionExpression / MapMethodInvocation strategy
Dependencies: None
Breaking Change: No — changes generated code but output is semantically equivalent
               for cases where source was non-null. Only difference: null-safe for null.
AOT Impact:   None (null check is zero AOT overhead)
Performance Impact: One null check per nullable nested property — negligible
Generated Code Impact: Adds ternary: `source.X != null ? Map(source.X) : null`
Tests:        Add test: nullable nested property maps to null when source is null.
              Add test: nullable nested property maps correctly when source is non-null.
              Add snapshot verification of both cases.
Benchmarks:   None (null check is compiler-eliminated when property is non-null)
Documentation: Update nested mapping docs. Update ADR-017.
Acceptance Criteria:
  - `public AddressDto? Address { get; set; }` + `source.Address == null`
    → generated `Address = null` (no exception) (Met)
  - `public AddressDto Address { get; set; }` + `source.Address == null`
    → ArgumentNullException still thrown (non-nullable contract) (Met)
  - All existing nested mapping tests still pass (Met)
Priority:     P0
Complexity:   Medium
```

---

#### T004 — DI Detection via ForAttributeWithMetadataName

```
ID:           T004
Title:        Fix DI Registration Detection to Use ForAttributeWithMetadataName
Status:       DONE (Implemented & Verified)
Problem:      The detection of [assembly: GenerateMapperRegistration] uses
              CreateSyntaxProvider with a predicate that matches ALL AttributeListSyntax
              nodes with assembly target. This scans every assembly-level attribute in
              the compilation, not just the specific one.
Current State: Resolved. Uses ForAttributeWithMetadataName("EricksonLopez.Mapper.GenerateMapperRegistrationAttribute").
Target State: Replace with ForAttributeWithMetadataName("EricksonLopez.Mapper.GenerateMapperRegistrationAttribute")
              for efficient, targeted detection.
Why:          CreateSyntaxProvider with a broad predicate triggers on every keystroke
              in files that have ANY assembly-level attribute. ForAttributeWithMetadataName
              is the correct Roslyn incremental pattern for this use case.
Affected Package: EricksonLopez.Mapper.Generator
Affected API: Initialize() method in MapperGenerator
Dependencies: None
Breaking Change: No
AOT Impact:   None
Performance Impact: Improves IDE responsiveness. Reduces spurious generator re-runs.
Generated Code Impact: None (same output)
Tests:        Add incremental generator test verifying DI registration is only triggered
              when [GenerateMapperRegistration] is present.
Benchmarks:   Optional: measure generator invocation count before/after.
Documentation: Update ADR-002 (incremental stability).
Acceptance Criteria:
  - assemblyHasDiAttrProvider uses ForAttributeWithMetadataName (Met)
  - DI registration generated correctly in all existing tests (Met)
  - No spurious generator runs when unrelated assembly attributes change (Met)
Priority:     P1
Complexity:   Low
```

---

#### T005 — Regression Test for ELM012 ID Uniqueness

```
ID:           T005
Title:        Regression Test: ELM Diagnostic IDs Must Be Unique
Status:       DONE (Implemented & Verified)
Problem:      There is no automated check that prevents two diagnostics from sharing
              the same ELM ID. Without this test, the ELM012 collision can recur.
Current State: Resolved. DiagnosticDescriptors_AllIds_MustBeUnique unit test added.
Target State: A unit test that enumerates all DiagnosticDescriptor fields in
              DiagnosticDescriptors and all in MapperAnalyzer and asserts no duplicate IDs.
Why:          Prevents regression. Catches future ID collisions at development time.
Affected Package: EricksonLopez.Mapper.Generator.Tests / Analyzer.Tests
Affected API: DiagnosticDescriptors, MapperAnalyzer
Dependencies: T001
Breaking Change: No
AOT Impact:   None
Performance Impact: None
Tests:        DiagnosticIdUniquenessTest — reflection over DiagnosticDescriptors fields.
Benchmarks:   None
Documentation: None
Acceptance Criteria:
  - Test fails if any two ELM IDs are identical (Met)
  - Test passes after T001 is implemented (Met)
Priority:     P0
Complexity:   Low
```

---

### Phase 1 — Core Feature Gaps

---

#### T010 — Enum → Enum Mapping by Member Name

```
ID:           T010
Title:        Implement Enum → Enum Mapping by Member Name
Status:       DONE (Implemented & Verified)
Problem:      When source and destination types are both TypeKind.Enum, the generator
              currently falls through to ELM003 (unsupported conversion). This forces
              developers to write a [UseConverter] for the most common mapping scenario
              in any enterprise application.
Current State: Resolved. Generates direct numeric cast `(DestEnum)(source.Field)` when
               members match by name. Emits ELM014 when unmatched values exist.
Target State: When both source and destination are TypeKind.Enum:
              1. Collect all source enum members.
              2. For each source member, find matching destination member by name
                 (case-sensitive, with optional case-insensitive fallback).
              3. If all source values have a destination match:
                 Emit: `(DestEnum)(int)source.Field` (numeric cast — efficient).
              4. If StrictMapping = true and a source value has no match:
                 Emit ELM014 — "Enum source value '{0}' has no matching destination".
              5. If StrictMapping = false and gaps exist:
                 Emit ELM015 (warning) — "Enum mapping may lose values".
Why:          This is the #1 missing feature vs Mapperly. Every project has enums.
              Every cross-layer mapping has enum → enum. Requiring a converter for this
              is unacceptable friction that costs adoption.
Affected Package: EricksonLopez.Mapper.Generator, EricksonLopez.Mapper.Generator.Tests
Affected API: GetBuiltinConversionStrategy, DiagnosticDescriptors (add ELM014)
Dependencies: T001 (ID namespace clean), T002 (ELM015 exists)
Breaking Change: No — currently these cases produce ELM003 (now they work)
AOT Impact:   None — (int) cast is AOT-safe
Performance Impact: Positive — numeric cast is faster than string comparison
Generated Code Impact: `(DestStatusEnum)(int)source.Status`
Tests:        Same enum type → passes without ELM.
              Same-name members → passes.
              Missing source value → ELM014 in strict mode.
              Missing source value → ELM015 warning in permissive mode.
              Numeric-equivalent enum → correct cast.
Benchmarks:   Enum→Enum micro-benchmark (should be indistinguishable from direct cast).
Documentation: Update api-reference.md conversions. Create ADR-016.
Acceptance Criteria:
  - `SourceStatus.Active → DestStatus.Active` without [UseConverter] (Met)
  - ELM014 fired when source member has no destination name match (strict mode) (Met)
  - ELM015 warning when permissive + incomplete coverage (Met)
  - Generated code uses numeric cast, not string comparison (Met)
Priority:     P0
Complexity:   Medium
```

---

#### T011 — int → Enum and Enum → int Builtin Conversions

```
ID:           T011
Title:        Implement int → Enum and Enum → int as Builtin Conversions
Status:       DONE (Implemented & Verified)
Problem:      Mapping DB integer columns to domain enums (or vice versa) is extremely
              common in repository/infrastructure layers. Currently ELM003.
Current State: Resolved. Generates `(DestEnum)source.Value` and `(int)source.Status`.
Target State: int source + Enum dest → `(DestEnum)source.Value`
              Enum source + int dest → `(int)source.Status`
              Both are explicit casts — zero overhead, AOT-safe.
Why:          Infrastructure layer always deals with int-backed enums.
              This is table-stakes for any mapper targeting DDD projects.
Affected Package: EricksonLopez.Mapper.Generator
Affected API: GetBuiltinConversionStrategy
Dependencies: T002 (narrowing/conversion framework established)
Breaking Change: No
AOT Impact:   None
Performance Impact: Zero (explicit cast)
Generated Code Impact: `(OrderStatus)source.StatusCode` or `(int)source.Status`
Tests:        int→Enum maps correctly. Enum→int maps correctly. Nullable variants.
Benchmarks:   None (trivial cast)
Documentation: Update conversion table in api-reference.md.
Acceptance Criteria:
  - `int → enum` generates `(DestEnum)source.IntProp` (Met)
  - `enum → int` generates `(int)source.EnumProp` (Met)
  - No ELM003 for these scenarios (Met)
Priority:     P0
Complexity:   Low
```

---

#### T012 — DateOnly ↔ DateTime / DateTimeOffset Inverse Conversions

```
ID:           T012
Title:        Add DateOnly→DateTime, DateOnly→DateTimeOffset, DateTimeOffset→DateTime Builtins
Status:       DONE (Implemented & Verified)
Problem:      DateTime→DateOnly is implemented. The inverse (DateOnly→DateTime) is not.
              DateTimeOffset→DateTime is not. These cause ELM003 for common date scenarios.
Current State: Resolved. All 4 temporal conversion directions are supported nativelly.
Target State:
  DateOnly → DateTime: `{0}.ToDateTime(TimeOnly.MinValue)`
  DateOnly → DateTimeOffset: `new DateTimeOffset({0}.ToDateTime(TimeOnly.MinValue))`
  DateTimeOffset → DateTime: `{0}.DateTime`
  DateTimeOffset → DateOnly: `DateOnly.FromDateTime({0}.DateTime)`
Why:          All four directions are equally common. Having DateTime→DateOnly but not
              the reverse creates an asymmetric, surprising API.
Affected Package: EricksonLopez.Mapper.Generator
Affected API: GetBuiltinConversionStrategy
Dependencies: None
Breaking Change: No (currently ELM003)
AOT Impact:   None
Performance Impact: None (direct method calls)
Generated Code Impact: `source.Date.ToDateTime(TimeOnly.MinValue)` etc.
Tests:        One test per conversion direction, including nullable variants.
Benchmarks:   None
Documentation: Update conversion table.
Acceptance Criteria:
  - All 4 inverse date conversions work without [UseConverter] (Met)
  - Nullable variants handled via ELM004 pattern (Met)
Priority:     P1
Complexity:   Low
```

---

#### T014 — ImmutableList<T> and FrozenSet<T> as Collection Targets

```
ID:           T014
Title:        Add ImmutableList<T> and FrozenSet<T> as Collection Target Types
Status:       DONE (Implemented & Verified)
Problem:      ImmutableList<T> is a common collection in DDD value objects and
              FrozenSet<T> is the high-performance read-only set for .NET 8+.
              Neither is handled — falls through to ELM003.
Current State: Resolved. Emits ImmutableList.CreateBuilder and FrozenSet.ToFrozenSet.
Target State:
  ImmutableList<T>: ImmutableList.CreateBuilder<T>() → Add → ToImmutable()
  FrozenSet<T>: new List<T>() → ToFrozenSet()
Why:          These are natural targets for .NET 8+ projects. Generating ELM003
              forces unnecessary converters.
Affected Package: EricksonLopez.Mapper.Generator
Affected API: Collection target resolution
Dependencies: None
Breaking Change: No
AOT Impact:   None (both are AOT-safe)
Tests:        ImmutableList target test. FrozenSet target test (net8.0+ only).
Benchmarks:   None
Documentation: Update ADR-006 collection table.
Acceptance Criteria:
  - `ImmutableList<T>` target generates ImmutableList.CreateBuilder pattern (Met)
  - `FrozenSet<T>` target generates ToFrozenSet pattern (Met)
Priority:     P1
Complexity:   Low
```

---

### Phase 2 — IDE Code Fix Expansion

---

#### T020 — Code Fix for ELM001 (Unmapped Destination Member)

```
ID:           T020
Title:        Implement Code Fix for ELM001 — Unmapped Destination Member
Status:       DONE (Implemented & Verified)
Problem:      When ELM001 fires, the developer must manually add [MapIgnore] or
              [MapProperty]. This is the most common diagnostic and the most
              frequently encountered friction in the IDE.
Current State: Resolved. MapIgnoreCodeFixProvider and MapPropertyCodeFixProvider registered for ELM001.
Target State: Two code fix options presented in IDE:
  Option A: Add [MapIgnore("DestinationProperty")] to the mapper method
  Option B: Add [MapProperty("SourceName", "DestinationProperty")] placeholder
Why:          IDE experience is the #1 differentiator in adopt vs. reject. When
              ELM001 underlines a method, a Quick Fix that resolves it in one click
              eliminates friction. Mapperly has this. We should too.
Affected Package: EricksonLopez.Mapper.Analyzers
Affected API: New AddMapIgnoreCodeFixProvider, new AddMapPropertyCodeFixProvider
Dependencies: ELM001 diagnostic location correctly pointing to method declaration
Breaking Change: No
AOT Impact:   None
Tests:        CodeFixTest: verify inserted [MapIgnore] suppresses ELM001.
              CodeFixTest: verify inserted [MapProperty] links correctly.
Benchmarks:   None
Documentation: Update IDE experience docs.
Acceptance Criteria:
  - ELM001 shows Quick Fix lightbulb in VS/Rider (Met)
  - "Add [MapIgnore]" inserts correct attribute on method (Met)
  - "Add [MapProperty]" inserts attribute with placeholder names (Met)
Priority:     P1
Complexity:   Medium
```

---

#### T021 — Code Fix for ELM007 (Ambiguous Constructor → MapFactory)

```
ID:           T021
Title:        Implement Code Fix for ELM007 — Add MapFactory Suggestion
Status:       DONE (Implemented & Verified)
Problem:      When ELM007 fires (multiple constructors), the developer must know to
              add [MapFactory] manually. There is no Quick Fix to help.
Current State: Resolved. MapFactoryCodeFixProvider registered for ELM007.
Target State: Code fix: "Add [MapFactory(\"MethodName\")] to specify factory method"
              The fix inserts a [MapFactory("")] placeholder with the cursor positioned
              inside the string for the developer to complete the method name.
Why:          [MapFactory] is a unique feature. Guiding developers to it via Quick Fix
              increases discoverability and adoption of this differentiator.
Affected Package: EricksonLopez.Mapper.Analyzers
Affected API: New ELM007CodeFixProvider
Dependencies: ELM007 diagnostic
Breaking Change: No
Tests:        CodeFixTest: verify [MapFactory("")] inserted on ELM007.
Benchmarks:   None
Documentation: None
Acceptance Criteria:
  - ELM007 shows Quick Fix in IDE (Met)
  - "Add [MapFactory]" inserts attribute with empty string placeholder (Met)
Priority:     P1
Complexity:   Medium
```

---

#### T022 — Code Fix for ELM004 (Nullability Mismatch → MapNullFallback)

```
ID:           T022
Title:        Implement Code Fix for ELM004 — Add MapNullFallback
Status:       DONE (Implemented & Verified)
Problem:      ELM004 fires when mapping T? → T without a fallback. The fix
              [MapNullFallback("PropertyName", "...")] is non-obvious. No Quick Fix exists.
Current State: Resolved. MapNullFallbackCodeFixProvider registered for ELM004.
Target State: Code fix: "Add [MapNullFallback(\"PropertyName\", \"default\")]"
              Inserts attribute with the correct property name and a placeholder expression.
Why:          ELM004 is the second most common diagnostic. The required syntax of
              MapNullFallback is non-obvious. A Quick Fix removes friction significantly.
Affected Package: EricksonLopez.Mapper.Analyzers
Affected API: New ELM004CodeFixProvider
Dependencies: ELM004 diagnostic with correct SourceSpan targeting the property name
Breaking Change: No
Tests:        CodeFixTest: verify [MapNullFallback] inserted correctly.
Benchmarks:   None
Acceptance Criteria:
  - ELM004 shows Quick Fix in IDE (Met)
  - "Add [MapNullFallback]" inserts correctly parameterized attribute (Met)
Priority:     P1
Complexity:   Medium
```

---

### Phase 3 — Generator Refactoring

---

#### T030 — Decompose MapperGenerator.cs into Separate Responsibilities

```
ID:           T030
Title:        Decompose MapperGenerator.cs into Separate Responsibilities
Status:       DONE (Implemented & Verified)
Problem:      MapperGenerator.cs has 1163 lines performing: incremental pipeline setup,
              semantic target resolution, model building, member resolution, conversion
              strategy selection, cycle detection, code emission, and DI code generation.
              This violates SRP and makes the file extremely hard to modify without
              unintended consequences.
Current State: Resolved. Modularized into 7 specialized files:
               MapperGenerator.cs, MemberResolutionEngine.cs, ConversionStrategyFactory.cs,
               CycleDetector.cs, CodeEmitter.cs, DependencyInjectionEmitter.cs, Diagnostics.cs.
Target State:
  MapperGenerator.cs     — IIncrementalGenerator pipeline only (~100 lines)
  MemberResolver.cs      — GetAllProperties, MatchProperty, ambiguity detection
  ConversionResolver.cs  — GetConversionStrategy, GetBuiltinConversionStrategy, IsWideningNumeric
  ConstructionResolver.cs — SelectConstructionStrategy, factory resolution
  CycleDetector.cs       — DetectCycles, DFS logic
  CodeEmitter.cs         — GenerateConversionExpression, all StringBuilder building
  DiagnosticFactory.cs   — CreateDiagnostic helpers
  DICodeGenerator.cs     — ExecuteDIRegistration, DI extension method emission
Why:          Feature additions (T010, T013) will be painful in a 1163-line file.
              A future engineer adding enum→enum mapping should modify only
              ConversionResolver.cs, not navigate a monolith.
Affected Package: EricksonLopez.Mapper.Generator (internal only)
Affected API: No public API change
Dependencies: T010, T011, T012 (complete before refactor to avoid conflicts)
Breaking Change: No (internal refactor)
AOT Impact:   None
Performance Impact: None (same logic, different files)
Tests:        All existing snapshot tests must pass unchanged after refactor.
              Add unit tests for individual components where possible.
Benchmarks:   None
Documentation: Update architecture-guide.md
Acceptance Criteria:
  - All snapshot tests pass without modification (Met)
  - No class has more than 300 lines (Met)
  - Each file has a single clear responsibility (Met)
  - SRP enforced by code review (Met)
Priority:     P1
Complexity:   High (pure refactor, high regression risk)
```

---

#### T031 — ELM010 Cycle Detection: Report All Cycles

```
ID:           T031
Title:        Fix ELM010 to Report All Circular Mapping References
Status:       DONE (Implemented & Verified)
Problem:      The DFS cycle detection in MapperGenerator uses `break` after the first
              cycle is found. If the mapper class has multiple independent cycles
              (e.g., A→B→A AND C→D→C), only A→B→A is reported. The developer
              must fix and recompile to discover C→D→C.
Current State: Resolved. DFS traverses all mapper methods and accumulates every cycle diagnostic.
Target State: Remove break. Accumulate all DiagnosticInfo for all detected cycles.
              Emit one ELM010 per cycle.
Why:          "Fix one, compile, find next" is a frustrating developer experience.
              Showing all errors at once is the compile-time guarantee we promise.
Affected Package: EricksonLopez.Mapper.Generator
Affected API: DetectCycles internal method
Dependencies: None (T030 if done first — modify in CycleDetector.cs)
Breaking Change: No
AOT Impact:   None
Tests:        Add test with two independent cycles. Assert two ELM010 diagnostics.
Benchmarks:   None
Documentation: Update ADR-013 diagnostics.
Acceptance Criteria:
  - Two independent cycles → two ELM010 diagnostics (Met)
  - Existing single-cycle test still passes (Met)
Priority:     P1
Complexity:   Low
```

---

#### T032 — Expand ELM008 to Cover Marshal, RuntimeHelpers, FormatterServices

```
ID:           T032
Title:        Expand ELM008 AOT Reflection Detection
Status:       DONE (Implemented & Verified)
Problem:      ELM008 currently detects System.Reflection namespace usage,
              Type.Get* calls, and Activator. It does NOT detect:
                - System.Runtime.InteropServices.Marshal
                - System.Runtime.CompilerServices.RuntimeHelpers
                - System.Runtime.Serialization.FormatterServices
                - System.Runtime.Loader.AssemblyLoadContext
              All of these can bypass AOT safety.
Current State: Resolved. MapperAnalyzer checks and flags all dangerous reflection bypass types.
Target State: Expand the dangerous API set to include the above types.
Why:          An AOT-first library must close all reflection bypass paths.
              A developer using Marshal.SizeOf<T>() or RuntimeHelpers.GetUninitializedObject
              in a [Mapper] class bypasses the AOT guarantee silently.
Affected Package: EricksonLopez.Mapper.Analyzers
Affected API: MapperAnalyzer reflection detection logic
Dependencies: None
Breaking Change: No (only catches more violations)
Tests:        Add negative tests: Marshal usage in mapper → ELM008.
              RuntimeHelpers usage → ELM008. FormatterServices → ELM008.
Benchmarks:   None
Documentation: Update ADR-007 forbidden list.
Acceptance Criteria:
  - All listed types trigger ELM008 in a [Mapper] class (Met)
  - Existing tests unchanged (Met)
Priority:     P1
Complexity:   Low
```

---

## SECTION E: PERFORMANCE CONTRACT

```
Core Mapping Runtime Contract:
  ├── Reflection calls in hot path:        0
  ├── Dynamic code generation:             0
  ├── DI container calls per mapping:      0 (DI is startup-time only)
  ├── Extra allocations per mapping:       0 (beyond destination object itself)
  ├── Overhead vs. handwritten code:       < 1ns (statistically indistinguishable)
  ├── Startup overhead:                    0 (no expression compilation)
  └── IL2026 / IL3050 warnings:           0

Collection Mapping Contract:
  ├── LINQ iterator allocations:           0 (explicit loops)
  ├── Closure captures:                    0
  ├── Enumerator allocations:             0 (for-loop on arrays, foreach on lists)
  └── Growth reallocations:               0 (capacity pre-sized via TryGetNonEnumeratedCount)

Generator Build-Time Contract:
  ├── Cold run per mapper class:           < 50ms
  ├── Incremental re-run trigger:          Only when semantic mapping changes
  ├── Unnecessary re-runs on whitespace:  0 (EquatableArray<T> equality)
  └── Memory during generation:           Minimal (no full compilation copy)
```

---

## SECTION F: AOT CONTRACT

```
GUARANTEE LEVEL: Zero Tolerance

Core Runtime Package:
  Native AOT Compatible:    YES (IsAotCompatible=true)
  Trim Safe:                YES (no RequiresUnreferencedCode)
  Reflection:               PROHIBITED (enforced by ELM008)
  Dynamic Code:             PROHIBITED (enforced by ELM009)
  RequiresDynamicCode:      NEVER in any public API

Generated Code:
  Reflection calls:         0 — GUARANTEED
  dynamic keyword:          0 — GUARANTEED
  Activator.CreateInstance: 0 — GUARANTEED
  PropertyInfo.*:           0 — GUARANTEED
  Type.GetType(string):     0 — GUARANTEED
  Expression.Compile():     0 — GUARANTEED
  DynamicMethod:            0 — GUARANTEED
  LINQ in hot path:         0 — GUARANTEED
  Closure captures:         0 — GUARANTEED
  Boxing of value types:    0 (except when semantically required by type system)

Generator (Build-Time Only):
  Roslyn APIs:              ALLOWED (build-time execution only)
  SemanticModel:            ALLOWED (build-time execution only)
  ITypeSymbol.*:            ALLOWED (build-time execution only)
  NOT included in output:   Any Roslyn API usage stays in generator, never emitted

Violation Policy:
  Any feature that cannot meet these guarantees must:
  1. Be isolated in a clearly marked optional package
  2. Have an ADR documenting the violation and its justification
  3. Be suppressible without affecting core functionality
  4. Emit a build-time warning when enabled
```

---

## SECTION G: WHY NOT AUTOMAPPER?

AutoMapper solves the mapping problem with a fundamentally different architecture:

| Architectural Decision | AutoMapper | EricksonLopez.Mapper |
|---|---|---|
| Configuration time | Runtime startup | Compile time |
| Mapping execution | Runtime reflection + expression tree | Direct C# assignment |
| Type discovery | `Assembly.GetTypes()` scanning | Roslyn attributes |
| Configuration API | Fluent `CreateMap<T,U>()` | Attributes `[Mapper]`, `[MapProperty]` |
| Error discovery | `AssertConfigurationIsValid()` at startup | Build error |
| Failure mode | Production runtime exception | Build failure |
| AOT compatibility | ❌ Structurally incompatible | ✅ Core guarantee |
| Trimming | ❌ Requires extensive configuration | ✅ Zero configuration |
| Performance (POCO) | ~24.90 ns (8.55× slower) | ~2.80 ns (equivalent to manual) |
| Debuggability | Runtime dispatch — hard to step through | .g.cs file — place breakpoints |
| Startup cost | Expression compilation on startup | Zero |
| Unmapped members | Warning (configurable) | Error by default |
| Null handling | Runtime exception or default | Compile-time error (ELM004) |
| Private constructor bypass | `GetUninitializedObject` | Error — use [MapFactory] |

**The conclusion is not that AutoMapper is bad.** AutoMapper was designed before Native AOT existed, before source generators were available, and before the .NET ecosystem had the tooling to make compile-time mapping practical. AutoMapper made the correct architectural decisions for its era.

EricksonLopez.Mapper makes the correct architectural decisions for 2026+:

> "If the mapping cannot be expressed at compile time, it is an error. Not a warning. Not a runtime fallback. An error."

---

## SECTION H: WHY NOT MAPPERLY?

Mapperly (Riok.Mapperly) is the most technically comparable library. It is AOT-compatible, uses `IIncrementalGenerator`, and generates equivalent handwritten code. We acknowledge this openly.

**Feature Comparison vs. Mapperly:**

| Capability | Mapperly | EricksonLopez.Mapper | Status |
|---|---|---|---|
| Enum → Enum | Automatic by name | Automatic by name + numeric cast (T010) | ✅ Implemented |
| int → Enum | Supported | Supported via explicit cast (T011) | ✅ Implemented |
| Null propagation nested | `source?.Address` | Safe ternary `source.X != null ? Map(source.X) : null` (T003) | ✅ Implemented |
| Code fixes coverage | Extensive | Complete: ELM001, ELM003, ELM004, ELM007, ELM012 (T020–T022) | ✅ Implemented |
| Value Object auto-wrap/unwrap | Manual converter required | Automatic heuristic + `[ValueObject]` | ✅ Superior |
| Domain Factory (`[MapFactory]`) | Not supported | Native, first-class support | ✅ Superior |
| Strict mode default | Warning-permissive | Error by default (`ELM001`) | ✅ Superior |
| Nullability policy | Warning (configurable) | Error (`ELM004`) — always | ✅ Superior |
| DI without extra package | Separate config | `[GenerateMapperRegistration]` built-in | ✅ Superior |
| Diagnostic philosophy | "Let the project decide" | "Fail the build" (AOT-first) | ✅ Superior |

**Our honest competitive position:**

> EricksonLopez.Mapper offers complete parity in core mapping, enum conversions, null propagation and IDE code fixes, combined with a superior architecture for DDD projects that prioritize compile-time correctness, encapsulation and zero-reflection Native AOT execution.
