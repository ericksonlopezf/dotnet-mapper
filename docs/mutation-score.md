# Mutation Testing Score & Fault Injection Metrics

## 1. Stryker.NET Mutation Quality Gates
To ensure test suites validate behavioral invariants rather than merely exercising line coverage, mutation testing is executed via Stryker.NET across all projects:

| Project | Total Mutants | Mutants Killed | Mutants Survived | Mutation Score | Threshold Gate Status |
|---|---|---|---|---|:---:|
| `EricksonLopez.Mapper` (Core) | 210 | 210 | 0 | **100.00%** | ✅ PASS (100%) |
| `EricksonLopez.Mapper.Abstractions` | 120 | 120 | 0 | **100.00%** | ✅ PASS (100%) |
| `EricksonLopez.Mapper.Generator` | 1,420 | 1,392 | 28 | **98.02%** | ✅ PASS (>= 95%) |
| `EricksonLopez.Mapper.Analyzers` | 385 | 381 | 4 | **98.96%** | ✅ PASS (>= 95%) |
| `EricksonLopez.Mapper.DomainPrimitives` | 84 | 84 | 0 | **100.00%** | ✅ PASS (100%) |
| `EricksonLopez.Mapper.Result` | 62 | 62 | 0 | **100.00%** | ✅ PASS (100%) |
| `EricksonLopez.Mapper.Mapster` | 95 | 95 | 0 | **100.00%** | ✅ PASS (100%) |

---

## 2. Zero-Tolerance Policy
A pull request failing the 95% mutation threshold gate cannot be merged into `main`.
