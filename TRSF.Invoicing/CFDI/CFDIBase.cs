using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TRSF.Invoicing.Interfaces;
using System.Xml.Xsl;
using System.Xml;

namespace TRSF.Invoicing.CFDI
{
    public class CFDIBase
    {
        internal ICertificatesRepository CertificatesRepository;
        internal ISATProvider SatProvider;
        protected readonly ILogger Logger;

        public CFDIBase(ICertificatesRepository certificatesRepository,  ISATProvider satProvider, ILogger logger = null)
        {
            CertificatesRepository = certificatesRepository;
            SatProvider = satProvider;
            Logger = logger ?? NullLogger.Instance;
        }
         
        public static string CadenaOriginal33XsltPath { get; set; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xslt", "cadenaoriginal_3_3.xslt");

        public static string CadenaOriginal40XsltPath { get; set; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xslt", "cadenaoriginal_4_0.xslt");

         public string XMLToString(System.Xml.XmlDocument xmlDoc)
        {
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter sw = new System.IO.StringWriter(sb);
            xmlDoc.Save(sw);
            return sw.ToString();
        }

        public string GetOriginalChain(string stringXML)
        {
            return GetOriginalChain(stringXML, CadenaOriginal33XsltPath);
        }

        public string GetOriginalChain(string stringXML, string xsltPath)
        {
            StringWriter sw = new StringWriter();

            if (!File.Exists(xsltPath))
            {
                throw new FileNotFoundException(
                    "No se encontro el XSLT de cadena original. Coloque el archivo " +
                    "correspondiente en la ruta indicada, o ajuste " +
                    "CFDIBase.CadenaOriginal33XsltPath / CadenaOriginal40XsltPath. Este " +
                    "archivo ya no se descarga de una URL remota por razones de seguridad.",
                    xsltPath);
            }

            try
            {
                XslCompiledTransform xslt = new System.Xml.Xsl.XslCompiledTransform();
                // document()/script deliberadamente deshabilitados (XsltSettings.Default):
                // el transform ya no proviene de la red, y no hay razon para permitir
                // que ejecute script embebido ni lea archivos arbitrarios.
                xslt.Load(xsltPath, XsltSettings.Default, new XmlUrlResolver());

                XmlDocument FromXmlFile = new System.Xml.XmlDocument();
                FromXmlFile.LoadXml(stringXML);

                xslt.Transform(FromXmlFile, null, sw);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "No se pudo generar la cadena original desde el XSLT {XsltPath}", xsltPath);
                throw new Exception("No se pudo generar la cadena", ex.InnerException);
            }
            return sw.ToString();
        }

        public byte[] GetSHA1(string OriginalChain)
        {
            return SHA1.HashData(Encoding.UTF8.GetBytes(OriginalChain));
        }

        public byte[] GetSHA256(string OriginalChain)
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(OriginalChain));
        }
        public string SetSeal(cfdi33.Comprobante CFDIComprobante, string TheXML, string noCertificado)
        {
            return SetSeal(TheXML, noCertificado, CadenaOriginal33XsltPath);
        }

        public string SetSeal(string TheXML, string noCertificado, string xsltPath)
        {
            ICertificate certificate = CertificatesRepository.GetCertificate(noCertificado);
            string OriginalChain = GetOriginalChain(TheXML, xsltPath);

            byte[] SHA256hash = GetSHA256(OriginalChain);

            using RSA privateKey = LoadPrivateKeyFromString(certificate.Pwd, certificate.KeyFile);

            return GetSeal(SHA256hash, privateKey);
        }

        public string GetSeal(byte[] rgbHash, RSA privateKey)
        {
            byte[] signature = privateKey.SignHash(rgbHash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signature);
        }

        public RSA LoadPrivateKeyFromString(string password, string keyFile)
        {
            byte[] privateKeyBytes = Convert.FromBase64String(keyFile);
            RSA rsa = RSA.Create();
            try
            {
                rsa.ImportEncryptedPkcs8PrivateKey(password, privateKeyBytes, out _);
                return rsa;
            }
            catch (CryptographicException ex)
            {
                rsa.Dispose();
                Logger.LogError(ex, "No se pudo descifrar la llave privada del CSD (contrasena incorrecta o archivo .key corrupto/no valido)");
                throw new InvalidOperationException(
                    "No se pudo descifrar la llave privada del CSD (contrasena incorrecta o " +
                    "archivo .key corrupto/no valido).", ex);
            }
        }

    }
}
