# Public API Surface Declaration

## 1. Core Namespace: `EricksonLopez.Mapper`
- `[MapperAttribute]`
- `[MapAttribute]`
- `[MapPropertyAttribute(string target, string source)]`
- `[MapIgnoreAttribute(string member)]`
- `[MapFactoryAttribute(string methodName)]`
- `[MapConstructorAttribute(params Type[] types)]`
- `[MapNullFallbackAttribute(string member, string fallback)]`
- `[MapEnumAttribute(object source, object destination)]`
- `[MapDerivedAttribute(Type sourceType, Type destinationType)]`
- `[UseConverterAttribute(Type converterType)]`
- `[MapperDefaultsAttribute]`
- `IConverter<in TSource, out TDestination>`
