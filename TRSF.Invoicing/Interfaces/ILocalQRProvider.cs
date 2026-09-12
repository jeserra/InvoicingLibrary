namespace TRSF.Invoicing.Interfaces
{
    /// <summary>
    /// Genera la imagen QR de verificacion del CFDI localmente: el QR del SAT solo
    /// codifica una URL publica armada con datos que ya existen tras el timbrado
    /// (UUID, RFCs, Total, Sello), asi que no requiere contactar al PAC.
    /// </summary>
    public interface ILocalQRProvider
    {
        /// <param name="total">
        /// El Total exactamente como quedo serializado en el XML timbrado (mismo numero de
        /// decimales) - el QR debe reflejar ese valor, no uno reformateado.
        /// </param>
        byte[] GenerateQR(string rfcEmisor, string rfcReceptor, string uuid, string total, string selloCFD);
    }
}
