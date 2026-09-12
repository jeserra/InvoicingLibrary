# ImportCatalogDescriptions

Merges human-readable descriptions into an existing master `catalogs.sqlite` (produced by
`tools/GenerateCatalogDb`) — see `TRSF.Invoicing/Schemas40/xsd/README.md` for why that
database exists and why `catCFDI.xsd` alone can't populate it.

## Source

SAT's "Formato de Factura (Anexo 20)" page publishes a separate workbook,
`catCFDI_V_4_<date>.xls` (tens of MB, not checked into this repo), that does carry
descriptions — unlike `catCFDI.xsd`, which is code-only.

```
http://omawww.sat.gob.mx/tramitesyservicios/Paginas/anexo_20.htm
```

Follow the "Iniciar" link and download the current `catCFDI_V_4_*.xls` from there; the
exact filename changes whenever SAT republishes the catalog.

## Scope: only ClaveProdServ and ClaveUnidad

Of the five large catalogs, only these two are a clean one-to-one `Codigo -> Descripcion`
map in the source workbook, so only these two are imported:

| Catalog | Source sheet | Description column |
|---|---|---|
| `ClaveProdServ` | `c_ClaveProdServ` | `Descripción` |
| `ClaveUnidad` | `c_ClaveUnidad` | `Nombre` (falls back to `Descripción` if blank — `Nombre` is the short label; `Descripción` is a longer technical definition and is often empty) |

`CodigoPostal`, `Colonia`, and `Municipio` are deliberately excluded: in the same workbook
their "descriptions" are scoped by a second key (Estado for Municipio, CodigoPostal for
Colonia — e.g. `c_Municipio` "001" is "Aguascalientes" in state AGU but "Ensenada" in
state BCN), and `CodigoPostal` has no description at all in the source, just references to
Estado/Municipio/Localidad codes. The current schema (`Codigo TEXT PRIMARY KEY`, single
column) can't represent a scoped name without a composite key — importing them here would
silently pick one arbitrary name per code.

## Usage

Merge-only: existing `Codigo` rows get their `Descripcion` updated in place. Codes present
in the workbook but absent from `catalogs.sqlite` are not inserted; codes in
`catalogs.sqlite` absent from the workbook keep whatever `Descripcion` they already had.

```
dotnet run --project tools/ImportCatalogDescriptions -- <path-to-catCFDI_V_4_*.xls> TRSF.Invoicing.Catalogs.Sqlite/Data/catalogs.sqlite
```

Re-run whenever SAT republishes the workbook, after re-running `GenerateCatalogDb` against
the matching `catCFDI.xsd`.
