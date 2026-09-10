using TRSF.Invoicing.BindingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Helpers
{
    public   class GenerateComprobanteIngreso
    {
        public static BindingModels.Comprobante GenerateNewIngreso()
        {

            BindingModels.Comprobante to = new BindingModels.Comprobante()
            {
                Version = "3.3",
                SubTotal = 0,
                FormaPago = null,
                CondicionesDePago = null,
                Moneda = "MXN",
                Total = 0,
                MetodoPago = "PUE",
                TipoComprobante = "I",
                UsoCFDI = "G01",
                SupplierId = 1, // Hardcodeado a customerId = 1
                Fecha = DateTime.Now.AddHours(-5),
                Conceptos = new List<Concepto>()
            };
            return to;
        }
    }
}
