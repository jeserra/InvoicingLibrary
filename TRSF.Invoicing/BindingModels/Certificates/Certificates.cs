using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.BindingModels.Certificates
{
   public class Certificates:ICertificate
    {

        public int idCertificate
        {
            get;
            set; 
        }

        public string CerFile
        {
            get;
            set; 
        }

        public string KeyFile
        {
            get;
            set; 
        }

        public string Pwd
        {
            get;

            set;  
        }

        public string NoCertificate
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTime ValidFrom
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTime ValidUntil
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
