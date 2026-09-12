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
`ICatalogValidator` (in `TRSF.Invoicing/Interfaces/`, core library, no extra dependency)
and its default implementation `TRSF.Invoicing.Catalogs.Sqlite.SqliteCatalogValidator`
(separate opt-in project, depends on `Microsoft.Data.Sqlite`). Every other catalog
referenced by CFDI 4.0 or the Pagos 2.0 complement (under 300 values each —
`c_RegimenFiscal`, `c_UsoCFDI`, `c_Moneda`, `c_Pais`, etc.) is a real,
compile-time-checked C# enum, generated normally from `catCFDI-slim.xsd`, which carries
only those smaller catalog definitions extracted from the full `catCFDI.xsd`.

**Note on descriptions**: `catCFDI.xsd` carries no human-readable text at all for any of
these five catalogs — every entry is a bare `<xs:enumeration value="X"/>` with no
`<xs:documentation>`. `SqliteCatalogValidator.ObtenerDescripcion` and the `Descripcion`
column in `catalogs.sqlite` exist to hold that text when it's available from elsewhere.
`Existe`/code validation is unaffected — it doesn't need a description.

SAT separately publishes a workbook (`catCFDI_V_4_*.xls`, from the "Formato de Factura
(Anexo 20)" page, not `catCFDI.xsd`) that does carry descriptions. For `ClaveProdServ` and
`ClaveUnidad` it's a clean one-to-one Codigo -> Descripcion map, and `tools/ImportCatalogDescriptions`
merges it into an existing `catalogs.sqlite` (see that tool's README). `CodigoPostal`,
`Colonia`, and `Municipio` are deliberately **not** covered by that tool: in the same
workbook their "descriptions" are scoped by a second key (Estado for Municipio,
CodigoPostal for Colonia; CodigoPostal itself has no description at all), so the same
Codigo legitimately means different things in different contexts — populating the current
single-column schema from it would silently pick one arbitrary name per code. Doing that
correctly would need a composite key, which is out of scope here.

## Regenerating

```
xsd.exe /c /l:CS /n:TRSF.Invoicing.cfdi40 cfdv40-local.xsd Pagos20-local.xsd catCFDI-slim.xsd tdCFDI.xsd catPagos.xsd
xsd.exe /c /l:CS /n:TRSF.Invoicing.cfdi40 valesdedespensa.xsd consumodecombustibles.xsd
```

## Regenerating the catalog database

`../../../TRSF.Invoicing.Catalogs.Sqlite/Data/catalogs.sqlite` is generated from the same
`catCFDI.xsd` by a small standalone tool (not part of the shipped library):

```
dotnet run --project tools/GenerateCatalogDb -- <path-to-catCFDI.xsd> TRSF.Invoicing.Catalogs.Sqlite/Data/catalogs.sqlite
```

Re-run this whenever SAT republishes `catCFDI.xsd` with updated catalog values.
