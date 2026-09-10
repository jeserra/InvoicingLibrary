using System;

namespace TRSF.Invoicing.Translates40
{
    /// <summary>
    /// Equivalente a Translates.TranslateModelsToCatalogosCFDI pero para los tipos
    /// generados en TRSF.Invoicing.cfdi40 (no se pueden compartir: son tipos distintos,
    /// aunque los valores de catalogo sean los mismos).
    /// </summary>
    public class TranslateModelsToCatalogosCFDI40
    {
        public static cfdi40.c_UsoCFDI TranslateUsoCFDI(string from)
        {
            if (Enum.TryParse(from, out cfdi40.c_UsoCFDI to))
                return to;
            throw new InvalidCastException("Uso de CFDI no soportado / definido en el esquema cfdi 4.0");
        }

        public static cfdi40.c_RegimenFiscal TranslateRegimenFiscal(string from)
        {
            var item = "Item" + from;
            if (Enum.TryParse(item, out cfdi40.c_RegimenFiscal to))
                return to;
            throw new InvalidCastException("Regimen fiscal no soportado / definido en el esquema cfdi 4.0");
        }

        public static cfdi40.c_FormaPago TranslateFormaPago(string from, ref bool specifiedField)
        {
            if (string.IsNullOrEmpty(from))
            {
                specifiedField = false;
                return cfdi40.c_FormaPago.Item01;
            }

            var item = "Item" + from;
            if (Enum.TryParse(item, out cfdi40.c_FormaPago to))
            {
                specifiedField = true;
                return to;
            }
            throw new InvalidCastException("Forma de pago no soportada / definida en el esquema cfdi 4.0");
        }

        public static cfdi40.c_MetodoPago TranslateMetodoPago(string from, ref bool isSpecified)
        {
            if (string.IsNullOrEmpty(from))
            {
                isSpecified = false;
                return cfdi40.c_MetodoPago.PPD;
            }

            if (Enum.TryParse(from, out cfdi40.c_MetodoPago to))
            {
                isSpecified = true;
                return to;
            }
            throw new InvalidCastException("Metodo de pago no definido / soportado por el esquema cfdi 4.0");
        }

        public static cfdi40.c_TipoDeComprobante TranslateTipoComprobante(string from)
        {
            if (Enum.TryParse(from, out cfdi40.c_TipoDeComprobante to))
                return to;
            throw new InvalidCastException("Tipo de comprobante no definido / soportado por el esquema cfdi 4.0");
        }

        public static cfdi40.c_Moneda TranslateMoneda(string from)
        {
            if (Enum.TryParse(from, out cfdi40.c_Moneda to))
                return to;
            throw new InvalidCastException("Moneda no definida / soportada por el esquema cfdi 4.0");
        }

        /// <summary>
        /// CFDI 4.0: obligatorio en Comprobante y, con su propia semantica, en cada
        /// Concepto (ObjetoImp) y DoctoRelacionado de Pagos (ObjetoImpDR).
        /// </summary>
        public static cfdi40.c_Exportacion TranslateExportacion(string from)
        {
            var valor = string.IsNullOrEmpty(from) ? "01" : from;
            var item = "Item" + valor;
            if (Enum.TryParse(item, out cfdi40.c_Exportacion to))
                return to;
            throw new InvalidCastException("Clave de exportacion no definida / soportada por el esquema cfdi 4.0");
        }

        public static cfdi40.c_ObjetoImp TranslateObjetoImp(string from)
        {
            var item = "Item" + from;
            if (Enum.TryParse(item, out cfdi40.c_ObjetoImp to))
                return to;
            throw new InvalidCastException("Objeto de impuesto no definido / soportado por el esquema cfdi 4.0");
        }

        public static cfdi40.c_TipoCadenaPago TranslateToCadenaPago(string from, out bool isNullValue)
        {
            if (string.IsNullOrEmpty(from))
            {
                isNullValue = true;
                return cfdi40.c_TipoCadenaPago.Item01;
            }

            var item = "Item" + from;
            if (Enum.TryParse(item, out cfdi40.c_TipoCadenaPago to))
            {
                isNullValue = false;
                return to;
            }
            throw new InvalidCastException("Tipo cadena de pago no definida / soportada por el esquema cfdi 4.0");
        }

        public static DateTime TranslateFecha(DateTime fecha)
        {
            var formattedFecha = fecha.ToString("yyyy-MM-ddThh:mm:ss");
            return DateTime.Parse(formattedFecha);
        }
    }
}
