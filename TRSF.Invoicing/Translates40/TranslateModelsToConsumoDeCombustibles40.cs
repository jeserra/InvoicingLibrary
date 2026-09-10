using System;
using System.Collections.Generic;
using TRSF.Invoicing.BindingModels;

namespace TRSF.Invoicing.Translates40
{
    /// <summary>
    /// ConsumoDeCombustibles es un complemento independiente de la version de CFDI -
    /// esta clase existe solo porque xsd.exe genero los tipos bajo el namespace
    /// TRSF.Invoicing.cfdi40, distintos de TRSF.Invoicing.cfdi33.
    /// </summary>
    public class TranslateModelsToConsumoDeCombustibles40
    {
        public static cfdi40.ConsumoDeCombustibles To(IConsumoDeCombustibles from)
        {
            var listConceptos = new List<cfdi40.ConsumoDeCombustiblesConceptoConsumoDeCombustibles>();
            foreach (var item in from.Conceptos)
            {
                listConceptos.Add(TranslateConcepto(item));
            }

            return new cfdi40.ConsumoDeCombustibles
            {
                numeroDeCuenta = from.NumeroDeCuenta,
                subTotal = from.SubTotal,
                total = from.Total,
                Conceptos = listConceptos.ToArray(),
            };
        }

        private static cfdi40.ConsumoDeCombustiblesConceptoConsumoDeCombustibles TranslateConcepto(ConceptoConsumoDeCombustibles from)
        {
            var listaDeterminados = new List<cfdi40.ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminado>();
            foreach (var item in from.ListaDeterminados)
            {
                listaDeterminados.Add(TranslateToDeterminado(item));
            }

            return new cfdi40.ConsumoDeCombustiblesConceptoConsumoDeCombustibles
            {
                cantidad = from.Cantidad,
                claveEstacion = from.ClaveEstacion,
                fecha = from.Fecha,
                folioOperacion = from.FolioOperacion,
                identificador = from.Identificador,
                rfc = from.RFC,
                importe = from.Importe,
                valorUnitario = from.ValorUnitario,
                nombreCombustible = from.NombreCombustible,
                Determinados = listaDeterminados.ToArray(),
            };
        }

        private static cfdi40.ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminado TranslateToDeterminado(Determinados from)
        {
            return new cfdi40.ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminado
            {
                tasa = from.Tasa,
                importe = from.Importe,
                impuesto = TranslateImpuesto(from.Impuesto),
            };
        }

        private static cfdi40.ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminadoImpuesto TranslateImpuesto(string from)
        {
            if (Enum.TryParse(from, out cfdi40.ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminadoImpuesto to))
                return to;
            throw new InvalidCastException("El tipo de impuesto no esta definido / soportado en el complemento Consumo de combustibles");
        }
    }
}
