using System;
using System.Collections.Generic;
using TRSF.Invoicing.BindingModels;

namespace TRSF.Invoicing.Translates40
{
    public class TranslateModelConceptosToCFDI40
    {
        public static List<cfdi40.ComprobanteConcepto> TranslateConceptos(List<Concepto> from)
        {
            var to = new List<cfdi40.ComprobanteConcepto>();
            foreach (var item in from)
            {
                var retenciones = new List<cfdi40.ComprobanteConceptoImpuestosRetencion>();
                var traslados = new List<cfdi40.ComprobanteConceptoImpuestosTraslado>();

                if (item.ConceptosImpuestos != null)
                {
                    foreach (var itemImpuesto in item.ConceptosImpuestos)
                    {
                        if (itemImpuesto.RetencionOTraslado == "Retencion")
                            retenciones.Add(TranslateModelImpuestosToCFDI40.TranslateConceptoImpuestoRetencion(itemImpuesto));
                        else if (itemImpuesto.RetencionOTraslado == "Traslado")
                            traslados.Add(TranslateModelImpuestosToCFDI40.TranslateConceptoImpuestoTraslado(itemImpuesto));
                        else
                            throw new InvalidCastException("Tipo de impuesto no definido / soportado");
                    }
                }

                cfdi40.ComprobanteConceptoImpuestos listaImpuestos = null;
                if (retenciones.Count > 0 || traslados.Count > 0)
                {
                    listaImpuestos = new cfdi40.ComprobanteConceptoImpuestos
                    {
                        Retenciones = retenciones.Count > 0 ? retenciones.ToArray() : null,
                        Traslados = traslados.Count > 0 ? traslados.ToArray() : null,
                    };
                }

                to.Add(new cfdi40.ComprobanteConcepto
                {
                    Cantidad = item.Cantidad,
                    ClaveProdServ = item.ClaveProductoServicio,
                    ClaveUnidad = item.ClaveUnidad,
                    Descripcion = item.Descripcion,
                    Importe = item.Importe,
                    NoIdentificacion = item.noIdentificador,
                    Unidad = item.Unidad,
                    ValorUnitario = item.ValorUnitario,
                    Descuento = item.Descuento,
                    DescuentoSpecified = item.Descuento > 0,
                    ObjetoImp = TranslateModelsToCatalogosCFDI40.TranslateObjetoImp(item.ObjetoImp),
                    Impuestos = listaImpuestos,
                });
            }
            return to;
        }
    }
}
