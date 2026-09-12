using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Interfaces
{
    // Legado: contrato de asignacion de serie/folio para un backend multi-producto/cliente.
    // CFDIBase/CFDIv33/CFDIv40 no lo referencian - solo aparece en los mocks de prueba de
    // TRSF.Invoicing.Test. No es parte del flujo de sellado/timbrado activo.
    public enum EstatusSerieFolio
    {
        disponible = 0,
        emitido ,
        descartado 
    }

    public class SerieFolioStruct
    {
        public String Serie { get; set; }
        public int MinFolio { get; set; }
        public int MaxFolio { get; set; }

    }

    public interface IFolioSerieSetter
    {
        SerieFolioStruct SetSerieFolioStruct(int CustomerId, int ProductId, int DocumentTypeId, int noDocuments);
        SerieFolioItem GetNextSerieFolio { get;  }

        bool SetEstatusSerieFolio(SerieFolioItem item);
    }

    public class SerieFolioItem
    {
        public string Serie { get; set; }
        public string Folio { get; set; }
        public EstatusSerieFolio Estatus { get; set; }
    }
}
