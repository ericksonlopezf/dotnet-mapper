---
name: Feature Request
about: Suggest a new feature or improvement
title: "[FEATURE] "
labels: enhancement
assignees: ''
---

## Problem Statement

A clear and concise description of what the problem is. Ex. I'm always frustrated when [...]

## Affected Package(s)

- [ ] `EricksonLopez.Mapper` (umbrella package)
- [ ] `EricksonLopez.Mapper.Abstractions` (new attribute or interface contract)
- [ ] `EricksonLopez.Mapper.Generator` (generator behavior change)
- [ ] `EricksonLopez.Mapper.Analyzers` (new diagnostic or code fix)
- [ ] `EricksonLopez.Mapper.DomainPrimitives` (DomainPrimitives integration)
- [ ] `EricksonLopez.Mapper.Mapster` (Mapster bridge)
- [ ] `EricksonLopez.Mapper.Result` (Result monad extensions)
- [ ] New package (describe below)

## Proposed Solution

A clear and concise description of what you want to happen. Include an example of the proposed API if applicable:

```csharp
[Mapper]
public partial class ExampleMapper
{
    // ...
}
```

## Non-Goals Check

Please review the [Architecture Decision Records](https://github.com/ericksonlopezf/dotnet-mapper/tree/main/docs/adr) before submitting. The following features are permanently excluded by design decision:
- Field mapping (ADR-D01)
- Private member access bypass (ADR-D02)
- Automatic flattening (ADR-D03)
- Circular mapping (ADR-D04)
- Existing-instance mapping (ADR-D05)
- Reverse/bidirectional mapping (ADR-D06)
- Convention-based naming (ADR-D07)
- Global converter registry (ADR-D08)
- Conditional mapping (ADR-D09)
- Before/after mapping hooks (ADR-D10)
- IQueryable projection (ADR-D11)
- Generic IMapper<T> interface (ADR-D12)

## Alternatives Considered

A clear and concise description of any alternative solutions or features you've considered.

## Additional Context

Add any other context, mockups, code samples, or references about the feature request here.
