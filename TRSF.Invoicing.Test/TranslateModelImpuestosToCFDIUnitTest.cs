using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRSF.Invoicing.Translates;
using TRSF.Invoicing.BindingModels;
using System.Collections.Generic;
using System.Linq;

namespace TRSF.Invoicing.Test
{
    [TestClass]
    public class TranslateModelImpuestosToCFDIUnitTest
    {
        [TestMethod]
        public void TranslateImpuestoTest()
        {
            string input = "ISR";
            var output = TranslateModelImpuestosToCFDI.TranslateImpuesto(input);
            Assert.AreEqual(cfdi33.c_Impuesto.Item001, output);
        }

        [TestMethod]
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
                Assert.IsTrue(true);
            }

        }

        [TestMethod]
        public void TranslateTipoFactorTest()
        {
            string input = "Cuota";
            var output = TranslateModelImpuestosToCFDI.TranslateTipoFactor(input);
            Assert.AreEqual(cfdi33.c_TipoFactor.Cuota, output);
        }

        [TestMethod]
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
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TranslateTasaOCuotaTrasladoTest()
        {
            string input = "0.0298800";
            var output = TranslateModelImpuestosToCFDI.TranslateTasaOCuotaTraslado(input);
            Assert.AreEqual(cfdi33.c_TasaOCuota.Item0298800, output);
        }

        [TestMethod]
        public void TranslateTasaOCuotaTrasladoInvalidTest()
        {
            string input = "029880023232";
            try
            {
                var output = TranslateModelImpuestosToCFDI.TranslateTasaOCuotaTraslado(input);
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(true);
            }
        }


         [TestMethod]
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
                Assert.AreEqual(null, output.First().Impuestos);
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(false);
            }
        }
    }
}
