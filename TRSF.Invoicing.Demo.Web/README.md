# TRSF.Invoicing.Demo.Web

A small ASP.NET Core minimal API + static HTML/JS demo that walks through the actual
end-user flow of `TRSF.Invoicing`, end to end, in a browser:

1. **Generate initial catalog** — runs `tools/GenerateCuratedCatalogDb` live against the
   real master `catalogs.sqlite`, using one of the industry sample manifests.
2. **Add emisor data with certificate** — RFC/Nombre/Régimen Fiscal, plus either the
   bundled SAT test CSD (default) or an uploaded `.cer`/`.key` pair.
3. **Create a new invoice** — parses a Constancia de Situación Fiscal PDF for the
   receptor's data, and picks a product/service from the curated catalog (with
   autocomplete) for a single concepto line.
4. **Generate sealed invoice** — builds and seals (not stamps) a real CFDI 4.0 XML via
   `CFDIv40.CreateCFDI(..., Timbrado: false)` — the same no-PAC path the test suite uses.

## Running it

```
dotnet run --project TRSF.Invoicing.Demo.Web
```

Then open the URL Kestrel prints (e.g. `http://localhost:5000`).

## Safety

- **Test/demo only.** Do not upload a real CSD. The default option uses SAT's own public
  test certificate (`TRSF.Invoicing.Test/Resources/CSD_Pruebas_CFDI_LAN7008173R5.*`,
  password `12345678a`) — the same one `CFDIv33Test`/`CFDIv40Test` already use.
- Nothing is timbrado (stamped) — this demo never contacts a PAC, matching the library's
  documented "test without a PAC" path (`Timbrado: false`).
- Uploaded certificates, keys, and the Constancia PDF are held **in memory only**, for the
  life of the session (an in-process dictionary, no database) — never written to disk.
  The curated catalog `.sqlite` a session generates *is* written to a temp file (SQLite
  needs a file), and is deleted when the session is removed from the store; sessions
  otherwise live for the process's lifetime (this is a local single-user demo, not
  something meant to run multi-tenant or stay up indefinitely).

## Notable implementation detail

`DemoCertificate` (this project only) sets `CerFile` to the full base64 of the raw `.cer`
file bytes — what CFDI's `@Certificado` attribute actually needs. The test double
`TRSF.Invoicing.Test.Certifcate.CertificateMoq` instead uses
`X509Certificate.GetPublicKey()`, which is only the public key, not the full DER
certificate; harmless there since no existing test byte-compares `@Certificado`, but
worth getting right here since the whole point of this demo is producing a plausible
sealed CFDI.
