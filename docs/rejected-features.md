# Rejected Features & Technical Trade-Offs

## 1. Discarded Architectural Features
1. **Dynamic Mapping Configuration via Fluents**: Discarded in favor of declarative attributes (`[MapProperty]`, `[MapIgnore]`) to ensure 100% compile-time syntax tree inspectability.
2. **Runtime Auto-Coercion of Mismatched Enums**: Discarded to prevent silent business logic corruptions; requires explicit `[MapEnum]` or `[MapperDefaults]`.
3. **Auto-Mapping Inaccessible Private Fields**: Discarded to preserve encapsulation and avoid reflection fallback.
