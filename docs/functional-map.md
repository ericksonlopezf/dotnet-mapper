# Functional Domain Map & Mapping Topology

## 1. System Mapping Topology

```mermaid
flowchart TB
    subgraph Presentation["Presentation & API Layer"]
        RequestDTO[Incoming Request DTO]
        ResponseDTO[Outgoing Response DTO]
    end

    subgraph Application["Application Layer (CQRS)"]
        Command[Application Command]
        Query[Application Query]
        ResultObj[Result Pattern Value]
    end

    subgraph Domain["Core Domain Layer"]
        DomainEntity[Domain Entity / Aggregate]
        ValueObj[Value Objects & Strong IDs]
    end

    subgraph Persistence["Persistence & Infrastructure"]
        DbRecord[Database Entity / Dapper Record]
    end

    RequestDTO -->|EricksonLopez.Mapper| Command
    Command -->|EricksonLopez.Mapper| DomainEntity
    DomainEntity -->|EricksonLopez.Mapper.DomainPrimitives| DbRecord
    DbRecord -->|EricksonLopez.Mapper| DomainEntity
    DomainEntity -->|EricksonLopez.Mapper.Result| ResultObj
    ResultObj -->|EricksonLopez.Mapper| ResponseDTO
```

---

## 2. Layer Responsibilities
- **Presentation $\leftrightarrow$ Application**: Maps unvalidated JSON DTOs to strongly-typed command/query records.
- **Application $\leftrightarrow$ Domain**: Converts primitives into encapsulated Domain Primitives and Value Objects.
- **Domain $\leftrightarrow$ Persistence**: Maps entities to flat Dapper/PostgreSQL data structures.
