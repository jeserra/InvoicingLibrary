
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRSF.Invoicing.BindingModels;

namespace TRSF.Invoicing.Helpers
{
    public class GenerateComprobantePago
    {
        public static BindingModels.Comprobante GenerateNew()
        {
            BindingModels.Comprobante to = new BindingModels.Comprobante()
            {
                 Version = "3.3",
                 SubTotal = 0,
                 FormaPago = null,
                 CondicionesDePago = null,
                 Moneda = "XXX",
                 Total = 0,
                 MetodoPago = null,
                 TipoComprobante = "P",
                 UsoCFDI = "P01",
                 Conceptos = new List<Concepto>()
                 {
                     new Concepto()
                     {
                         ClaveProductoServicio = "84111506",
                         Cantidad = 1,
                         ClaveUnidad = "ACT",
                         Descripcion = "Pago",
                         ValorUnitario = 0,
                         Importe = 0,
                         ConceptosImpuestos = null
                     }
                 }
            };
            return to;

            
        }
    }
}
