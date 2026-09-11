using Microsoft.Data.Sqlite;

namespace GenerateCuratedCatalogDb
{
    public record CatalogSummary(string Table, int Kept, int Total);

    /// <summary>
    /// Deriva un catalogs.sqlite curado (mismo esquema que el generado por
    /// tools/GenerateCatalogDb) a partir de un catalogo maestro y un manifiesto de
    /// curacion. Separado de Program.cs para poder probarse sin invocar el CLI.
    /// </summary>
    public static class CuratedCatalogGenerator
    {
        private static readonly IReadOnlyDictionary<string, Func<CurationManifest, CatalogSection?>> Sections = new Dictionary<string, Func<CurationManifest, CatalogSection?>>
        {
            ["ClaveProdServ"] = m => m.ClaveProdServ,
            ["ClaveUnidad"] = m => m.ClaveUnidad,
            ["CodigoPostal"] = m => m.CodigoPostal,
            ["Colonia"] = m => m.Colonia,
            ["Municipio"] = m => m.Municipio,
        };

        public static List<CatalogSummary> Generate(string masterDbPath, CurationManifest manifest, string outputDbPath)
        {
            if (File.Exists(outputDbPath))
                File.Delete(outputDbPath);

            using var master = new SqliteConnection($"Data Source={masterDbPath};Mode=ReadOnly");
            master.Open();

            using var output = new SqliteConnection($"Data Source={outputDbPath}");
            output.Open();

            var summaries = new List<CatalogSummary>();

            foreach (var (table, sectionSelector) in Sections)
            {
                using var createCmd = output.CreateCommand();
                createCmd.CommandText = $"CREATE TABLE {table} (Codigo TEXT PRIMARY KEY, Descripcion TEXT)";
                createCmd.ExecuteNonQuery();

                var section = sectionSelector(manifest);
                var rows = ReadMatchingRows(master, table, section);

                using var transaction = output.BeginTransaction();
                using var insertCmd = output.CreateCommand();
                insertCmd.CommandText = $"INSERT INTO {table} (Codigo, Descripcion) VALUES (@c, @d)";
                var codigoParam = insertCmd.Parameters.Add("@c", SqliteType.Text);
                var descripcionParam = insertCmd.Parameters.Add("@d", SqliteType.Text);
                foreach (var (codigo, descripcion) in rows)
                {
                    codigoParam.Value = codigo;
                    descripcionParam.Value = descripcion ?? (object)DBNull.Value;
                    insertCmd.ExecuteNonQuery();
                }
                transaction.Commit();

                using var totalCmd = master.CreateCommand();
                totalCmd.CommandText = $"SELECT COUNT(*) FROM {table}";
                var total = Convert.ToInt32(totalCmd.ExecuteScalar());

                summaries.Add(new CatalogSummary(table, rows.Count, total));
            }

            return summaries;
        }

        private static List<(string Codigo, string? Descripcion)> ReadMatchingRows(SqliteConnection master, string table, CatalogSection? section)
        {
            var result = new List<(string, string?)>();

            using var cmd = master.CreateCommand();
            cmd.CommandText = $"SELECT Codigo, Descripcion FROM {table}";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var codigo = reader.GetString(0);
                var descripcion = reader.IsDBNull(1) ? null : reader.GetString(1);

                if (Matches(codigo, section))
                    result.Add((codigo, descripcion));
            }

            return result;
        }

        private static bool Matches(string codigo, CatalogSection? section)
        {
            // Sin seccion en el manifiesto = "no curar este catalogo" = copiar todo.
            if (section == null)
                return true;

            if (section.Codigos != null && section.Codigos.Contains(codigo))
                return true;

            if (section.CodigosAdicionales != null && section.CodigosAdicionales.Contains(codigo))
                return true;

            // Segmentos (prefijo de 2 digitos) solo tiene sentido semantico para
            // ClaveProdServ (codigo UNSPSC de 8 digitos) - se aplica igual a cualquier
            // catalogo que lo declare, sin forzarlo a solo esa tabla, para no acoplar
            // esta funcion al nombre de tabla.
            if (section.Segmentos != null && codigo.Length >= 2 && section.Segmentos.Contains(codigo[..2]))
                return true;

            // Si el manifiesto declaro una seccion para este catalogo pero ninguno de
            // los filtros anteriores aplica (todos nulos/vacios), no hay nada que
            // incluir - una seccion vacia no significa "copiar todo".
            return false;
        }
    }
}
