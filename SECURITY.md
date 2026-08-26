# Security Policy

## Supported Versions

`EricksonLopez.Mapper` follows Semantic Versioning.

| Version | Supported          | Security Patch SLA |
| :--- | :--- | :--- |
| **1.x** | :white_check_mark: | Best effort (high priority) |
| **< 1.0** | :x: | None (Pre-release) |

---

## Reporting a Vulnerability

**DO NOT** report security vulnerabilities via public GitHub Issues or Discussions.

If you discover a potential vulnerability, please email **[ericksonlopezf@gmail.com](mailto:ericksonlopezf@gmail.com)**.
You can expect an initial response within **24 hours**.

Please include the following information in your report:
- The exact package ID and version of `EricksonLopez.Mapper` you are using.
- Your .NET target framework (e.g., `net8.0`, `net9.0`, `net10.0`).
- Whether you are compiling under NativeAOT (`PublishAot=true`).
- A clear, concise description of the vulnerability and its potential impact.
- Minimal reproduction steps (a C# code snippet or small repository).

### Disclosure Process

1. **Acknowledgment:** We will acknowledge receipt of your vulnerability report within 24 hours.
2. **Investigation:** We will investigate the issue and determine its severity and exploitability.
3. **Private Remediation:** If confirmed, we will develop and verify a private fix.
4. **Coordinated Release:** We will release a patch release across all supported versions and publish a GitHub Security Advisory.
5. **Credit:** We will publicly credit you in the release notes and advisory (unless you prefer anonymity).

---

## Supply Chain Security Guarantee

We implement comprehensive supply chain security controls across build and release pipelines:

- **OIDC Publishing**: Our GitHub Actions pipeline publishes to NuGet.org via OpenID Connect (`NuGet/login@v1`). No static, long-lived API keys are stored in repository secrets.
- **Sigstore Provenance Attestation**: All `.nupkg` packages receive cryptographic Sigstore provenance attestations (`actions/attest-build-provenance@v2`). Consumers can verify build integrity with GitHub CLI:
  ```bash
  gh attestation verify <package.nupkg> --repo ericksonlopezf/dotnet-mapper
  ```
- **Strong Name Signing**: Assemblies are signed with `EricksonLopez.snk` when the `SNK_KEY` CI secret is provided. The private key is stored only as a base64-encoded GitHub secret and decoded ephemerally at build time.
- **Automated Dependency Monitoring**: Dependabot continuously monitors NuGet dependencies and GitHub Actions workflows.

---

## Known Security Boundaries

`EricksonLopez.Mapper` is a **compile-time code generation ecosystem**. It emits clean, inspectable C# code during compilation and performs no runtime reflection, no network requests, and no dynamic code generation.

Security boundaries outside the scope of this library:
- **Input Validation**: Generated mapping code copies values between object graphs without domain sanitization. Input validation is the responsibility of your application domain or validator layers.
- **Serialization Safety**: If mapping untrusted input data, ensure destination objects are properly validated prior to persistence or exposure.
