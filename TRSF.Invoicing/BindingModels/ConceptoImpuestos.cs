using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.BindingModels
{
    public class ConceptoImpuestos
    {
        public decimal BaseImpuesto { get; set; }
        public decimal Importe { get; set; }
        public string Impuesto { get; set; }  // IVA, ISR, IEPS
        public string TipoFactor { get; set; }
        public string TasaOCuota { get; set; }
        public string RetencionOTraslado { get; set; }
    }
}
