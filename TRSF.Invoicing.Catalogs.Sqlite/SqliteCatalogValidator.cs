using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Catalogs.Sqlite
{
    /// <summary>
    /// Valida los catalogos grandes de CFDI 4.0 (ClaveProdServ, CodigoPostal, ClaveUnidad,
    /// Colonia, Municipio) contra una base de datos SQLite en lugar de un enum de C#.
    /// Ver TRSF.Invoicing/Schemas40/xsd/README.md para por que estos catalogos no se
    /// generan como enum, y Tools/GenerateCatalogDb para como se genera Data/catalogs.sqlite.
    /// </summary>
    public class SqliteCatalogValidator : ICatalogValidator, IDisposable
    {
        private readonly SqliteConnection connection;

        private static readonly Dictionary<CatalogoGrande, string> Tables = new()
        {
            [CatalogoGrande.ClaveProdServ] = "ClaveProdServ",
            [CatalogoGrande.ClaveUnidad] = "ClaveUnidad",
            [CatalogoGrande.CodigoPostal] = "CodigoPostal",
            [CatalogoGrande.Colonia] = "Colonia",
            [CatalogoGrande.Municipio] = "Municipio",
        };

        public SqliteCatalogValidator(string databasePath)
        {
            connection = new SqliteConnection($"Data Source={databasePath};Mode=ReadOnly");
            connection.Open();
        }

        public bool Existe(CatalogoGrande catalogo, string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
                return false;

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT 1 FROM {Tables[catalogo]} WHERE Codigo = @codigo LIMIT 1";
            command.Parameters.AddWithValue("@codigo", codigo);
            using var reader = command.ExecuteReader();
            return reader.Read();
        }

        public string ObtenerDescripcion(CatalogoGrande catalogo, string codigo)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT Descripcion FROM {Tables[catalogo]} WHERE Codigo = @codigo LIMIT 1";
            command.Parameters.AddWithValue("@codigo", codigo);
            using var reader = command.ExecuteReader();
            return reader.Read() ? reader.GetString(0) : null;
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
