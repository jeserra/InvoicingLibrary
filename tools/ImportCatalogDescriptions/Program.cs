using System.Text;
using ExcelDataReader;
using Microsoft.Data.Sqlite;

// Merges human-readable descriptions from SAT's official Anexo 20 workbook
// (catCFDI_V_4_*.xls, distinct from the code-only catCFDI.xsd - see
// TRSF.Invoicing/Schemas40/xsd/README.md) into an existing master catalogs.sqlite.
// Only ClaveProdServ and ClaveUnidad are handled: they are clean one-to-one
// Codigo -> Descripcion maps in the source. CodigoPostal/Colonia/Municipio are
// NOT included here - in the source workbook their "descriptions" are scoped by
// a second key (Estado for Municipio, CodigoPostal for Colonia; CodigoPostal
// itself has no description at all), so the same Codigo legitimately maps to
// different names depending on context. The current schema (Codigo TEXT PRIMARY
// KEY, single column) can't represent that without a composite key - populating
// it here would silently pick one arbitrary name per code.
//
// Merge-only: existing Codigo rows get their Descripcion updated; codes present
// in the workbook but absent from catalogs.sqlite are not inserted.

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

if (args.Length < 2)
{
    Console.WriteLine("Usage: ImportCatalogDescriptions <path-to-catCFDI_V_4_*.xls> <path-to-catalogs.sqlite>");
    return 1;
}

var xlsPath = args[0];
var dbPath = args[1];

var sheets = new Dictionary<string, (string Table, Func<IExcelDataReader, string?> ReadDescripcion)>
{
    ["c_ClaveProdServ"] = ("ClaveProdServ", r => r.GetValue(1)?.ToString()),
    ["c_ClaveUnidad"] = ("ClaveUnidad", r =>
    {
        var nombre = r.GetValue(1)?.ToString();
        return string.IsNullOrWhiteSpace(nombre) ? r.GetValue(2)?.ToString() : nombre;
    }),
};

using var connection = new SqliteConnection($"Data Source={dbPath}");
connection.Open();
using var transaction = connection.BeginTransaction();

using var stream = File.Open(xlsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
using var reader = ExcelReaderFactory.CreateReader(stream);

do
{
    if (!sheets.TryGetValue(reader.Name, out var mapping))
        continue;

    using var updateCmd = connection.CreateCommand();
    updateCmd.Transaction = transaction;
    updateCmd.CommandText = $"UPDATE {mapping.Table} SET Descripcion = @d WHERE Codigo = @c";
    updateCmd.Parameters.Add("@c", SqliteType.Text);
    updateCmd.Parameters.Add("@d", SqliteType.Text);

    long sourceRows = 0, matched = 0;
    bool pastHeader = false;
    while (reader.Read())
    {
        var firstCell = reader.GetValue(0)?.ToString();
        if (!pastHeader)
        {
            if (firstCell == reader.Name)
                pastHeader = true;
            continue;
        }

        if (string.IsNullOrWhiteSpace(firstCell))
            continue;

        sourceRows++;
        updateCmd.Parameters["@c"].Value = firstCell.Trim();
        updateCmd.Parameters["@d"].Value = (object?)mapping.ReadDescripcion(reader)?.Trim() ?? DBNull.Value;
        matched += updateCmd.ExecuteNonQuery();
    }

    Console.WriteLine($"{mapping.Table}: {sourceRows} filas en el catalogo fuente, {matched} coincidieron con un Codigo existente en {Path.GetFileName(dbPath)}.");
} while (reader.NextResult());

transaction.Commit();
Console.WriteLine("Listo.");
return 0;
