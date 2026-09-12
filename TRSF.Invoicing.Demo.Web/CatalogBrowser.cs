using Microsoft.Data.Sqlite;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Demo.Web
{
    public record CatalogMatch(string Codigo, string? Descripcion);

    /// <summary>
    /// Busqueda por codigo (prefijo) o descripcion (contiene) sobre el catalogo curado de la
    /// sesion, para el autocompletado del wizard. Deliberadamente separado de ICatalogValidator
    /// (Existe/ObtenerDescripcion es un contrato de validacion, no de navegacion) - la validacion
    /// real de lo que el usuario elige aqui sigue pasando por ICatalogValidator.Existe antes de sellar.
    /// </summary>
    public static class CatalogBrowser
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
}
