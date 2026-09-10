using System.Collections.Generic;
using TRSF.Invoicing.cfdi33;
namespace TRSF.Invoicing.Translates
{
    public class TranslateModelsValesDeDespensa
    {
        public static TRSF.Invoicing.cfdi33.ValesDeDespensa TranslateTo(BindingModels.ValesDeDespensa from)
        {
            if (from != null)
            {
                ValesDeDespensa to = new ValesDeDespensa()
                {
                    version = "1.0",
                    numeroDeCuenta = from.NumeroCuenta,
                    registroPatronal = from.RegistroPatronal,
                    total = from.Total,
                    Conceptos = TranslateToConcepto(from.conceptos).ToArray()
                };

                return to;
            }
            else
                return null;
        }

        public static List<TRSF.Invoicing.cfdi33.ValesDeDespensaConcepto> TranslateToConcepto (List<BindingModels.ConceptosValesDespensa> from)
        {
            var to = new List<ValesDeDespensaConcepto>();
            foreach (var item in from)
            {
                var toItem = new ValesDeDespensaConcepto()
                {
                    curp = item.curp,
                    fecha = item.fecha,
                    identificador = item.identificador,
                    importe = item.Importe,
                    nombre = item.nombre,
                    numSeguridadSocial = item.numSeguridadSocial,
                    rfc = item.rfc

                };
                to.Add(toItem);
            }
            return to;
        }
    }
}
