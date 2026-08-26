# Internationalization & Culture Invariants

## 1. Cultural Invariance in Numeric & Temporal Conversions
`EricksonLopez.Mapper` enforces `CultureInfo.InvariantCulture` for all generated numeric and temporal conversions (`DateTime.ToString()`, `decimal.Parse()`, etc.).

This guarantees that data serialized or mapped across distributed servers produces consistent representations regardless of the local server operating system locale.
