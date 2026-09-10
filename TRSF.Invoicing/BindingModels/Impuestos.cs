using System;
using System.Collections.Generic;

namespace TRSF.Invoicing.BindingModels
{
    public class ImpuestosPagos
    {
        public List<Traslado> Traslados { get; set; }
        public List<Retencion>  Retenciones { get; set; }

        public decimal TotalImpuestosRetenidos;
        public decimal TotalImpuestosTrasladadosField;
    }

    public class Traslado
    {
        public String Impuesto { get; set; }
        public decimal Importe { get; set; }
    }

    public class Retencion
    {
        public String Impuesto { get; set; }
        public decimal Importe { get; set; }
        public decimal Tasa { get; set; }
    }


    
}