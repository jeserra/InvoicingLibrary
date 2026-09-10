# CFDI 4.0 schema sources

These files were fetched from SAT's official schema hosting and used to generate
`../cfdv40.cs` and `../Complementos.cs` via `xsd.exe`. Kept here for reproducibility.

| File | Source |
|---|---|
| `cfdv40.xsd` | http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd (unmodified, for reference/validation) |
| `catCFDI-slim.xsd` | Extracted from `catCFDI.xsd` (http://www.sat.gob.mx/sitio_internet/cfd/catalogos/catCFDI.xsd) — see "Large catalogs" below |
| `tdCFDI.xsd` | http://www.sat.gob.mx/sitio_internet/cfd/tipoDatos/tdCFDI/tdCFDI.xsd |
| `Pagos20.xsd` | http://www.sat.gob.mx/sitio_internet/cfd/Pagos/Pagos20.xsd (unmodified, for reference) |
| `catPagos.xsd` | http://www.sat.gob.mx/sitio_internet/cfd/catalogos/Pagos/catPagos.xsd |
| `valesdedespensa.xsd` | http://www.sat.gob.mx/sitio_internet/cfd/valesdedespensa/valesdedespensa.xsd |
| `consumodecombustibles.xsd` | http://www.sat.gob.mx/sitio_internet/cfd/consumodecombustibles/consumodecombustibles.xsd |
| `cfdv40-local.xsd` | `cfdv40.xsd` with imports pointed at the local files above, and the five large catalogs (see below) retyped to `xs:string` |
| `Pagos20-local.xsd` | `Pagos20.xsd` with imports pointed at the local files above |

## Large catalogs are not embedded

SAT's `catCFDI.xsd` (as of this writing, ~6MB / 162k lines) inlines every value of every
catalog, including some genuinely enormous ones. Five of them are large/volatile enough
that generating them as C# enums would be impractical and would go stale the moment SAT
republishes the catalog:

| Catalog | Enumerated values |
|---|---|
| `c_CodigoPostal` | 95,777 |
| `c_ClaveProdServ` | 52,747 |
| `c_Colonia` | 9,999 |
| `c_ClaveUnidad` | 2,418 |
| `c_Municipio` | 570 |

These five are typed as plain `string` in `cfdv40-local.xsd` / the generated model, and are
validated at runtime against a SQLite catalog database instead of at compile time — see
`../../Catalogs/`. Every other catalog referenced by CFDI 4.0 or the Pagos 2.0 complement
(under 300 values each — `c_RegimenFiscal`, `c_UsoCFDI`, `c_Moneda`, `c_Pais`, etc.) is a
real, compile-time-checked C# enum, generated normally from `catCFDI-slim.xsd`, which
carries only those smaller catalog definitions extracted from the full `catCFDI.xsd`.

## Regenerating

```
xsd.exe /c /l:CS /n:TRSF.Invoicing.cfdi40 cfdv40-local.xsd Pagos20-local.xsd catCFDI-slim.xsd tdCFDI.xsd catPagos.xsd
xsd.exe /c /l:CS /n:TRSF.Invoicing.cfdi40 valesdedespensa.xsd consumodecombustibles.xsd
```
