using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRSF.Invoicing.BindingModels;
using TRSF.Invoicing.cfdi33;


namespace TRSF.Invoicing.Translates
{
    public class TranslatesModelsToPagos
    {
        public static cfdi33.Pagos TranslateTo(BindingModels.Pagos from)
        {
            if (from != null)
            {
                var to = new cfdi33.Pagos();
                var listaPagos = new List<cfdi33.PagosPago>();
                foreach (var itemPago in from.ListaPagos)
                {
                    listaPagos.Add(TranslateToPago(itemPago));
                }
                to.Pago = listaPagos.ToArray();
                return to;
            }
            else
                return null;
        }

        public static cfdi33.PagosPago TranslateToPago(BindingModels.Pago from)
        {
            bool formaPagoSpecified = false;
            byte[] certPago = null; 
            byte[] selloPago = null;
            

            if (!String.IsNullOrEmpty(from.CertPago))
                certPago = Encoding.ASCII.GetBytes(from.CertPago);
            if (!String.IsNullOrEmpty(from.SelloPago))
                selloPago = Encoding.ASCII.GetBytes(from.SelloPago);
 
            var listaDoctosRelacionados = new List<PagosPagoDoctoRelacionado>();

            if (from.ListaDocumentos != null)
            {
                foreach (var item in from.ListaDocumentos)
                {
                    listaDoctosRelacionados.Add(TranslateToDoctosRelacionados(item));
                }
            }
            else
                throw new Exception("No existen facturas relacionadas con el pago");

            var to = new cfdi33.PagosPago()
            {
                FechaPago = TranslateModelsToCatalogosCFDI.TranslateFecha(from.FechaPago.LocalDateTime), // from.FechaPago.LocalDateTime,
                MonedaP = TranslateModelsToCatalogosCFDI.TranslateMoneda(from.MonedaP),
                FormaDePagoP = TranslateModelsToCatalogosCFDI.TranslateFormaPago(from.FormaDePagoP, ref formaPagoSpecified),
                Monto = from.Monto,
                CtaBeneficiario = from.CtaBeneficiario,
                CadPago = from.CadPago,
                CertPago = certPago,
                CtaOrdenante = from.CtaOrdenante,
                NomBancoOrdExt = from.NomBancoOrdExt,
                NumOperacion = from.NumOperation,
                RfcEmisorCtaBen = from.RfcEmisorCtaBen,
                RfcEmisorCtaOrd = from.RfcEmisorCtaOrd,
                SelloPago = selloPago,
                TipoCadPagoSpecified = !String.IsNullOrEmpty(from.TipoCadPago),
                TipoCambioP = from.TipoCambioP,
                TipoCambioPSpecified = (from.TipoCambioP > 0),
                DoctoRelacionado = listaDoctosRelacionados.ToArray() 
               // Impuestos = from.ListaImpuestos

            };

            var TipoCadPago = TranslateModelsToCatalogosCFDI.TranslateToCadenaPago(from.TipoCadPago, out bool isNullValue);
            if (!isNullValue)
                to.TipoCadPago = TipoCadPago;
            return to;
        }

        public static cfdi33.PagosPagoDoctoRelacionado TranslateToDoctosRelacionados(BindingModels.DoctosRelacionados from)
        {
            bool MetodoPagoSpecified = false;

            var to = new cfdi33.PagosPagoDoctoRelacionado()
            {
                Folio = from.Folio.ToString(),
                Serie = from.Serie,
                IdDocumento = from.idDocumento.ToString(),
                MonedaDR = TranslateModelsToCatalogosCFDI.TranslateMoneda(from.MonedaDR),
                NumParcialidad = from.NumParcialidad.ToString(),
                TipoCambioDR = from.TipoCambioDR,
                TipoCambioDRSpecified = (from.TipoCambioDR > 0),
                MetodoDePagoDR = TranslateModelsToCatalogosCFDI.TranslateMetodoPago(from.MetodoDePagoDR, ref MetodoPagoSpecified),
                ImpPagado = from.ImpPagado,
                ImpPagadoSpecified = (from.ImpPagado > 0),
                ImpSaldoAnt = from.ImpSaldoAnt,
                ImpSaldoAntSpecified = (from.ImpSaldoAnt > 0),
                ImpSaldoInsoluto = from.ImpSaldoInsoluto,
                ImpSaldoInsolutoSpecified = (from.ImpSaldoInsoluto > 0)
            };
            return to;
        }

        //    public static cfdi33.PagosPagoImpuestos TranslatePagosImpuestos(BindingModels.ImpuestosPagos from)
        //    {
        //        decimal TotalRetenciones = 0;
        //        decimal TotalTraslados = 0;

        //        var listaRetenciones = new List<cfdi33.PagosPagoImpuestosRetencion>();
        //        var listaTranslados = new List<cfdi33.PagosPagoImpuestosTraslado>();




        //        foreach (var item in from)
        //        {

        //            foreach (var itemImpuesto in item.ConceptosImpuestos)
        //            {
        //                if (itemImpuesto.RetencionOTraslado == "Retencion")
        //                {
        //                    var ItemImpuesto = TranslateModelImpuestosToCFDI.TranslateConceptoImpuestoRetencion(itemImpuesto);
        //                    TotalRetenciones += itemImpuesto.Importe;
        //                    var result = listaRetenciones.Where(o => o.Impuesto == TranslateModelImpuestosToCFDI.TranslateImpuesto(itemImpuesto.Impuesto));
        //                    if (result.Count() > 0)
        //                        result.FirstOrDefault().Importe += itemImpuesto.Importe;
        //                    else
        //                    {
        //                        listaRetenciones.Add(
        //                         new cfdi33.ComprobanteImpuestosRetencion()
        //                         {
        //                             Importe = itemImpuesto.Importe,
        //                             Impuesto = TranslateModelImpuestosToCFDI.TranslateImpuesto(itemImpuesto.Impuesto)
        //                         });
        //                    }
        //                }
        //                else if (itemImpuesto.RetencionOTraslado == "Traslado")
        //                {
        //                    var ItemImpuesto = TranslateModelImpuestosToCFDI.TranslateConceptoImpuestoTraslado(itemImpuesto);
        //                    TotalTraslados += itemImpuesto.Importe;
        //                    var result = listaTranslados.Where(o => o.Impuesto == TranslateModelImpuestosToCFDI.TranslateImpuesto(itemImpuesto.Impuesto));
        //                    if (result.Count() > 0)
        //                        result.FirstOrDefault().Importe += itemImpuesto.Importe;
        //                    else
        //                    {
        //                        listaTranslados.Add(
        //                         new cfdi33.ComprobanteImpuestosTraslado()
        //                         {
        //                             Importe = itemImpuesto.Importe,
        //                             TasaOCuota = TranslateModelImpuestosToCFDI.TranslateTasaOCuotaTraslado(itemImpuesto.TasaOCuota),
        //                             Impuesto = TranslateModelImpuestosToCFDI.TranslateImpuesto(itemImpuesto.Impuesto),
        //                             TipoFactor = TranslateModelImpuestosToCFDI.TranslateTipoFactor(itemImpuesto.TipoFactor)
        //                         });
        //                    }
        //                }
        //                else
        //                    throw new InvalidCastException("Tipo de impuesto no definido / soportado");
        //            }
        //        }
        //}
    }
}


