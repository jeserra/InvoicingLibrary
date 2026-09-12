using System.IO;

namespace TRSF.Invoicing.Interfaces
{
    /// <summary>
    /// Renderiza la representacion impresa (PDF) de un CFDI ya sellado/timbrado. No es
    /// requerido por CFDIBase/CFDIv33/CFDIv40 - es un paso posterior que el caller invoca
    /// por su cuenta (ver TRSF.Invoicing.PDF.ItextPDFProvider para una implementacion de
    /// referencia con iText7, valida para ambas versiones). <paramref name="imageQR"/> se
    /// recibe ya renderizado en bytes: esta interfaz deliberadamente no genera el QR ella
    /// misma, para no atarse a <see cref="IQRProvider"/> ni a <see cref="ILocalQRProvider"/>
    /// - el caller elige la fuente y puede pasar null si aun no hay UUID (comprobante sin
    /// timbrar).
    /// </summary>
    public interface IPDFProvider
    {
        void EscribirCFDIPDF(cfdi33.Comprobante comprobante, Stream destino, byte[] imageQR, string emisorDireccionLinea1 = null, string emisorDireccionLinea2 = null);

        void EscribirCFDIPDF(cfdi40.Comprobante comprobante, Stream destino, byte[] imageQR, string emisorDireccionLinea1 = null, string emisorDireccionLinea2 = null);
    }
}
