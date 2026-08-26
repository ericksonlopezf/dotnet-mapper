# EricksonLopez.Mapper.AotSmokeTest — Native AOT and Trimming Validation

This project is an executable application (`<OutputType>Exe</OutputType>`) specifically designed as an **Autonomous Smoke Test for Native AOT (`PublishAot=true`) and Trimming (`IsTrimmable=true`)**.

---

## Why is this not a conventional xUnit test project?

1. **Trimming and ILLink Isolation**: Conventional test runners (xUnit, NUnit) rely heavily on dynamic reflection, assembly scanning, and runtime type loading. This reflection infrastructure interferes with the ILLink analyzer and the AOT compiler, causing false positives or suppressing real warnings that end consumers would encounter.
2. **Real Binary Validation**: When compiled as a standalone executable with `<PublishAot>true</PublishAot>` and `<StripSymbols>true</StripSymbols>`, the compiler generates a native binary (machine code) completely free of reflection. If any portion of `EricksonLopez.Mapper` were to generate AOT-incompatible code, compilation or execution of this executable will immediately fail.

---

## How to Execute Validation

### Standard Build and Execution
```bash
dotnet run --project tests/EricksonLopez.Mapper.AotSmokeTest/EricksonLopez.Mapper.AotSmokeTest.csproj
```

### Native AOT Publishing and Binary Validation
```bash
# Windows (x64)
dotnet publish tests/EricksonLopez.Mapper.AotSmokeTest/ -c Release -r win-x64

# Linux (x64)
dotnet publish tests/EricksonLopez.Mapper.AotSmokeTest/ -c Release -r linux-x64

# Direct execution of the generated native binary:
./tests/EricksonLopez.Mapper.AotSmokeTest/bin/Release/net8.0/win-x64/publish/EricksonLopez.Mapper.AotSmokeTest.exe
```

---

## Architectural References
- **[ADR-007: AOT First Zero Tolerance](../../docs/adr/adr-007-aot-first-zero-tolerance.md)**
- **[ADR-011: Native AOT Strategy](../../docs/adr/adr-011-native-aot-strategy.md)**
- **[ADR-012: Trimming Strategy](../../docs/adr/adr-012-trimming-strategy.md)**
