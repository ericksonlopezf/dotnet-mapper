# NuGet Packages Summary

This document provides a concise summary and installation guide for the 7 NuGet packages produced by the `EricksonLopez.Mapper` repository.

For the comprehensive packaging and CPM reference, see [packages.md](packages.md).

---

## 1. Package Catalog & Installation

```bash
# Umbrella package (recommended for most applications)
dotnet add package EricksonLopez.Mapper --version 2.0.0

# Optional Domain Primitives extension
dotnet add package EricksonLopez.Mapper.DomainPrimitives --version 2.0.0

# Optional Result monad extension
dotnet add package EricksonLopez.Mapper.Result --version 2.0.0

# Optional Mapster bridge extension
dotnet add package EricksonLopez.Mapper.Mapster --version 2.0.0
```

| Package Name | Target Framework(s) | Role |
|---|---|---|
| **`EricksonLopez.Mapper`** | `net8.0`, `net9.0`, `net10.0` | Umbrella package referencing Abstractions and Generator. |
| **`EricksonLopez.Mapper.Abstractions`** | `netstandard2.0`, `net8.0`, `net9.0`, `net10.0` | Mapping attributes and `IConverter<TSource, TDestination>` contracts. |
| **`EricksonLopez.Mapper.Generator`** | `netstandard2.0` | Roslyn incremental source generator analyzer. |
| **`EricksonLopez.Mapper.Analyzers`** | `netstandard2.0` | Roslyn diagnostic analyzer and code fix providers. |
| **`EricksonLopez.Mapper.DomainPrimitives`** | `net8.0`, `net9.0`, `net10.0` | Value object & strongly typed ID converters. |
| **`EricksonLopez.Mapper.Mapster`** | `net8.0`, `net9.0`, `net10.0` | Bi-directional bridge with Mapster `TypeAdapterConfig`. |
| **`EricksonLopez.Mapper.Result`** | `net8.0`, `net9.0`, `net10.0` | Functional Railway-Oriented Programming projection extensions. |

---

## 2. Feature & Runtime Support Matrix

| Feature | Supported | Notes |
|---|---|---|
| .NET 8.0 / 9.0 / 10.0 | ✅ Yes | Fully supported and tested in CI across all runtime libraries. |
| Native AOT Compilation | ✅ Yes | Enforced via `aot-smoke-test.yml` with `PublishAot=true`. |
| Assembly IL Trimming | ✅ Yes | Zero `IL2026` or `IL3050` trim warnings. |
| C# Records & Primary Ctors | ✅ Yes | Parameterized constructor mapping matches parameter names. |
| Value Objects & Strongly Typed IDs | ✅ Yes | Direct wrap/unwrap support via `[ValueObject]`. |
| Modern Immutable Collections | ✅ Yes | `ImmutableArray<T>`, `ImmutableList<T>`, `FrozenSet<T>`, `FrozenDictionary<K,V>`. |
| Cross-Platform Execution | ✅ Yes | Linux, macOS, Windows (X64, ARM64). |
