using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Test
{
    public class SerieFolioMoq : IFolioSerieSetter
    {


        private SerieFolioItem _serieFolioItem;
        private SerieFolioStruct _serieFolioStruct;

        public SerieFolioMoq()
        {
            _serieFolioStruct = new SerieFolioStruct()
            {
                MinFolio = 1,
                MaxFolio = 2,
                Serie = "AAA"
            };

            _serieFolioItem = new SerieFolioItem()
            {
                Serie = "AAA",

                Folio = "1",
                  Estatus = EstatusSerieFolio.disponible
            };

        }


        public  SerieFolioItem GetNextSerieFolio
        {
            get {
                return _serieFolioItem;
            }             
        }

        public bool SetEstatusSerieFolio(SerieFolioItem item)
        {
            return true;
        }

        public SerieFolioStruct SetSerieFolioStruct(int CustomerId, int ProductId, int DocumentTypeId, int noDocuments)
        {
            
            return _serieFolioStruct;

            
        }
    }
}
