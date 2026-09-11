using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Xunit;
using GenerateCuratedCatalogDb;
using TRSF.Invoicing.Catalogs.Sqlite;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Test.Catalogs
{
    /// <summary>
    /// Prueba tools/GenerateCuratedCatalogDb contra un catalogo maestro sintetico (no el
    /// catalogs.sqlite real de 5MB) para mantener la prueba rapida y autocontenida.
    /// </summary>
    public class CuratedCatalogGeneratorTest : IDisposable
    {
        private readonly string masterDbPath;
        private readonly string outputDbPath;

        public CuratedCatalogGeneratorTest()
        {
            masterDbPath = Path.Combine(Path.GetTempPath(), $"master_{Guid.NewGuid():N}.sqlite");
            outputDbPath = Path.Combine(Path.GetTempPath(), $"curated_{Guid.NewGuid():N}.sqlite");
            BuildFixtureMasterDb(masterDbPath);
        }

        private static void BuildFixtureMasterDb(string path)
        {
            using var conn = new SqliteConnection($"Data Source={path}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "CREATE TABLE ClaveProdServ (Codigo TEXT PRIMARY KEY, Descripcion TEXT); " +
                               "CREATE TABLE ClaveUnidad (Codigo TEXT PRIMARY KEY, Descripcion TEXT); " +
                               "CREATE TABLE CodigoPostal (Codigo TEXT PRIMARY KEY, Descripcion TEXT); " +
                               "CREATE TABLE Colonia (Codigo TEXT PRIMARY KEY, Descripcion TEXT); " +
                               "CREATE TABLE Municipio (Codigo TEXT PRIMARY KEY, Descripcion TEXT); " +
                               "INSERT INTO ClaveProdServ (Codigo) VALUES ('50101700'), ('50201800'), ('10101500'), ('72101500'); " +
                               "INSERT INTO ClaveUnidad (Codigo) VALUES ('H87'), ('E48'); " +
                               "INSERT INTO CodigoPostal (Codigo) VALUES ('44100'), ('44600'), ('99100'); " +
                               "INSERT INTO Colonia (Codigo) VALUES ('0001'), ('0002'); " +
                               "INSERT INTO Municipio (Codigo) VALUES ('039');";
            cmd.ExecuteNonQuery();
        }

        [Fact]
        public void SegmentoCurado_incluye_codigos_del_segmento_y_excluye_los_demas()
        {
            var manifest = new CurationManifest
            {
                ClaveProdServ = new CatalogSection { Segmentos = new[] { "50" }, CodigosAdicionales = new[] { "10101500" } },
            };

            CuratedCatalogGenerator.Generate(masterDbPath, manifest, outputDbPath);
            using var validator = new SqliteCatalogValidator(outputDbPath);

            Assert.True(validator.Existe(CatalogoGrande.ClaveProdServ, "50101700"));
            Assert.True(validator.Existe(CatalogoGrande.ClaveProdServ, "50201800"));
            Assert.True(validator.Existe(CatalogoGrande.ClaveProdServ, "10101500"));
            Assert.False(validator.Existe(CatalogoGrande.ClaveProdServ, "72101500"));
        }

        [Fact]
        public void CodigoExplicito_solo_incluye_los_codigos_listados()
        {
            var manifest = new CurationManifest
            {
                CodigoPostal = new CatalogSection { Codigos = new[] { "44100", "44600" } },
            };

            CuratedCatalogGenerator.Generate(masterDbPath, manifest, outputDbPath);
            using var validator = new SqliteCatalogValidator(outputDbPath);

            Assert.True(validator.Existe(CatalogoGrande.CodigoPostal, "44100"));
            Assert.True(validator.Existe(CatalogoGrande.CodigoPostal, "44600"));
            Assert.False(validator.Existe(CatalogoGrande.CodigoPostal, "99100"));
        }

        [Fact]
        public void CatalogoOmitidoDelManifiesto_se_copia_completo()
        {
            var manifest = new CurationManifest
            {
                CodigoPostal = new CatalogSection { Codigos = new[] { "44100" } },
                // ClaveUnidad, Colonia, Municipio no se declaran: deben copiarse completos.
            };

            CuratedCatalogGenerator.Generate(masterDbPath, manifest, outputDbPath);
            using var validator = new SqliteCatalogValidator(outputDbPath);

            Assert.True(validator.Existe(CatalogoGrande.ClaveUnidad, "H87"));
            Assert.True(validator.Existe(CatalogoGrande.ClaveUnidad, "E48"));
            Assert.True(validator.Existe(CatalogoGrande.Colonia, "0001"));
            Assert.True(validator.Existe(CatalogoGrande.Municipio, "039"));
        }

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(masterDbPath)) File.Delete(masterDbPath);
            if (File.Exists(outputDbPath)) File.Delete(outputDbPath);
        }
    }
}
