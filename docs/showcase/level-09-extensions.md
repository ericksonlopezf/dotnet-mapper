# Level 9 — Extensions & Ecosystem Integration

> **Showcase Source Files:**
> - [`Level9_Extensions/ExtensionsDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level9_Extensions/ExtensionsDemo.cs)
> - [`Level9_Extensions/DependencyInjectionDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level9_Extensions/DependencyInjectionDemo.cs)
> - [`Level9_Extensions/DomainPrimitivesDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level9_Extensions/DomainPrimitivesDemo.cs)
> - [`Level9_Extensions/ResultIntegrationDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level9_Extensions/ResultIntegrationDemo.cs)
> - [`Level9_Extensions/MapsterBridgeDemo.cs`](file:///d:/DevData/ericksonlopez.dev/dotnet-mapper/samples/EricksonLopez.Mapper.Samples/Level9_Extensions/MapsterBridgeDemo.cs)  
> **Complexity Level:** Advanced  
> **API Surface Covered:** `[GenerateMapperRegistration]`, `AddGeneratedMappers()`, `EricksonLopez.Mapper.DomainPrimitives`, `EricksonLopez.Mapper.Result`, `EricksonLopez.Mapper.Mapster`

---

## 1. Automated Dependency Injection Registration (`[assembly: GenerateMapperRegistration]`)

To automatically register all non-static mappers across the assembly into `IServiceCollection`:

```csharp
// In any configuration or AssemblyInfo.cs file:
[assembly: EricksonLopez.Mapper.GenerateMapperRegistration]

// In Program.cs:
services.AddGeneratedMappers();
```

The generator emits an extension method that registers every non-static `[Mapper]` class as a `Singleton` in the container.

---

## 2. Domain Primitives Integration (`EricksonLopez.Mapper.DomainPrimitives`)

Package tailored for Clean Architecture and Domain-Driven Design (DDD):

```csharp
using EricksonLopez.Mapper.DomainPrimitives;

// 1. Extract raw Guid from Strongly-Typed ID:
var strongIdConverter = new StrongIdToValueConverter<CustomerId, Guid>();
Guid rawId = strongIdConverter.Convert(customerId);

// 2. Extract raw scalar from Domain Primitive:
var primitiveConverter = new DomainPrimitiveToValueConverter<EmailAddress, string>();
string rawEmail = primitiveConverter.Convert(emailAddress);

// 3. Rehydrate Domain Primitive from scalar value:
var valueToPrimitive = new ValueToDomainPrimitiveConverter<string, EmailAddress>();
EmailAddress domainEmail = valueToPrimitive.Convert("architect@example.com");
```

---

## 3. Functional Result Pattern Integration (`EricksonLopez.Mapper.Result`)

Fluent monadic projection using Railway-Oriented Programming (ROP) without manual `if (result.IsSuccess)` branching:

```csharp
using EricksonLopez.Mapper.Result;
using EricksonLopez.Result;

// 1. Synchronous mapping:
Result<UserEntity> entityResult = repository.GetById(id);
Result<UserDto> dtoResult = entityResult.Map(userMapper.Map);

// 2. Asynchronous mapping on Task:
Task<Result<UserEntity>> entityTask = repository.GetByIdAsync(id);
Result<UserDto> asyncDtoResult = await entityTask.MapAsync(userMapper.Map);

// 3. Asynchronous mapping on ValueTask (zero heap allocations):
ValueTask<Result<UserEntity>> entityValueTask = repository.GetByIdValueTaskAsync(id);
Result<UserDto> vtDtoResult = await entityValueTask.MapAsync(userMapper.Map);

// 4. Mapping collections wrapped in Result:
Result<IEnumerable<UserEntity>> listResult = repository.GetAll();
Result<IReadOnlyList<UserDto>> dtosResult = listResult.MapList(userMapper.Map);
```

If the initial `Result` is in a failure state (`IsFailure = true`), the original error propagates untouched and the mapping method is **never invoked**, avoiding wasted CPU cycles.

---

## 4. Interoperability Bridge with Mapster (`EricksonLopez.Mapper.Mapster`)

Facilitates incremental migration from existing Mapster codebases:

```csharp
using EricksonLopez.Mapper.Mapster;
using Mapster;

// 1. Wrap Mapster as an EricksonLopez.Mapper IConverter:
IConverter<SourceModel, TargetModel> converter = new MapsterConverter<SourceModel, TargetModel>();
TargetModel target = converter.Convert(source);

// 2. Wrap with isolated TypeAdapterConfig instance:
var config = new TypeAdapterConfig();
config.NewConfig<SourceModel, TargetModel>().Map(dest => dest.Name, src => src.Name.ToUpper());
var customConverter = new MapsterConverter<SourceModel, TargetModel>(config);

// 3. Register IConverter into Mapster configuration:
config.UseConverter(new MyCustomEricksonLopezConverter());
```
