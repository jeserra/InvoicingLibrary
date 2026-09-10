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
