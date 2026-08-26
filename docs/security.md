# Security Architecture & Vulnerability Mitigation

## 1. Zero Runtime Dynamic Code Injection
By compiling all mappings into static C# source code at build time, `EricksonLopez.Mapper` eliminates runtime code generation attack vectors (`TypeFilterLevel`, deserialization gadget chains, dynamic IL injection).

## 2. Supply Chain Security & Attestation
- **NuGet Package Signing**: Strong-name signed using `EricksonLopez.snk`.
- **Sigstore Provenance**: Continuous Integration publishes cryptographically verifiable OIDC build provenance attestations on every release.
- **Vulnerability Scanning**: Continuous `dotnet list package --vulnerable` dependency auditing with `NuGetAuditMode=all`.
