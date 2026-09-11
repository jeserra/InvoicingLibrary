# GenerateCuratedCatalogDb

Derives a small, curated `catalogs.sqlite` for a single deployment from the full master
catalog produced by `tools/GenerateCatalogDb` — see `TRSF.Invoicing/Schemas40/xsd/README.md`
for why the 5 large CFDI 4.0 catalogs (`ClaveProdServ`, `ClaveUnidad`, `CodigoPostal`,
`Colonia`, `Municipio`) are validated at runtime via SQLite instead of C# enums.

Most deployments only ever need a slice of the national catalog: a single business sells a
handful of product categories and only operates in a few postal codes. This tool shrinks the
shipped database to that slice, and doubles as a compliance allowlist — codes outside the
curated subset are rejected even though they're valid nationally.

## Usage

```
dotnet run --project tools/GenerateCuratedCatalogDb -- <path-to-master-catalogs.sqlite> <path-to-manifest.json> <output-curated.sqlite>
```

The output database has the **exact same schema** as the master one (`Codigo TEXT PRIMARY KEY,
Descripcion TEXT` per table), so `TRSF.Invoicing.Catalogs.Sqlite.SqliteCatalogValidator` works
against it completely unchanged — just point it at the curated file instead of the master one.

## Manifest format

One optional section per catalog. **Omitting a catalog's section copies that catalog through
unchanged** — curation is opt-in per catalog, not all-or-nothing:

```json
{
  "claveProdServ": { "segmentos": ["50", "72"], "codigosAdicionales": ["01010101"] },
  "claveUnidad":   { "codigos": ["H87", "E48", "KGM"] },
  "codigoPostal":  { "codigos": ["44100", "44600"] },
  "colonia":       { "codigos": ["0001", "0002"] },
  "municipio":     { "codigos": ["039"] }
}
```

- **`segmentos`** (only meaningful for `claveProdServ`): matches by the first 2 digits of the
  code. SAT's `c_ClaveProdServ` is UNSPSC-based — the 8-digit code is itself hierarchical, with
  digits 1-2 identifying the business category ("segmento", e.g. food & beverage, construction
  services). This needs no extra data source: it's a prefix match on the code the master
  catalog already has. Verified against the real catalog: 52,747 codes, all 8 digits, 58
  distinct 2-digit segment prefixes.
- **`codigos`** / **`codigosAdicionales`**: an explicit code allowlist, used as-is (exact match).
  This is the *only* mechanism for `codigoPostal`/`colonia`/`municipio` — SAT's `catCFDI.xsd`
  carries no state/municipio/colonia relationship at all for these (flat lists, no hierarchy),
  so there's no reliable way to filter them "by state" yet. Doing that for real would mean
  vendoring a second, richer geographic dataset (from SAT or INEGI) — deliberately out of scope
  here; a business curates these by listing the exact codes it operates in.

## Regenerating

Re-run whenever the master `catalogs.sqlite` is regenerated (see
`TRSF.Invoicing/Schemas40/xsd/README.md`) or a deployment's curated manifest changes. The tool
prints a per-catalog "kept N / M" summary so you can sanity-check a manifest actually narrowed
anything.
