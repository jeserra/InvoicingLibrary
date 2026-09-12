using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Interfaces
{
    /// <summary>
    /// Cliente de un PAC (Proveedor Autorizado de Certificacion) para timbrar un CFDI ya
    /// sellado. Requerido por CFDIBase/CFDIv33/CFDIv40 - su constructor lo exige aunque el
    /// caller nunca llame a CreateCFDI con Timbrado:true (ver
    /// TRSF.Invoicing.CFDIProviders.EcodexProvider para una implementacion SOAP real, o un
    /// stub que lance NotSupportedException si su flujo solo sella sin timbrar).
    /// </summary>
    public interface ISATProvider
    {
        /// <summary>
        /// Envia el XML ya sellado (con Sello/Certificado/NoCertificado, sin
        /// TimbreFiscalDigital todavia) al PAC y devuelve el XML timbrado (con el
        /// complemento TimbreFiscalDigital ya insertado por el PAC).
        /// </summary>
        /// <param name="RFC">RFC del emisor, para autenticar la sesion con el PAC.</param>
        /// <param name="Comprobante">XML del comprobante ya sellado.</param>
        /// <param name="transactionId">Identificador de transaccion propio del PAC/integrador.</param>
        String Timbrar(String RFC, String Comprobante, long transactionId);

        /// <summary>
        /// Pide al PAC la imagen QR de verificacion para un CFDI ya timbrado. Alternativa
        /// hospedada-por-el-PAC a <see cref="ILocalQRProvider"/> (que arma el mismo QR
        /// localmente, sin esta llamada).
        /// </summary>
        byte[] ObtenerQR(string RFC,  String UUID, long transactionId);
    }
}
