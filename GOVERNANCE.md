# Governance Model

The `EricksonLopez.Mapper` project operates under a Benevolent Dictator for Life (BDFL) model, transitioning towards a meritocratic consensus model as community participation expands. This document outlines project leadership, the decision-making process, and inviolable design principles.

---

## 1. Roles and Responsibilities

### Project Lead & Core Maintainer

- **Erickson López** (Creator & BDFL — `@ericksonlopezf`)

**Responsibilities:**
- Strategic direction, roadmap stewardship, and architecture evolution.
- Enforcing the **Zero-Reflection** and **NativeAOT-First** invariants across all contributions.
- Reviewing and approving Architecture Decision Records (ADRs).
- Managing release lifecycles, NuGet package publishing, and security patch coordination.

### Contributors

Anyone who opens an issue, participates in GitHub Discussions, or submits a pull request is a contributor. Consistent high-impact contributions (such as maintaining Roslyn generator components, analyzer rules, or ecosystem extensions) can lead to maintainer privileges.

---

## 2. Decision-Making Process

- **Standard Changes**: Bug fixes, documentation updates, performance optimizations, and test additions follow standard pull request reviews.
- **Architectural Changes**: Any addition to the public API surface in `Abstractions`, modifications to generator construction strategies, or adjustments to diagnostic rules require an **Architecture Decision Record (ADR)** submitted to `docs/adr/`.
- **Non-Goal Protections**: Proposals that conflict with established non-goal ADRs (ADR-D01 through ADR-D12) are closed with reference to the existing rationale.

---

## 3. Inviolable Architectural Principles

All contributions must strictly adhere to the foundational principles of the project:

1. **Zero Runtime Reflection**: Absolutely no usage of `System.Reflection`, `Activator.CreateInstance`, `FormatterServices`, or runtime IL emit in generator or emitted code.
2. **NativeAOT & Trimming First**: Every feature must compile and execute cleanly with `PublishAot=true` and zero `IL2026` / `IL3050` trim warnings.
3. **Compile-Time Incremental Generation**: All code emission is driven by Roslyn's `IIncrementalGenerator` with immutable value models (`EquatableArray<T>`) for optimal IDE performance.
4. **Attribute-Only Declarative Configuration**: Mapping behaviors are declared explicitly on classes and methods without runtime configuration registries.
5. **DDD & Invariant Safety**: Immutability, constructor validation, and factory patterns are protected by default.
