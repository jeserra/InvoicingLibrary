using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRSF.Invoicing.BindingModels;
using System.Xml.Serialization;
using TRSF.Invoicing.Utils;

namespace TRSF.Invoicing.Translates
{
    public class TranslateModelToCFDI
    {
        public static  cfdi33.Comprobante TranslateToCFDI  (BindingModels.Comprobante from)
        {
            bool MetodoPagoSpecified = false;
            bool formaPagoSpecified = false;
           
                var cfdi33 = new cfdi33.Comprobante()
            {
                // TODO: revisar si se aplican descuento, 
                Emisor = new TRSF.Invoicing.cfdi33.ComprobanteEmisor()
                {
                    Rfc = from.Emisor.RFC,
                    RegimenFiscal = TranslateModelsToCatalogosCFDI.TranslateRegimenFiscal(from.Emisor.RegimenFiscal),
                    Nombre = from.Emisor.Nombre
                },
                Receptor = new TRSF.Invoicing.cfdi33.ComprobanteReceptor()
                {
                    Rfc = from.Receptor.RFC,
                    Nombre = from.Receptor.Nombre,
                    UsoCFDI = TranslateModelsToCatalogosCFDI.TranslateUsoCFDI(from.UsoCFDI),
                    ResidenciaFiscalSpecified = false
                    // TODO : Revisar si se implementa la resencia fiscal y el No. de registro tributario para extranjeros   
                },
                Complemento = new TRSF.Invoicing.cfdi33.ComprobanteComplemento()
                {
                    Items = new object[] { TranslateModelsValesDeDespensa.TranslateTo(from.ValesDespensa) , TranslatesModelsToPagos.TranslateTo( (  from).Pagos) }
                },
                Impuestos = TranslateModelsToTotalImpuestos.TranslateCuadroImpuesto(from.Conceptos),
                Conceptos = TranslateModelConceptosToCFDI.TranslateConceptos(from.Conceptos).ToArray(),
                LugarExpedicion =  from.LugarExpedicion, // TranslateModelsToCatalogosCFDI.TranslateCodigoPostal(from.LugarExpedicion),
                TipoDeComprobante = TranslateModelsToCatalogosCFDI.TranslateTipoComprobante(from.TipoComprobante),
                FormaPago =   TranslateModelsToCatalogosCFDI.TranslateFormaPago(from.FormaPago, ref formaPagoSpecified),
                FormaPagoSpecified = formaPagoSpecified,
                Certificado = from.Certificado,
                NoCertificado = from.noCertificado,
                CondicionesDePago = from.CondicionesDePago,
                CondicionesDePagoSpecified = !String.IsNullOrEmpty(from.CondicionesDePago),
                Serie = from.Serie,
                Folio = from.Folio,
                Moneda = TranslateModelsToCatalogosCFDI.TranslateMoneda(from.Moneda),
                MetodoPago = TranslateModelsToCatalogosCFDI.TranslateMetodoPago(from.MetodoPago, ref MetodoPagoSpecified),
                MetodoPagoSpecified = MetodoPagoSpecified,
                Version = "3.3",
                SubTotal = from.SubTotal,
                Total = from.Total,
                Fecha = TranslateModelsToCatalogosCFDI.TranslateFecha( from.Fecha),
                                 
            };

            return cfdi33;
        }


       
    }
}
