﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using  NSubstitute;
using  InvoicingLibrary.Translates;
using  InvoicingLibrary.BindingModels;
using  InvoicingLibrary.cfdi33;

namespace InvoicingLibrary.Test.Translates
{
    [TestClass]
    public class TranslateModelsToConsumoDeCombustiblesTest
    {
        private IConsumoDeCombustibles input;

        [TestInitialize]
        public void TestInitialize()
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
        [TestMethod]
        public void TranslateConsumoDeCombustible()
        {
             var output = InvoicingLibrary.Translates.TranslateModelsToConsumoDeCombustibles.To(input);
            Assert.IsNotNull(output);

        }
    }
}
