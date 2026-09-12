using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Interfaces
{
    /// <summary>
    /// Resuelve el CSD (Certificado de Sello Digital) del emisor. Requerido por
    /// CFDIBase/CFDIv33/CFDIv40 - solo <see cref="GetCertificate(string)"/> es llamado por
    /// el flujo de sellado que este repositorio trae (CreateCFDI lo usa para leer
    /// Certificado/Sello); los otros dos miembros son un contrato multi-cuenta heredado que
    /// nada en este repo invoca hoy - un implementador que solo necesite sellar puede
    /// lanzar NotSupportedException en ellos.
    /// </summary>
    public interface ICertificatesRepository
    {
        /// <summary>Busca el CSD por su numero de certificado (el mismo que va en el atributo NoCertificado del CFDI). Llamado por CFDIv33/CFDIv40.CreateCFDI y por CFDIBase.SetSeal.</summary>
        ICertificate GetCertificate(string noCertificado);

        /// <summary>Contrato multi-cuenta heredado (busqueda por cuenta+RFC) - sin llamadores en este repo.</summary>
        ICertificate GetCertificate(string AccountId, String RFC);

        /// <summary>Contrato multi-cuenta heredado (persistir un CSD) - sin llamadores en este repo.</summary>
        bool SaveCertificate(int accountId, ICertificate certificate);
    }
}
