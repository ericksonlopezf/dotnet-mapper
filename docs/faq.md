# Frequently Asked Questions (FAQ): EricksonLopez.Mapper

Technical questions and answers covering architecture, design, compatibility, and common troubleshooting across the **EricksonLopez.Mapper** ecosystem.

---

## 1. Concepts and Architecture

### How does `EricksonLopez.Mapper` differ from AutoMapper and Mapster?
- **AutoMapper:** Relies on runtime reflection and dynamic type scanning. This incurs significant startup latency, memory allocations in Gen 0, and is fundamentally incompatible with **Native AOT** due to assembly IL trimming.
- **Mapster:** Utilizes runtime dynamic IL emission or compiled expression trees (`Expression.Compile()`). While faster than AutoMapper, it fails in strict Native AOT environments where runtime JIT code generation is disallowed.
- **EricksonLopez.Mapper:** Implemented as a **Roslyn Incremental Source Generator**. It resolves all types, member validations, and mapping routes at **compile time**. The emitted code is clean, direct C# (`new Dest { Prop = src.Prop }`), with zero reflection, zero startup overhead, and 100% Native AOT compatibility.

### Why is it 100% compatible with Native AOT?
Because it contains zero invocations of `System.Reflection`, `Activator.CreateInstance`, `System.Reflection.Emit`, or `Expression.Compile()`. The compiler generates pure C# code identical to handwritten code, allowing the AOT compiler to trim and analyze binaries safely with zero risk of runtime reflection exceptions.

### Why is in-place mutation (`mapper.Map(source, existingTarget)`) prohibited?
Under architectural principle **[ADR-D05](adr/adr-d05-no-existing-instance-mapping.md)**, the library enforces data immutability and modern C# patterns such as `record` types with `init`-only properties. Mutating an existing in-memory object encourages unintended side-effects, violates domain entity encapsulation, and cannot work with positional primary constructors. Instead, map to a clean new instance or delegate mutations to dedicated domain entity business methods.

---

## 2. Syntax and Compilation

### Why must classes and mapping methods be declared as `partial`?
Because mapping implementations are emitted into a companion generated file (`*.g.cs`) during compilation. The C# `partial class` and `partial method` language feature allows the compiler to merge your handwritten declaration with the generator's emitted implementation into a single compiled type. If you omit the `partial` modifier, the Roslyn analyzer emits compiler error `ELM012`.

### What does compilation error `ELM001` mean and how is it resolved?
`ELM001: Destination property is unmapped in strict mode.`  
This occurs when the mapper operates in strict mode (`StrictMapping = true`, the default) and the destination type contains a property that does not match any source property or explicit mapping rule. Resolve it using one of four approaches:
1. Ensure the source property shares the same name.
2. Pair properties explicitly using `[MapProperty("SourceProp", "DestProp")]`.
3. Deliberately ignore the unmapped destination property using `[MapIgnore("DestProp")]`.
4. Assign a constant or computed expression using `[MapValue("DestProp", "expression")]`.

### What does diagnostic `ELM004` (Nullability mismatch) indicate?
It indicates that a source property is a nullable value type (e.g., `int?`, `decimal?`) while the destination property is non-nullable (`int`, `decimal`). To resolve this safely without custom converters, annotate the mapping method with `[MapNullFallback("DestProp", "fallbackLiteral")]` (e.g., `[MapNullFallback("Stock", "0")]`).

---

## 3. Dependency Injection and Lifecycle

### Should mappers be registered as `Singleton`, `Scoped`, or `Transient`?
Generated mappers are completely **stateless** and **thread-safe**. Therefore, the recommended registration lifetime is **`Singleton`**, minimizing object allocations in your dependency injection container.

### How can all mappers in an assembly be registered automatically?
Add the assembly-level attribute:
```csharp
[assembly: EricksonLopez.Mapper.GenerateMapperRegistration]
```
And register them in your service configuration:
```csharp
services.AddGeneratedMappers();
```

### Do `static partial class` mappers require dependency injection registration?
No. Static mappers are invoked directly (`MyMapper.Map(source)`), making them ideal for LINQ projections such as `.Select(MyMapper.Map)` without requiring DI container resolution or instantiation overhead.

---

## 4. Ecosystem and Integrations

### How can a project migrate incrementally from Mapster?
Install the official extension package `EricksonLopez.Mapper.Mapster`:
- Use `MapsterConverter<TSource, TDestination>` to wrap existing Mapster mappings behind the `IConverter<TSource, TDestination>` interface.
- Register `EricksonLopez.Mapper` converters inside Mapster configuration using `config.UseConverter(...)`.

### Does the library support Strongly-Typed IDs and Domain Primitives?
Yes. The `EricksonLopez.Mapper.DomainPrimitives` package provides built-in converters for `IStrongId<TSelf, TValue>` and `IDomainPrimitive<TSelf, TValue>`. Additionally, decorating records with `[ValueObject]` allows the generator to automatically unwrap the inner `.Value` property without custom converters.

### How does integration with `Result<T>` work?
Install `EricksonLopez.Mapper.Result`. This extension provides fluent monadic methods `result.Map(mapper.Map)` and `resultTask.MapAsync(mapper.Map)` based on Railway-Oriented Programming (ROP). If the source result is in a failure state, the error propagates untouched and the mapping method is never invoked, saving CPU cycles.
