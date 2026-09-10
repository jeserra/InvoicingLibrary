using System;
using System.Text;
using TRSF.Invoicing.Interfaces;
using System.IO;
using System.Security.Cryptography.X509Certificates;
namespace TRSF.Invoicing.Test.Certifcate
{
    public class CertificateMoq : ICertificate
    {

        
        private void Initialize ()
        {
           var pathCer = Path.Combine(AppContext.BaseDirectory, "Resources", "CSD_Pruebas_CFDI_LAN7008173R5.cer");
           var pathKey = Path.Combine(AppContext.BaseDirectory, "Resources", "CSD_Pruebas_CFDI_LAN7008173R5.key");
            X509Certificate x509Certificate = X509CertificateLoader.LoadCertificateFromFile(pathCer);

            CerFile = Convert.ToBase64String(x509Certificate.GetPublicKey());
            byte[] serialNumber = x509Certificate.GetSerialNumber();
            Array.Reverse(serialNumber);
            NoCertificate = Encoding.UTF8.GetString(serialNumber);
            Pwd = "12345678a";
            KeyFile = Convert.ToBase64String( File.ReadAllBytes(pathKey));
        }
     
        public CertificateMoq()
        {
            Initialize();
        }
        public string CerFile
        {
            get;
            set;            
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

        public int idCertificate
        {
            get;
            set;
        }

        public string KeyFile
        {
            get;
            set;
        }

        public string NoCertificate
        {
            get;
            set;
        }

        public string Pwd
        {
            get;
            set;
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
