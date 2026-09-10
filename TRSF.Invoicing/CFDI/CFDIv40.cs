using System;
using System.Text;
using TRSF.Invoicing.Utils;
using System.Xml.Serialization;
using TRSF.Invoicing.Interfaces;
using System.IO;
using Microsoft.Extensions.Logging;

namespace TRSF.Invoicing.CFDI
{
    public class CFDIv40 : CFDIBase
    {
        public CFDIv40(ICertificatesRepository certificatesRepository, ISATProvider satProvider, ILogger<CFDIv40> logger = null)
            : base(certificatesRepository, satProvider, logger)
        {

        }

        public string GetXML(cfdi40.Comprobante comprobante)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("cfdi", "http://www.sat.gob.mx/cfd/4");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            ns.Add("Pagos20", "http://www.sat.gob.mx/Pagos20");
            ns.Add("ValesDeDespensa", "http://www.sat.gob.mx/valesdedespensa");

            return XmlSerializerHelper.ToXmlString<cfdi40.Comprobante>(comprobante, ns, Encoding.UTF8);
        }

        public string CreateCFDI(TRSF.Invoicing.BindingModels.Comprobante apiComprobante, bool Timbrado = true)
        {
            long transaccion = 100;
            var comprobante = Translates40.TranslateModelToCFDI40.TranslateToCFDI(apiComprobante);
            comprobante.Certificado = CertificatesRepository.GetCertificate(comprobante.NoCertificado).CerFile;
            var xmlComprobante = GetXML(comprobante);
            comprobante.Sello = SetSeal(xmlComprobante, comprobante.NoCertificado, CadenaOriginal40XsltPath);
            var xmlComprobanteSellado = GetXML(comprobante);

            if (!Timbrado)
                return xmlComprobanteSellado;
            else
            {
                return Timbrar(apiComprobante, xmlComprobanteSellado, transaccion);
            }
        }

        public string Timbrar(TRSF.Invoicing.BindingModels.Comprobante apiComprobante, string xmlComprobanteSellado, long transaccion)
        {
            Logger.LogInformation("Enviando comprobante a timbrar para RFC {Rfc}, transaccion {Transaccion}", apiComprobante.Emisor.RFC, transaccion);
            var xmlTimbrado = SatProvider.Timbrar(apiComprobante.Emisor.RFC, xmlComprobanteSellado, transaccion);
            var timbrado = UtilTimbrado.ObtenerDatosTimbrado(xmlTimbrado);
            apiComprobante.UUID = Guid.Parse(timbrado.UUID);
            apiComprobante.FechaTimbrado = timbrado.FechaTimbrado;
            Logger.LogInformation("Comprobante timbrado con UUID {Uuid}", apiComprobante.UUID);
            return xmlTimbrado;
        }

        public cfdi40.Comprobante DeserializeXML(string xmlCFDIFile)
        {
            StringReader reader = new StringReader(xmlCFDIFile);
            return XmlSerializerHelper.Deserialize<cfdi40.Comprobante>(reader);
        }
    }
}
