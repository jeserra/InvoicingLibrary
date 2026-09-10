using System;
using System.IO;
using Xunit;
using TRSF.Invoicing.Catalogs.Sqlite;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Test.Catalogs
{
    public class SqliteCatalogValidatorTest : IDisposable
    {
        private readonly SqliteCatalogValidator validator;

        public SqliteCatalogValidatorTest()
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "Data", "catalogs.sqlite");
            validator = new SqliteCatalogValidator(dbPath);
        }

        [Fact]
        public void Existe_ClaveProdServ_returns_true_for_a_real_code()
        {
            Assert.True(validator.Existe(CatalogoGrande.ClaveProdServ, "01010101"));
        }

        [Fact]
        public void Existe_CodigoPostal_returns_true_for_a_real_code()
        {
            Assert.True(validator.Existe(CatalogoGrande.CodigoPostal, "99100"));
        }

        [Fact]
        public void Existe_ClaveUnidad_returns_true_for_a_real_code()
        {
            Assert.True(validator.Existe(CatalogoGrande.ClaveUnidad, "05"));
        }

        [Fact]
        public void Existe_returns_false_for_a_made_up_code()
        {
            Assert.False(validator.Existe(CatalogoGrande.ClaveProdServ, "99999999999"));
            Assert.False(validator.Existe(CatalogoGrande.CodigoPostal, "00001"));
        }

        [Fact]
        public void Existe_returns_false_for_null_or_empty()
        {
            Assert.False(validator.Existe(CatalogoGrande.CodigoPostal, null));
            Assert.False(validator.Existe(CatalogoGrande.CodigoPostal, ""));
        }

        public void Dispose()
        {
            validator.Dispose();
        }
    }
}
