using System;
using System.Collections.Generic;
using Xunit;
using  NSubstitute;
using  TRSF.Invoicing.Translates;
using  TRSF.Invoicing.BindingModels;
using  TRSF.Invoicing.cfdi33;

namespace TRSF.Invoicing.Test.Translates
{
    public class TranslateModelsToConsumoDeCombustiblesTest
    {
        private IConsumoDeCombustibles input;

        public TranslateModelsToConsumoDeCombustiblesTest()
        {
            input = new BindingModels.ConsumoDeCombustibles()
            {
                NumeroDeCuenta = "12321321",
                 SubTotal = 90,
                 Total =  100,
                 Conceptos = new List<ConceptoConsumoDeCombustibles>()
                 {
                     new ConceptoConsumoDeCombustibles()
                     {
                          Cantidad = 50,
                           ClaveEstacion = "FRESA1321",
                            FolioOperacion = "1232131",
                             Identificador = "2PP",
                             Fecha = DateTime.Now,
                             Importe = 5000,
                             NombreCombustible = "Magna sin plomo",
                              ListaDeterminados = new List<Determinados>()
                              {
                                  new Determinados()
                                  {
                                       Impuesto = "IVA",
                                        Importe =4000,
                                         Tasa = 10
                                  }
                              }

                     }
                 }
            };
        }
        [Fact]
        public void TranslateConsumoDeCombustible()
        {
             var output = TRSF.Invoicing.Translates.TranslateModelsToConsumoDeCombustibles.To(input);
            Assert.NotNull(output);

        }
    }
}
