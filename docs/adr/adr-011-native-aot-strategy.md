# ADR-011: Native AOT Strategy

**Status**: Accepted
**Date**: 2026-08-13
**Deciders**: EricksonLopez.Mapper Architecture Team

## Context

.NET NativeAOT compilation removes the JIT and requires the entire application graph to be statically analyzable at publish time. Reflection-based code (`Activator.CreateInstance`, `Type.GetMethod`, `Expression.Compile`) causes the ILC (Ahead-of-Time compiler) to emit `IL2026`/`IL3050` warnings and may produce runtime crashes in trimmed builds.

## Decision

The library is **AOT-safe by design**. All mapping logic is moved to compile time via source generation. The runtime execution is entirely static C# assignments — no dynamic code path exists.

AOT safety is enforced as a hard CI gate via the `aot-smoke-test.yml` workflow:

```bash
dotnet publish tests/EricksonLopez.Mapper.AotSmokeTest -c Release \
  -p:PublishAot=true -p:TreatWarningsAsErrors=true
```

With `DOTNET_EnableAotCompilationWarningsAsErrors=true` set, any `IL2026` or `IL3050` warning from any code path reachable from the mapper fails the CI build.

`Directory.Build.props` sets `<IsAotCompatible>true</IsAotCompatible>` on all `src/` projects, enabling the Roslyn analyzer that flags AOT incompatibilities at development time.

## Alternatives Considered

- **Claiming "AOT Compatible" without a CI gate:** Rejected — AOT compatibility silently regresses without a compile-time check.
- **Providing a "reflection fallback mode":** Rejected — violates the Zero-Reflection principle (ADR-001) and fragments the API surface.

## Consequences

### Positive
- Consumers can publish with `PublishAot=true` without any workarounds or `[RequiresUnreferencedCode]` suppressions.
- CI gate prevents AOT regressions from slipping into releases.

### Negative
- Runtime configuration APIs (e.g., `Mapper.CreateMap()` at runtime) cannot be offered.
