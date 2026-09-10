using System;
using System.Collections.Generic;
using System.Linq;
using TRSF.Invoicing.BindingModels;

namespace TRSF.Invoicing.Translates40
{
    public class TranslateModelsToTotalImpuestos40
    {
        public static cfdi40.ComprobanteImpuestos TranslateCuadroImpuesto(List<Concepto> from)
        {
            decimal totalRetenciones = 0;
            decimal totalTraslados = 0;

            var listaRetenciones = new List<cfdi40.ComprobanteImpuestosRetencion>();
            var listaTraslados = new List<cfdi40.ComprobanteImpuestosTraslado>();

            foreach (var item in from)
            {
                if (item.ConceptosImpuestos == null || item.ConceptosImpuestos.Count == 0)
                    return null;

                foreach (var itemImpuesto in item.ConceptosImpuestos)
                {
                    if (itemImpuesto.RetencionOTraslado == "Retencion")
                    {
                        totalRetenciones += itemImpuesto.Importe;
                        var impuesto = TranslateModelImpuestosToCFDI40.TranslateImpuesto(itemImpuesto.Impuesto);
                        var existente = listaRetenciones.FirstOrDefault(o => o.Impuesto == impuesto);
                        if (existente != null)
                            existente.Importe += itemImpuesto.Importe;
                        else
                            listaRetenciones.Add(new cfdi40.ComprobanteImpuestosRetencion
                            {
                                Importe = itemImpuesto.Importe,
                                Impuesto = impuesto,
                            });
                    }
                    else if (itemImpuesto.RetencionOTraslado == "Traslado")
                    {
                        totalTraslados += itemImpuesto.Importe;
                        var impuesto = TranslateModelImpuestosToCFDI40.TranslateImpuesto(itemImpuesto.Impuesto);
                        var tasaOCuota = TranslateModelImpuestosToCFDI40.ParseTasaOCuota(itemImpuesto.TasaOCuota);
                        // agrupado por impuesto Y tasa: dos traslados del mismo impuesto a
                        // tasas distintas (p.ej. IVA 16% y IVA 8%) no deben sumarse juntos.
                        var existente = listaTraslados.FirstOrDefault(o => o.Impuesto == impuesto && o.TasaOCuota == tasaOCuota);
                        if (existente != null)
                        {
                            existente.Importe += itemImpuesto.Importe;
                            existente.Base += itemImpuesto.BaseImpuesto;
                        }
                        else
                            listaTraslados.Add(new cfdi40.ComprobanteImpuestosTraslado
                            {
                                Base = itemImpuesto.BaseImpuesto,
                                Importe = itemImpuesto.Importe,
                                ImporteSpecified = true,
                                TasaOCuota = tasaOCuota,
                                TasaOCuotaSpecified = true,
                                Impuesto = impuesto,
                                TipoFactor = TranslateModelImpuestosToCFDI40.TranslateTipoFactor(itemImpuesto.TipoFactor),
                            });
                    }
                    else
                        throw new InvalidCastException("Tipo de impuesto no definido / soportado");
                }
            }

            return new cfdi40.ComprobanteImpuestos
            {
                TotalImpuestosRetenidos = totalRetenciones,
                TotalImpuestosRetenidosSpecified = listaRetenciones.Count > 0,
                TotalImpuestosTrasladados = totalTraslados,
                TotalImpuestosTrasladadosSpecified = listaTraslados.Count > 0,
                Retenciones = listaRetenciones.Count > 0 ? listaRetenciones.ToArray() : null,
                Traslados = listaTraslados.Count > 0 ? listaTraslados.ToArray() : null,
            };
        }
    }
}
