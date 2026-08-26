# Ecosystem Capability & Feature Support Matrix

| Capability | EricksonLopez.Mapper | Riok.Mapperly | Mapster | AutoMapper |
|---|:---:|:---:|:---:|:---:|
| **Source Generator Compilation** | ✅ Yes (Roslyn Incremental) | ✅ Yes | ❌ No | ❌ No |
| **Native AOT 100% Trimming Safe** | ✅ Yes | ✅ Yes | ⚠️ Partial | ❌ No |
| **Zero Reflection in Production** | ✅ Yes | ✅ Yes | ❌ No | ❌ No |
| **0 B Allocation for Direct Maps** | ✅ Yes (0 B) | ✅ Yes (0 B) | ⚠️ 32 B | ❌ 128 B |
| **Compile-Time Missing Member Validation** | ✅ Yes (`ELM001`) | ✅ Yes | ❌ No | ❌ No (Runtime) |
| **Strongly-Typed Domain Primitives Bridge** | ✅ Yes (`EricksonLopez.Mapper.DomainPrimitives`) | ❌ No | ❌ No | ❌ No |
| **Functional Result Pattern Bridge** | ✅ Yes (`EricksonLopez.Mapper.Result`) | ❌ No | ❌ No | ❌ No |
| **Automated Roslyn Analyzer Code Fixes** | ✅ Yes (Visual Studio / Rider) | ⚠️ Partial | ❌ No | ❌ No |
| **Pre-Allocated Collection Projections** | ✅ Yes | ✅ Yes | ⚠️ LINQ | ⚠️ LINQ |
| **Mapster Migration Compatibility** | ✅ Yes (`EricksonLopez.Mapper.Mapster`) | ❌ No | N/A | ❌ No |
