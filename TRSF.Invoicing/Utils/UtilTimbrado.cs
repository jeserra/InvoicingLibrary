using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Utils
{
    public class UtilTimbrado
    {
        public static cfdi33.TimbreFiscalDigital ObtenerDatosTimbrado (cfdi33.Comprobante comprobante)
        {
            var datosTimbrado = new cfdi33.TimbreFiscalDigital();
            foreach (var item in comprobante.Complemento.Items)
            {
                if (item.GetType() == typeof(cfdi33.TimbreFiscalDigital))
                {
                    datosTimbrado = ((cfdi33.TimbreFiscalDigital)item);
                }
            }
            return datosTimbrado;
        }

        public static cfdi33.TimbreFiscalDigital ObtenerDatosTimbrado(string ComprobanteTimbrado)
        {
            var controller = new CFDI.CFDIv33(null, null);

            var comprobanteTimbrado = controller.DeserializeXML(ComprobanteTimbrado);
           
            var datosTimbrado = new cfdi33.TimbreFiscalDigital();
            foreach (var item in comprobanteTimbrado.Complemento.Items)
            {
                if (item.GetType() == typeof(cfdi33.TimbreFiscalDigital))
                {
                    datosTimbrado = ((cfdi33.TimbreFiscalDigital)item);
                }
            }
            return datosTimbrado;
        }
    }
}
