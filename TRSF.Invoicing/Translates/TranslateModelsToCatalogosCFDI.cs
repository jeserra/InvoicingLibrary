using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Translates
{
    public class TranslateModelsToCatalogosCFDI
    {
        public static cfdi33.c_UsoCFDI TranslateUsoCFDI(String from)
        {
            cfdi33.c_UsoCFDI to = new cfdi33.c_UsoCFDI();
            if (Enum.TryParse(from, out to))
                return to;
            else
                throw new InvalidCastException();
        }

        public static cfdi33.c_RegimenFiscal TranslateRegimenFiscal(String from)
        {
            var item = "Item" + from;
            cfdi33.c_RegimenFiscal to = new cfdi33.c_RegimenFiscal();
            Enum.TryParse(item, out to);
            if (Enum.TryParse(item, out to))
                return to;
            else
                throw new InvalidCastException("Régimen fiscal no soportada / definida en el esquema cfdi");
        }

        public static cfdi33.c_CodigoPostal TranslateCodigoPostal(String from)
        {
            var item = "Item" + from;
            cfdi33.c_CodigoPostal to = new cfdi33.c_CodigoPostal();
            if (Enum.TryParse(item, out to))
                return to;
            else
                throw new InvalidCastException("Codigo Fiscal no encontrado en la lista del SAT");
        }

        public static cfdi33.c_ClaveProdServ TranslateClaveProdServ(String from)
        {
            var item = "Item" + from;
            cfdi33.c_ClaveProdServ to = new cfdi33.c_ClaveProdServ();
            if(Enum.TryParse(item, out to)) 
                    return to;
             else
                throw new InvalidCastException("Producto o servicio no existente en la lista del sat");
        }

        public static cfdi33.c_ClaveUnidad TranslateClaveUnidad(string from)
        {
            cfdi33.c_ClaveUnidad to = new cfdi33.c_ClaveUnidad();
            if (Enum.TryParse(from, out to))
                return to;
            else
                throw new InvalidCastException("Unidad de medida no reconocida por el SAT");
        }

        public static cfdi33.c_FormaPago TranslateFormaPago (string from, ref bool specifiedField)
        {
            if (String.IsNullOrEmpty(from))
            {
                specifiedField = false;
                return cfdi33.c_FormaPago.Item01;
            }
            else
            {
                cfdi33.c_FormaPago to = new cfdi33.c_FormaPago();
                var item = "Item" + from;
                if (Enum.TryParse(item, out to))
                {
                    specifiedField = true;
                    return to;
                }
                else
                    throw new InvalidCastException("Forma de pago no soportada / definida en el esquema cfdi");
            }
        }

        public static cfdi33.c_MetodoPago TranslateMetodoPago (string from, ref bool isSpecified )
        {

            if (String.IsNullOrEmpty(from))
            {
                isSpecified = false;
                return cfdi33.c_MetodoPago.PPD;
            }
            else
            {
                cfdi33.c_MetodoPago to = new cfdi33.c_MetodoPago();
                if (Enum.TryParse(from, out to))
                {
                    isSpecified = true;
                    return to;
                }
                else
                    throw new InvalidCastException("Metodo de pago no definido / soportado por el esquema cfdi");
            }
        }

        public static cfdi33.c_TipoDeComprobante TranslateTipoComprobante (string from)
        {
            cfdi33.c_TipoDeComprobante to = new cfdi33.c_TipoDeComprobante();
            if (Enum.TryParse(from, out to))
                return to;
            else
                throw new InvalidCastException("Tipo de comprobante no definido / Soportado por el esquema cfdi");
        }

        public static cfdi33.c_Moneda TranslateMoneda (string from)
        {
            cfdi33.c_Moneda to = new cfdi33.c_Moneda();
            if (Enum.TryParse(from, out to))
                return to;
            else
                throw new InvalidCastException("Moneda no definida / soportada por el esquema cfdi");
        }

        public static cfdi33.c_TipoCadenaPago TranslateToCadenaPago(String from , out bool isNullValue)
        {


            if (String.IsNullOrEmpty(from))
            {
                isNullValue = true;
                return cfdi33.c_TipoCadenaPago.Item01;
            }
            var item = "Item" + from;
            var to = new cfdi33.c_TipoCadenaPago();
            if (Enum.TryParse(item, out to))
            {
                isNullValue = false;
                return to;
            }
            else
                throw new InvalidCastException("Tipo Cadena de pago no definida / soportada por el esquema cfdi");
        }

        public static DateTime TranslateFecha(DateTime Fecha)
        {
            var FormattedFecha = Fecha.ToString("yyyy-MM-ddThh:mm:ss");
            return DateTime.Parse(FormattedFecha);
        }
    }
}
