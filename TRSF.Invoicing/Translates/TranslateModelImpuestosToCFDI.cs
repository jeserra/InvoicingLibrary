using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRSF.Invoicing.BindingModels;
namespace TRSF.Invoicing.Translates
{
    public class TranslateModelImpuestosToCFDI
    {
        public static cfdi33.ComprobanteConceptoImpuestosRetencion TranslateConceptoImpuestoRetencion (BindingModels.ConceptoImpuestos from)
        {
            cfdi33.ComprobanteConceptoImpuestosRetencion to = new cfdi33.ComprobanteConceptoImpuestosRetencion()
                {
                    Base = from.BaseImpuesto,
                    Importe = from.Importe,
                    Impuesto = TranslateImpuesto(from.Impuesto),
                    TasaOCuota = decimal.Parse(from.TasaOCuota),
                    TipoFactor = TranslateTipoFactor(from.TipoFactor)
                };
            return to;
        }

        public static cfdi33.ComprobanteConceptoImpuestosTraslado TranslateConceptoImpuestoTraslado (BindingModels.ConceptoImpuestos from)
        {             
            cfdi33.ComprobanteConceptoImpuestosTraslado to = new cfdi33.ComprobanteConceptoImpuestosTraslado()
            {
                Base = from.BaseImpuesto,
                Importe = from.Importe,
                ImporteSpecified = true,
                Impuesto = TranslateImpuesto(from.Impuesto),
                TasaOCuota = TranslateTasaOCuotaTraslado(from.TasaOCuota),
                TasaOCuotaSpecified = true,
                TipoFactor = TranslateTipoFactor(from.TipoFactor)
            };
            return to;     
        }


        public static cfdi33.c_Impuesto TranslateImpuesto (string impuesto)
        {
            cfdi33.c_Impuesto to = new cfdi33.c_Impuesto();
            switch(impuesto.ToUpper())
            {
                case "ISR": to = cfdi33.c_Impuesto.Item001;
                    break;
                case "IVA": to = cfdi33.c_Impuesto.Item002;
                    break;
                case "IEPS": to = cfdi33.c_Impuesto.Item003;
                    break;
                default: throw new InvalidCastException("Tipo de impuesto no soportado / Definido ");  
            }
            return to;
        }

        public static cfdi33.c_TipoFactor TranslateTipoFactor (String from)
        {
            cfdi33.c_TipoFactor to = new cfdi33.c_TipoFactor();
            if (Enum.TryParse(from, out to))
                return to;
            else
                throw new InvalidCastException("Tipo de factor no soportado / definido");
        }

        public static cfdi33.c_TasaOCuota TranslateTasaOCuotaTraslado(string from)
        {
            float cuotaParsed;

            if(float.TryParse(from, out cuotaParsed))
            {
                 
                if (nearlyEqual(cuotaParsed ,0))
                    return cfdi33.c_TasaOCuota.Item0000000;
                else if (nearlyEqual(cuotaParsed, 0.160000f))
                    return cfdi33.c_TasaOCuota.Item0160000;
                else if (nearlyEqual(cuotaParsed, 0.265000f))
                    return cfdi33.c_TasaOCuota.Item0265000;
                else if (nearlyEqual(cuotaParsed, 0.0298800f))
                    return cfdi33.c_TasaOCuota.Item0298800;
                else if (nearlyEqual(cuotaParsed, 0.300000f))
                    return cfdi33.c_TasaOCuota.Item0300000;
                else if (nearlyEqual(cuotaParsed, 0.304000f))
                    return cfdi33.c_TasaOCuota.Item0304000;
                else if (nearlyEqual(cuotaParsed, 0.350000f))
                    return cfdi33.c_TasaOCuota.Item0350000;
                else if (nearlyEqual(cuotaParsed, 0.500000f))
                    return cfdi33.c_TasaOCuota.Item0500000;
                else if (nearlyEqual(cuotaParsed, 0.530000f))
                    return cfdi33.c_TasaOCuota.Item0530000;
                else if (nearlyEqual(cuotaParsed, 1.600000f))
                    return cfdi33.c_TasaOCuota.Item1600000;
                else if (nearlyEqual(cuotaParsed, 3.000000f))
                    return cfdi33.c_TasaOCuota.Item3000000;
                else
                    throw new InvalidCastException("Tasa o cuota de traslado no soportado / definido");


            }
            else
            {
                throw new InvalidCastException("Tasa o cuota de traslado no soportado / definido");
            }
             

         }

        public static bool nearlyEqual(float a, float b, float epsilon= 0)
        {
            float absA = Math.Abs(a);
            float absB = Math.Abs(b);
            float diff = Math.Abs(a - b);

            if (a == b)
            { // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || diff < float.MinValue)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * float.MinValue);
            }
            else
            { // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }
    }
}
