# Architecture & Technical Diagrams

> **Canonical Architectural Diagrams:** Derived strictly from the discovered architecture and implementation of `EricksonLopez.Mapper` (synchronous Roslyn Incremental Source Generator and NativeAOT-first design).

---

## 1. Ecosystem Architecture

```mermaid
graph TB
    subgraph DevelopmentHost["Development Host Environment (.NET 8 / 9 / 10)"]
        ConsumerApp["Consumer Application / Minimal API / Worker"]
        DevCode["User C# Code with [Mapper]"]
    end

    subgraph CompilerLayer["Compiler Layer (Roslyn Host)"]
        AnalyzerHost["Roslyn Analyzer Host"]
        SGHost["Roslyn Incremental Generator Host"]
    end

    subgraph EricksonLopezEcosystem["EricksonLopez.Mapper Ecosystem"]
        CoreMetapackage["EricksonLopez.Mapper (Metapackage)"]
        AbstractionsPkg["EricksonLopez.Mapper.Abstractions"]
        GeneratorPkg["EricksonLopez.Mapper.Generator"]
        AnalyzersPkg["EricksonLopez.Mapper.Analyzers"]
        DomainPrimitivesPkg["EricksonLopez.Mapper.DomainPrimitives"]
        ResultPkg["EricksonLopez.Mapper.Result"]
        MapsterPkg["EricksonLopez.Mapper.Mapster"]
    end

    subgraph NativeArtifacts["Output Artifacts"]
        GenFiles["Emitted Source Files (*.g.cs)"]
        AotBinary["Native Binary / AOT Executable"]
    end

    ConsumerApp --> CoreMetapackage
    CoreMetapackage --> AbstractionsPkg
    CoreMetapackage -.->|Analyzer Reference| GeneratorPkg
    CoreMetapackage -.->|Analyzer Reference| AnalyzersPkg

    ConsumerApp -.->|Optional| DomainPrimitivesPkg
    ConsumerApp -.->|Optional| ResultPkg
    ConsumerApp -.->|Optional| MapsterPkg

    DomainPrimitivesPkg --> AbstractionsPkg
    ResultPkg --> AbstractionsPkg
    MapsterPkg --> AbstractionsPkg

    DevCode --> SGHost
    DevCode --> AnalyzerHost
    SGHost --> GenFiles
    GenFiles --> AotBinary
```

---

## 2. Main Runtime Flow

```mermaid
flowchart LR
    SourceInstance["Source Entity Instance"] --> NullCheck{"Is source null?"}
    NullCheck -- Yes --> ThrowNull["throw ArgumentNullException"]
    NullCheck -- No --> TargetInit["Instantiate Destination\n(new TDest / Record Ctor / [MapFactory])"]

    TargetInit --> MemberAssign["Direct Property Assignment\n- Naming convention\n- [MapProperty]\n- [MapValue]\n- [MapNullFallback]"]
    MemberAssign --> SubMethods{"Nested property?"}
    SubMethods -- Yes --> CallSubMapper["Sub-method invocation\nMapSubProperty(source.Sub)"]
    SubMethods -- No --> DirectValue["Scalar value assignment"]
    CallSubMapper --> ReturnTarget["Return Target DTO"]
    DirectValue --> ReturnTarget
```

---

## 3. Sequence Diagram (Mapping & Converters)

```mermaid
sequenceDiagram
    autonumber
    participant App as Client Application
    participant Mapper as Generated Partial Mapper
    participant SubMapper as Sub-Mapping Method
    participant Converter as IConverter<TSrc, TDst>

    App->>Mapper: Map(sourceEntity)
    activate Mapper
    Mapper->>Mapper: Validate nullability (source != null)
    
    opt Contains Nested Complex Object
        Mapper->>SubMapper: MapAddress(source.ShippingAddress)
        activate SubMapper
        SubMapper-->>Mapper: AddressDto
        deactivate SubMapper
    end

    opt Decorated with [UseConverter]
        Mapper->>Converter: Convert(source.RawCoordinates)
        activate Converter
        Converter-->>Mapper: FormattedCoordinatesDto
        deactivate Converter
    end

    Mapper->>Mapper: Populate target properties
    Mapper-->>App: Return TargetDto
    deactivate Mapper
```

---

## 4. Compiler / Incremental Generator State Machine

```mermaid
stateDiagram-v2
    [*] --> SyntacticDiscovery: Roslyn starts compilation

    SyntacticDiscovery --> AttributeFiltering: Scan classes with [Mapper]
    AttributeFiltering --> SemanticExtraction: Extract type and method symbols
    
    state SemanticExtraction {
        [*] --> MemberAnalysis
        MemberAnalysis --> CycleDetection: DetectCycles()
        CycleDetection --> StrategyResolution: MemberResolutionEngine
    }

    SemanticExtraction --> DiagnosticEvaluation: Check for invariant violations

    state DiagnosticEvaluation <<choice>>
    DiagnosticEvaluation --> DiagnosticEmission: Violations found (ELM001-ELM016)
    DiagnosticEvaluation --> IncrementalCache: Valid structure

    DiagnosticEmission --> [*]: Halts compilation with errors

    IncrementalCache --> EquatableComparison: EquatableArray<T>
    
    state EquatableComparison <<choice>>
    EquatableComparison --> SkipEmission: Unchanged (Cache Hit)
    EquatableComparison --> CodeEmission: Modified (Cache Miss)

    SkipEmission --> [*]
    CodeEmission --> SourceAddition: context.AddSource("*.g.cs")
    SourceAddition --> [*]: Compilation successful
```

---

## 5. Component Dependencies

```mermaid
graph TD
    subgraph AbstractionsLayer["Contracts Layer (Zero Runtime Dependencies)"]
        ABS["EricksonLopez.Mapper.Abstractions\n- [Mapper], [MapProperty], ...\n- IConverter<T, D>"]
    end

    subgraph ToolingLayer["Compiler Tooling Layer"]
        GEN["EricksonLopez.Mapper.Generator\n(Roslyn IIncrementalGenerator)"]
        ANA["EricksonLopez.Mapper.Analyzers\n(Roslyn DiagnosticAnalyzer)"]
    end

    subgraph MetapackageLayer["Consumer Metapackage"]
        MAPPER["EricksonLopez.Mapper\n(Convenience Metapackage)"]
    end

    subgraph ExtensionsLayer["Official Integration Extensions"]
        DP["EricksonLopez.Mapper.DomainPrimitives"]
        RES["EricksonLopez.Mapper.Result"]
        MP["EricksonLopez.Mapper.Mapster"]
    end

    subgraph SamplesLayer["Reference Implementation"]
        SHOWCASE["EricksonLopez.Mapper.Samples\n(11 Levels + Cookbook)"]
    end

    MAPPER --> ABS
    MAPPER -.->|Private Asset: Analyzer| GEN
    MAPPER -.->|Private Asset: Analyzer| ANA

    DP --> ABS
    RES --> ABS
    MP --> ABS

    SHOWCASE --> MAPPER
    SHOWCASE --> DP
    SHOWCASE --> RES
    SHOWCASE --> MP
```

---

## 6. Incremental Roslyn Pipeline

```mermaid
flowchart TD
    CompilationTrigger(["dotnet build / IDE keystroke"]) --> SyntaxProvider["SyntaxValueProvider.ForAttributeWithMetadataName\n('EricksonLopez.Mapper.MapperAttribute')"]
    
    SyntaxProvider --> Transform["Semantic Transformation\n- TypeDeclarationSyntax to ClassDeclaration\n- Extract [MapProperty], [MapIgnore], etc."]
    
    Transform --> CacheBarrier{"Incremental Cache Barrier\n(EquatableArray<TypeMappingModel>)"}
    
    CacheBarrier -- No structural changes --> EarlyExit["Early Exit (0 ms CPU)"]
    CacheBarrier -- Changes detected --> GeneratorStage["Code Generation Stage\nCodeEmitter.Emit(...)"]
    
    GeneratorStage --> DiagnosticCheck{"Diagnostics present?"}
    DiagnosticCheck -- Errors present --> ReportDiagnostics["context.ReportDiagnostic(ELM001..ELM016)"]
    DiagnosticCheck -- Clean --> AddSource["context.AddSource(mapperName + '.g.cs', sourceText)"]
    
    CompilationTrigger --> DISyntax["SyntaxProvider: [assembly: GenerateMapperRegistration]"]
    DISyntax --> DIEmitter["DependencyInjectionEmitter"]
    DIEmitter --> DIAddSource["context.AddSource('MapperServiceCollectionExtensions.g.cs')"]
```

---

## 7. Batch & Concurrency Processing

```mermaid
flowchart TD
    BatchInput["In-Memory Input Collection (100,000 entities)"] --> PLINQFork["PLINQ .AsParallel().WithDegreeOfParallelism(N)"]
    
    subgraph WorkerPool["CPU Thread Pool (Lock-Free Execution)"]
        Thread1["Thread 1: mapper.Map(item 0..25k)"]
        Thread2["Thread 2: mapper.Map(item 25k..50k)"]
        Thread3["Thread 3: mapper.Map(item 50k..75k)"]
        Thread4["Thread 4: mapper.Map(item 75k..100k)"]
    end

    PLINQFork --> Thread1
    PLINQFork --> Thread2
    PLINQFork --> Thread3
    PLINQFork --> Thread4

    Thread1 --> DirectAlloc["Direct Instantiation (new Dto)\nLock-Free / No Shared Mutable State"]
    Thread2 --> DirectAlloc
    Thread3 --> DirectAlloc
    Thread4 --> DirectAlloc

    DirectAlloc --> BatchOutput["Output Collection (List / Array of DTOs)\nThroughput: 30,000,000+ ops/sec"]
```

---

## 8. Diagnostic Gate & Error Boundaries

```mermaid
flowchart TD
    InputModel["Class & Mapping Method Definitions"] --> CompilerGate{"Roslyn Compiler Gate"}
    
    CompilerGate -- "Destination unmapped in strict mode" --> ELM001["Error ELM001: Destination unmapped"]
    CompilerGate -- "No accessible ctor or factory" --> ELM002["Error ELM002: Missing constructor / factory"]
    CompilerGate -- "Incompatible type conversion" --> ELM003["Error ELM003: Unsupported conversion"]
    CompilerGate -- "Nullable source to non-nullable target" --> ELM004["Error ELM004: Nullability mismatch"]
    CompilerGate -- "Circular dependency detected" --> ELM010["Error ELM010: Circular dependency"]
    CompilerGate -- "Class missing partial modifier" --> ELM012["Error ELM012: Class must be partial"]

    CompilerGate -- "Validation Successful" --> RuntimeBoundary["Runtime Boundary"]

    subgraph RuntimeResilience["Host Runtime Resilience (Application Layer)"]
        RuntimeBoundary --> ROPFlow{"Returns Result<T>?"}
        ROPFlow -- Yes --> FunctionalROP["ResultMappingExtensions.Map / MapAsync\n(Functional railway failure propagation without throwing)"]
        ROPFlow -- No --> TryCatch["Standard try/catch block"]
        TryCatch -- "Exception in IConverter" --> HostStrategy{"Host Strategy"}
        HostStrategy -- "Transient Error" --> PollyRetry["Polly Retry Policy"]
        HostStrategy -- "Permanent Error" --> DLQ["Dead Letter Queue (DLQ)"]
    end
```
