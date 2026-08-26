# Architecture & Flow Diagrams

This document contains visual diagrams documenting the architecture, compiler pipeline, runtime flows, extension bridges, and diagnostic paths of the **EricksonLopez.Mapper** ecosystem.

---

## 1. Build-Time Compilation Sequence

```mermaid
sequenceDiagram
    participant Dev as Developer / Source Code
    participant Roslyn as Roslyn Compiler Host
    participant SG as MapperGenerator (IIncrementalGenerator)
    participant Analyzer as MapperAnalyzer (DiagnosticAnalyzer)

    Dev->>Roslyn: Defines partial class with [Mapper]
    Roslyn->>SG: Invokes syntax provider (MapperAttribute metadata)
    SG->>SG: GetSemanticTargetForGeneration()
    Note over SG: Extracts member models, attributes, strategies
    SG->>SG: DetectCycles() — ELM010 verification
    alt Compilation Diagnostic Found (ELM001-ELM016)
        SG-->>Roslyn: Emits Diagnostic (Error/Warning)
        Roslyn-->>Dev: Fails compilation with line location
    else Validation Succeeded
        SG-->>Roslyn: Emits optimized *.g.cs source
    end
    Roslyn->>Analyzer: Runs Roslyn Analyzers
    alt Analyzer Rule Violation (ELM008/ELM009/ELM012)
        Analyzer-->>Dev: Reports IDE diagnostic + Code Fix
    else Clean Code
        Roslyn-->>Dev: Emits NativeAOT-ready binary
    end
```

---

## 2. Ecosystem Component Dependencies

```mermaid
graph TD
    App[Consumer Application] -->|PackageReference| CORE(EricksonLopez.Mapper)
    App -.->|Optional Reference| DP(EricksonLopez.Mapper.DomainPrimitives)
    App -.->|Optional Reference| MAP(EricksonLopez.Mapper.Mapster)
    App -.->|Optional Reference| RES(EricksonLopez.Mapper.Result)

    CORE --> ABS[EricksonLopez.Mapper.Abstractions]
    CORE -.->|Build-time Analyzer| GEN[EricksonLopez.Mapper.Generator]
    CORE -.->|Build-time Analyzer| ANA[EricksonLopez.Mapper.Analyzers]

    DP --> ABS
    MAP --> ABS
    RES --> ABS

    ABS --> ATTR([Attributes: Mapper, MapProperty, MapValue, ...])
    ABS --> ICONV([Interface: IConverter<TSource, TDestination>])

    GEN -->|Emits at build time| OUT((Generated *.g.cs files))
    ANA -->|Reports in IDE & CI| DIAG((Diagnostics ELM008, ELM009, ELM012))
```

---

## 3. Incremental Generator Pipeline Architecture

```mermaid
flowchart TD
    BuildTrigger([dotnet build]) --> SyntaxProvider
    SyntaxProvider["SyntaxProvider.ForAttributeWithMetadataName\n('EricksonLopez.Mapper.MapperAttribute')"]
    SyntaxProvider --> SemanticExtraction["Semantic Extraction\n(TypeMapping, MethodMapping, MemberMapping)"]
    SemanticExtraction --> IncrementalCache{"Incremental Cache\nEquatableArray<T>"}
    IncrementalCache -- Unchanged --> Skip[Skip Code Emission]
    IncrementalCache -- Changed --> ValidationEngine["Validation Engine\n(Cycle detection, member resolution, nullability)"]

    ValidationEngine --> DiagnosticGate{Errors detected?}
    DiagnosticGate -- Yes --> EmitDiagnostics[Report Diagnostics ELM001..ELM016]
    DiagnosticGate -- No --> CodeEmission[CodeEmitter: Emit *.g.cs]
    CodeEmission --> OutputContext[context.AddSource]

    BuildTrigger --> DISyntax["SyntaxProvider: [assembly: GenerateMapperRegistration]"]
    DISyntax --> DIEmission["DependencyInjectionEmitter: Emit AddGeneratedMappers()"]
    DIEmission --> OutputContext
```

---

## 4. Property Resolution & Conversion Strategy Decision Tree

```mermaid
flowchart TD
    DestMember[For each destination member] --> CheckIgnore{Has [MapIgnore]?}
    CheckIgnore -- Yes --> SkipMember[Exclude member]
    CheckIgnore -- No --> CheckValue{Has [MapValue]?}
    CheckValue -- Yes --> EmitLiteral[Emit literal C# expression]
    CheckValue -- No --> CheckPropertyOverride{Has [MapProperty] override?}

    CheckPropertyOverride -- Yes --> ResolveCustom[Search source by explicit custom name / deep path]
    CheckPropertyOverride -- No --> ResolveConvention[Search source by case-insensitive name]

    ResolveCustom --> MatchCheck{Found in source?}
    ResolveConvention --> MatchCheck

    MatchCheck -- No --> CheckStrict{StrictMapping == true?}
    CheckStrict -- Yes --> ELM001[Emit ELM001 Error: Unmapped Member]
    CheckStrict -- No --> SkipMember

    MatchCheck -- Yes --> TypeCompatibility{Are types compatible?}
    TypeCompatibility -- Same Primitive / Scalar --> DirectAssign[Emit Direct Assignment]
    TypeCompatibility -- Widening Numeric --> WideningAssign[Emit Implicit Widening]
    TypeCompatibility -- Narrowing Numeric --> NarrowingAssign[Emit Explicit Cast + ELM015 Warning]
    TypeCompatibility -- Enum matching --> EnumResolution[Emit Enum Strategy Cast / Switch]
    TypeCompatibility -- Temporal bridging --> TemporalAssign[Emit DateOnly/DateTime/Offset Conversion]
    TypeCompatibility -- Value Object --> VOWrap[Emit Wrap / Unwrap .Value]
    TypeCompatibility -- Collection target --> CollectionLoop[Emit Pre-sized for/foreach Loop]
    TypeCompatibility -- Dictionary target --> DictLoop[Emit KVP Iteration Loop]
    TypeCompatibility -- Sub-mapper exists --> SubMapperCall[Emit sub-mapper invocation]
    TypeCompatibility -- [UseConverter] present --> ConverterCall[Emit IConverter.Convert]
    TypeCompatibility -- Incompatible --> ELM003[Emit ELM003 Error: Unsupported Conversion]
```

---

## 5. Result<T> Railway-Oriented Pipeline (`EricksonLopez.Mapper.Result`)

```mermaid
flowchart LR
    InResult["Result<TSource>"] --> CheckSuccess{IsSuccess?}
    CheckSuccess -- Success --> MapFunc["mapper.Map(Value)"]
    MapFunc --> OutSuccess["Result<TDest>.Success(mappedValue)"]
    CheckSuccess -- Failure --> PropagateError["Propagate Failure(Error)"]
    PropagateError --> OutFailure["Result<TDest>.Failure(Error)"]
```

---

## 6. Mapster Adapter Bridge (`EricksonLopez.Mapper.Mapster`)

```mermaid
graph LR
    subgraph "Mapster to EricksonLopez.Mapper"
        MConfig[Mapster TypeAdapterConfig] --> MC[MapsterConverter<TSrc, TDst>]
        MC -->|implements| IC[IConverter<TSrc, TDst>]
    end

    subgraph "EricksonLopez.Mapper to Mapster"
        IC2[IConverter<TSrc, TDst>] -->|registered via| MExt[TypeAdapterConfig.UseConverter]
        MExt --> MConfig2[Mapster Configuration]
    end
```

---

## 7. Polymorphic Dispatch Execution Flow

```mermaid
sequenceDiagram
    participant App as Application Code
    participant Mapper as VehicleMapper (Generated)
    participant CarMap as MapCar(Car source)
    participant TruckMap as MapTruck(Truck source)

    App->>Mapper: Map(vehicle) where vehicle : Vehicle
    Note over Mapper: switch (source)
    alt source is Car car
        Mapper->>CarMap: MapCar(car)
        CarMap-->>Mapper: CarDto
        Mapper-->>App: VehicleDto (CarDto)
    else source is Truck truck
        Mapper->>TruckMap: MapTruck(truck)
        TruckMap-->>Mapper: TruckDto
        Mapper-->>App: VehicleDto (TruckDto)
    else Unmatched Type
        Mapper-->>App: throw InvalidOperationException
    end
```

---

## 8. Dependency Injection Synthesis & Registration

```mermaid
flowchart TD
    Attr["[assembly: GenerateMapperRegistration]"] --> SG[MapperGenerator]
    SG --> DIEmitter[DependencyInjectionEmitter]
    DIEmitter --> OutputFile["MapperServiceCollectionExtensions.g.cs"]
    OutputFile --> Method["public static IServiceCollection AddGeneratedMappers(this IServiceCollection services)"]
    Method --> Reg1["services.AddSingleton<OrderMapper>();"]
    Method --> Reg2["services.AddSingleton<CustomerMapper>();"]
    Method --> Reg3["// Static mappers omitted (no DI required)"]
    Reg1 --> Container[ASP.NET Core DI Container]
    Reg2 --> Container
```
