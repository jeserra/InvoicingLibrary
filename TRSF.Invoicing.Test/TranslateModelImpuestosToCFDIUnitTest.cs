using System;
using Xunit;
using TRSF.Invoicing.Translates;
using TRSF.Invoicing.BindingModels;
using System.Collections.Generic;
using System.Linq;

namespace TRSF.Invoicing.Test
{
    public class TranslateModelImpuestosToCFDIUnitTest
    {
        [Fact]
        public void TranslateImpuestoTest()
        {
            string input = "ISR";
            var output = TranslateModelImpuestosToCFDI.TranslateImpuesto(input);
            Assert.Equal(cfdi33.c_Impuesto.Item001, output);
        }

        [Fact]
        public void TranslateImpuestoInvalidTest()
        {
            string input = "ITT";
            try
            {
                var output = TranslateModelImpuestosToCFDI.TranslateImpuesto(input);
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
                Assert.True(true);
            }

        }

        [Fact]
        public void TranslateTipoFactorTest()
        {
            string input = "Cuota";
            var output = TranslateModelImpuestosToCFDI.TranslateTipoFactor(input);
            Assert.Equal(cfdi33.c_TipoFactor.Cuota, output);
        }

        [Fact]
        public void TranslateTipoFactorInvalidTest()
        {
            string input = "CuotaInvalida";
            try
            {
                var output = TranslateModelImpuestosToCFDI.TranslateTipoFactor(input);
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void TranslateTasaOCuotaTrasladoTest()
        {
            string input = "0.0298800";
            var output = TranslateModelImpuestosToCFDI.TranslateTasaOCuotaTraslado(input);
            Assert.Equal(cfdi33.c_TasaOCuota.Item0298800, output);
        }

        [Fact]
        public void TranslateTasaOCuotaTrasladoInvalidTest()
        {
            string input = "029880023232";
            try
            {
                var output = TranslateModelImpuestosToCFDI.TranslateTasaOCuotaTraslado(input);
            }
            catch (InvalidCastException)
            {
                Assert.True(true);
            }
        }


         [Fact]
         public void TranslateConceptosSinImpuestosTest()
        {
                 var input = new List<BindingModels.Concepto>();
            input.Add(
               new BindingModels.Concepto()
               {
                   Cantidad = 100,
                   ClaveProductoServicio = "01010101",
                   ClaveUnidad = "M55",
                   Descripcion = "Una madre aqui sin impuestos",
                   Importe = 200,
                   Unidad = "Radianes",
                   ValorUnitario = 200
               });

            try
            {
                var output = TranslateModelConceptosToCFDI.TranslateConceptos(input);
                Assert.Equal(null, output.First().Impuestos);
            }
            catch (InvalidCastException)
            {
                Assert.True(false);
            }
        }
    }
}
