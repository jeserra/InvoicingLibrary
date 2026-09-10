using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRSF.Invoicing.Interfaces;
using TRSF.Invoicing.Utils;
using NSubstitute;
using TRSF.Invoicing.cfdi33;
using System.Xml.Serialization;

namespace TRSF.Invoicing.Test
{
    public class SatProviderMoq : ISATProvider
    {
        public byte[] ObtenerQR(string RFC, string UUID, long transactionId)
        {
            throw new NotImplementedException();
        }

        public string Timbrar(string RFC, string Comprobante, long transactionId)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            // ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            ns.Add("Pagos", "http://www.sat.gob.mx/Pagos");


            StringReader reader = new StringReader(Comprobante);
            var serializedComprobante = XmlSerializerHelper.Deserialize<cfdi33.Comprobante>(reader);
            var listcomplementos = serializedComprobante.Complemento.Items.ToList();
            TimbreFiscalDigital timbre = new TimbreFiscalDigital()
            {
                FechaTimbrado = DateTime.Now,
                NoCertificadoSAT = "0000000000000000000",
                UUID = Guid.NewGuid().ToString()
            };

            listcomplementos.Add(timbre);
            serializedComprobante.Complemento.Items = listcomplementos.ToArray();

            return XmlSerializerHelper.ToXmlString<cfdi33.Comprobante>(serializedComprobante, ns, Encoding.UTF8);
           // return Comprobante;
        }
    }
}
