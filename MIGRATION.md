# Migration log: CFDI 3.3/.NET Framework 4.6.1 → CFDI 4.0/.NET 10

This is a running, chronological log of an AI-assisted migration of this library, kept
during the work rather than reconstructed afterward. Entries are grouped by phase; each
one links the commit(s) it corresponds to.

## Starting point (commit `de78b83`)

A prior audit pass (same session) had already:
- Replaced a signature-critical XSLT fetch from a since-deleted, unclaimed Azure Storage
  subdomain (with script execution and the `document()` function both enabled) with a
  locally vendored, verified-offline transform.
- Fixed a bug where per-concept discounts were never subtracted from the invoice total.
- Removed shared mutable static state that raced under concurrent use in the PAC QR
  provider and the PDF printer.
- Deleted a dead code path that carried a hardcoded CSD passphrase.
- Fixed ~20 build references that pointed at other private projects on the original
  developer's machine, which meant the repo could not build from a clean clone.

47 of 50 MSTest tests passing (3 pre-existing skips), on .NET Framework 4.6.1.

## Phase A — Port to .NET 10

Goal: identical CFDI 3.3 behavior, same test results, running on `net10.0` SDK-style
projects instead of the old-style .NET Framework project format.

**Result: 47 passed / 3 skipped / 0 failed — identical to the .NET Framework baseline —
including two tests with hardcoded expected values from before this migration started
(`OriginalChain33Test`'s cadena original string, `GetSealTest`'s RSA signature), both
matching byte-for-byte. That's not "it doesn't throw" — it's proof the rewritten crypto
and XSLT paths produce bit-identical output to the original.**

What changed, and why:

- **Both `.csproj` files converted from the old MSBuild format (packages.config,
  explicit `<Reference>`/`HintPath`, per-configuration property groups) to SDK-style**
  targeting `net10.0`. This alone deleted the entire class of "HintPath points at the
  wrong machine" bugs fixed earlier in the audit — SDK-style projects resolve
  dependencies through the lockfile, not hand-maintained paths.
- **Deleted ~15 NuGet packages that were only there as .NET-Framework-era BCL shims**
  (`System.Buffers`, `System.AppContext`, `System.Console`, `System.Net.Http`,
  `System.IO.Compression*`, `System.Security.Cryptography.*`, `System.Runtime.*`,
  `Microsoft.Win32.Primitives`, etc.) — all of that is simply part of .NET 10 already.
- **`Utils/SSLKey.cs` — a 470-line hand-rolled ASN.1/PKCS8 parser — deleted outright**,
  replaced by `RSA.ImportEncryptedPkcs8PrivateKey` (one BCL call). Along with it,
  `System.Security.SecureString` is gone from the whole codebase (`CFDIBase.SetSeal`,
  `ValidateCertificate.Validate`) — it's Windows-only in modern .NET and would have
  thrown `PlatformNotSupportedException` on Linux/macOS, which mattered given the goal
  of letting anyone test this. `RSAPKCS1SignatureFormatter`/`RSACryptoServiceProvider`
  (Windows-CryptoAPI-backed, legacy) were replaced with `RSA.Create()`/`RSA.SignHash` —
  cross-platform, backed by OpenSSL on Linux. `GetSealTest`'s hardcoded expected
  signature confirms this produces the exact same output as the code it replaced.
- **`ValidateCertificate.Validate`'s exception handling narrowed** from a blanket
  `catch (Exception)` to `catch (CryptographicException)`/`catch (FormatException)` —
  a real bug and a wrong password now look different from an actual code defect instead
  of both silently returning `false`.
- **Two dead `System.Drawing.Bitmap` helpers deleted** from `CFDIBase.cs`
  (`imageToByte`/`BmpToBytes_MemStream`) — confirmed zero callers first.
  `System.Drawing.Common` is Windows-only-supported on modern .NET, so removing this
  dead code also removed a cross-platform blocker that would otherwise have needed a
  real replacement (e.g. ImageSharp) for no functional gain.
- **`Newtonsoft.Json` removed entirely, replaced with `System.Text.Json`** (built into
  the framework, zero extra dependency) in `EcodexQRProvider` and three test files —
  this fully resolves the `GHSA-5crp-9r3c-p9vr` CVE the earlier audit flagged, by
  removing the vulnerable package rather than bumping it.
- **WCF client proxies (`Service References/*/Reference.cs`) kept, not regenerated
  from scratch** — the generated code turned out to compile against the modern
  `System.ServiceModel.Http`/`Primitives` NuGet packages almost as-is. The only break:
  three constructor overloads per client (`TimbradoClient`, `SeguridadClient`,
  `ClientesClient`) that resolved an endpoint by a named `<client><endpoint>` section in
  `app.config` — that concept doesn't exist the same way on modern .NET. Removed those
  overloads, kept the `(Binding, EndpointAddress)` one, and rewrote `EcodexProvider` to
  build the binding/endpoint explicitly in code (a `BasicHttpBinding` with
  `BasicHttpSecurityMode.Transport`, matching what the deleted app.config used to
  configure) instead of relying on config-file lookup by name.
- **Checked Ecodex's actual current infrastructure rather than assuming.**
  `pruebas.ecodex.com.mx` (the old SOAP endpoint `EcodexProvider` targets) has **no DNS
  record at all** as of this migration — dead. `pruebasapi.ecodex.com.mx` (the REST
  endpoint `EcodexQRProvider` targets) is alive, resolving to a real Azure Web App.
  `www.ecodex.com.mx` resolves but serves a mismatched TLS certificate. `EcodexProvider`
  and `EcodexQRProvider` both stay in the library per an explicit decision to keep them
  as reference implementations regardless — their code now compiles and is structurally
  correct, but the SOAP path has no live endpoint to test against today.
- **Removed both classes' dependency on `System.Configuration.ConfigurationManager`**
  (not available by default on modern .NET without an extra compat package, and reading
  the *consuming* application's config from a library works differently on .NET Core
  than it did on .NET Framework anyway). `EcodexProvider` and `EcodexQRProvider` now
  take their configuration (integrator ID, endpoint URLs) as constructor parameters.
- **`itext7` pinned to `7.2.6`** (the newest release still in the v7 line) rather than
  jumping to the v8/v9 line — turned out not to matter: the `SetWidthPercent`/
  `FontConstants`/`Color.BLACK`/implicit-string-to-`Cell` APIs the original code used
  were already removed by 7.2.6, so `PrintPDFService.cs` needed the same modernization
  either way (`UseAllAvailableWidth()`/`SetWidth(UnitValue...)`, `StandardFonts.HELVETICA`,
  `ColorConstants.BLACK`/`WHITE`, explicit `Paragraph` wrapping).
  `itext7.pdfhtml`/`itext7.licensekey` dropped entirely — confirmed zero usages anywhere
  in the codebase.
- **Three more dead methods found and deleted** in the same pass: `PrintPDFService.
  PrintCFDI()` and `.ConvertXMLtoHTML()` (hardcoded dev-machine-relative paths like
  `..\\..\\Resources\\output.pdf`, never called from anywhere) and `.Print(string)`
  (`return null;`, likewise uncalled).
- **A fourth previously-missed hardcoded secret found**: `Invoicing.Test/SealChilkat/
  SealChilkat.cs` — an orphaned alternative sealing implementation using a third-party
  paid `Chilkat` library, with a hardcoded unlock code (`"ECODEXRSA_D5WpF9zunPvz"`).
  This file was never even in the old project's explicit `<Compile Include>` list, so
  it silently never built under .NET Framework either — pure dead source. Deleted.
- **`CreateCFDITest`'s side effect of writing a dated `.xml` file into the source tree's
  `Resources/` folder on every run** — changed to write to `Path.GetTempPath()` instead;
  also removed a pointless `try { ... } catch (Exception) { Assert.Fail(); }` wrapper
  that only threw away the real exception/stack trace MSTest would otherwise report.
- **`CertificateMoq`'s hardcoded `..\\..\\Resources\\...` relative path** — assumed the
  test runner's working directory was `bin\Debug\` (true on .NET Framework); SDK-style
  builds add a TFM segment (`bin\Debug\net10.0\`), which broke that assumption. Fixed
  to resolve from `AppContext.BaseDirectory` with `Resources/` copied to output via the
  new csproj's `CopyToOutputDirectory`. Also swapped the obsolete
  `new X509Certificate(path)` constructor for `X509CertificateLoader.LoadCertificateFromFile`.
- Removed the stray internal Word document (`Bitácora mensual de avances técnicos Abril
  2018.docx`) and a committed PDF build artifact from `Invoicing.Test/Resources` and
  `/Output` — neither belonged in a repo about to go public.

## Interlude — rebrand under the TRSF umbrella namespace

Before starting Phase B, renamed everything from `Invoicing`/`ProcessCFDI` to sit under
a `TRSF` umbrella namespace, ahead of the public release:

- `Invoicing.*` → `TRSF.Invoicing.*` everywhere (namespaces, `using` statements,
  fully-qualified references) across both projects.
- `ProcessCFDI.Utils` and the bare `ProcessCFDI` namespace (`Security.cs`, `General.cs`,
  `UNCAccessWithCredentials.cs`, `ValidateXML.cs`) folded into `TRSF.Invoicing.Utils` —
  these had inconsistently lived under a different root than the rest of `Utils/` since
  before this migration started; unifying them was a natural side effect of touching
  every namespace anyway.
- Project folders and files renamed to match: `Invoicing/` → `TRSF.Invoicing/`,
  `Invoicing.csproj`/`.sln` → `TRSF.Invoicing.csproj`/`.sln`, and the same pattern for
  the test project. `AssemblyName`/`RootNamespace` updated in both `.csproj` files; the
  `.sln`'s stale `x64`/`stage` platform configs (leftover from the pre-SDK-style project,
  meaningless now) were dropped down to just `Debug`/`Release|Any CPU` while the file was
  already being rewritten for the new names.
- Verified with the same regression tests as Phase A: 47 passed / 3 skipped / 0 failed,
  unchanged.
- One rough edge: renaming the `Invoicing/` folder itself hit a Windows file lock from
  an external process (never identified — not a build server, not a dotnet process);
  worked around by moving its contents into the new `TRSF.Invoicing/` folder instead of
  renaming the directory in place. The old, now-empty `Invoicing/` folder may need a
  manual delete once whatever holds it open is closed.

## Interlude — test project: MSTest → xUnit v3

- `[TestClass]` removed, `[TestMethod]` → `[Fact]`, `Assert.AreEqual`/`IsTrue`/`IsFalse`/
  `IsNotNull` → `Assert.Equal`/`True`/`False`/`NotNull`, `Assert.IsInstanceOfType(x,
  typeof(T))` → `Assert.IsType<T>(x)`.
- `[TestInitialize]` methods (5 of them) converted to constructors — xUnit creates a
  fresh test class instance per test, so the constructor *is* the per-test setup hook;
  there's no direct attribute equivalent.
- The 3 `[Ignore]`d tests became `[Fact(Skip = "...")]` with an actual reason each,
  written from what the test itself revealed was wrong (two assert an enum's `.ToString()`
  equals a bare numeric string it never will; one is timezone-offset-dependent per its
  own inline comment) — the original `[Ignore]` attributes carried no reason at all.
- Deleted a stale MSTest-template `TestContext` property block from
  `TranslateModelToCFDIUnitTest.cs` (VS-generated boilerplate, unused, and `TestContext`
  doesn't exist outside MSTest).
- Chose **xUnit v3** (not v2) — it's the actively developed line and, unlike v2, runs
  natively on `Microsoft.Testing.Platform` (MTP) rather than the legacy VSTest adapter.
- Hit a .NET 10 SDK change along the way: `dotnet test` on .NET 10 no longer supports
  the VSTest execution path by default. Fixed by adding a `global.json` at the repo root
  with `"test": { "runner": "Microsoft.Testing.Platform" }` — the opt-in is a `global.json`
  setting, not a project-level MSBuild property.
- Verified: **47 passed / 3 skipped / 0 failed** — same result as MSTest, with the skip
  reasons now visible directly in `dotnet test` output instead of silently disappearing.
- Noted but not fixed (pre-existing, out of scope for this migration): xUnit's analyzer
  package flagged several test-quality smells that MSTest's own analyzer had also
  flagged after the .NET 10 port — swapped `expected`/`actual` argument order in several
  `Assert.Equal` calls, a couple of `Assert.Equal(true/false, x)` that should be
  `Assert.True`/`False`, and a helper method on a couple of test classes that's public
  but not itself a test.

## Interlude — pluggable logging

Added `Microsoft.Extensions.Logging.Abstractions` (interfaces only, no concrete
provider — same pluggable-interface philosophy already used for `ISATProvider`/
`IQRProvider`/`ICertificatesRepository`) and threaded an optional `ILogger`/`ILogger<T>`
constructor parameter through `CFDIBase` (and `CFDIv33`, which derives from it),
`EcodexProvider`, and `EcodexQRProvider`. Defaults to `NullLogger` when not supplied, so
every existing call site — including every test that constructs these classes with the
old constructor arity — keeps working unchanged (verified: 47 passed / 3 skipped / 0
failed, same as before).

Logging was added only where it adds real diagnostic value, not sprinkled everywhere:
- `CFDIBase`: before throwing on a failed cadena-original XSLT transform, and before
  throwing on a CSD private-key decryption failure.
- `CFDIv33.Timbrar`: an info-level line before sending a comprobante to the PAC and
  after it comes back stamped with a UUID.
- `EcodexProvider`: an error-level line in each SOAP fault handler (`FallaServicio`/
  `FallaSesion`/`FallaValidacion`), which previously just re-threw with no logging at all.
- `EcodexQRProvider`: replaced two raw `Console.WriteLine`/`Console.Write` calls with
  proper logger calls — a library should never write directly to the console, and this
  was the only place in the codebase still doing so.

README gained a short "Logging" section showing how to wire NLog (or any other
`Microsoft.Extensions.Logging`-compatible provider) from the consuming application.

## Phase B — CFDI 3.3 → CFDI 4.0

Goal: produce schema-valid, correctly-sealed CFDI 4.0 documents. CFDI 3.3 stays
functional in parallel until the new 4.0 path is proven end-to-end — `CFDIv32`/
`Schemas32` in particular can't be deleted yet, because the *current* CFDI 3.3 creation
path still depends on the complement types living in `Schemas32/valesdedespensa.cs` and
`consumodecombustibles.cs` (they were generated into the `cfdi33` namespace but filed
under a differently-named folder; deleting that folder before the 4.0 translate layer
is repointed at the new complement classes would break the still-working 3.3 path).

### Schema classes generated (commit follows)

Fetched SAT's current official schemas and generated `Schemas40/cfdv40.cs` (core
`Comprobante` graph, Pagos 2.0 complement) and `Schemas40/Complementos.cs`
(ValesDeDespensa, ConsumoDeCombustibles — unchanged, version-independent complements,
same classes CFDI 3.3 already reuses) via `xsd.exe`. Confirmed present and correctly
typed: the three new CFDI 4.0 mandatory fields (`Comprobante.Exportacion`,
`Receptor.RegimenFiscalReceptor`, `Receptor.DomicilioFiscalReceptor`).

The one real design decision in this step: SAT's shared catalog schema (`catCFDI.xsd`)
is ~6MB / 162k lines because it inlines every value of every catalog — including
`c_ClaveProdServ` (52,747 entries) and `c_CodigoPostal` (95,777 entries). Generating
those as C# enums, the way the CFDI 3.3 schema classes do, would produce a
multi-megabyte file that goes stale the moment SAT republishes the catalog — precisely
the "catalogs are hardcoded snapshots that will drift" problem flagged in the original
audit. Five catalogs over 300 enumerated values (`c_CodigoPostal`, `c_ClaveProdServ`,
`c_ClaveUnidad`, `c_Colonia`, `c_Municipio`) are typed as plain `string` instead, with
runtime validation against a SQLite catalog database (in progress — see
`Schemas40/xsd/README.md` for the exact catalogs and sizes, and how to regenerate).
Every catalog under that threshold stays a real, compile-time-checked C# enum, extracted
into a slim `catCFDI-slim.xsd` fed to `xsd.exe` alongside a locally-rewritten
`cfdv40-local.xsd`/`Pagos20-local.xsd` (imports pointed at local files; the five large
catalogs retyped to `xs:string`). The unmodified official `cfdv40.xsd`/`Pagos20.xsd` are
also vendored, for later schema validation against SAT's real, stricter contract.

### SQLite catalog validation

Added `ICatalogValidator` to the core library (`TRSF.Invoicing/Interfaces/` — just the
interface and a `CatalogoGrande` enum naming the five demoted catalogs, zero extra
dependency) plus a new, separate, opt-in project `TRSF.Invoicing.Catalogs.Sqlite` with
the default implementation, matching the same pluggable-package pattern already planned
for the Azure Key Vault certificate repository — the core library never gains a hard
SQLite dependency.

Wrote a small standalone tool (`tools/GenerateCatalogDb`) that streams `catCFDI.xsd`
with `XmlReader` (not a DOM load — the file is ~6MB) and populates
`TRSF.Invoicing.Catalogs.Sqlite/Data/catalogs.sqlite` with one table per large catalog.
Ran it once: **161,511 rows inserted** (95,777 + 52,747 + 2,418 + 9,999 + 570 — exactly
matching the enumeration counts from the schema). `CodigoPostal` deduplicates down to
95,749 distinct rows (`Codigo` is a primary key; 28 codes appear more than once in SAT's
source with no distinguishing data). Discovered along the way: `catCFDI.xsd` carries no
per-value description text at all for any of these five catalogs — every entry is a bare
`<xs:enumeration value="X"/>` — so `Descripcion` is schema-ready but empty today;
documented in `Schemas40/xsd/README.md` rather than silently shipping a column that
looks populated but isn't.

Added 5 tests (`TRSF.Invoicing.Test/Catalogs/SqliteCatalogValidatorTest.cs`) against the
real generated database — known-good codes in three of the five catalogs, a made-up
code, and null/empty input. Verified: **52 passed / 3 skipped / 0 failed** (up from 47/3,
the 5 new tests all passing), confirming the ~5MB database round-trips correctly through
the project's `CopyToOutputDirectory` content-propagation chain.

### BindingModels gap audit

Compared every `BindingModels` class field-by-field against the real generated
`Schemas40` classes. Findings, purely additive (nothing removed yet — see below):

- **Must add** (new CFDI 4.0 mandatory fields): `Comprobante.Exportacion` (defaults to
  `"01"`, domestic), `Receptor.RegimenFiscalReceptor`, `Receptor.DomicilioFiscalReceptor`,
  and one easy to miss — `Concepto.ObjetoImp`, a **per-concept** mandatory field. (The
  generated schema code is how this was confirmed rather than assumed: xsd.exe only
  emits a `FooSpecified` companion property for *optional* value-type attributes, and
  `ObjetoImp` has none, meaning the schema itself marks it required.)
- **Should add** (new-in-4.0 optional, real scenarios): `Emisor.FacAtrAdquirente`
  (third-party issuance), `Receptor.ResidenciaFiscal`/`NumRegIdTrib` (foreign
  receptors), `Comprobante.TipoCambio`, `Comprobante.Confirmacion`.
- **Modeling fix**: `UsoCFDI` belongs on `cfdi:Receptor` in the real schema, not
  `cfdi:Comprobante` — the old wrapper had it on `Comprobante`. Added
  `Receptor.UsoCFDI` for the new 4.0 path rather than moving the old one, since the
  *currently still-active* CFDI 3.3 creation path (`CFDIv33`/`TranslateModelToCFDI`)
  reads `Comprobante.UsoCFDI` — removing it now would break the one working creation
  path before the 4.0 replacement exists. Both properties coexist until 3.3 is retired.
- **Deliberately not modeled**: `InformacionGlobal`, `CfdiRelacionados`,
  `ACuentaTerceros`, `InformacionAduanera`, `CuentaPredial`, `Parte[]`, `Addenda` — all
  real CFDI 4.0 schema elements, but none of them were in this library's scope even for
  3.3 (global/simplified invoices, related-CFDI references, third-party billing, customs
  info, real estate, bundled parts, free-form vendor extensions). Not a 4.0-specific
  regression; flagged here so it's a documented decision rather than a silent gap.

Verified: 52 passed / 3 skipped / 0 failed, unchanged — confirms every addition here is
purely additive and the still-active CFDI 3.3 path is untouched.

## Interlude — build a Receptor from a Constancia de Situación Fiscal (CSF)

Requested feature, timed well against the new CFDI 4.0 mandatory receptor fields: SAT's
own "Constancia de Situación Fiscal" PDF is the canonical source of exactly the two new
required fields (`RegimenFiscalReceptor`, `DomicilioFiscalReceptor`), which are
otherwise annoying for callers to collect reliably.

Grounded the parser in a real sample CSF the user provided locally (their own document —
never committed, and no real RFC/CURP/name from it appears anywhere in the repo; every
test fixture uses entirely invented identities that mirror the same document
*structure*). Verified the exact text iText7 (already a project dependency, via
`PrintPDFService`) produces from that real PDF before writing any regex, rather than
guessing at line-break/whitespace behavior.

Design: `PdfConstanciaFiscalReader` is a thin wrapper that only extracts text via
iText7; all the actual parsing logic lives in `ConstanciaFiscalTextParser.Parse(string)`,
which is fully unit-testable without a PDF at all. A CSF can list more than one active
regime at once (the sample document has two: salaried employment since 2015, plus a
professional-services activity added in 2022) — `ConstanciaSituacionFiscal.RegimenPrincipal()`
picks the most recently started one with no end date as a sensible default, and
`ToReceptor(usoCFDI, regimenFiscalCodigo)` lets a caller override that when the invoice
needs a different one of the taxpayer's regimes.

One real gap found along the way: the CSF prints each regime as free text (e.g.
"Régimen de Sueldos y Salarios e Ingresos Asimilados a Salarios"), not as its catalog
code ("605") — added `CatalogoRegimenFiscalTexto`, a description→code lookup for all 23
`c_RegimenFiscal` values, with a substring-match fallback for minor wording variance
(e.g. the CSF says "las Personas Físicas con Actividades..." — with "las" — while SAT's
own catalog text for code 612 doesn't include it).

Field recognition is best-effort against the current CSF layout (not a versioned,
stable format) — unrecognized fields come back `null` rather than throwing, and the
persona-moral case (`Denominación o Razón Social:` instead of `Nombre (s)`/apellidos) is
handled from domain knowledge, not a second real sample.

Verified twice: 6 new unit tests against synthetic text covering persona física, persona
moral, and the "everything unrecognized" case (58 passed / 3 skipped / 0 failed
overall, up from 52/3); and once more, transiently, against the real sample PDF end to
end through the actual `PdfConstanciaFiscalReader` — both regimes recognized with the
correct codes (605, 612), `RegimenPrincipal()` correctly picked 612 (the more recent of
the two), and the resulting `Receptor` carried the real postal code through correctly.
That verification run printed only boolean "was this recognized" checks and the postal
code (not sensitive on its own) — never the real RFC/CURP/name — and used no repo files.

## Phase B (continued) — translate layer, cadena original 4.0, and a working CFDI 4.0 creation path

With the gap-audited `BindingModels` in place, wrote the actual `Translates40/` layer
that turns them into `cfdi40.Comprobante` object graphs, then wired it end to end so a
CFDI 4.0 document can actually be built and sealed — mirroring the existing `Translates`/
`CFDIv33` structure one file at a time rather than one big rewrite, so each piece stayed
individually reviewable.

**Pagos 2.0 is a real schema rewrite, not a version bump** (as flagged in the plan):
`DoctoRelacionado` lost `MetodoDePagoDR`, renamed `TipoCambioDR`→`EquivalenciaDR`, and
gained a new mandatory `ObjetoImpDR`; `ImpSaldoAnt`/`ImpPagado`/`ImpSaldoInsoluto` went
from optional to mandatory; and a new mandatory `Pagos.Totales` element was added.
`TranslatesModelsToPagos20.cs` computes `Totales.MontoTotalPagos` as the sum of each
payment's `Monto`, but deliberately does **not** populate the optional per-payment/
per-document tax breakdowns (`ImpuestosP`, `ImpuestosDR`, the IVA breakdown in `Totales`)
— `BindingModels.Pagos` doesn't carry data granular enough to compute them correctly,
and omitting an optional field is safer than inventing a tax breakdown that might be
wrong. `ObjetoImpDR` was added to `BindingModels.Pagos.DoctosRelacionados` to satisfy the
new mandatory field.

**`cfdi40.ComprobanteComplemento.Any` is `XmlElement[]`** (pre-serialized), unlike
cfdi33's polymorphic `object[] Items` — each complement (ValesDeDespensa,
ConsumoDeCombustibles, Pagos) now has to be serialized to its own `XmlElement` via
`XmlSerializer` + `XmlDocument.CreateNavigator().AppendChild()` before being attached,
handled by a small `SerializarComoElemento<T>` helper in `TranslateModelToCFDI40`.

**Two pre-existing 3.3 bugs found and fixed while porting, not carried forward:**
- `TranslateModelsToTotalImpuestos.cs` (3.3) never set the required `Base` attribute on
  Comprobante-level `Traslado`, and grouped totals only by `Impuesto` — two traslados of
  the same tax at different rates (e.g. IVA 16% and IVA 8%) would have been merged into
  one incorrect total. The 4.0 version (`TranslateModelsToTotalImpuestos40.cs`) sets
  `Base` and groups by `Impuesto` **and** `TasaOCuota` together.
- `CFDIv33.CreateCFDI(..., Timbrado: false)` returned the XML **before** `Sello` was set
  — silently defeating the documented "test without a PAC" workflow (Phase A/B plan:
  "generate and digitally seal a valid CFDI without any PAC account"). Fixed in both
  `CFDIv33` and the new `CFDIv40` to return the sealed XML.

**A C# namespace-ambiguity trap, caught by the compiler, not by inspection:**
`TranslateModelToCFDI40.cs` lives in `TRSF.Invoicing.Translates40`, a sibling of both
`TRSF.Invoicing.BindingModels` and `TRSF.Invoicing.cfdi40` — both of which declare a
`Comprobante` type. With `using TRSF.Invoicing.BindingModels;` in scope, an unqualified
`Comprobante` parameter silently resolved to `cfdi40.Comprobante` instead (enclosing-
namespace member lookup wins over `using` directives in C#), producing ~40 cascading
"does not contain a definition for ..." errors with misleading case-mismatched member
names. Fixed by dropping the `using` and qualifying every reference as
`BindingModels.Comprobante` explicitly.

**Vendored the CFDI 4.0 cadena original XSLT** the same way Phase A vendored 3.3's:
fetched SAT's current `cadenaoriginal_4_0.xslt` plus five includes not present in the
3.3 tree (`ComercioExterior20`, `Pagos20`, `CartaPorte30`, `CartaPorte31`,
`HidrocarburosPetro`/`hidrocarburospetroliferos` — CFDI 4.0 dropped `terceros11`,
`consumodecombustibles` v1, `ecc11`, and `CartaPorte` v1 from the chain entirely),
rewrote every `xsl:include` from SAT's absolute URLs to the same relative-path
convention already used under `xslt/cfd/`, and confirmed zero remaining
`sitio_internet` references before committing. `CFDIBase` gained a
`CadenaOriginal40XsltPath` static property and version-agnostic `GetOriginalChain`/
`SetSeal` overloads that take an explicit XSLT path, so both CFDI versions now share one
implementation instead of duplicating the transform/seal logic.

**Built `CFDIv40`**, mirroring `CFDIv33`: `GetXML` (with the `cfdi:4`/`Pagos20`
namespaces), `CreateCFDI` (translate → seal via the 4.0 cadena original → optionally
timbrar), `Timbrar`, `DeserializeXML`.

**New tests** (`CFDIv40Test.cs`), following the same regression-proof pattern
`OriginalChain33Test` already established for 3.3:
- `CreateCFDITest` — builds and seals a full CFDI 4.0 sample end to end, asserting the
  returned XML actually carries a non-empty `Sello` (this is exactly the bug described
  above — the test would have caught it).
- `OriginalChain40Test` — a hardcoded expected cadena original string against a hand-
  built CFDI 4.0 XML sample, run once through the vendored 4.0 XSLT to capture the
  real output before hardcoding it as the regression baseline.
- `ValidatesAgainstCfdi40Schema` — schema-validates the generated sample against the
  vendored `cfdv40-local.xsd` (the same local/slim schema set used to generate
  `Schemas40/cfdv40.cs`, so no live network call is needed). Along the way, hit and
  documented a real .NET behavior difference: `XmlSchemaSet.XmlResolver` defaults to
  `null` on modern .NET (unlike .NET Framework's implicit `XmlUrlResolver`), so
  relative `<xs:import schemaLocation="...">` references silently fail to resolve and
  every type from the unresolved namespace reports as "not declared" — a confusing
  symptom with a one-line fix (`schemaSet.XmlResolver = new XmlUrlResolver();`) once
  traced back to the actual cause via a standalone isolation repro.

Verified: 64 total / 61 passed / 3 skipped / 0 failed (up from 58/3), including the new
CFDI 4.0 creation, cadena original, and schema-validation tests, with the CFDI 3.3 path
untouched except for the `Timbrado: false` seal-ordering fix described above.

**Still open before Phase B is complete**: CFDI 3.2 (`Schemas32`/`CFDIv32`) and the
duplicated `Comprobante.UsoCFDI` are still present — both stay until the 4.0 path has
seen real-world exercise, per the plan's explicit sequencing decision.

## Interlude — curated/segmented `ICatalogValidator`

Requested feature: most real deployments only ever need a slice of the full national
catalog (161,511 rows) — a single business sells a handful of product categories and
operates in a few postal codes. Rather than shipping/validating against the entire
catalog, added `tools/GenerateCuratedCatalogDb`, a build-time tool that derives a small
curated `.sqlite` from the master one, driven by a declarative JSON manifest.

Two decisions made explicit before building this (see the plan's "Curated/segmented
`ICatalogValidator`" section):
- **Geographic grouping (state → municipio → colonia → codigo postal) is deferred.** The
  vendored `catCFDI.xsd` has zero hierarchy for these three catalogs — flat code lists,
  no state/municipio relationship, no documentation (same limitation already noted in
  `Schemas40/xsd/README.md`). Real state-based grouping would need a second, richer
  geographic dataset (SAT or INEGI) vendored separately — out of scope here. Curation for
  `CodigoPostal`/`Colonia`/`Municipio` is therefore **explicit-code-list only**.
- **`ClaveProdServ` needs no new data source for "business category" grouping.** SAT's
  catalog is UNSPSC-based: the 8-digit code is itself hierarchical, with digits 1-2
  identifying the segment (business category). Verified directly against the real
  `catalogs.sqlite` before relying on this: all 52,747 codes are exactly 8 digits, with
  58 distinct 2-digit segment prefixes (segment `50`, food & beverage, dominates with
  17,974 codes — consistent with SAT's known catalog composition). So curation here is
  just a prefix match on data already present, no vendoring needed.

**No new runtime class.** The curated `.sqlite` the tool produces has the exact same
schema as the master one (`Codigo TEXT PRIMARY KEY, Descripcion TEXT` per table), so the
existing `SqliteCatalogValidator` works against it completely unchanged — construct it
with the curated file's path instead of the master's. The deliverable is one new tool
plus a manifest format, not a new package.

The manifest is one optional JSON section per `CatalogoGrande` member; omitting a
catalog's section copies that catalog through unfiltered (curation is opt-in per
catalog). `segmentos` (prefix match, `ClaveProdServ` only) and `codigos`/
`codigosAdicionales` (exact allowlist, every catalog) can combine within one section.
Full format and rationale in `tools/GenerateCuratedCatalogDb/README.md`.

Verified: a new `CuratedCatalogGeneratorTest` (3 tests, against a small synthetic fixture
master db, not the real 5MB one — kept fast and self-contained) confirms segment-prefix
matching, explicit-code-list matching, and pass-through of catalogs omitted from the
manifest, going through the real `SqliteCatalogValidator` end to end rather than
asserting against the generator's internal state. Also manually run once against the
real `catalogs.sqlite` with a sample manifest (segments `50`+`72`, two explicit postal
codes) and cross-checked real codes both inside and outside the curated subset resolve
correctly. 67 total / 64 passed / 3 skipped / 0 failed (up from 64/3) — purely additive,
no changes to `TRSF.Invoicing` core or the shipped `Catalogs.Sqlite` runtime code.

**Follow-up**: pre-documented the segment-picking side of this. Queried the real
`catalogs.sqlite` for the complete list of `ClaveProdServ` segment prefixes actually
present (58, matching the earlier count) and cross-referenced against the public UNSPSC
standard SAT's catalog is based on (plus two point-checks against SAT's own catalog data
via a third-party mirror, not the empty `catCFDI.xsd`: segment `01` is exactly the
generic placeholder code `01010101` = "No existe en el catálogo", segment `95` is
land/buildings/structures) to write `samples/segmentos-claveprodserv.md` — every segment
present in the vendored data, named, with its code count, explicit about which names are
externally-sourced rather than from SAT's own (undocumented) XSD, and honest about the
one segment (`64`, a single code) no source could identify.

Added seven ready-to-run sample manifests under `samples/` (restaurant, grocery,
hardware/construction, professional services, medical office, software/IT, freight —
each with a `_notas` field explaining the segment choice), covering both curated-
geography examples (restaurant, grocery, medical, hardware — explicit postal code lists)
and deliberately-uncurated-geography ones (consulting, software, freight — a business
that bills nationally has no reason to restrict `CodigoPostal`). Every code in every
sample (segments, `ClaveUnidad` codes, postal codes) was checked against the real
`catalogs.sqlite` before being written down, then all seven manifests were actually run
through the generator against the real catalog as a final check, not just eyeballed.

## Interlude — small web demo (`TRSF.Invoicing.Demo.Web`)

Requested feature: a runnable, in-browser walkthrough of the real end-user flow, both as
a "does this actually all work together" check and as a live showcase for the "anyone can
test this" goal. New project, minimal API + a single static HTML/JS page (no build
tooling, no framework), four steps mapping directly onto pieces already built and tested
separately this session: generate a curated catalog (`GenerateCuratedCatalogDb`, run
live against the real master `catalogs.sqlite`), collect emisor + certificate data,
parse a Constancia de Situación Fiscal PDF for the receptor and pick a product/service
from the curated catalog via autocomplete, then seal (not stamp) a real CFDI 4.0 via
`CFDIv40.CreateCFDI(..., Timbrado: false)`.

State is a single in-process `ConcurrentDictionary` (`DemoSessionStore`) — no database,
no auth, one session per browser tab. Nothing uploaded (cert, key, CSF PDF) is ever
written to disk; the per-session curated `.sqlite` is (SQLite needs a file) but gets
deleted when its session is removed from the store.

**Real correctness bug fixed along the way, not carried over from the test double**: the
existing `CertificateMoq` test helper sets `CerFile` from
`X509Certificate.GetPublicKey()` — only the public key, not the full DER certificate
`cfdi:Comprobante/@Certificado` actually needs. Harmless in the test suite (nothing
byte-compares `@Certificado` there), but would have produced a CFDI with a garbage
`@Certificado` attribute here. The demo's own `DemoCertificate.FromBytes` sets `CerFile`
to the full base64 of the raw `.cer` file bytes instead — confirmed correct by inspecting
the sealed output end to end (see verification below). `CertificateMoq` itself was left
untouched; this is a new, separate class.

**A recurring namespace gotcha, in a new place**: unqualified `Comprobante` inside
`Program.cs` resolved to `cfdi40.Comprobante` instead of `BindingModels.Comprobante`,
the same failure mode documented earlier for `TranslateModelToCFDI40.cs` — except this
time in a top-level-statements file with no explicit `namespace` block. `RootNamespace`
in the `.csproj` (`TRSF.Invoicing.Demo.Web`, nested under the `TRSF.Invoicing.*` tree)
turned out to still give the compiler an ambient namespace for top-level statements,
enough to trigger the same enclosing-namespace lookup. Fixed the same way: fully qualify
`TRSF.Invoicing.BindingModels.Comprobante`/`Receptor`/`Concepto` at the construction site
rather than relying on the `using`.

Verified by actually running the app (`dotnet run`) and exercising every endpoint via
curl end to end, not just building it: generated the restaurant catalog (18,067/52,747
`ClaveProdServ` rows kept), loaded the bundled test CSD, sealed a real single-concepto
CFDI 4.0 (2 × $150 + 16% IVA = $348.00 `Total`, correct `NoCertificado`, non-empty
`Sello`, and — the point of the `CerFile` fix above — a full, correct `@Certificado`),
downloaded it as a file, and confirmed the error paths return clean 400/404s instead of
crashing: an invalid `ClaveProdServ` against the curated catalog, an unknown session id,
and a non-PDF upload to the Constancia endpoint ("PDF header not found").
