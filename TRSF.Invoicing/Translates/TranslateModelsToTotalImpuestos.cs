
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Translates
{
    public class TranslateModelsToTotalImpuestos
    {
            public static cfdi33.ComprobanteImpuestos TranslateCuadroImpuesto(List<BindingModels.Concepto> from)
            {
                 decimal TotalRetenciones = 0;
                 decimal TotalTraslados = 0;

            var listaRetenciones = new List<cfdi33.ComprobanteImpuestosRetencion>();
            var listaTranslados = new List<cfdi33.ComprobanteImpuestosTraslado>();
 
            foreach (var item in from)
            {
                if (item.ConceptosImpuestos != null)
                {
                    if (item.ConceptosImpuestos.Count > 0)
                    {
                        foreach (var itemImpuesto in item.ConceptosImpuestos)
                        {
                            if (itemImpuesto.RetencionOTraslado == "Retencion")
                            {
                                var ItemImpuesto = TranslateModelImpuestosToCFDI.TranslateConceptoImpuestoRetencion(itemImpuesto);
                                TotalRetenciones += itemImpuesto.Importe;
                                var result = listaRetenciones.Where(o => o.Impuesto == TranslateModelImpuestosToCFDI.TranslateImpuesto(itemImpuesto.Impuesto));
                                if (result.Count() > 0)
                                    result.FirstOrDefault().Importe += itemImpuesto.Importe;
                                else
                                {
                                    listaRetenciones.Add(
                                     new cfdi33.ComprobanteImpuestosRetencion()
                                     {
                                         Importe = itemImpuesto.Importe,
                                         Impuesto = TranslateModelImpuestosToCFDI.TranslateImpuesto(itemImpuesto.Impuesto)
                                     });
                                }
                            }
                            else if (itemImpuesto.RetencionOTraslado == "Traslado")
                            {
                                var ItemImpuesto = TranslateModelImpuestosToCFDI.TranslateConceptoImpuestoTraslado(itemImpuesto);
                                TotalTraslados += itemImpuesto.Importe;
                                var result = listaTranslados.Where(o => o.Impuesto == TranslateModelImpuestosToCFDI.TranslateImpuesto(itemImpuesto.Impuesto));
                                if (result.Count() > 0)
                                    result.FirstOrDefault().Importe += itemImpuesto.Importe;
                                else
                                {
                                    listaTranslados.Add(
                                     new cfdi33.ComprobanteImpuestosTraslado()
                                     {
                                         Importe = itemImpuesto.Importe,
                                         TasaOCuota = TranslateModelImpuestosToCFDI.TranslateTasaOCuotaTraslado(itemImpuesto.TasaOCuota),
                                         Impuesto = TranslateModelImpuestosToCFDI.TranslateImpuesto(itemImpuesto.Impuesto),
                                         TipoFactor = TranslateModelImpuestosToCFDI.TranslateTipoFactor(itemImpuesto.TipoFactor)
                                     });
                                }
                            }
                            else
                                throw new InvalidCastException("Tipo de impuesto no definido / soportado");
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            cfdi33.ComprobanteImpuestos To = new cfdi33.ComprobanteImpuestos()
            {

                TotalImpuestosRetenidos = TotalRetenciones,
                TotalImpuestosTrasladados = TotalTraslados,
                TotalImpuestosTrasladadosSpecified = (listaTranslados.Count > 0 ? true : false),
                TotalImpuestosRetenidosSpecified = (listaRetenciones.Count > 0 ? true : false),
                Retenciones = (listaRetenciones.Count > 0?listaRetenciones.ToArray():null),
                Traslados = (listaTranslados.Count > 0 ? listaTranslados.ToArray() : null) 
            };
            return To;

        }

    }
}
