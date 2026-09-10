using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace TRSF.Invoicing.Translates40
{
    public class TranslateModelToCFDI40
    {
        // "Comprobante" (y ningun otro tipo de BindingModels) es ambiguo aqui: existe
        // tanto en TRSF.Invoicing.BindingModels como en TRSF.Invoicing.cfdi40, y por ser
        // este archivo parte de TRSF.Invoicing.Translates40 (espacio hermano de ambos),
        // la resolucion de un "Comprobante" sin calificar favorece incorrectamente al de
        // cfdi40. Se califica explicitamente con BindingModels. en vez de agregar un
        // using que reintroduciria la ambiguedad.
        public static cfdi40.Comprobante TranslateToCFDI(BindingModels.Comprobante from)
        {
            bool metodoPagoSpecified = false;
            bool formaPagoSpecified = false;

            var comprobante = new cfdi40.Comprobante
            {
                Emisor = new cfdi40.ComprobanteEmisor
                {
                    Rfc = from.Emisor.RFC,
                    RegimenFiscal = TranslateModelsToCatalogosCFDI40.TranslateRegimenFiscal(from.Emisor.RegimenFiscal),
                    Nombre = from.Emisor.Nombre,
                    FacAtrAdquirente = from.Emisor.FacAtrAdquirente,
                },
                Receptor = new cfdi40.ComprobanteReceptor
                {
                    Rfc = from.Receptor.RFC,
                    Nombre = from.Receptor.Nombre,
                    UsoCFDI = TranslateModelsToCatalogosCFDI40.TranslateUsoCFDI(from.Receptor.UsoCFDI),
                    DomicilioFiscalReceptor = from.Receptor.DomicilioFiscalReceptor,
                    RegimenFiscalReceptor = TranslateModelsToCatalogosCFDI40.TranslateRegimenFiscal(from.Receptor.RegimenFiscalReceptor),
                    NumRegIdTrib = from.Receptor.NumRegIdTrib,
                    ResidenciaFiscalSpecified = false,
                    // TODO: Revisar si se implementa la residencia fiscal para extranjeros
                    // (from.Receptor.ResidenciaFiscal es un codigo c_Pais en texto; falta
                    // el parseo equivalente a los demas catalogos si se necesita).
                },
                Complemento = TranslateComplemento(from),
                Impuestos = TranslateModelsToTotalImpuestos40.TranslateCuadroImpuesto(from.Conceptos),
                Conceptos = TranslateModelConceptosToCFDI40.TranslateConceptos(from.Conceptos).ToArray(),
                LugarExpedicion = from.LugarExpedicion,
                TipoDeComprobante = TranslateModelsToCatalogosCFDI40.TranslateTipoComprobante(from.TipoComprobante),
                Exportacion = TranslateModelsToCatalogosCFDI40.TranslateExportacion(from.Exportacion),
                FormaPago = TranslateModelsToCatalogosCFDI40.TranslateFormaPago(from.FormaPago, ref formaPagoSpecified),
                FormaPagoSpecified = formaPagoSpecified,
                Certificado = from.Certificado,
                NoCertificado = from.noCertificado,
                CondicionesDePago = from.CondicionesDePago,
                Serie = from.Serie,
                Folio = from.Folio,
                Moneda = TranslateModelsToCatalogosCFDI40.TranslateMoneda(from.Moneda),
                TipoCambio = from.TipoCambio ?? 0,
                TipoCambioSpecified = from.TipoCambio.HasValue,
                MetodoPago = TranslateModelsToCatalogosCFDI40.TranslateMetodoPago(from.MetodoPago, ref metodoPagoSpecified),
                MetodoPagoSpecified = metodoPagoSpecified,
                Confirmacion = from.Confirmacion,
                SubTotal = from.SubTotal,
                Descuento = from.Descuento,
                DescuentoSpecified = from.Descuento > 0,
                Total = from.Total,
                Fecha = TranslateModelsToCatalogosCFDI40.TranslateFecha(from.Fecha),
            };

            return comprobante;
        }

        private static cfdi40.ComprobanteComplemento TranslateComplemento(BindingModels.Comprobante from)
        {
            var elementos = new List<XmlElement>();

            if (from.ValesDespensa != null)
                elementos.Add(SerializarComoElemento(TranslateModelsValesDeDespensa40.TranslateTo(from.ValesDespensa)));

            if (from.ConsumoCombustibles != null)
                elementos.Add(SerializarComoElemento(TranslateModelsToConsumoDeCombustibles40.To(from.ConsumoCombustibles)));

            if (from.Pagos != null)
                elementos.Add(SerializarComoElemento(TranslatesModelsToPagos20.TranslateTo(from.Pagos)));

            if (elementos.Count == 0)
                return null;

            return new cfdi40.ComprobanteComplemento { Any = elementos.ToArray() };
        }

        /// <summary>
        /// Complemento.Any en el esquema 4.0 es XmlElement[] (contenido ya serializado),
        /// no un object[] polimorfico como en el esquema 3.3 - cada complemento se
        /// serializa por separado y se adjunta como un elemento ya formado.
        /// </summary>
        private static XmlElement SerializarComoElemento<T>(T objeto)
        {
            var serializer = new XmlSerializer(typeof(T));
            var doc = new XmlDocument();
            using (var writer = doc.CreateNavigator().AppendChild())
            {
                serializer.Serialize(writer, objeto);
            }
            return doc.DocumentElement;
        }
    }
}
