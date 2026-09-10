using TRSF.Invoicing.Translates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Test
{
    [TestClass]
    public class TranslateModelCuadroImpuestos
    {
        [TestMethod]
        public void TranslateCuadroImpuestosISR_Retenido_Exento()
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
                   ValorUnitario = 200,
                   ConceptosImpuestos = new System.Collections.Generic.List<BindingModels.ConceptoImpuestos>()
                            {
                                new BindingModels.ConceptoImpuestos()
                                {
                                    BaseImpuesto = 200,
                                    Importe = 100,
                                    Impuesto =  "ISR",
                                    RetencionOTraslado = "Retencion",
                                    TasaOCuota = "0",
                                    TipoFactor =  "Exento"
                                }
                            }
               });



            try
            {
                var output = TranslateModelsToTotalImpuestos.TranslateCuadroImpuesto(input);
                Assert.AreEqual(100, output.TotalImpuestosRetenidos );
                Assert.AreEqual(cfdi33.c_Impuesto.Item001, output.Retenciones.FirstOrDefault().Impuesto);
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void TranslateCuadroImpuestosIVA_Retenido_Exento()
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
                   ValorUnitario = 200,
                   ConceptosImpuestos = new System.Collections.Generic.List<BindingModels.ConceptoImpuestos>()
                            {
                                new BindingModels.ConceptoImpuestos()
                                {
                                    BaseImpuesto = 200,
                                    Importe = 100,
                                    Impuesto =  "IVA",
                                    RetencionOTraslado = "Retencion",
                                    TasaOCuota = "0",
                                    TipoFactor =  "Exento"
                                }
                            }
               });



            try
            {
                var output = TranslateModelsToTotalImpuestos.TranslateCuadroImpuesto(input);
                Assert.AreEqual(100, output.TotalImpuestosRetenidos);
                Assert.AreEqual(cfdi33.c_Impuesto.Item002, output.Retenciones.FirstOrDefault().Impuesto);
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void TranslateCuadroImpuestosISR_Trasladado_Exento()
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
                   ValorUnitario = 200,
                   ConceptosImpuestos = new System.Collections.Generic.List<BindingModels.ConceptoImpuestos>()
                            {
                                new BindingModels.ConceptoImpuestos()
                                {
                                    BaseImpuesto = 200,
                                    Importe = 100,
                                    Impuesto =  "ISR",
                                    RetencionOTraslado = "Traslado",
                                    TasaOCuota = "0000000",
                                    TipoFactor =  "Exento"
                                }
                            }
               });



            try
            {
                var output = TranslateModelsToTotalImpuestos.TranslateCuadroImpuesto(input);
                Assert.AreEqual(100, output.TotalImpuestosTrasladados);
                Assert.AreEqual(cfdi33.c_Impuesto.Item001, output.Traslados.FirstOrDefault().Impuesto);
                Assert.AreEqual(cfdi33.c_TasaOCuota.Item0000000, output.Traslados.FirstOrDefault().TasaOCuota);
                Assert.AreEqual(cfdi33.c_TipoFactor.Exento, output.Traslados.FirstOrDefault().TipoFactor);
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(false);
            }
        }


        [TestMethod]
        public void TranslateCuadroSinImpuestos()
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
                var output = TranslateModelsToTotalImpuestos.TranslateCuadroImpuesto(input); 
                Assert.AreEqual(null, output);
            }
            catch (InvalidCastException)
            {
                Assert.IsTrue(false);
            }
        }
    }
}
