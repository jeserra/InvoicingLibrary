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
