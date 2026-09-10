using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TRSF.Invoicing.BindingModels
{
    public class Receptor
    {
        public string RFC { get; set; }
        public String Nombre { get; set; }

        /// <summary>CFDI 4.0: obligatorio. Codigo postal del domicilio fiscal del receptor.</summary>
        public string DomicilioFiscalReceptor { get; set; }

        /// <summary>CFDI 4.0: obligatorio. Regimen fiscal del receptor.</summary>
        public string RegimenFiscalReceptor { get; set; }

        /// <summary>
        /// Codigo de uso del CFDI (catalogo c_UsoCFDI). Vive en cfdi:Receptor en el
        /// esquema real. Comprobante.UsoCFDI tambien existe todavia y es lo que usa la
        /// ruta de creacion CFDI 3.3 actual (CFDIv33/TranslateModelToCFDI); esta
        /// propiedad es la que usara la ruta CFDI 4.0. Una vez que la ruta 3.3 se
        /// retire, Comprobante.UsoCFDI debe eliminarse para no tener el campo duplicado.
        /// </summary>
        public string UsoCFDI { get; set; }

        /// <summary>Opcional. Solo aplica si ResidenciaFiscal esta definida (receptor extranjero).</summary>
        public string NumRegIdTrib { get; set; }

        /// <summary>Opcional. Pais de residencia fiscal del receptor si es extranjero (catalogo c_Pais).</summary>
        public string ResidenciaFiscal { get; set; }
    }
}