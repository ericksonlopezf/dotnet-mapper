## Description

Brief description of the changes in this PR.

## Type of Change

- [ ] 🐛 Bug fix (non-breaking change that fixes an issue)
- [ ] ✨ New feature (non-breaking change that adds functionality)
- [ ] 💥 Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] 📖 Documentation update
- [ ] ♻️ Refactoring (no functional changes)
- [ ] ⚡ Performance improvement
- [ ] 🔧 Build / CI / infrastructure change

## Affected Package(s)

- [ ] `EricksonLopez.Mapper` (umbrella package)
- [ ] `EricksonLopez.Mapper.Abstractions` (attributes, interfaces)
- [ ] `EricksonLopez.Mapper.Generator` (Roslyn incremental source generator)
- [ ] `EricksonLopez.Mapper.Analyzers` (diagnostics, code fix providers)
- [ ] `EricksonLopez.Mapper.DomainPrimitives` (value object / strong ID converters)
- [ ] `EricksonLopez.Mapper.Mapster` (Mapster adapter bridge)
- [ ] `EricksonLopez.Mapper.Result` (functional Result monad extensions)
- [ ] Benchmarks / Tests / Sample

## Checklist

- [ ] My code follows the project's code standards
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] All new and existing tests pass (`dotnet test EricksonLopez.Mapper.slnx`)
- [ ] The mutation score has not decreased below the threshold (`pwsh ./run-stryker.ps1`)
- [ ] NativeAOT compatibility is maintained (zero `IL2026` / `IL3050` warnings)
- [ ] My commits follow [Conventional Commits](https://www.conventionalcommits.org) format
- [ ] I have updated documentation as needed (API Reference, XML docstrings, ADR if architectural)
- [ ] If a public API changed, `PublicAPI.Shipped.txt` or `PublicAPI.Unshipped.txt` has been updated

## Related Issues

Closes #<!-- issue number -->
