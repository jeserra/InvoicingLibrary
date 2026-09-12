using System;
using QRCoder;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.QRProviders
{
    /// <summary>
    /// Arma la URL publica de verificacion (verificacfdi.facturaelectronica.sat.gob.mx) y
    /// renderiza el QR con QRCoder - sin llamar a ningun PAC. Alternativa a
    /// <see cref="EcodexQRProvider"/>/<see cref="EcodexProvider"/> cuando no se quiere
    /// depender de la disponibilidad de esos endpoints solo para obtener la imagen.
    /// </summary>
    public class LocalQRProvider : ILocalQRProvider
    {
        private const string BaseUrl = "https://verificacfdi.facturaelectronica.sat.gob.mx/default.aspx";

        public byte[] GenerateQR(string rfcEmisor, string rfcReceptor, string uuid, string total, string selloCFD)
        {
            var url = $"{BaseUrl}?id={uuid}&re={rfcEmisor}&rr={rfcReceptor}&tt={total}&fe={UltimosOchoCaracteres(selloCFD)}";

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
            var pngQRCode = new PngByteQRCode(qrCodeData);
            return pngQRCode.GetGraphic(20);
        }

        private static string UltimosOchoCaracteres(string selloCFD)
        {
            if (string.IsNullOrEmpty(selloCFD))
                return string.Empty;

            return selloCFD.Length <= 8 ? selloCFD : selloCFD.Substring(selloCFD.Length - 8);
        }
    }
}
