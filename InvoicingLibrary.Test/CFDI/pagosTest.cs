﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InvoicingLibrary.cfdi33;

namespace InvoicingLibrary.Test.CFDI
{

    [TestClass]
    public class cfdi33Test
    {

        [Ignore]
        [TestMethod]
        public void ObtenerItemClaveProducto()
        {
            var item = c_ClaveProdServ.Item01010101;
            Assert.AreEqual(item.ToString(), "01010101");
        }

        [TestMethod]
        public void ObtenerClaveProductoByItem()
        {
            c_ClaveProdServ item;
            Enum.TryParse("Item01010101", out item);
            Assert.IsInstanceOfType(item, typeof(c_ClaveProdServ));
        }

        [TestMethod]
        public void CreatePayment()
        {
            InvoicingLibrary.cfdi33.Comprobante comprobante = new cfdi33.Comprobante()
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
