using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRSF.Invoicing.Translates;
 
namespace TRSF.Invoicing.Test
{
    /// <summary>
    /// Summary description for TranslateModelToCFDIUnitTest
    /// </summary>
    [TestClass]
    public class TranslateModelToCFDIUnitTest
    {
        public TranslateModelToCFDIUnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
       

        [TestMethod]
        public void TranslateUsoCFDITest()
        {
            string input = "G01";

            var output = TranslateModelsToCatalogosCFDI.TranslateUsoCFDI(input);
            Assert.AreEqual(cfdi33.c_UsoCFDI.G01, output);

        }


        [TestMethod]
        public void TranslateUsoCFDIInvalidTest()
        {
            string input = "T01";

            try
            {
                var output = TranslateModelsToCatalogosCFDI.TranslateUsoCFDI(input);
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TranslateCodigoPostalTest()
        {
            string input = "99100";
            var output = TranslateModelsToCatalogosCFDI.TranslateCodigoPostal(input);
            Assert.AreEqual(cfdi33.c_CodigoPostal.Item99100, output);
        }

        [TestMethod]
        public void TranslateCodigoPostalInvalidTest()
        {
            string input = "900";
            try
            {
                var output = TranslateModelsToCatalogosCFDI.TranslateCodigoPostal(input);
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TranslateRegimenFiscalTest()
        {
            string input = "601";

            var output = TranslateModelsToCatalogosCFDI.TranslateRegimenFiscal(input);
            Assert.AreEqual(cfdi33.c_RegimenFiscal.Item601, output);

        }

        [TestMethod]
        public void TranslateRegimenFiscalInvalidTest()
        {
            string input = "Item601";
            try
            {
                var output = TranslateModelsToCatalogosCFDI.TranslateRegimenFiscal(input);
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TranslateClaveProdServTest()
        {
            string input = "01010101";
            var output = TranslateModelsToCatalogosCFDI.TranslateClaveProdServ(input);
            Assert.AreEqual(cfdi33.c_ClaveProdServ.Item01010101, output);
        }

        [TestMethod]
        public void TranslateClaveProdServInvalidTest()
        {
            string input = "ITEM10202402";
            try
            {
                var output = TranslateModelsToCatalogosCFDI.TranslateClaveProdServ(input);
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TranslateClaveUnidadTest()
        {
            string input = "A111";
            var output = TranslateModelsToCatalogosCFDI.TranslateClaveUnidad(input);
            Assert.AreEqual(cfdi33.c_ClaveUnidad.A111, output);
        }

        [TestMethod]
        public void TranslateClaveUnidadInvalidTest()
        {
            string input = "8888888888";
            try
            {
                var output = TranslateModelsToCatalogosCFDI.TranslateClaveUnidad(input);
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TranslateFormaPagoTest()
        {
            bool specifiedField = false;
            string input = "01";
            var output = TranslateModelsToCatalogosCFDI.TranslateFormaPago(input, ref specifiedField);
            Assert.AreEqual(true, specifiedField);
            Assert.AreEqual(cfdi33.c_FormaPago.Item01, output);
        }

        [TestMethod]
        public void TranslateFormaPagoInvalidTest()
        {
            string input = "8888888888";
            var specifiedField = false;
            try
            {
                var output = TranslateModelsToCatalogosCFDI.TranslateFormaPago(input, ref specifiedField);
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TranslateFormaPagoNullTest()
        {
            bool specifiedField = false;
            string input = null;
            try
            {
                var output = TranslateModelsToCatalogosCFDI.TranslateFormaPago(input, ref specifiedField);
                Assert.AreEqual(false, specifiedField);
            }
            catch (InvalidCastException)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TranslateMetodoPagoTest()
        {
            string input = "PUE";
            bool specifiedField = false;
            var output = TranslateModelsToCatalogosCFDI.TranslateMetodoPago(input, ref specifiedField);
            Assert.AreEqual(true, specifiedField);
            Assert.AreEqual(cfdi33.c_MetodoPago.PUE, output);
        }

        [TestMethod]
        public void TranslateMetodoPagoInvalidTest()
        {
            string input = "8888888888";
            bool specifiedField = false;
            try
            {
                var output = TranslateModelsToCatalogosCFDI.TranslateMetodoPago(input, ref specifiedField);
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TranslateMetodoPagoNull()
        {
            string input = null;
            bool specifiedField = false;
            try
            {
                var output = TranslateModelsToCatalogosCFDI.TranslateMetodoPago(input, ref specifiedField);
                Assert.AreEqual(false, specifiedField);
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(true);
            }
        }


        [TestMethod]
        public void TranslateMoneda()
        {
            string input = "XXX";
            var output = TranslateModelsToCatalogosCFDI.TranslateMoneda(input);
            Assert.AreEqual(cfdi33.c_Moneda.XXX, output);
        }

        [TestMethod]
        public void TranslateMonedaMXN()
        {
            string input = "MXN";
            var output = TranslateModelsToCatalogosCFDI.TranslateMoneda(input);
            Assert.AreEqual(cfdi33.c_Moneda.MXN, output);
        }

        [TestMethod]
        public void TranslateMonedaInvalid()
        {
            string input = "8888888888";
            try
            {
                var output = TranslateModelsToCatalogosCFDI.TranslateMoneda(input);
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(true);
            }
        }

        [Ignore]
        [TestMethod]
        public void TranslateFechaTest()
        {
            // Revisar el cambio de hora. PRobablemente por el offset
            DateTime input = DateTime.Parse("2017-06-24T08:56:06.155Z");
            var output = TranslateModelsToCatalogosCFDI.TranslateFecha(input);
            var result = output.ToString("yyyy-MM-ddThh:mm:ss");
            Assert.AreEqual("2017-06-24T03:56:06", result);
        }


        [TestMethod]
        public void TranslateCadenaPagoTest()
        {
           
            var input = "01";
            var output = TranslateModelsToCatalogosCFDI.TranslateToCadenaPago(input, out bool isnullValue);
            Assert.AreEqual(isnullValue, false);
            Assert.AreEqual(output, cfdi33.c_TipoCadenaPago.Item01);
        }

        [TestMethod]
        public void TranslateCadenaPagoNullTest()
        {

            string input = null;
            var output = TranslateModelsToCatalogosCFDI.TranslateToCadenaPago(input, out bool isnullValue);
            Assert.AreEqual(isnullValue, true);
        }

        [TestMethod]
        public void TranslateCadenaPagoInvalidTest()
        {
            try
            {
                string input = "03";
                var output = TranslateModelsToCatalogosCFDI.TranslateToCadenaPago(input, out bool isnullValue);
                Assert.Fail();
            }
            catch(InvalidCastException)
            {
                Assert.IsTrue(true);
            }
        }
    }
}
