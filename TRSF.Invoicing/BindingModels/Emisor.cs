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

        /// <summary>
        /// CFDI 4.0: RFC del contribuyente que actua a nombre y por cuenta del emisor
        /// (facturacion a traves de un adquirente). Opcional.
        /// </summary>
        public string FacAtrAdquirente { get; set; }
    }
}