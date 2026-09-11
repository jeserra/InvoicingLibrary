using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Demo.Web
{
    /// <summary>
    /// El demo solo sella (CreateCFDI con Timbrado:false) - nunca contacta un PAC, asi
    /// que esta implementacion nunca se invoca realmente. Existe solo porque el
    /// constructor de CFDIv40 requiere un ISATProvider.
    /// </summary>
    public class DemoSatProvider : ISATProvider
    {
        public static readonly DemoSatProvider Instance = new();

        public string Timbrar(string RFC, string Comprobante, long transactionId) =>
            throw new System.NotSupportedException("Este demo solo sella (Timbrado: false); no esta conectado a ningun PAC.");

        public byte[] ObtenerQR(string RFC, string UUID, long transactionId) =>
            throw new System.NotSupportedException("Este demo solo sella (Timbrado: false); no esta conectado a ningun PAC.");
    }
}
