# ADR-D12: IMapper Generic Interface Rejected

## Status
Rejected

## Date
2026-08-13

**Status**: Accepted  
**Date**: 2026-08-13

## Decision

A generic `IMapper` interface with `Map<TSource, TDestination>(source)` is **rejected**.

## Rationale

1. **Runtime type resolution** — `IMapper.Map<T, U>` requires a `Dictionary<(Type, Type), MappingDelegate>` resolved at runtime.
2. **AOT incompatibility** — `MakeGenericMethod` at runtime is not AOT-safe.
3. **IntelliSense degradation** — All types in the codebase gain access to `IMapper.Map<T, U>()`, polluting IntelliSense.
4. **Go-to-Definition failure** — Navigating the mapping takes you to the interface, not the generated implementation.

## Alternative: Static Mapper (Primary)

```csharp
[Mapper]
public static partial class UserMapper
{
    public static partial UserDto ToDto(User source);
}
// Usage:
var dto = UserMapper.ToDto(user); // Go-to-Definition works perfectly
```

## Alternative: Per-Mapper Interface (DI/Testing)

```csharp
[Mapper]
public partial class UserMapper : IUserMapper
{
    public partial UserDto ToDto(User source);
}
// The interface is domain-specific, not a generic IMapper
```

This approach provides mockability without the runtime overhead or IntelliSense pollution.
