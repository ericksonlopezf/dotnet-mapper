// Copyright © Erickson Lopez. MIT License.
// =============================================================================
// Assembly-level configuration via [assembly: MapperDefaults]
//
// This file consolidates all assembly-scoped mapper configuration.
// Each [assembly: ...] attribute can only appear ONCE per assembly.
//
// [assembly: MapperDefaults] applies default settings to ALL mappers in the
// assembly. Individual mappers can override these defaults using class-level
// or method-level attributes (which take precedence).
//
// Precedence (highest to lowest):
//   1. Method-level attributes ([EnumMappingStrategy], [MapProperty], etc.)
//   2. Class-level attributes ([Mapper(StrictMapping = ...)], [EnumMappingStrategy])
//   3. Assembly-level [assembly: MapperDefaults]
//   4. Built-in defaults (StrictMapping = true, EnumMappingStrategy = ByName, EnumIgnoreCase = false)
// =============================================================================
using EricksonLopez.Mapper;

// Assembly-level enum configuration: all mappers will use ByName + IgnoreCase by default.
// Any mapper or method can override this with its own [EnumMappingStrategy] attribute.
[assembly: MapperDefaults(
    StrictMapping = true,
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumIgnoreCase = true)]
