# ADR-012: Assembly Trimming Strategy

## Status
Accepted

## Date
2026-08-13

**Status**: Accepted
**Date**: 2026-08-13
**Deciders**: EricksonLopez.Mapper Architecture Team

## Context

.NET Trimming removes unused code to reduce application size. Libraries that access types dynamically via reflection require `[DynamicDependency]` annotations or linker descriptor files to prevent the trimmer from removing properties that are accessed by name at runtime — leading to `MissingMethodException` in trimmed builds.

## Decision

Because the generator emits **direct, statically-typed property accesses** (`dto.Name = source.Name;`), the .NET trimmer's static analyzer understands that these members are in use and preserves them automatically — no `[DynamicDependency]` annotations are required.

`Directory.Build.props` sets `<IsTrimmable>true</IsTrimmable>` and `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>` on all `src/` library projects, activating the Roslyn trim-compatibility analyzer at development time.

## Alternatives Considered

- **Using `[DynamicDependency]` to preserve members:** Unnecessary since the code is statically bound. Using it anyway would bloat the package and signal incorrect intent. Rejected.
- **Linker descriptor XML files:** Obsolete approach; superseded by the `[DynamicDependency]` attribute and, in our case, entirely unnecessary. Rejected.

## Consequences

### Positive
- Trimming works flawlessly without any consumer-side workarounds.
- No trimmer warnings when consumed in a trimming-enabled application.

### Negative
- None. Trimming compatibility is a free benefit of the source-generator architecture.
