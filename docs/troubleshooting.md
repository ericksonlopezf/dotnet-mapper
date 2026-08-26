# Troubleshooting & FAQ

This document covers frequently asked questions, compilation error resolutions, and diagnostic troubleshooting steps for `EricksonLopez.Mapper`.

---

## 1. Frequently Asked Questions (FAQ)

### Q: Why does the generator emit `ELM001` when all properties seem mapped?

**A:** `ELM001` indicates that a **destination** property has no matching source property under strict mapping.
- Verify whether the destination property name matches the source property name (case-insensitively).
- If the name differs, apply `[MapProperty(nameof(Source.OldName), nameof(Dest.NewName))]`.
- If the property should not be mapped, apply `[MapIgnore(nameof(Dest.UnmappedProp))]` or `[MapValue(nameof(Dest.Prop), "defaultValue")]`.

---

### Q: Why do I get CS8795 ("Partial method … must have an implementation part")?

**A:** This occurs when the Roslyn source generator did not emit code for your partial method. Common causes:
1. The class is missing the `[Mapper]` attribute.
2. The class is not declared with the `partial` modifier (check for `ELM012`).
3. The method signature does not match `public partial TDestination MethodName(TSource source)`.
4. Run `dotnet clean && dotnet build` to force Roslyn generator re-evaluation.

---

### Q: Where can I inspect the generated C# files?

**A:** By default, generated code resides in memory during compilation. To emit physical files on disk for inspection, add this property to your `.csproj`:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Generated files will appear under `obj/Generated/EricksonLopez.Mapper.Generator/`.

---

### Q: Does the mapper support mutating an existing object instance (`Map(src, dest)`)?

**A:** **No.** This is an explicit permanent non-goal ([ADR-D05](adr/ADR-D05-no-existing-instance-mapping.md)). Mutating existing instances breaks DDD domain invariants, creates race hazards in concurrent pipelines, and compromises NativeAOT predictability. Always instantiate clean new instances.

---

### Q: Does the mapper support runtime `IQueryable` expression tree rewriting (`ProjectTo<T>`)?

**A:** **No.** This is an explicit permanent non-goal ([ADR-D11](adr/ADR-D11-no-iqueryable-projection.md)). Runtime expression tree rewriting breaks NativeAOT compilation and obscures SQL generation costs. Use static mappers inside standard LINQ `.Select()` projections.

---

### Q: Can I inject services into generated mappers?

**A:** Yes. When `[assembly: GenerateMapperRegistration]` is present, non-static mappers are registered as Singletons via `services.AddGeneratedMappers()`. You can inject converters or services into the partial mapper's constructor in a companion file, or reference injected converter fields using `[UseConverter(nameof(_myInjectedConverter))]`.

---

## 2. Compiler Diagnostics Reference & Fixes

| Diagnostic | Severity | Cause | Recommended Solution |
|---|---|---|---|
| **`ELM001`** | Error | Unmapped destination member | Add `[MapProperty]`, `[MapIgnore]`, or `[MapValue]`. |
| **`ELM002`** | Error | No accessible constructor or factory | Add accessible constructor or apply `[MapFactory("FactoryMethod")]`. |
| **`ELM003`** | Error | Unsupported type conversion | Implement and register custom `IConverter<TSource, TDestination>` via `[UseConverter]`. |
| **`ELM004`** | Error | Nullable source mapped to non-nullable destination | Add `[MapNullFallback("PropName", "defaultValue")]`. |
| **`ELM005`** | Error | Ambiguous case-insensitive member match | Specify explicit match via `[MapProperty]`. |
| **`ELM006`** | Error | Missing supported constructor or public setters | Add accessible parameterless constructor or settable properties. |
| **`ELM007`** | Error | Multiple parameterized constructors | Disambiguate constructor choice using `[MapFactory]`. |
| **`ELM008`** | Error | Prohibited reflection API used in mapper | Remove reflection; use static C# types. |
| **`ELM009`** | Error | Dynamic keyword used in mapper | Remove `dynamic`; use strongly-typed models. |
| **`ELM010`** | Error | Recursive circular mapping graph | Restructure DTOs to eliminate recursive loops. |
| **`ELM011`** | Warning | Abstract base target with uncovered derived types | Add `[MapDerivedType]` for all concrete subtypes. |
| **`ELM012`** | Error | Class with `[Mapper]` is not `partial` | Add `partial` keyword (use code fix provider). |
| **`ELM013`** | Error | Invalid converter type specified | Ensure class implements `IConverter<TSource, TDestination>`. |
| **`ELM014`** | Error / Warn | Unmapped enum member in strict mode | Add `[MapEnumValue]` or configure `EnumMappingStrategy`. |
| **`ELM015`** | Warning | Narrowing numeric conversion | Add explicit cast or verify precision loss is acceptable. |
| **`ELM016`** | Warning | String to enum runtime parsing risk | Validate string before mapping or use typed enums. |
