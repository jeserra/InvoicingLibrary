using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.BindingModels
{
    public class ValesDeDespensa
    {
        public string RegistroPatronal { get; set; }
        public string NumeroCuenta { get; set; }
        public decimal Total { get; set; }
        public List<ConceptosValesDespensa>  conceptos {get; set;}
    }
}
