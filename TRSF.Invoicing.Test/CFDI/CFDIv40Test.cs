using System;
using Xunit;
using TRSF.Invoicing.CFDI;
using NSubstitute;
using TRSF.Invoicing.Interfaces;
using TRSF.Invoicing.Test.Certifcate;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace TRSF.Invoicing.Test.CFDI
{
    public class CFDIv40Test
    {
        public BindingModels.Comprobante _testComprobante;
        public Interfaces.ICertificatesRepository _MockRepository;
        public Interfaces.ISATProvider _moqSatProvider;

        public void InitializeRepository()
        {
            _MockRepository = Substitute.For<ICertificatesRepository>();
            _MockRepository.GetCertificate("20001000000200000258").ReturnsForAnyArgs(new CertificateMoq());

            _moqSatProvider = new SatProviderMoq();
        }

        public CFDIv40Test()
        {
            InitializeRepository();

            _testComprobante = new BindingModels.Comprobante()
            {
                Version = "4.0",
                Exportacion = "01",
                Fecha = new DateTime(2017, 6, 29, 12, 0, 0),
                Emisor = new BindingModels.Emisor()
                {
                    Nombre = "Juan perez",
                    RegimenFiscal = "611",
                    RFC = "LAN7008173R5"
                },
                Receptor = new BindingModels.Receptor()
                {
                    Nombre = "Pepe perez",
                    RFC = "AAA010101AAA",
                    DomicilioFiscalReceptor = "99100",
                    RegimenFiscalReceptor = "616",
                    UsoCFDI = "G03",
                },
                Conceptos = new System.Collections.Generic.List<BindingModels.Concepto>()
                {
                    new BindingModels.Concepto()
                    {
                            Cantidad = 100,
                            ClaveProductoServicio = "01010101",
                            ClaveUnidad = "M55",
                            Descripcion = "Una madre aqui sin impuestos",
                            Importe = 200,
                            Unidad = "Radianes",
                            ValorUnitario = 200,
                            ObjetoImp = "02",
                            ConceptosImpuestos = new System.Collections.Generic.List<BindingModels.ConceptoImpuestos>()
                            {
                                new BindingModels.ConceptoImpuestos()
                                {
                                    BaseImpuesto = 200,
                                    Importe = 100,
                                    Impuesto =  "ISR",
                                    RetencionOTraslado = "Retencion",
                                    TasaOCuota = "0",
                                    TipoFactor =  "Exento"
                                }
                            }
                    },
                },
                LugarExpedicion = "99100",
                FormaPago = "01",
                MetodoPago = "PUE",
                TipoComprobante = "I",
                CondicionesDePago = "Pago al contado",
                SubTotal = 80,
                Total = 100,
                Moneda = "MXN",
                noCertificado = "20001000000200000258",
            };
        }

        [Fact]
        public void CreateCFDITest()
        {
            var cfdiController = new CFDIv40(_MockRepository, _moqSatProvider);
            var xmlComprobante = cfdiController.CreateCFDI(_testComprobante, Timbrado: false);
            Assert.True(!String.IsNullOrEmpty(xmlComprobante));
            // El XML que se devuelve con Timbrado:false ya debe venir sellado (mismo
            // contrato documentado para CFDIv33.CreateCFDI): quien prueba sin PAC
            // necesita un CFDI valido y firmado, no uno a medio terminar.
            Assert.Contains("Sello=", xmlComprobante);
            Assert.DoesNotContain("Sello=\"\"", xmlComprobante);
        }

        [Fact]
        public void OriginalChain40Test()
        {
            string ExpectedOutput = "||4.0|12|12123|2017-06-29T12:00:00|03|20001000000300022815|12312|MXN|14281.92|I|01|PUE|99100|LAN7008173R5|mi empresa|622|AAA010101AAA|Arlie Cassarubias|99100|616|G03|01010101|1|1|ZZ|NA|cargo nuevo|12312|12312.000000|02|12312|002|Tasa|0.160000|1969.92||";
            var cfdiController = new CFDIv40(_MockRepository, _moqSatProvider);
            var xmlComprobante = "<?xml version=\"1.0\" encoding=\"utf-8\"?> " +
                                "<cfdi:Comprobante xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.sat.gob.mx/cfd/4 http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd\" Version=\"4.0\" Serie=\"12\" Folio=\"12123\" Fecha=\"2017-06-29T12:00:00\" Sello=\"\" FormaPago=\"03\" NoCertificado=\"20001000000300022815\" Certificado=\"MIIFxTCCA62gAwIBAgIUMjAwMDEwMDAwMDAzMDAwMjI4MTUwDQYJKoZIhvcNAQEL\" SubTotal=\"12312\" Moneda=\"MXN\" Total=\"14281.92\" TipoDeComprobante=\"I\" Exportacion=\"01\" MetodoPago=\"PUE\" LugarExpedicion=\"99100\" xmlns:cfdi=\"http://www.sat.gob.mx/cfd/4\">" +
                                "<cfdi:Emisor Rfc=\"LAN7008173R5\" Nombre=\"mi empresa\" RegimenFiscal=\"622\" />" +
                                "<cfdi:Receptor Rfc=\"AAA010101AAA\" Nombre=\"Arlie Cassarubias\" RegimenFiscalReceptor=\"616\" DomicilioFiscalReceptor=\"99100\" UsoCFDI=\"G03\" />" +
                                "<cfdi:Conceptos>" +
                                "<cfdi:Concepto ClaveProdServ=\"01010101\" NoIdentificacion=\"1\" Cantidad=\"1\" ClaveUnidad=\"ZZ\" Unidad=\"NA\" Descripcion=\"cargo nuevo\" ValorUnitario=\"12312\" Importe=\"12312.000000\" ObjetoImp=\"02\">" +
                                "<cfdi:Impuestos>" +
                                "<cfdi:Traslados>" +
                                "<cfdi:Traslado Base=\"12312\" Impuesto=\"002\" TipoFactor=\"Tasa\" TasaOCuota=\"0.160000\" Importe=\"1969.92\" />" +
                                "</cfdi:Traslados>" +
                                "</cfdi:Impuestos>" +
                                "</cfdi:Concepto>" +
                                "</cfdi:Conceptos>" +
                                "<cfdi:Complemento/>" +
                                "</cfdi:Comprobante>";
            var output = cfdiController.GetOriginalChain(xmlComprobante, CFDIBase.CadenaOriginal40XsltPath);
            Assert.Equal(ExpectedOutput, output);
        }

        [Fact]
        public void ValidatesAgainstCfdi40Schema()
        {
            var cfdiController = new CFDIv40(_MockRepository, _moqSatProvider);
            var xmlComprobante = cfdiController.CreateCFDI(_testComprobante, Timbrado: false);

            var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TRSF.Invoicing"));
            var officialXsdPath = Path.Combine(repoRoot, "Schemas40", "xsd", "cfdv40-local.xsd");

            var errors = new System.Collections.Generic.List<string>();

            var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
            var schemaSet = new XmlSchemaSet();
            // XmlSchemaSet.XmlResolver es null por default en .NET moderno (a diferencia
            // de .NET Framework): sin esto, los <xs:import> con schemaLocation relativo
            // (catCFDI-slim.xsd, tdCFDI.xsd) fallan en silencio y cada tipo de esos
            // namespaces aparece como "no declarado" al validar.
            schemaSet.XmlResolver = new XmlUrlResolver();
            schemaSet.ValidationEventHandler += (s, e) => errors.Add("[compile] " + e.Message);
            schemaSet.Add(null, officialXsdPath);
            schemaSet.Compile();
            settings.Schemas = schemaSet;

            settings.ValidationEventHandler += (s, e) => errors.Add(e.Message);

            using var reader = XmlReader.Create(new StringReader(xmlComprobante), settings);
            while (reader.Read()) { }

            Assert.True(errors.Count == 0, string.Join("\n", errors));
        }
    }
}
