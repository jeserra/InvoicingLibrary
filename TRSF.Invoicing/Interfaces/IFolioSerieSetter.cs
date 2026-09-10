using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRSF.Invoicing.Interfaces
{
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
