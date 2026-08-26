# Architectural Boundary Specification: EricksonLopez.Mapper.Abstractions

## 1. Purpose

`EricksonLopez.Mapper.Abstractions` defines declarative mapping attributes (`[Mapper]`, `[MapProperty]`, `[MapValue]`, etc.) and the foundational `IConverter<TSource, TDestination>` interface contract, completely decoupled from runtime mapping engines.

---

## 2. Owns

- Declarative mapping attributes: `[Mapper]`, `[MapProperty]`, `[MapIgnore]`, `[MapIgnoreSource]`, `[MapperIgnore]`, `[MapValue]`, `[EnumMappingStrategy]`, `[MapEnumValue]`, `[MapperDefaults]`, `[MapFactory]`, `[MapDerivedType]`, `[UseConverter]`, `[MapNullFallback]`, `[GenerateMapperRegistration]`, `[ValueObject]`.
- Public interfaces: `IConverter<TSource, TDestination>`.
- Public enums: `EnumMappingStrategy`.

---

## 3. Does Not Own

- Roslyn compile-time source generator (`EricksonLopez.Mapper.Generator`).
- Roslyn diagnostic analyzers & code fixes (`EricksonLopez.Mapper.Analyzers`).
- Mapster adapter bridge (`EricksonLopez.Mapper.Mapster`).
- DomainPrimitives mapping converters (`EricksonLopez.Mapper.DomainPrimitives`).
- Functional Result mapping extensions (`EricksonLopez.Mapper.Result`).

---

## 4. Allowed Dependencies

- **.NET BCL only** (`netstandard2.0`, `net8.0`, `net9.0`, `net10.0`).
- **Zero** external runtime dependencies.

---

## 5. Forbidden Dependencies

- Runtime reflection-emitting libraries.
- Third-party mapping frameworks (`AutoMapper`, `Mapster`).
- Generic runtime `IMapper` service facades (prohibited per ADR-D12).

---

## 6. Who Can Depend On It

- `EricksonLopez.Mapper` (Umbrella Metapackage).
- `EricksonLopez.Mapper.Generator` (build-time metadata reading).
- `EricksonLopez.Mapper.DomainPrimitives`.
- `EricksonLopez.Mapper.Mapster`.
- `EricksonLopez.Mapper.Result`.
- Consumer application layers.

---

## 7. Public API & AOT Invariants

- Fully compatible with Native AOT compilation (`IsAotCompatible=true`).
- Fully trimmable (`IsTrimmable=true`).
- Surface tracked continuously via `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`.
