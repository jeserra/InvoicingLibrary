using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.Demo.Web
{
    /// <summary>
    /// ICertificate en memoria para el demo - nunca se persiste a disco. A diferencia de
    /// TRSF.Invoicing.Test.Certifcate.CertificateMoq (que usa X509Certificate.GetPublicKey()
    /// para CerFile, solo la llave publica), aqui CerFile es el base64 del archivo .cer
    /// completo (DER), que es lo que realmente espera el atributo cfdi:Comprobante/@Certificado.
    /// </summary>
    public class DemoCertificate : ICertificate
    {
        public string CerFile { get; set; } = "";
        public string KeyFile { get; set; } = "";
        public string Pwd { get; set; } = "";
        public string NoCertificate { get; set; } = "";
        public int idCertificate { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }

        public static DemoCertificate FromBytes(byte[] cerBytes, byte[] keyBytes, string password)
        {
            var x509 = X509CertificateLoader.LoadCertificate(cerBytes);

            byte[] serialNumber = x509.GetSerialNumber();
            Array.Reverse(serialNumber);

            return new DemoCertificate
            {
                CerFile = Convert.ToBase64String(cerBytes),
                KeyFile = Convert.ToBase64String(keyBytes),
                Pwd = password,
                NoCertificate = Encoding.UTF8.GetString(serialNumber),
                ValidFrom = x509.NotBefore,
                ValidUntil = x509.NotAfter,
            };
        }
    }

    /// <summary>Repositorio de un solo certificado, en memoria, para la sesion del demo.</summary>
    public class DemoCertificateRepository : ICertificatesRepository
    {
        private readonly DemoCertificate certificate;

        public DemoCertificateRepository(DemoCertificate certificate)
        {
            this.certificate = certificate;
        }

        public ICertificate GetCertificate(string noCertificado) => certificate;

        public ICertificate GetCertificate(string accountId, string rfc) => certificate;

        public bool SaveCertificate(int accountId, ICertificate certificate) =>
            throw new NotSupportedException("El demo no persiste certificados.");
    }
}
