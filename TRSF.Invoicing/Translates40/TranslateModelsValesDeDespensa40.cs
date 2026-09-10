using System.Collections.Generic;
using TRSF.Invoicing.BindingModels;

namespace TRSF.Invoicing.Translates40
{
    /// <summary>
    /// ValesDeDespensa es un complemento independiente de la version de CFDI (misma
    /// estructura desde 3.2) - esta clase existe solo porque xsd.exe genero los tipos
    /// bajo el namespace TRSF.Invoicing.cfdi40, distintos de TRSF.Invoicing.cfdi33.
    /// </summary>
    public class TranslateModelsValesDeDespensa40
    {
        public static cfdi40.ValesDeDespensa TranslateTo(ValesDeDespensa from)
        {
            if (from == null)
                return null;

            return new cfdi40.ValesDeDespensa
            {
                version = "1.0",
                numeroDeCuenta = from.NumeroCuenta,
                registroPatronal = from.RegistroPatronal,
                total = from.Total,
                Conceptos = TranslateToConcepto(from.conceptos).ToArray(),
            };
        }

        public static List<cfdi40.ValesDeDespensaConcepto> TranslateToConcepto(List<ConceptosValesDespensa> from)
        {
            var to = new List<cfdi40.ValesDeDespensaConcepto>();
            foreach (var item in from)
            {
                to.Add(new cfdi40.ValesDeDespensaConcepto
                {
                    curp = item.curp,
                    fecha = item.fecha,
                    identificador = item.identificador,
                    importe = item.Importe,
                    nombre = item.nombre,
                    numSeguridadSocial = item.numSeguridadSocial,
                    rfc = item.rfc,
                });
            }
            return to;
        }
    }
}
