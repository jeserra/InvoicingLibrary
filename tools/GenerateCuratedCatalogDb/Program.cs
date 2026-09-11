using System.Text.Json;
using GenerateCuratedCatalogDb;

if (args.Length < 3)
{
    Console.WriteLine("Usage: GenerateCuratedCatalogDb <path-to-master-catalogs.sqlite> <path-to-manifest.json> <output-curated.sqlite>");
    return 1;
}

var masterDbPath = args[0];
var manifestPath = args[1];
var outputDbPath = args[2];

var manifestJson = File.ReadAllText(manifestPath);
var manifest = JsonSerializer.Deserialize<CurationManifest>(manifestJson, new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
}) ?? throw new InvalidOperationException($"No se pudo leer el manifiesto de curacion en {manifestPath}");

var summaries = CuratedCatalogGenerator.Generate(masterDbPath, manifest, outputDbPath);

Console.WriteLine($"Catalogo curado generado en {outputDbPath}:");
foreach (var s in summaries)
    Console.WriteLine($"  {s.Table}: {s.Kept} / {s.Total} filas conservadas");

return 0;
