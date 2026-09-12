using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Interfaces
{
    /// <summary>
    /// Un CSD (Certificado de Sello Digital) del emisor, ya cargado en memoria. Lo que el
    /// flujo de sellado (CFDIBase.SetSeal) realmente lee es <see cref="KeyFile"/> +
    /// <see cref="Pwd"/> (para firmar la cadena original) y <see cref="CerFile"/> (para el
    /// atributo Certificado del XML) - <see cref="idCertificate"/> es una llave de base de
    /// datos heredada sin uso en este repo, y <see cref="ValidFrom"/>/<see cref="ValidUntil"/>
    /// son solo informativos (se exponen en las respuestas de register_certificate/Paso 2,
    /// nunca se validan contra la fecha del comprobante).
    /// </summary>
    public interface ICertificate
    {
        int idCertificate { get; set; }

        /// <summary>Contenido del .cer, en Base64 - va directo al atributo Certificado del CFDI.</summary>
        string CerFile { get; set; }

        /// <summary>Contenido del .key (PKCS8 cifrado), en Base64 - se descifra con <see cref="Pwd"/> para firmar.</summary>
        string KeyFile { get; set; }

        /// <summary>Contrasena de la llave privada.</summary>
        string Pwd { get; set; }

        /// <summary>Numero de certificado del CSD - la misma clave que <see cref="ICertificatesRepository.GetCertificate(string)"/> recibe.</summary>
        string NoCertificate { get; set; }

        DateTime ValidFrom { get; set; }
        DateTime ValidUntil { get; set; }
    }
}
