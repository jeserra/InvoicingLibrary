using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.BindingModels
{
    public interface IConsumoDeCombustibles
    {
        string NumeroDeCuenta { get; set; }
        decimal SubTotal { get; set; }
        decimal Total { get; set; }
        List<ConceptoConsumoDeCombustibles> Conceptos { get; set; }
    }

    public class ConsumoDeCombustibles : IConsumoDeCombustibles
    {
         
        public string NumeroDeCuenta { get; set; }

        public decimal SubTotal { get; set; }

        public decimal Total { get; set; }

        public List<ConceptoConsumoDeCombustibles> Conceptos { get; set; }
    }
}
