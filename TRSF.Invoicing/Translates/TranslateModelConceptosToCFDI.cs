using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Translates
{
    public class TranslateModelConceptosToCFDI
    {
        public static List<cfdi33.ComprobanteConcepto> TranslateConceptos(List<BindingModels.Concepto> from)
        {
            List<cfdi33.ComprobanteConcepto> to = new List<cfdi33.ComprobanteConcepto>();
            foreach (var item in from)
            {
                List<cfdi33.ComprobanteConceptoImpuestosRetencion> Retenciones = new List<cfdi33.ComprobanteConceptoImpuestosRetencion>();
                List<cfdi33.ComprobanteConceptoImpuestosTraslado> Traslados = new List<cfdi33.ComprobanteConceptoImpuestosTraslado>();

                if (item.ConceptosImpuestos != null)
                {
                    if(item.ConceptosImpuestos.Count > 0)
                    { 
                        foreach (var itemImpuesto in item.ConceptosImpuestos)
                        {
                            if (itemImpuesto.RetencionOTraslado == "Retencion")
                            {
                                Retenciones.Add(TranslateModelImpuestosToCFDI.TranslateConceptoImpuestoRetencion(itemImpuesto));
                            }
                            else if (itemImpuesto.RetencionOTraslado == "Traslado")
                            {
                                Traslados.Add(TranslateModelImpuestosToCFDI.TranslateConceptoImpuestoTraslado(itemImpuesto));
                            }
                            else
                                throw new InvalidCastException("Tipo de impuesto no definido / soportado");
                        }
                    }                   
                }

                cfdi33.ComprobanteConceptoImpuestos listaImpuestos = null;

                if (Retenciones.Count > 0 || Traslados.Count > 0)
                {
                    listaImpuestos = new cfdi33.ComprobanteConceptoImpuestos()
                    {
                        Retenciones = (Retenciones.Count > 0 ? Retenciones.ToArray() : null),
                        Traslados = (Traslados.Count > 0 ? Traslados.ToArray() : null)
                    };
                }

                cfdi33.ComprobanteConcepto concepto = new cfdi33.ComprobanteConcepto()
                {
                    Cantidad = item.Cantidad,
                    ClaveProdServ = item.ClaveProductoServicio, //TranslateModelsToCatalogosCFDI.TranslateClaveProdServ(item.ClaveProductoServicio),
                    ClaveUnidad = TranslateModelsToCatalogosCFDI.TranslateClaveUnidad(item.ClaveUnidad),
                    Descripcion = item.Descripcion,
                    Importe = item.Importe,
                    NoIdentificacion = item.noIdentificador,
                    Unidad = item.Unidad,
                    ValorUnitario = item.ValorUnitario,
                        Impuestos = listaImpuestos
                };
                to.Add(concepto);
            }
            return to;
        }
    }
}
