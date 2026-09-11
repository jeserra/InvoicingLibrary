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

## Sample manifests

`samples/` has ready-to-run manifests for a few common industries — copy one as a starting
point instead of writing a manifest from scratch:

| File | Industry |
|---|---|
| `restaurante-jalisco.json` | Restaurant, single metro area (segments `50` food, `90` food services) |
| `comercio-abarrotes.json` | Grocery / food retailer (segments `10`, `50`) |
| `construccion-ferreteria.json` | Hardware store / small construction business (segments `27`, `30`, `31`, `72`) |
| `servicios-profesionales.json` | Consulting / professional services, no geography curated (segments `80`, `81`) |
| `salud-consultorio.json` | Doctor's office / small clinic (segments `42`, `51`, `85`) |
| `tecnologia-ti.json` | Software / IT services, no geography curated (segments `43`, `81`) |
| `transporte-logistica.json` | Freight / logistics (segments `25`, `78`) |

Each file has a `_industria`/`_notas` field explaining the choice (ignored by the parser —
System.Text.Json skips unmapped members by default). Try one directly:

```
dotnet run --project tools/GenerateCuratedCatalogDb -- \
  TRSF.Invoicing.Catalogs.Sqlite/Data/catalogs.sqlite \
  tools/GenerateCuratedCatalogDb/samples/restaurante-jalisco.json \
  /tmp/curated.sqlite
```

`samples/segmentos-claveprodserv.md` lists every one of the 58 `ClaveProdServ` segments that
actually exist in the vendored catalog, with a name and how many codes fall under it — use it
to pick which `segmentos` belong in a manifest for an industry not covered above.

## Regenerating

Re-run whenever the master `catalogs.sqlite` is regenerated (see
`TRSF.Invoicing/Schemas40/xsd/README.md`) or a deployment's curated manifest changes. The tool
prints a per-catalog "kept N / M" summary so you can sanity-check a manifest actually narrowed
anything.
