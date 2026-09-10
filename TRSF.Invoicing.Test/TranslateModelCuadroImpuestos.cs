using TRSF.Invoicing.Translates;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Test
{
    public class TranslateModelCuadroImpuestos
    {
        [Fact]
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
                Assert.Equal(100, output.TotalImpuestosRetenidos );
                Assert.Equal(cfdi33.c_Impuesto.Item001, output.Retenciones.FirstOrDefault().Impuesto);
            }
            catch (InvalidCastException)
            {
                Assert.True(false);
            }
        }

        [Fact]
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
                Assert.Equal(100, output.TotalImpuestosRetenidos);
                Assert.Equal(cfdi33.c_Impuesto.Item002, output.Retenciones.FirstOrDefault().Impuesto);
            }
            catch (InvalidCastException)
            {
                Assert.True(false);
            }
        }

        [Fact]
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
                Assert.Equal(100, output.TotalImpuestosTrasladados);
                Assert.Equal(cfdi33.c_Impuesto.Item001, output.Traslados.FirstOrDefault().Impuesto);
                Assert.Equal(cfdi33.c_TasaOCuota.Item0000000, output.Traslados.FirstOrDefault().TasaOCuota);
                Assert.Equal(cfdi33.c_TipoFactor.Exento, output.Traslados.FirstOrDefault().TipoFactor);
            }
            catch (InvalidCastException)
            {
                Assert.True(false);
            }
        }


        [Fact]
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
                Assert.Equal(null, output);
            }
            catch (InvalidCastException)
            {
                Assert.True(false);
            }
        }
    }
}
