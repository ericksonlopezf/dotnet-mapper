---
name: Bug Report
about: Report a bug to help us improve
title: "[BUG] "
labels: bug
assignees: ''
---

## Description

A clear and concise description of what the bug is.

## Affected Package(s)

- [ ] `EricksonLopez.Mapper` (umbrella package)
- [ ] `EricksonLopez.Mapper.Abstractions` (attributes, `IConverter<T,T>`)
- [ ] `EricksonLopez.Mapper.Generator` (Roslyn source generator)
- [ ] `EricksonLopez.Mapper.Analyzers` (diagnostics, code fixes)
- [ ] `EricksonLopez.Mapper.DomainPrimitives` (value object / strong ID converters)
- [ ] `EricksonLopez.Mapper.Mapster` (Mapster adapter bridge)
- [ ] `EricksonLopez.Mapper.Result` (functional Result monad extensions)

## Steps to Reproduce

1. ...
2. ...
3. See error

## Expected Behavior

A clear and concise description of what you expected to happen.

## Actual Behavior

A clear and concise description of what actually happened. Include the full compiler error or diagnostic message if applicable (e.g., ELM001, ELM003, ELM008, etc.).

## Minimal Reproduction

Please provide a minimal C# snippet or link to a repository that reproduces the issue:

```csharp
[Mapper]
public partial class ExampleMapper
{
    public partial DestinationDto Map(SourceEntity source);
}
```

## Environment

- **OS**: [e.g. Windows 11, macOS 15, Ubuntu 24.04]
- **Package Version**: [e.g. 1.0.0]
- **Target Framework**: [e.g. net8.0, net9.0, net10.0]
- **NativeAOT**: [Yes / No / Not applicable]
- **IDE**: [e.g. Visual Studio 2022 17.10, Rider 2024.1, VS Code]

## Additional Context

Add any other context about the problem here (logs, screenshots, generated code from `obj/Generated/`, etc.).
