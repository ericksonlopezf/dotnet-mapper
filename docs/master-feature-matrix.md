# Master Feature Matrix — EricksonLopez.Mapper

> **Status Key**: Implemented | Partial | Broken | Documented Only | Missing | Deprecated | Rejected  
> **Classification Key**: CORE | STRATEGIC | SUPPORTING | OPTIONAL | ADAPTER | EXPERIMENTAL | OUT-OF-SCOPE | REJECTED  
> **AOT Key**: ✅ Full | ⚠️ Risk | ❌ Incompatible  
> **Reflection Key**: ✅ None | ⚠️ Build-time Only | ❌ Runtime  
> **Allocations Key**: ✅ Zero-extra | ⚠️ Minimal | ❌ Significant  

---

## CORE MAPPING

| Domain | Feature | Status | Classification | Priority | Package | AOT | Reflection | Allocations | Complexity | Competitive Value | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Core | Property → Property (exact, public) | Implemented | CORE | P0 | Abstractions + Generator | ✅ | ✅ | ✅ | Low | Table Stakes | Keep |
| Core | Property → Property (case-insensitive fallback) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Table Stakes | Keep |
| Core | Field → Field / Field → Property / Property → Field | Rejected | REJECTED | N/A | N/A | N/A | N/A | N/A | N/A | Low (ADR-D01) | Permanent Reject |
| Core | Nested Object mapping (via sub-method delegation) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep |
| Core | Null nested object — null guard on source | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Core | Null nested object → null destination (null-propagation) | Implemented | STRATEGIC | P0 | Generator | ✅ | ✅ | ✅ | Low | High (Parity) | Keep |
| Core | Nullable → Non-nullable (ELM004 + [MapNullFallback]) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Core | Non-nullable → Nullable | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Core | Numeric widening (byte→int, int→long, etc.) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Table Stakes | Keep |
| Core | Numeric narrowing (long→int, double→float) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep (cast + ELM015 warning) |
| Core | string → string | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | None | Table Stakes | Keep |
| Core | Guid → string | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Core | string → Guid | Implemented | CORE | P0 | Generator | ✅ | ⚠️ | ✅ | Low | Medium | Keep (ELM016 warning) |
| Core | DateTime → DateOnly | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Core | DateTime → DateTimeOffset | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Core | DateOnly → DateTime | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Core | DateTimeOffset → DateTime | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Core | Enum → string | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Core | string → Enum | Implemented | CORE | P0 | Generator | ⚠️ | ✅ | ✅ | Low | Medium | Keep + ELM016 warning |
| Core | Enum → Enum (by member name) | Implemented | STRATEGIC | P0 | Generator | ✅ | ✅ | ✅ | Medium | High (Parity) | Keep — ADR-016 |
| Core | Enum → int | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Core | int → Enum | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Core | Nullable Enum | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Core | Explicit cast operators | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Core | Implicit cast operators | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Core | Custom IConverter<TSource,TDest> | Implemented | STRATEGIC | P0 | Abstractions + Generator | ✅ | ✅ | ⚠️ | Low | High | Keep (ELM013 validation) |
| Core | IConverter with DI constructor | Implemented | SUPPORTING | P1 | Abstractions + Generator | ✅ | ✅ | ✅ | Medium | High | Keep — ADR-019 ([UseConverter(fieldName)]) |

---

## OBJECT CONSTRUCTION

| Domain | Feature | Status | Classification | Priority | Package | AOT | Reflection | Allocations | Complexity | Competitive Value | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Construction | Object Initializer (parameterless ctor + setters) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Table Stakes | Keep |
| Construction | init-only properties | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Construction | Parameterized constructor (single) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep |
| Construction | Parameterized constructor (ambiguous → ELM007) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep |
| Construction | Static factory method [MapFactory] | Implemented | STRATEGIC | P0 | Abstractions + Generator | ✅ | ✅ | ✅ | Medium | High (unique diff.) | Keep |
| Construction | Record (positional parameters) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep |
| Construction | record struct | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep |
| Construction | readonly struct | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep |
| Construction | required properties | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Construction | private setters | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep (via ctor/factory) |
| Construction | FormatterServices.GetUninitializedObject | Rejected | REJECTED | N/A | N/A | ❌ | ❌ | ❌ | N/A | N/A | Permanent Reject |
| Construction | Activator.CreateInstance | Rejected | REJECTED | N/A | N/A | ❌ | ❌ | N/A | N/A | N/A | Permanent Reject |

---

## COLLECTIONS

| Domain | Feature | Status | Classification | Priority | Package | AOT | Reflection | Allocations | Complexity | Competitive Value | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Collections | T[] → T[] | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Table Stakes | Keep |
| Collections | List<T> → List<T> | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Table Stakes | Keep |
| Collections | IEnumerable<T> → List<T> | Implemented | CORE | P0 | Generator | ✅ | ✅ | ⚠️ | Medium | Table Stakes | Keep |
| Collections | T[] → List<T> / List<T> → T[] | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Collections | IReadOnlyList<T> target | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep (emits List<T>) |
| Collections | IReadOnlyCollection<T> target | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep (emits List<T>) |
| Collections | IList<T> / ICollection<T> targets | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep (emits List<T>) |
| Collections | ImmutableArray<T> | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep |
| Collections | ImmutableList<T> | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Collections | HashSet<T> | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Collections | Dictionary<K,V> | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ⚠️ | Low | Medium | Keep (add capacity) |
| Collections | IDictionary / IReadOnlyDictionary targets | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Collections | FrozenSet<T> (.NET 8+) | Implemented | OPTIONAL | P2 | Generator | ✅ | ✅ | ✅ | Low | Low | Keep |
| Collections | ReadOnlySpan<T> source | Implemented | OPTIONAL | P2 | Generator | ✅ | ✅ | ✅ | High | Medium | Keep (ADR-006) |
| Collections | Memory<T> source | Missing | OPTIONAL | P3 | Generator | ✅ | ✅ | ✅ | High | Low | Defer |
| Collections | ArrayPool<T> in generated code | Missing | OPTIONAL | P3 | Generator | ✅ | ✅ | ✅ | High | Low | Defer — only for large arrays |
| Collections | LINQ .Select().ToList() | Rejected | REJECTED | N/A | N/A | ✅ | ✅ | ❌ | N/A | N/A | Permanent Reject (ADR-006) |
| Collections | Capacity pre-allocation (List with Count) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Collections | TryGetNonEnumeratedCount for IEnumerable | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |

---

## MEMBER RESOLUTION / RENAMING / IGNORING

| Domain | Feature | Status | Classification | Priority | Package | AOT | Reflection | Allocations | Complexity | Competitive Value | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Resolution | Exact name match | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Table Stakes | Keep |
| Resolution | Case-insensitive fallback | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | Table Stakes | Keep |
| Resolution | [MapProperty] rename source → dest | Implemented | CORE | P0 | Abstractions + Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Resolution | [MapIgnore] exclude destination member | Implemented | CORE | P0 | Abstractions + Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Resolution | [MapIgnoreSource] exclude source member | Implemented | SUPPORTING | P1 | Abstractions + Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Resolution | nameof() in attributes | Implemented | CORE | P0 | Compiler | ✅ | ✅ | ✅ | None | High | Keep (compiler feature) |
| Resolution | snake_case / camelCase / prefix/suffix conventions | Rejected | REJECTED | N/A | N/A | N/A | N/A | N/A | N/A | Low (ADR-D07) | Permanent Reject |
| Resolution | Fluent mapping configuration | Rejected | REJECTED | N/A | N/A | N/A | N/A | N/A | N/A | Low (ADR-003) | Permanent Reject (runtime) |
| Resolution | Strict mode (ELM001 for unmapped) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep as default=true |
| Resolution | Permissive mode (StrictMapping=false) | Implemented | CORE | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |

---

## DDD / VALUE OBJECTS / STRONGLY TYPED IDs

| Domain | Feature | Status | Classification | Priority | Package | AOT | Reflection | Allocations | Complexity | Competitive Value | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| DDD | Value Object wrap (Guid → UserId) | Implemented | STRATEGIC | P0 | Generator | ✅ | ✅ | ✅ | Medium | High (unique diff.) | Keep |
| DDD | Value Object unwrap (UserId → Guid) | Implemented | STRATEGIC | P0 | Generator | ✅ | ✅ | ✅ | Medium | High (unique diff.) | Keep |
| DDD | [ValueObject] attribute explicit marker | Implemented | STRATEGIC | P0 | Abstractions | ✅ | ✅ | ✅ | Low | High | Keep |
| DDD | Heuristic VO detection (readonly record struct + Value prop) | Implemented | STRATEGIC | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep |
| DDD | [MapFactory] — Domain factory method | Implemented | STRATEGIC | P0 | Abstractions + Generator | ✅ | ✅ | ✅ | Medium | High (unique diff.) | Keep |
| DDD | Private constructor enforcement (ELM002) | Implemented | STRATEGIC | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| DDD | Private setter bypass | Rejected | REJECTED | N/A | N/A | N/A | N/A | N/A | N/A | N/A | Permanent Reject (ADR-D02) |
| DDD | Domain invariants via factory | Implemented | STRATEGIC | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| DDD | AggregateRoot / Entity awareness | Rejected | OUT-OF-SCOPE | N/A | N/A | N/A | N/A | N/A | N/A | Low | Mapper stays agnostic |
| DDD | Result<T> / Option<T> structural awareness | Rejected | OUT-OF-SCOPE | N/A | N/A | N/A | N/A | N/A | N/A | Low (ADR-000) | Permanent Reject |

---

## POLYMORPHISM

| Domain | Feature | Status | Classification | Priority | Package | AOT | Reflection | Allocations | Complexity | Competitive Value | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Polymorphism | [MapDerivedType] switch-expression dispatch | Implemented | STRATEGIC | P0 | Abstractions + Generator | ✅ | ✅ | ✅ | Medium | High | Keep |
| Polymorphism | ELM011 warning for abstract base | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep (expand coverage) |
| Polymorphism | Interface source polymorphism | Missing | SUPPORTING | P2 | Generator | ✅ | ✅ | ✅ | High | Medium | Defer |
| Polymorphism | Runtime discriminator-based dispatch | Rejected | REJECTED | N/A | N/A | N/A | ❌ | N/A | N/A | N/A | Permanent Reject (runtime) |

---

## DIAGNOSTICS

| Domain | Feature | Status | Classification | Priority | Package | AOT | Reflection | Allocations | Complexity | Competitive Value | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Diagnostics | ELM001 — Unmapped destination member | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Diagnostics | ELM002 — Missing factory or constructor | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Diagnostics | ELM003 — Unsupported conversion | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Diagnostics | ELM004 — Nullability mismatch | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Diagnostics | ELM005 — Ambiguous property match | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Diagnostics | ELM006 — Missing constructor mapping | Implemented | CORE | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep (clarify vs ELM002) |
| Diagnostics | ELM007 — Ambiguous constructor | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Diagnostics | ELM008 — Reflection in mapper (Analyzer) | Implemented | STRATEGIC | P0 | Analyzers | ✅ | ✅ | ✅ | Medium | High | Keep (expanded coverage) |
| Diagnostics | ELM009 — dynamic keyword (Analyzer) | Implemented | STRATEGIC | P0 | Analyzers | ✅ | ✅ | ✅ | Medium | High | Keep |
| Diagnostics | ELM010 — Circular mapping reference | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep (reports all cycles) |
| Diagnostics | ELM011 — Incomplete polymorphism (Warning) | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Diagnostics | ELM012 — Mapper must be partial (Analyzer) | Implemented | CORE | P0 | Analyzers | ✅ | ✅ | ✅ | Low | High | Keep |
| Diagnostics | ELM013 — Invalid converter type (Generator) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep (unique ID) |
| Diagnostics | ELM014 — Enum source value has no dest match | Implemented | STRATEGIC | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep (strict/permissive) |
| Diagnostics | ELM015 — Narrowing conversion warning | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Diagnostics | ELM016 — string→Enum potential runtime exception | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Diagnostics | Code fix for ELM001 (Add MapIgnore/MapProperty) | Implemented | SUPPORTING | P1 | Analyzers | ✅ | ✅ | ✅ | Medium | High | Keep |
| Diagnostics | Code fix for ELM004 (Add MapNullFallback) | Implemented | SUPPORTING | P1 | Analyzers | ✅ | ✅ | ✅ | Medium | High | Keep |
| Diagnostics | Code fix for ELM007 (Add MapFactory) | Implemented | SUPPORTING | P1 | Analyzers | ✅ | ✅ | ✅ | Medium | High | Keep |
| Diagnostics | Code fix for ELM012 — Make partial | Implemented | SUPPORTING | P0 | Analyzers | ✅ | ✅ | ✅ | Low | High | Keep |
| Diagnostics | Code fix for MapIgnore (MapIgnoreCodeFixProvider) | Implemented | SUPPORTING | P0 | Analyzers | ✅ | ✅ | ✅ | Low | High | Keep |

---

## SOURCE GENERATOR

| Domain | Feature | Status | Classification | Priority | Package | AOT | Reflection | Allocations | Complexity | Competitive Value | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Generator | IIncrementalGenerator | Implemented | CORE | P0 | Generator | ✅ | ⚠️ | ✅ | High | Table Stakes | Keep |
| Generator | ForAttributeWithMetadataName (mapper detection) | Implemented | CORE | P0 | Generator | ✅ | ⚠️ | ✅ | Low | High | Keep |
| Generator | ForAttributeWithMetadataName (DI detection) | Implemented | CORE | P1 | Generator | ✅ | ⚠️ | ✅ | Low | Medium | Keep |
| Generator | EquatableArray<T> for incremental caching | Implemented | CORE | P0 | Generator | ✅ | ⚠️ | ✅ | High | High | Keep |
| Generator | Deterministic output (OrderBy members) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Generator | Snapshot tests (Verify.SourceGenerators) | Implemented | CORE | P0 | Generator.Tests | ✅ | ✅ | ✅ | Medium | High | Keep |
| Generator | MapperGenerator.cs modular architecture | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep (refactored into 7 modules) |
| Generator | No LINQ in emitted code | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep |
| Generator | #nullable enable in generated files | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Generator | global:: prefix consistency | Implemented | CORE | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep |
| Generator | Readable variable names in generated code | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Generator | generator never throws exceptions | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep |

---

## DEPENDENCY INJECTION

| Domain | Feature | Status | Classification | Priority | Package | AOT | Reflection | Allocations | Complexity | Competitive Value | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| DI | [GenerateMapperRegistration] assembly attribute | Implemented | STRATEGIC | P1 | Abstractions | ✅ | ✅ | ✅ | Low | High | Keep |
| DI | Emits AddGeneratedMappers() extension method | Implemented | STRATEGIC | P1 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep |
| DI | Registers only non-static mappers as Singleton | Implemented | STRATEGIC | P1 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| DI | Core package zero-dependency on M.E.DI | Implemented | CORE | P0 | Mapper | ✅ | ✅ | ✅ | Low | High | Keep |
| DI | Static mapper support (no DI needed) | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| DI | IMapper<T> generic interface | Rejected | REJECTED | N/A | N/A | N/A | N/A | N/A | N/A | Low (ADR-D12) | Permanent Reject |
| DI | Assembly scanning for DI | Rejected | REJECTED | N/A | N/A | N/A | ❌ | N/A | N/A | Low | Permanent Reject |

---

## AOT / NATIVE AOT / TRIMMING

| Domain | Feature | Status | Classification | Priority | Package | AOT | Reflection | Allocations | Complexity | Competitive Value | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| AOT | IsAotCompatible=true in all packages | Implemented | CORE | P0 | All | ✅ | ✅ | ✅ | None | High | Keep |
| AOT | Zero RequiresDynamicCode in surface | Implemented | CORE | P0 | All | ✅ | ✅ | ✅ | Low | High | Keep |
| AOT | Zero RequiresUnreferencedCode | Implemented | CORE | P0 | All | ✅ | ✅ | ✅ | Low | High | Keep |
| AOT | AOT smoke test project (PublishAot=true) | Implemented | CORE | P0 | AotTest | ✅ | ✅ | ✅ | Low | High | Keep |
| AOT | ELM008 — reflection enforcement | Implemented | STRATEGIC | P0 | Analyzers | ✅ | ✅ | ✅ | Medium | High | Keep (expanded coverage) |
| AOT | ELM009 — dynamic enforcement | Implemented | STRATEGIC | P0 | Analyzers | ✅ | ✅ | ✅ | Medium | High | Keep |
| AOT | Enum.Parse<T> risk (Enum.TryParse / ELM016) | Implemented | CORE | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep (ELM016 warning emitted) |
| AOT | Marshal / RuntimeHelpers detection | Implemented | STRATEGIC | P1 | Analyzers | ✅ | ✅ | ✅ | Low | High | Keep (in ELM008) |
| AOT | FormatterServices detection | Implemented | STRATEGIC | P1 | Analyzers | ✅ | ✅ | ✅ | Low | High | Keep (in ELM008) |
| AOT | Runtime reflection fallback | Rejected | REJECTED | N/A | N/A | ❌ | ❌ | N/A | N/A | N/A | Permanent Reject (ADR-001) |

---

## PERFORMANCE

| Domain | Feature | Status | Classification | Priority | Package | AOT | Reflection | Allocations | Complexity | Competitive Value | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Performance | Generated code == handwritten assignment | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | High | High | Keep |
| Performance | Zero extra allocations in hot path | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Medium | High | Keep |
| Performance | Collection capacity pre-sizing | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep |
| Performance | Dictionary capacity pre-sizing | Implemented | CORE | P1 | Generator | ✅ | ✅ | ✅ | Low | Medium | Keep (implemented with SourceHasCount) |
| Performance | IConverter instance-per-call | Implemented | SUPPORTING | P1 | Generator | ✅ | ✅ | ⚠️ | Low | Low | Document; offer instance pattern |
| Performance | BenchmarkDotNet benchmarks exist | Implemented | CORE | P0 | Benchmarks | ✅ | ✅ | ✅ | Low | High | Keep |
| Performance | Benchmark regression gates in CI | Implemented | STRATEGIC | P1 | CI | ✅ | ✅ | ✅ | Medium | High | Keep (.github/workflows/benchmark-regression-gate.yml) |
| Performance | LINQ in generated code | Rejected | REJECTED | N/A | N/A | N/A | N/A | ❌ | N/A | N/A | Permanent Reject (ADR-007) |
| Performance | Closure capture in generated code | Rejected | REJECTED | N/A | N/A | N/A | N/A | ❌ | N/A | N/A | Permanent Reject (ADR-007) |
| Performance | Boxing of value types in mapping | Implemented | CORE | P0 | Generator | ✅ | ✅ | ✅ | Low | High | Keep avoiding (verified) |

---

## PACKAGE ARCHITECTURE

| Domain | Feature | Status | Classification | Priority | Package | AOT | Reflection | Allocations | Complexity | Competitive Value | Decision |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Packages | EricksonLopez.Mapper (umbrella) | Implemented | CORE | P0 | Mapper | ✅ | ✅ | ✅ | Low | High | Keep |
| Packages | EricksonLopez.Mapper.Abstractions | Implemented | CORE | P0 | Abstractions | ✅ | ✅ | ✅ | Low | High | Keep |
| Packages | EricksonLopez.Mapper.Generator | Implemented | CORE | P0 | Generator | ✅ | ⚠️ | ✅ | High | High | Keep (refactor internals) |
| Packages | EricksonLopez.Mapper.Analyzers | Implemented | STRATEGIC | P0 | Analyzers | ✅ | ✅ | ✅ | Medium | High | Keep (expand code fixes) |
| Packages | EricksonLopez.Mapper.Testing | Missing | OPTIONAL | P3 | Testing | ✅ | ✅ | ✅ | High | Medium | Defer |
| Packages | EricksonLopez.Mapper.Dapper | Missing | ADAPTER | P3 | Dapper | ✅ | ✅ | ✅ | High | Low | Only if demand proven |
| Packages | EricksonLopez.Mapper.Projection | Missing | REJECTED | N/A | N/A | N/A | N/A | N/A | N/A | Low (ADR-D11) | Permanent Reject |
| Packages | EricksonLopez.Mapper.DependencyInjection | Missing | OPTIONAL | P3 | DI | ✅ | ✅ | ✅ | Low | Low | Current integration sufficient |

---

## REJECTED FEATURES (Non-Goals)

| Domain | Feature | Status | Classification | Rejection ADR | Reason |
|---|---|---|---|---|---|
| Core | Runtime reflection fallback | Rejected | REJECTED | ADR-001, ADR-007 | AOT incompatible |
| DI | IMapper<T> generic interface | Rejected | REJECTED | ADR-D12 | Hides type; breaks AOT |
| Projection | IQueryable / Expression<Func<T,T>> | Rejected | REJECTED | ADR-D11 | Out of scope |
| Config | Global converter registry | Rejected | REJECTED | ADR-D08 | Runtime state; DI-hidden |
| Config | Naming conventions (snake_case, prefixes) | Rejected | REJECTED | ADR-D07 | Magic; brittle |
| Mapping | Existing instance update mapping | Rejected | REJECTED | ADR-D05 | Breaks immutability |
| Mapping | Reverse/bidirectional automatic | Rejected | REJECTED | ADR-D06 | Asymmetric semantics |
| Mapping | Circular reference mapping | Rejected | REJECTED | ADR-D04 | Unsolvable in compile-time |
| Mapping | Before/After hooks | Rejected | REJECTED | ADR-D10 | Side effects in mapper |
| Mapping | Deep path flattening (auto) | Rejected | REJECTED | ADR-D03 | Implicit, brittle |
| Mapping | Private member bypass | Rejected | REJECTED | ADR-D02 | Breaks domain invariants |
| Mapping | Field mapping | Rejected | REJECTED | ADR-D01 | DTO design enforcement |
| Config | Fluent configuration API | Rejected | REJECTED | ADR-003 | Inherently runtime |
| Config | Assembly scanning for DI | Rejected | REJECTED | ADR-007 | Runtime reflection |
| Data | IDataReader mapping (core) | Rejected | OUT-OF-SCOPE | ADR-000 | Separate domain |
| Domain | Result<T>/Option<T> structural awareness | Rejected | OUT-OF-SCOPE | ADR-000 | Library coupling |
| ORM | EF Core / IQueryable integration | Rejected | REJECTED | ADR-D11 | Out of scope |
| Pattern | Change tracker / lazy loading | Rejected | REJECTED | N/A | ORM responsibility |
| Pattern | Repository / UnitOfWork | Rejected | REJECTED | N/A | Domain responsibility |
| Pattern | Validation framework | Rejected | REJECTED | N/A | Domain responsibility |
