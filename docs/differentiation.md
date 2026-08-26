# Strategic Differentiation & Architecture Positioning

## 1. Core Differentiators

`EricksonLopez.Mapper` is differentiated by four foundational principles:

### 1. Compile-Time Determinism Over Dynamic Magic
Traditional mappers attempt to infer and coerce data structures at runtime through reflection. `EricksonLopez.Mapper` forces all mapping rules, constructor selection, nullability propagation, and polymorphic subtypes to be validated and emitted at compile time by the Roslyn compiler.

### 2. Native AOT-First as a Hard Invariant
Every line of code emitted by the generator is 100% trimmable and Native AOT compliant. There are no fallback paths to reflection.

### 3. Integrated Tier-0 Domain-Driven Design (DDD) Ecosystem
Unlike generic object mappers, `EricksonLopez.Mapper` provides first-class, zero-allocation adapters for:
- `EricksonLopez.DomainPrimitives` (Strongly-Typed IDs, Value Objects).
- `EricksonLopez.Result` (Railway-Oriented Programming and functional failure mapping).

### 4. Zero Allocation Memory Model
Generated mappings for flat objects, collections, and primitives avoid LINQ allocations, boxing, and delegate closures.
