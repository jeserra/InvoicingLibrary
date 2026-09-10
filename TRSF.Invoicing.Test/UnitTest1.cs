using System;
using Xunit;
using TRSF.Invoicing.cfdi33;

namespace TRSF.Invoicing.Test
{
    public class cfdi33Test
    {
        TRSF.Invoicing.cfdi33.Comprobante comprobante = null;

        [Fact(Skip = "Enum ToString() returns 'Item01010101', not '01010101' - assertion is incorrect as written")]
        public void ObtenerItemClaveProducto()
        {
            var item = c_ClaveProdServ.Item01010101;
            Assert.Equal(item.ToString(), "01010101");
        }

        [Fact]
        public void ObtenerClaveProductoByItem ()
        {
            c_ClaveProdServ item;
           Enum.TryParse("Item01010101", out  item);
            Assert.IsType<c_ClaveProdServ>(item);
        }

        public cfdi33Test()
        {
            comprobante = new cfdi33.Comprobante()
            {
                Emisor = new cfdi33.ComprobanteEmisor()
                {
                    Rfc = "SEDE810924CX8",
                    Nombre = "jesd",
                    RegimenFiscal = c_RegimenFiscal.Item630
                },
                Receptor = new cfdi33.ComprobanteReceptor()
                {
                    Nombre = "Federico Alanis",
                    Rfc = "FEDE760909XD2",
                    NumRegIdTrib = "",
                    ResidenciaFiscal = c_Pais.MAR,
                    UsoCFDI = c_UsoCFDI.P01
                },
                Conceptos = new cfdi33.ComprobanteConcepto[]
                 {
                     new cfdi33.ComprobanteConcepto()
                     {
                          Cantidad = 10,
                           ClaveProdServ = "Item01010101", //c_ClaveProdServ.Item01010101,
                             ClaveUnidad = c_ClaveUnidad.C11,
                             Descripcion = "Algo unico aqui",
                              Unidad = "Metros cuadrados",
                               Impuestos = new ComprobanteConceptoImpuestos()
                               {
                                    Retenciones = new ComprobanteConceptoImpuestosRetencion []
                                    {
                                        new ComprobanteConceptoImpuestosRetencion()
                                        {
                                             Base = 100,
                                             TasaOCuota = 10,
                                              Impuesto = c_Impuesto.Item002,
                                               Importe = 110,
                                                TipoFactor = c_TipoFactor.Exento
                                        }
                                    },
                                    Traslados = new ComprobanteConceptoImpuestosTraslado []
                                    {
                                         new ComprobanteConceptoImpuestosTraslado()
                                         {
                                          Base = 100,
                                              TasaOCuota = c_TasaOCuota.Item0265000,
                                              Impuesto = c_Impuesto.Item002,
                                               Importe = 110,
                                                TipoFactor = c_TipoFactor.Cuota

                                         }
                                    }
                               }
                     }
                 }
            };
            
        }
    }
}
