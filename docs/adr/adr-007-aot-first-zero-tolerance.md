# ADR-007: AOT-First, Zero Tolerance

**Status**: Accepted  
**Date**: 2026-08-13

## Context

Native AOT compatibility can be achieved at different levels:
- "We compile with AOT" (no guarantees)
- "We suppress AOT warnings" (technically broken but silently so)
- "We guarantee zero `RequiresDynamicCode` and `RequiresUnreferencedCode` in all paths"

## Decision

**Zero tolerance**. EricksonLopez.Mapper guarantees zero AOT warnings in both the runtime path and the generated code. This is a core architectural invariant, not a best-effort goal.

### Forbidden in core runtime:
```
System.Reflection.*
Activator.CreateInstance() / CreateInstance(Type)
Assembly.GetTypes() / GetExportedTypes()
Type.GetType(string)
MethodInfo.Invoke()
PropertyInfo.GetValue() / SetValue()
Expression.Compile()
DynamicMethod / ILGenerator / RuntimeMethodHandle
dynamic keyword / ExpandoObject
FormatterServices.GetUninitializedObject()
MakeGenericType() / MakeGenericMethod()
DynamicInvoke()
ConcurrentDictionary<Type, ...> for mapping resolution
```

### Forbidden in generated code:
```
LINQ lazy pipeline patterns: .Select().ToList(), .Where().Select(), etc.
  (These create intermediate IEnumerable<T> closures and allocate intermediary objects.)
Closure capture (lambdas in generated code)
Delegate allocation for mapping path
Boxing of value types (unless semantically required)
```

### Allowed in the generator (build-time only):
```
All Roslyn APIs (ITypeSymbol, SemanticModel, etc.)
IIncrementalGenerator pipeline
ForAttributeWithMetadataName
```

### Explicitly permitted in generated code (non-allocating System.Linq utilities):
```
Enumerable.TryGetNonEnumeratedCount(source, out int count)
  — A zero-allocation capacity-hint utility that attempts to retrieve the count from an
    ICollection<T> or IReadOnlyCollection<T> without enumeration. It does NOT create
    intermediate sequences, closures, or delegates. Permitted for pre-sizing collection
    builders to eliminate resize allocations.
```

### Enforced by:
- `ELM008` diagnostic: reflection usage in mapper class
- `ELM009` diagnostic: `dynamic` keyword usage in mapper class
- AOT test project (`aot-test/`) validated in CI with `PublishAot=true`

## Consequences

Features that are incompatible with this policy are rejected, even if popular:
- TypeConverter integration (rejected)
- Global converter registry (rejected)
- IMapper generic interface (rejected)
- Assembly scanning DI registration (rejected)

These rejections are each documented in their own ADR (ADR-D* series).
