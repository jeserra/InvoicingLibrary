using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Interfaces
{
    // Legado: contrato multi-cuenta (listar CSDs por cuenta). CFDIBase/CFDIv33/CFDIv40 no
    // lo referencian - solo aparece en los mocks de prueba de TRSF.Invoicing.Test. No es
    // parte del flujo de sellado/timbrado activo.
    public interface IInvoicingRepository
    {
       
        List<ICertificate> GetListCertificatesByAccount(int AccountId);

    } 
}
