using Microsoft.Data.Sqlite;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Mcp;

/// <summary>
/// Busqueda por codigo (prefijo) o descripcion (contiene) sobre el catalogo maestro, para que
/// un agente pueda encontrar claves validas en vez de adivinarlas. Mismo patron de consulta que
/// TRSF.Invoicing.Demo.Web/CatalogBrowser.cs, pero contra catalogs.sqlite completo - aqui no hay
/// un paso de curacion por sesion.
/// </summary>
public static class CatalogLookup
{
    private static readonly Dictionary<CatalogoGrande, string> Tables = new()
    {
        [CatalogoGrande.ClaveProdServ] = "ClaveProdServ",
        [CatalogoGrande.ClaveUnidad] = "ClaveUnidad",
        [CatalogoGrande.CodigoPostal] = "CodigoPostal",
        [CatalogoGrande.Colonia] = "Colonia",
        [CatalogoGrande.Municipio] = "Municipio",
    };

    public static List<CatalogMatch> Buscar(string dbPath, CatalogoGrande catalogo, string termino, int limite = 20)
    {
        var resultados = new List<CatalogMatch>();
        using var connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT Codigo, Descripcion FROM {Tables[catalogo]} WHERE Codigo LIKE @prefijo OR Descripcion LIKE @contiene ORDER BY Codigo LIMIT @limite";
        command.Parameters.AddWithValue("@prefijo", termino + "%");
        command.Parameters.AddWithValue("@contiene", "%" + termino + "%");
        command.Parameters.AddWithValue("@limite", limite);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var codigo = reader.GetString(0);
            var descripcion = reader.IsDBNull(1) ? null : reader.GetString(1);
            resultados.Add(new CatalogMatch(codigo, descripcion));
        }

        return resultados;
    }
}
