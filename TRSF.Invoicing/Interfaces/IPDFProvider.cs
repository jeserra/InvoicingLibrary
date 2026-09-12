using System.IO;

namespace TRSF.Invoicing.Interfaces
{
    public interface IPDFProvider
    {
        void EscribirCFDIPDF(cfdi33.Comprobante comprobante, Stream destino, byte[] imageQR, string emisorDireccionLinea1 = null, string emisorDireccionLinea2 = null);

        void EscribirCFDIPDF(cfdi40.Comprobante comprobante, Stream destino, byte[] imageQR, string emisorDireccionLinea1 = null, string emisorDireccionLinea2 = null);
    }
}
