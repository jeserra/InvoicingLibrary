using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TRSF.Invoicing.BindingModels;

namespace TRSF.Invoicing.Translates40
{
    /// <summary>
    /// Pagos 2.0 (obligatorio para CFDI 4.0) NO es solo un cambio de version del
    /// complemento de recepcion de pagos 1.0 - es un esquema distinto:
    ///   - DoctoRelacionado ya no tiene MetodoDePagoDR (se elimino).
    ///   - TipoCambioDR se renombro a EquivalenciaDR (misma semantica).
    ///   - DoctoRelacionado.ObjetoImpDR es nuevo y obligatorio (antes no existia).
    ///   - ImpSaldoAnt/ImpPagado/ImpSaldoInsoluto pasaron de opcionales a obligatorios.
    ///   - Se agrego Pagos.Totales (obligatorio) con MontoTotalPagos y, opcionalmente,
    ///     desgloses de retenciones/traslados de IVA a nivel del complemento completo.
    ///
    /// Los desgloses de impuestos por pago/documento (PagosPago.ImpuestosP,
    /// DoctoRelacionado.ImpuestosDR) y los desgloses de IVA en Totales no se generan
    /// aqui: BindingModels.Pagos no trae datos suficientemente granulares para
    /// calcularlos correctamente, y es preferible omitirlos (opcionales) a inventar un
    /// desglose fiscal incorrecto. Ver MIGRATION.md para el detalle de esta decision.
    /// </summary>
    public class TranslatesModelsToPagos20
    {
        public static cfdi40.Pagos TranslateTo(Pagos from)
        {
            if (from == null)
                return null;

            var listaPagos = from.ListaPagos.Select(TranslateToPago).ToList();

            return new cfdi40.Pagos
            {
                Pago = listaPagos.ToArray(),
                Totales = TranslateToTotales(listaPagos),
            };
        }

        private static cfdi40.PagosTotales TranslateToTotales(List<cfdi40.PagosPago> pagos)
        {
            return new cfdi40.PagosTotales
            {
                MontoTotalPagos = pagos.Sum(p => p.Monto),
            };
        }

        public static cfdi40.PagosPago TranslateToPago(Pago from)
        {
            bool formaPagoSpecified = false;
            byte[] certPago = string.IsNullOrEmpty(from.CertPago) ? null : Encoding.ASCII.GetBytes(from.CertPago);
            byte[] selloPago = string.IsNullOrEmpty(from.SelloPago) ? null : Encoding.ASCII.GetBytes(from.SelloPago);

            if (from.ListaDocumentos == null)
                throw new Exception("No existen facturas relacionadas con el pago");

            var to = new cfdi40.PagosPago
            {
                FechaPago = TranslateModelsToCatalogosCFDI40.TranslateFecha(from.FechaPago.LocalDateTime),
                MonedaP = TranslateModelsToCatalogosCFDI40.TranslateMoneda(from.MonedaP),
                FormaDePagoP = TranslateModelsToCatalogosCFDI40.TranslateFormaPago(from.FormaDePagoP, ref formaPagoSpecified),
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
                TipoCambioP = from.TipoCambioP,
                TipoCambioPSpecified = from.TipoCambioP > 0,
                DoctoRelacionado = from.ListaDocumentos.Select(TranslateToDoctosRelacionados).ToArray(),
            };

            var tipoCadPago = TranslateModelsToCatalogosCFDI40.TranslateToCadenaPago(from.TipoCadPago, out bool isNullValue);
            if (!isNullValue)
            {
                to.TipoCadPago = tipoCadPago;
                to.TipoCadPagoSpecified = true;
            }
            return to;
        }

        public static cfdi40.PagosPagoDoctoRelacionado TranslateToDoctosRelacionados(DoctosRelacionados from)
        {
            return new cfdi40.PagosPagoDoctoRelacionado
            {
                Folio = from.Folio.ToString(),
                Serie = from.Serie,
                IdDocumento = from.idDocumento.ToString(),
                MonedaDR = TranslateModelsToCatalogosCFDI40.TranslateMoneda(from.MonedaDR),
                NumParcialidad = from.NumParcialidad.ToString(),
                EquivalenciaDR = from.TipoCambioDR,
                EquivalenciaDRSpecified = from.TipoCambioDR > 0,
                ImpPagado = from.ImpPagado,
                ImpSaldoAnt = from.ImpSaldoAnt,
                ImpSaldoInsoluto = from.ImpSaldoInsoluto,
                ObjetoImpDR = TranslateModelsToCatalogosCFDI40.TranslateObjetoImp(from.ObjetoImpDR),
            };
        }
    }
}
