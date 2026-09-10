using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRSF.Invoicing.BindingModels;
using TRSF.Invoicing.cfdi33;
namespace TRSF.Invoicing.Translates
{
    public class TranslateModelsToConsumoDeCombustibles
    {
        public static cfdi33.ConsumoDeCombustibles To(BindingModels.IConsumoDeCombustibles from)
        {
            var  listConceptos = new List<ConsumoDeCombustiblesConceptoConsumoDeCombustibles>();
            foreach (var item in from.Conceptos)
            {
                listConceptos.Add(TranslateConcepto(item));
            }

            var to = new cfdi33.ConsumoDeCombustibles()
            {
                  numeroDeCuenta   =  from.NumeroDeCuenta,
                  subTotal =  from.SubTotal,
                  total =  from.Total,
                  Conceptos = listConceptos.ToArray()
            };
            return to;
        }

        private static cfdi33.ConsumoDeCombustiblesConceptoConsumoDeCombustibles TranslateConcepto(
            BindingModels.ConceptoConsumoDeCombustibles from)
        {
            var listaDeterminados = new List<ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminado>();

            foreach (var item in from.ListaDeterminados)
            {
                listaDeterminados.Add(TranslateToDeterminado(item));
            }
            var to = new cfdi33.ConsumoDeCombustiblesConceptoConsumoDeCombustibles()
            { 
                 cantidad =  from.Cantidad,
                 claveEstacion =  from.ClaveEstacion,
                 fecha =  from.Fecha,
                 folioOperacion =  from.FolioOperacion,
                 identificador =  from.Identificador,
                  rfc =  from.RFC,
                  importe =  from.Importe,
                  valorUnitario =  from.ValorUnitario,
                  nombreCombustible = from.NombreCombustible,
                  Determinados = listaDeterminados.ToArray()
            };
            return to;
        }

        private static cfdi33.ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminado TranslateToDeterminado(
            BindingModels.Determinados from)
        {
            var to = new ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminado()
            {
                  tasa =  from.Tasa,
                  importe =  from.Importe,
                  impuesto =  TranslateImpuesto(from.Impuesto)
            };
            return to;
        }

        private static ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminadoImpuesto TranslateImpuesto
            (String from)
        {
            ConsumoDeCombustiblesConceptoConsumoDeCombustiblesDeterminadoImpuesto to;
            if (Enum.TryParse(from, out to))
                return to;
            else
                throw  new InvalidCastException("El tipo de impuesto no esta definido / soportado en el complemento Consumo de combustibles");

        }
    }

}
