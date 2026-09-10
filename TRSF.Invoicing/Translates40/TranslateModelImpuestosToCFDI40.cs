using System;
using System.Globalization;
using TRSF.Invoicing.BindingModels;

namespace TRSF.Invoicing.Translates40
{
    public class TranslateModelImpuestosToCFDI40
    {
        public static cfdi40.ComprobanteConceptoImpuestosRetencion TranslateConceptoImpuestoRetencion(ConceptoImpuestos from)
        {
            return new cfdi40.ComprobanteConceptoImpuestosRetencion
            {
                Base = from.BaseImpuesto,
                Importe = from.Importe,
                Impuesto = TranslateImpuesto(from.Impuesto),
                TasaOCuota = ParseTasaOCuota(from.TasaOCuota),
                TipoFactor = TranslateTipoFactor(from.TipoFactor),
            };
        }

        public static cfdi40.ComprobanteConceptoImpuestosTraslado TranslateConceptoImpuestoTraslado(ConceptoImpuestos from)
        {
            return new cfdi40.ComprobanteConceptoImpuestosTraslado
            {
                Base = from.BaseImpuesto,
                Importe = from.Importe,
                ImporteSpecified = true,
                Impuesto = TranslateImpuesto(from.Impuesto),
                TasaOCuota = ParseTasaOCuota(from.TasaOCuota),
                TasaOCuotaSpecified = true,
                TipoFactor = TranslateTipoFactor(from.TipoFactor),
            };
        }

        public static cfdi40.c_Impuesto TranslateImpuesto(string impuesto)
        {
            switch (impuesto?.ToUpperInvariant())
            {
                case "ISR": return cfdi40.c_Impuesto.Item001;
                case "IVA": return cfdi40.c_Impuesto.Item002;
                case "IEPS": return cfdi40.c_Impuesto.Item003;
                default: throw new InvalidCastException("Tipo de impuesto no soportado / definido");
            }
        }

        public static cfdi40.c_TipoFactor TranslateTipoFactor(string from)
        {
            if (Enum.TryParse(from, out cfdi40.c_TipoFactor to))
                return to;
            throw new InvalidCastException("Tipo de factor no soportado / definido");
        }

        /// <summary>
        /// CFDI 4.0: TasaOCuota es un decimal libre (no un catalogo enumerado como en
        /// 3.3's c_TasaOCuota) - basta con parsearlo, sin tener que mapear a una lista
        /// fija de valores conocidos.
        /// </summary>
        public static decimal ParseTasaOCuota(string from)
        {
            if (decimal.TryParse(from, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor))
                return valor;
            throw new InvalidCastException("Tasa o cuota no valida (se esperaba un numero decimal)");
        }
    }
}
