using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRSF.Invoicing.BindingModels
{
    public class Emisor
    {
        public string RFC { get; set; }
        public string Nombre { get; set; }
        public string RegimenFiscal { get; set; }
    }
}