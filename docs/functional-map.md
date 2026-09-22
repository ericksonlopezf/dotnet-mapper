# Functional Domain Map & Architectural Topology

> **Canonical Functional Map:** This document defines the end-to-end functional domain map of the `EricksonLopez.Mapper` ecosystem, describing how all public API components interact from system ingress to egress.  
> The library operates as a high-performance **synchronous Roslyn Incremental Source Generator with a NativeAOT-first design**. External dispatch, message queue consumption, and HTTP hosting represent the integration boundaries of consumer hosts, while `EricksonLopez.Mapper` governs typed, reflection-free data transformation across those boundaries.

---

## 1. End-to-End Functional Topology

```mermaid
flowchart TD
    subgraph HostEntry["1. Host Application Entry Point"]
        direction TB
        HttpIn["HTTP / REST / gRPC Request"]
        BrokerIn["Message Broker Consumer / Event Payload"]
        InboundDTO["Request DTO / Raw Payload"]
        HttpIn --> InboundDTO
        BrokerIn --> InboundDTO
    end

    subgraph AppProcessing["2. Processing Layer (Application / CQRS)"]
        direction TB
        AppCommand["Application Command / Query"]
        DomainEntity["Domain Aggregate / Entity"]
        ResultWrapper["Result<T> (Railway-Oriented Programming)"]
        InboundDTO -->|EricksonLopez.Mapper\n[Mapper] / MapProperty| AppCommand
        AppCommand -->|Domain Service Logic| DomainEntity
        DomainEntity -->|Result.Success / Failure| ResultWrapper
    end

    subgraph DomainCore["3. Domain Core Layer (DDD & Domain Primitives)"]
        direction TB
        StrongId["IStrongId<TSelf, TValue>"]
        ValueObj["IDomainPrimitive / [ValueObject]"]
        DomainEntity --- StrongId
        DomainEntity --- ValueObj
    end

    subgraph PersistenceLayer["4. Persistence Layer"]
        direction TB
        DbRecord["Database Model / Dapper Record / EF Entity"]
        DomainEntity -->|EricksonLopez.Mapper.DomainPrimitives\nStrongIdToValueConverter\nDomainPrimitiveToValueConverter| DbRecord
        DbRecord -->|ValueToDomainPrimitiveConverter| DomainEntity
    end

    subgraph DispatchPublish["5. Dispatch & Publishing Layer (External Boundary)"]
        direction TB
        OutboundEvent["Integration Event / Notification DTO"]
        EventBus["External Event Bus / Message Broker"]
        DomainEntity -->|EricksonLopez.Mapper\n[MapDerivedType] / [MapIgnoreSource]| OutboundEvent
        OutboundEvent -->|Publish / Dispatch| EventBus
    end

    subgraph ConsumersAck["6. Consumers & Confirmation (ACK / NACK)"]
        direction TB
        WorkerConsumer["Background Worker / Integration Consumer"]
        AckDecision{"Result.IsSuccess?"}
        EventBus --> WorkerConsumer
        WorkerConsumer -->|EricksonLopez.Mapper.Result\nresult.Map / MapAsync| AckDecision
        AckDecision -- Yes --> EmitAck["Message ACK / Commit Transaction"]
        AckDecision -- No --> EmitNack["Dead Letter Queue / Retry Policy"]
    end

    subgraph MemoryCleanup["7. Memory Lifecycle & Deallocation"]
        direction TB
        ZeroAlloc["Zero-Reflection / Zero-Boxing Execution"]
        SpanPool["Pre-sized Collections / Stack Frame Allocation"]
        ZeroAlloc --> EndLifecycle(["End of Request Lifecycle"])
        SpanPool --> EndLifecycle
    end

    ResultWrapper --> OutboundDTO["Response DTO"]
    OutboundDTO --> HttpResponse["HTTP Response 200 OK / 400 Bad Request"]
    EmitAck --> MemoryCleanup
    HttpResponse --> MemoryCleanup
```

---

## 2. Detailed Layer Descriptions & Architectural Transitions

### 1. Host Application Entry Point
- **Components:** ASP.NET Core Minimal API endpoints, REST controllers, gRPC receivers, or message broker subscribers.
- **Payload Format:** Unvalidated Data Transfer Objects (`RequestDTO`), deserialized from JSON or Protocol Buffers.
- **Responsibility:** Ingest untrusted payloads without executing internal business invariant checks.
- **Transition:** Passes `RequestDTO` into the application layer.

### 2. Processing Layer (Application / CQRS)
- **Components:** `EricksonLopez.Mapper` annotated `[Mapper]` classes, Command Handlers, Query Handlers, and `EricksonLopez.Mapper.Result`.
- **Technical Transition:**
  - The generated mapper projects the incoming `RequestDTO` into an internal strongly-typed `Command` or `Query` via naming convention or explicit directives (`[MapProperty]`, `[MapNullFallback]`).
  - Use case logic executes domain business rules and entity state transitions.
  - Execution outcome is wrapped in a functional monad from `EricksonLopez.Result`.
  - `EricksonLopez.Mapper.Result` projects successful values to response DTOs via `result.Map(mapper.ToDto)` or `resultTask.MapAsync(mapper.ToDto)`, short-circuiting on failure without allocating response DTOs.

### 3. Domain Core Layer (DDD & Domain Primitives)
- **Components:** Domain Aggregates, Entities, `IDomainPrimitive<TSelf, TValue>`, `IStrongId<TSelf, TValue>`, and types annotated with `[ValueObject]`.
- **Technical Transition:**
  - The source generator automatically detects types marked with `[ValueObject]` or single-value record structs and wraps/unwraps scalar values (`.Value` property or positional constructor).
  - The extension package `EricksonLopez.Mapper.DomainPrimitives` provides pre-built `StrongIdToValueConverter` and `DomainPrimitiveToValueConverter` instances to translate between strongly-typed IDs and primitive CLR types (`Guid`, `string`, `int`).

### 4. Persistence Layer
- **Components:** Repositories, relational PostgreSQL models, Dapper row records, or EF Core entities.
- **Technical Transition:**
  - During persistence, domain aggregates are projected into database models (`DbRecord`). Strongly typed identifiers flatten into scalar columns.
  - When rehydrating from storage, `ValueToDomainPrimitiveConverter<TValue, TPrimitive>` restores domain instances via static factory methods (`TPrimitive.Create(value)`).
  - In read-only queries (`AsNoTracking`), static mapper methods (`static partial class`) enable zero-overhead projection directly inside LINQ `.Select()` expressions.

### 5. Dispatch & Publishing Layer (External Boundary)
- **Components:** Integration event dispatchers, message brokers (RabbitMQ, Kafka, Azure Service Bus), or internal event buses.
- **Technical Transition:**
  - Upon business commit, domain entities produce integration events.
  - The mapper projects domain event hierarchies into public event contracts using `[MapDerivedType]` for polymorphic pattern-matching dispatch without reflection.
  - `[MapIgnoreSource]` ensures sensitive internal attributes (`PasswordHash`, `InternalRevision`) are strictly excluded from published payloads.

### 6. Consumers & Confirmation (ACK / NACK)
- **Components:** Background workers, event handlers, queue listeners.
- **Technical Transition:**
  - The consumer receives a broker message and maps it to a processing command.
  - If processing succeeds (`Result.IsSuccess`), positive acknowledgment (`ACK`) is emitted to the broker to commit the stream offset.
  - If processing fails (`Result.IsFailure`), the host routes the message to a Dead Letter Queue (DLQ) or triggers exponential backoff via Polly.

### 7. Memory Lifecycle & Zero-Allocation Execution
- **Components:** .NET Garbage Collector, RyuJIT compiler, NativeAOT runtime.
- **Lifecycle Guarantees:**
  - Generated mappers are stateless singletons or static classes, eliminating object graph retention and memory leaks.
  - Collection projections pre-allocate exact capacity (`new List<T>(count)`, `ImmutableArray.CreateBuilder<T>(count)`), preventing continuous buffer reallocations.
  - Ephemeral intermediate DTOs are collected efficiently in Gen 0 with zero impact on LOH/POH.

---

## 3. Library Scope Boundaries vs Host Responsibilities

| Architectural Dimension | EricksonLopez.Mapper Scope | Host Application Scope |
|---|---|---|
| **Object Transformation** | ✅ Compile-time C# code generation with zero reflection. | None; emitted code is self-contained. |
| **Event Publishing & Brokers** | ❌ Does not include a broker or queue publisher. | Host must integrate messaging buses (MassTransit, RabbitMQ, Kafka). |
| **Transaction Management** | ❌ Does not manage transactions or `TransactionScope`. | Application layer / Unit of Work delimits database transactions. |
| **Retry Policies & DLQ** | ❌ Does not provide runtime retry policies. | Host implements retries using Polly or transport middleware. |
| **In-Place Mutation (`Map(src, dest)`)** | ❌ Permanently rejected (ADR-D05) for immutability & AOT safety. | Instantiate clean new instances or invoke domain business methods. |
| **Dynamic Reflection** | ❌ Strictly prohibited (ADR-D01) for NativeAOT integrity. | All type binding is resolved during Roslyn compilation. |
