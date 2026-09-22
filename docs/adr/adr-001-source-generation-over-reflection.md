# ADR-001: Source Generation over Runtime Reflection

## Status
Accepted

## Date
2026-08-13

**Status**: Accepted  
**Date**: 2026-08-13  
**Deciders**: EricksonLopez.Mapper Architecture Team

## Context

Object-to-object mapping can be implemented via:
- (A) **Runtime reflection** — `PropertyInfo.GetValue`, `Activator.CreateInstance`, Expression trees compiled at runtime (AutoMapper approach)
- (B) **Source Generation** — Roslyn `IIncrementalGenerator` generates explicit mapping code at compile time (Mapperly approach)

The .NET ecosystem is undergoing a structural transformation: Native AOT is now a first-class requirement for cloud-native microservices and high-performance applications.

## Decision

**Source Generation exclusively** via `IIncrementalGenerator`. Zero reflection in any runtime path.

The generator runs at compile time using Roslyn APIs and emits `.g.cs` files containing explicit, hand-written-equivalent mapping code. The generator itself may use all Roslyn APIs (build-time only). The emitted code must not contain any reflection usage.

## Consequences

### Positive
- **Native AOT compatible** — zero `RequiresDynamicCode` or `RequiresUnreferencedCode` in generated code
- **Zero overhead** — generated code is equivalent to manual mapping, no dictionary lookups or delegate invocations at runtime
- **Compile-time errors** — mapping failures are diagnosed at build time, never at production runtime
- **IntelliSense** — Go-to-Definition navigates to the generated `.g.cs` file

### Negative
- Dynamic types are not supportable (by design)
- Requires Roslyn expertise for generator maintenance
- Generator complexity is higher than a runtime implementation

## Alternatives Considered

- Runtime reflection (AutoMapper approach): Rejected. AOT incompatible by architecture.
- IL Emit / DynamicMethod: Rejected. AOT incompatible.
- Expression Trees compiled at runtime: Rejected. AOT incompatible.
