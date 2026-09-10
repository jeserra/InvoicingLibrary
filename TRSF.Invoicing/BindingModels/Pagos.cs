using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.BindingModels
{
    public class Pagos
    {
        public List<Pago> ListaPagos { get; set; }

    }

    public class Pago
    {
        public DateTimeOffset FechaPago { get; set; }
        public string FormaDePagoP { get; set; }
        public string MonedaP { get; set; }
        public decimal TipoCambioP { get; set; }
        public decimal Monto { get; set; }
        public string NumOperation { get; set; }
        public string RfcEmisorCtaOrd { get; set; }
        public string NomBancoOrdExt { get; set; }
        public string CtaOrdenante { get; set; }
        public string RfcEmisorCtaBen { get; set; }
        public string CtaBeneficiario { get; set; }
        public string TipoCadPago { get; set; }
        public string CertPago { get; set; }
        public string CadPago { get; set; }
        public string SelloPago { get; set; }

        public List<DoctosRelacionados> ListaDocumentos { get; set; }
        public  ImpuestosPagos ImpuestosPagos { get; set; }

    }

    public class DoctosRelacionados
    {
        public Guid idDocumento { get; set; }
        public String Serie { get; set; }
        public int Folio { get; set; }
        public string MonedaDR { get; set; }
        public decimal TipoCambioDR { get; set; }
        public string MetodoDePagoDR { get; set; }
        public int NumParcialidad { get; set; }
        public decimal ImpSaldoAnt { get; set; }
        public decimal ImpPagado { get; set; }
        public decimal ImpSaldoInsoluto { get; set; }

        /// <summary>
        /// Pagos 2.0 (CFDI 4.0): obligatorio. Misma semantica que Concepto.ObjetoImp
        /// pero a nivel del documento relacionado con el pago.
        /// </summary>
        public string ObjetoImpDR { get; set; }
    }

     
}
