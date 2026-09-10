using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRSF.Invoicing.BindingModels;

namespace TRSF.Invoicing.Helpers
{
    public class CalculateTotals
    {
        public static void Calculate(BindingModels.Comprobante comprobante)
        {
            decimal subTotal = 0;
            decimal totalDescuento = 0;
            decimal totalImpuestos = 0;
            foreach (var concepto in comprobante.Conceptos)
            {
                subTotal += concepto.Cantidad * concepto.ValorUnitario;
                totalDescuento += concepto.Descuento;
                foreach(var impuesto in concepto.ConceptosImpuestos)
                {
                    totalImpuestos += impuesto.Importe;
                }
            }

            comprobante.SubTotal = subTotal;
            comprobante.Descuento = totalDescuento;
            comprobante.Total = subTotal - totalDescuento + totalImpuestos;
        }
    }
}
