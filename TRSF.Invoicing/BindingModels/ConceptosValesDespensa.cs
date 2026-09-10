using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.BindingModels
{
    public class ConceptosValesDespensa
    {
        public string identificador { get; set; }

        public DateTime fecha  { get; set; }

        public string rfc { get; set; } 

        public string curp { get; set; } 

        public string nombre { get; set; }

        public string numSeguridadSocial { get; set; }

        public decimal Importe { get; set; }

    }
}
