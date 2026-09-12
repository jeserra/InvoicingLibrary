# Agent guide: implementing TRSF.Invoicing's interfaces

This library builds, seals, and (via a PAC) stamps Mexican CFDI 3.3/4.0
electronic invoices. Every external integration point — a PAC, a certificate
store, catalog validation, PDF rendering, QR generation, invoice storage — is
deliberately left as an interface under `TRSF.Invoicing/Interfaces/`, for the
consuming application to implement. This file exists so an AI assistant (or a
human skimming fast) can generate a *correct* implementation on the first try,
instead of guessing at a contract from the interface signature alone.

Read the XML doc comments on the interface itself first — they've been written
for this purpose and are kept up to date. This file adds the cross-cutting
context that doesn't fit in a single doc comment.

## The one thing that matters most: what's actually called

`CFDIBase`/`CFDIv33`/`CFDIv40` (the classes that build and seal a CFDI) only
take two interfaces in their constructor: **`ICertificatesRepository`** and
**`ISATProvider`**. Everything else (`ICatalogValidator`, `IPDFProvider`,
`IInvoiceStorageProvider`, `ILocalQRProvider`, `IQRProvider`) is invoked by
*your* calling code around the library, not by the library itself. Don't
assume an interface is wired into the seal/stamp flow just because it lives
next to `ISATProvider` in the same folder — check who actually calls it (a
`grep` for the interface name across the repo settles it in seconds).

Within the two constructor-required interfaces, not every member is on the hot
path either:

- `ICertificatesRepository.GetCertificate(string noCertificado)` is the only
  overload `CreateCFDI`/`SetSeal` call. The other two overloads
  (`GetCertificate(accountId, rfc)`, `SaveCertificate(...)`) are a legacy
  multi-account contract with no callers in this repo — a minimal
  implementation can throw `NotSupportedException` from them.
- `ICertificate.KeyFile` + `.Pwd` are what actually get read to sign (RSA,
  SHA-256, PKCS1 padding); `.CerFile` goes straight into the XML's
  `Certificado` attribute. `.idCertificate` is an unused legacy DB key.
  `.ValidFrom`/`.ValidUntil` are display-only, never checked against the
  invoice date.
- `ISATProvider.Timbrar` is only reached when a caller passes
  `Timbrado: true` to `CreateCFDI` — both consumers currently in this
  ecosystem (the demo web app, the MCP server) call it with `Timbrado: false`
  and never stamp. If your use case is seal-only too, a stub that throws
  `NotSupportedException` from both methods is a legitimate, complete
  implementation (see `NotConnectedSatProvider`/`DemoSatProvider` in the
  private consumers of this library for that exact pattern).

## Interfaces at a glance

| Interface | Required by the core classes? | Reference implementation in this repo | Notes |
|---|---|---|---|
| `ISATProvider` | Yes (constructor) | `TRSF.Invoicing.CFDIProviders.EcodexProvider` (SOAP) | Only needed for real if you actually stamp. |
| `ICertificatesRepository` / `ICertificate` | Yes (constructor) | none shipped here — implement against local files, a KMS, a DB, etc. | Only `GetCertificate(string)` is called. |
| `ICatalogValidator` | No — caller's responsibility | `TRSF.Invoicing.Catalogs.Sqlite.SqliteCatalogValidator` | Validate `ClaveProdServ`/`ClaveUnidad`/etc. before building the `Comprobante`; the library won't do it for you. |
| `IPDFProvider` | No — caller's responsibility | `TRSF.Invoicing.PDF.ItextPDFProvider` | Two overloads (CFDI 3.3 and 4.0) — the schemas differ enough (untyped `Complemento` in 4.0, decimal vs. enum `TasaOCuota`) that one doesn't trivially adapt to the other. |
| `ILocalQRProvider` | No — caller's responsibility | `TRSF.Invoicing.QRProviders.LocalQRProvider` | Builds the SAT verification URL and renders it locally (QRCoder) — no network call. Needs a real `uuid`, so it only makes sense once the CFDI is actually stamped. |
| `IQRProvider` | No — caller's responsibility | `TRSF.Invoicing.QRProviders.EcodexQRProvider` | REST call to a PAC for the same QR `ILocalQRProvider` builds offline. Kept as a reference client, not part of any active flow. |
| `IInvoiceStorageProvider` | No — caller's responsibility | none shipped here | Persist the sealed/stamped XML wherever you like (disk, blob storage, a DB). |
| `IFolioSerieSetter`, `IInvoicingRepository` | **No — legacy, unused** | test mocks only | Not referenced by `CFDIBase`/`CFDIv33`/`CFDIv40`. Don't implement these unless you're specifically extending the folio/serie or multi-account machinery they were designed for. |

## Practical guidance when generating an implementation

- **Match the interface's sync/async shape as-is.** Every interface here is
  synchronous except `IQRProvider` (the one with a `Task<byte[]>` return,
  kept for its REST reference client). Don't add `async`/`Task` to a
  synchronous interface member, and don't make `ILocalQRProvider` async just
  because "I/O usually is" — it does none.
- **Exceptions, not null/false, signal failure.** The shipped reference
  implementations (`EcodexProvider`, `AzureBlobInvoiceStorageProvider`-style
  patterns in the ecosystem, `SqliteCatalogValidator`) throw on failure
  rather than returning a sentinel value. Match that.
- **Stub what nothing calls.** If you only need to seal (not stamp), a
  one-line `throw new NotSupportedException(...)` on `ISATProvider`'s two
  methods is correct, not lazy — see the table above for exactly which
  members are actually on the hot path before spending effort elsewhere.
- **Domain vocabulary is Spanish and SAT-specific on purpose**: `RFC`, `CSD`,
  `Sello`, `Timbrado`, `Cadena Original`, `PAC`, `Comprobante`, `Receptor`,
  `Emisor` are the actual legal/technical terms from SAT's own CFDI spec —
  don't translate them in an implementation; naming things to match SAT's
  terminology is what makes the code traceable back to the spec.
- **Validate by building, not by reading.** `dotnet build` the project your
  new implementation lives in before calling it done — several of these
  interfaces (`ICertificate`, `cfdi33.Comprobante` vs `cfdi40.Comprobante`)
  have similarly-named members with different types across CFDI versions,
  and the compiler catches the mismatch a lot faster than a review pass will.
