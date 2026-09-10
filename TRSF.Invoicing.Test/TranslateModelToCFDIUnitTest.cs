using System;
using System.Text;
using System.Collections.Generic;
using Xunit;
using TRSF.Invoicing.Translates;
 
namespace TRSF.Invoicing.Test
{
    /// <summary>
    /// Summary description for TranslateModelToCFDIUnitTest
    /// </summary>
    public class TranslateModelToCFDIUnitTest
    {
        public TranslateModelToCFDIUnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }


        [Fact]
        public void TranslateUsoCFDITest()
        {
            string input = "G01";

            var output = TranslateModelsToCatalogosCFDI.TranslateUsoCFDI(input);
            Assert.Equal(cfdi33.c_UsoCFDI.G01, output);

        }


        [Fact]
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
                Assert.True(true);
            }
        }

        [Fact]
        public void TranslateCodigoPostalTest()
        {
            string input = "99100";
            var output = TranslateModelsToCatalogosCFDI.TranslateCodigoPostal(input);
            Assert.Equal(cfdi33.c_CodigoPostal.Item99100, output);
        }

        [Fact]
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
                Assert.True(true);
            }
        }

        [Fact]
        public void TranslateRegimenFiscalTest()
        {
            string input = "601";

            var output = TranslateModelsToCatalogosCFDI.TranslateRegimenFiscal(input);
            Assert.Equal(cfdi33.c_RegimenFiscal.Item601, output);

        }

        [Fact]
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
                Assert.True(true);
            }
        }

        [Fact]
        public void TranslateClaveProdServTest()
        {
            string input = "01010101";
            var output = TranslateModelsToCatalogosCFDI.TranslateClaveProdServ(input);
            Assert.Equal(cfdi33.c_ClaveProdServ.Item01010101, output);
        }

        [Fact]
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
                Assert.True(true);
            }
        }

        [Fact]
        public void TranslateClaveUnidadTest()
        {
            string input = "A111";
            var output = TranslateModelsToCatalogosCFDI.TranslateClaveUnidad(input);
            Assert.Equal(cfdi33.c_ClaveUnidad.A111, output);
        }

        [Fact]
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
                Assert.True(true);
            }
        }

        [Fact]
        public void TranslateFormaPagoTest()
        {
            bool specifiedField = false;
            string input = "01";
            var output = TranslateModelsToCatalogosCFDI.TranslateFormaPago(input, ref specifiedField);
            Assert.Equal(true, specifiedField);
            Assert.Equal(cfdi33.c_FormaPago.Item01, output);
        }

        [Fact]
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
                Assert.True(true);
            }
        }

        [Fact]
        public void TranslateFormaPagoNullTest()
        {
            bool specifiedField = false;
            string input = null;
            try
            {
                var output = TranslateModelsToCatalogosCFDI.TranslateFormaPago(input, ref specifiedField);
                Assert.Equal(false, specifiedField);
            }
            catch (InvalidCastException)
            {
                Assert.Fail();
            }
        }

        [Fact]
        public void TranslateMetodoPagoTest()
        {
            string input = "PUE";
            bool specifiedField = false;
            var output = TranslateModelsToCatalogosCFDI.TranslateMetodoPago(input, ref specifiedField);
            Assert.Equal(true, specifiedField);
            Assert.Equal(cfdi33.c_MetodoPago.PUE, output);
        }

        [Fact]
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
                Assert.True(true);
            }
        }

        [Fact]
        public void TranslateMetodoPagoNull()
        {
            string input = null;
            bool specifiedField = false;
            try
            {
                var output = TranslateModelsToCatalogosCFDI.TranslateMetodoPago(input, ref specifiedField);
                Assert.Equal(false, specifiedField);
            }
            catch (InvalidCastException)
            {
                Assert.True(true);
            }
        }


        [Fact]
        public void TranslateMoneda()
        {
            string input = "XXX";
            var output = TranslateModelsToCatalogosCFDI.TranslateMoneda(input);
            Assert.Equal(cfdi33.c_Moneda.XXX, output);
        }

        [Fact]
        public void TranslateMonedaMXN()
        {
            string input = "MXN";
            var output = TranslateModelsToCatalogosCFDI.TranslateMoneda(input);
            Assert.Equal(cfdi33.c_Moneda.MXN, output);
        }

        [Fact]
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
                Assert.True(true);
            }
        }

        [Fact(Skip = "Timezone-offset-dependent; result depends on the local machine's offset")]
        public void TranslateFechaTest()
        {
            // Revisar el cambio de hora. PRobablemente por el offset
            DateTime input = DateTime.Parse("2017-06-24T08:56:06.155Z");
            var output = TranslateModelsToCatalogosCFDI.TranslateFecha(input);
            var result = output.ToString("yyyy-MM-ddThh:mm:ss");
            Assert.Equal("2017-06-24T03:56:06", result);
        }


        [Fact]
        public void TranslateCadenaPagoTest()
        {
           
            var input = "01";
            var output = TranslateModelsToCatalogosCFDI.TranslateToCadenaPago(input, out bool isnullValue);
            Assert.Equal(isnullValue, false);
            Assert.Equal(output, cfdi33.c_TipoCadenaPago.Item01);
        }

        [Fact]
        public void TranslateCadenaPagoNullTest()
        {

            string input = null;
            var output = TranslateModelsToCatalogosCFDI.TranslateToCadenaPago(input, out bool isnullValue);
            Assert.Equal(isnullValue, true);
        }

        [Fact]
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
                Assert.True(true);
            }
        }
    }
}
