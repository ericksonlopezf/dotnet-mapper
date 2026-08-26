# Migration Guide: Moving from AutoMapper to EricksonLopez.Mapper

Step-by-step guide for migrating from reflection-heavy AutoMapper profiles to zero-reflection compile-time mapper declarations.

---

## Key Differences

| Concept | AutoMapper | EricksonLopez.Mapper |
|---|---|---|
| Execution Model | Dynamic reflection / Runtime IL emit | Compile-time incremental Roslyn generation |
| Configuration | `Profile.CreateMap<T1, T2>()` | `[Mapper] public static partial class MyMapper` |
| Native AOT | ❌ Incompatible / requires complex hints | ✅ 100% Native AOT & Trimming Compliant |
| Compile Safety | Runtime exceptions on startup/execution | Compile-time Roslyn diagnostics (`ELM001`–`ELM016`) |
