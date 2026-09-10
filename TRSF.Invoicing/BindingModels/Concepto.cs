using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRSF.Invoicing.BindingModels
{
    public class Concepto
    {
        public Concepto()
        {
            ConceptosImpuestos = new List<ConceptoImpuestos>();
        }

        public string noIdentificador { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public string ClaveUnidad { get; set; }
        public string ClaveProductoServicio { get; set; }
        public string Unidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Importe { get; set; }
        public decimal Descuento { get; set; }
        public List<ConceptoImpuestos> ConceptosImpuestos { get; set; }

    }
}