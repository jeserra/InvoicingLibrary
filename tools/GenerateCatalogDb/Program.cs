using System.Xml;
using Microsoft.Data.Sqlite;

if (args.Length < 2)
{
    Console.WriteLine("Usage: GenerateCatalogDb <path-to-catCFDI.xsd> <output-catalogs.sqlite>");
    return 1;
}

var xsdPath = args[0];
var dbPath = args[1];

// c_ prefix stripped for table names (ClaveProdServ, ClaveUnidad, CodigoPostal, Colonia, Municipio) -
// keep in sync with TRSF.Invoicing.Interfaces.CatalogoGrande and SqliteCatalogValidator.Tables.
var catalogsToExtract = new HashSet<string> { "c_ClaveProdServ", "c_ClaveUnidad", "c_CodigoPostal", "c_Colonia", "c_Municipio" };

if (File.Exists(dbPath))
    File.Delete(dbPath);

using var connection = new SqliteConnection($"Data Source={dbPath}");
connection.Open();

foreach (var name in catalogsToExtract)
{
    var table = name[2..];
    using var createCmd = connection.CreateCommand();
    createCmd.CommandText = $"CREATE TABLE {table} (Codigo TEXT PRIMARY KEY, Descripcion TEXT)";
    createCmd.ExecuteNonQuery();
}

using var transaction = connection.BeginTransaction();
var insertCommands = new Dictionary<string, SqliteCommand>();
foreach (var name in catalogsToExtract)
{
    var table = name[2..];
    var cmd = connection.CreateCommand();
    cmd.CommandText = $"INSERT OR REPLACE INTO {table} (Codigo, Descripcion) VALUES (@c, @d)";
    cmd.Parameters.Add("@c", SqliteType.Text);
    cmd.Parameters.Add("@d", SqliteType.Text);
    insertCommands[name] = cmd;
}

string? currentType = null;
string? pendingCode = null;
var docBuilder = new System.Text.StringBuilder();
bool inDocumentation = false;
long totalInserted = 0;

void FinishEnumeration()
{
    if (currentType != null && pendingCode != null)
    {
        var cmd = insertCommands[currentType];
        cmd.Parameters["@c"].Value = pendingCode;
        cmd.Parameters["@d"].Value = docBuilder.ToString().Trim();
        cmd.ExecuteNonQuery();
        totalInserted++;
        if (totalInserted % 20000 == 0)
            Console.WriteLine($"  ...{totalInserted} rows inserted");
    }
    pendingCode = null;
    docBuilder.Clear();
}

var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit };
using var reader = XmlReader.Create(xsdPath, settings);
while (reader.Read())
{
    switch (reader.NodeType)
    {
        case XmlNodeType.Element:
            switch (reader.LocalName)
            {
                case "simpleType":
                    var name = reader.GetAttribute("name");
                    currentType = (name != null && catalogsToExtract.Contains(name)) ? name : null;
                    break;
                case "enumeration":
                    if (currentType != null)
                    {
                        pendingCode = reader.GetAttribute("value");
                        docBuilder.Clear();
                        if (reader.IsEmptyElement)
                            FinishEnumeration();
                    }
                    break;
                case "documentation":
                    if (currentType != null && pendingCode != null)
                        inDocumentation = true;
                    break;
            }
            break;

        case XmlNodeType.Text:
        case XmlNodeType.CDATA:
            if (inDocumentation)
                docBuilder.Append(reader.Value);
            break;

        case XmlNodeType.EndElement:
            switch (reader.LocalName)
            {
                case "documentation":
                    inDocumentation = false;
                    break;
                case "enumeration":
                    if (currentType != null)
                        FinishEnumeration();
                    break;
                case "simpleType":
                    currentType = null;
                    break;
            }
            break;
    }
}

foreach (var cmd in insertCommands.Values)
    cmd.Dispose();

transaction.Commit();

Console.WriteLine($"Done. {totalInserted} total rows inserted into {dbPath}");
return 0;
