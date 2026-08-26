# Native AOT Compatibility & Trimming Safety

## 1. Native AOT Invariants
`EricksonLopez.Mapper` guarantees 100% Native AOT compatibility across all emitted code:
- **No Dynamic IL Generation**: Does not emit `Reflection.Emit` or use Expression Compilation.
- **No Unreferenced Code**: All types and members are referenced directly in generated syntax trees, preventing IL trimming from stripping required properties.
- **Single-File Publishing**: Compiles cleanly into self-contained single executables.
