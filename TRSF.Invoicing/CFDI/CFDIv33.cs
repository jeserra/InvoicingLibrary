using System;
using System.Text;
using TRSF.Invoicing.Utils;
using System.Xml.Serialization;
using TRSF.Invoicing.Interfaces;
using System.IO;
using Microsoft.Extensions.Logging;

namespace TRSF.Invoicing.CFDI
{
    public class CFDIv33:CFDIBase
    {
        public CFDIv33(ICertificatesRepository certificatesRepository, ISATProvider satProvider, ILogger<CFDIv33> logger = null)
            : base(certificatesRepository, satProvider, logger)
        {

        }

        public string GetXML(cfdi33.Comprobante comprobante)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            // ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            
            ns.Add("Pagos", "http://www.sat.gob.mx/Pagos");
            ns.Add("ValesDeDespensa", "http://www.sat.gob.mx/valesdedespensa");
            //      ns.Add("ecb", "http://www.sat.gob.mx/ecb");


            //     ns.Add("valesdedespensa", "http://www.sat.gob.mx/valesdedespensa");

            return XmlSerializerHelper.ToXmlString<cfdi33.Comprobante>(comprobante, ns, Encoding.UTF8);
        }

        public  string CreateCFDI(TRSF.Invoicing.BindingModels.Comprobante apiComprobante, bool Timbrado = true)
        {
            long transaccion = 100;
            var comprobante = Translates.TranslateModelToCFDI.TranslateToCFDI(apiComprobante);
            comprobante.Certificado = CertificatesRepository.GetCertificate(comprobante.NoCertificado).CerFile;
            var xmlComprobante = GetXML(comprobante);
            comprobante.Sello = SetSeal(comprobante, xmlComprobante, comprobante.NoCertificado);
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

        public string Timbrar(string RFCEmisor, string xmlComprobanteSellado, long transaccion)
        {
            Logger.LogInformation("Enviando comprobante a timbrar para RFC {Rfc}, transaccion {Transaccion}", RFCEmisor, transaccion);
            var xmlTimbrado = SatProvider.Timbrar(RFCEmisor, xmlComprobanteSellado, transaccion);
            var timbrado = UtilTimbrado.ObtenerDatosTimbrado(xmlTimbrado);
            Logger.LogInformation("Comprobante timbrado con UUID {Uuid}", timbrado.UUID);

            return xmlTimbrado;
        }
        public string AgregarConfirmacion (string xml,  string confirmacion)
        {
            // TODO: Agregar lectura del string a stream para deserializar
            //XmlSerializerHelper.Deserialize<Comprobante>();
            throw new System.NotImplementedException();
        }

        public cfdi33.Comprobante DeserializeXML( string xmlCFDIFile)
        {

            //load default template
            //return XmlSerializerHelper.Deserialize<cfdi33.Comprobante>(xmlCFDIFile);
            StringReader reader = new StringReader(xmlCFDIFile);
            return XmlSerializerHelper.Deserialize<cfdi33.Comprobante>(reader);
        }
    }
}
