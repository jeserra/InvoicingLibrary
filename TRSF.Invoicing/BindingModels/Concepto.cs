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

        /// <summary>
        /// CFDI 4.0: obligatorio en cada concepto (catalogo c_ObjetoImp). "01" = No
        /// objeto de impuesto, "02" = Si objeto de impuesto. A diferencia de la mayoria
        /// de los campos nuevos de 4.0, este no tiene un valor "vacio" razonable por
        /// default - debe reflejar si el concepto realmente causa o no impuestos.
        /// </summary>
        public string ObjetoImp { get; set; }

    }
}