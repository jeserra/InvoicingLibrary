﻿using System;
using System.Net.NetworkInformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InvoicingLibrary.CFDI;
using NSubstitute;
using InvoicingLibrary.Interfaces;
using InvoicingLibrary.Translates;

using InvoicingLibrary.Test.Certifcate;
using Newtonsoft.Json;
using ProcessCFDI.Controllers;
using System.IO;
using InvoicingLibrary.Test.Print;
using Microsoft.AspNetCore.Mvc;

namespace InvoicingLibrary.Test.CFDI
{
    [TestClass]
    public class CFDIv33Test
    {
        public BindingModels.Comprobante _testComprobante;
        public Interfaces.ICertificatesRepository _MockRepository;
        public Interfaces.IInvoicingRepository _MoInvoicingRepository;
        public Interfaces.ISATProvider _moqSatProvider;
        public MemoryStream stream = new MemoryStream();
        public void InitializeRepository()
        {
            _MockRepository = Substitute.For<ICertificatesRepository>();
            _MockRepository.GetCertificate("30001000000400002321").ReturnsForAnyArgs(new CertificateMoq());

            _MoInvoicingRepository = Substitute.For<IInvoicingRepository>();
            _moqSatProvider = new SatProviderMoq();
        }


   
        [TestInitialize]
        public void InitializeComprobante()
        {
            InitializeRepository();


            _testComprobante = new BindingModels.Comprobante()
            {
                Version = "3.3",
                Emisor = new BindingModels.Emisor()
                {
                    Nombre = "Juan perez",
                    RegimenFiscal = "611",
                    RFC = "FUNK671228PH6"
                },
                Receptor = new BindingModels.Receptor()
                {
                    Nombre = "Pepe perez",
                    RFC = "RETE901212DCA",
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
                ValesDespensa = new BindingModels.ValesDeDespensa()
                {
                    NumeroCuenta = "21321312",
                    RegistroPatronal = "12321321",
                    Total = 122000,
                    conceptos = new System.Collections.Generic.List<BindingModels.ConceptosValesDespensa>()
                        {
                             new BindingModels.ConceptosValesDespensa()
                             {
                                  curp = "Xxxxx",
                                   fecha = DateTime.Parse("2017-12-12"),
                                    identificador = "777",
                                     Importe = 100,
                                      nombre = "Juan camaney",
                                       numSeguridadSocial = "1231232131",
                                        rfc ="FUNK671228PH6"
                             },
                             new BindingModels.ConceptosValesDespensa()
                             {
                                  curp = "Xxxxx",
                                   fecha = DateTime.Parse("2017-12-12"),
                                    identificador = "777",
                                     Importe = 100,
                                      nombre = "Juan camaney",
                                       numSeguridadSocial = "1231232131",
                                        rfc ="SEDE810924CK8"
                             }
                        }
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
                UsoCFDI = "G01"
            };
            

        }

        [TestMethod]
        public void printPDFTest()
        {
            try
            {
                cfdi33.Comprobante testComprobanteDos = new cfdi33.Comprobante();
                testComprobanteDos = TranslateModelToCFDI.TranslateToCFDI(_testComprobante);
                testComprobanteDos.Sello = "sadaskdjasidjasid";
               
                PrintPDFService.PrintCFDIPDF(testComprobanteDos, ref stream);
                byte[] byteStream = stream.ToArray();
                var a = 10;
                using (FileStream fs = File.Create(@"..\\..\\..\\Resources\\output.pdf"))
                {
                    var b = 10;
                    fs.Write(byteStream, 0, (int)byteStream.Length);
                }
                              
            }
            catch(Exception e)
            {
                Assert.Fail();
            }

        }
        [TestMethod]
        public void CreateCFDITest()
        {
            try
            {
                var cfdiController = new CFDIv33( _MockRepository, _moqSatProvider);
                var xmlComprobante = cfdiController.CreateCFDI(_testComprobante);
                File.WriteAllText(String.Format(@"D:\\Facturacion\\InvoicingLibrary\\InvoicingLibrary.Test\\Resources\\{0}.xml", DateTime.Now.ToString("ddMMyyyy")), xmlComprobante );
                Assert.IsTrue(!String.IsNullOrEmpty(xmlComprobante));
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void SerializeCFDITest()
        {
            var CFDISerialize =
                    JsonConvert.SerializeObject(_testComprobante);

            Console.WriteLine(CFDISerialize);
            Assert.IsTrue(true);

        }


        [TestMethod]
        public void OriginalChain33Test()
        {
            
            string ExpectedOutput = "||3.3|12|12123|2017-06-29T12:00:00|03|20001000000300022815|12312|MXN|14281.92|I|PUE|99100|FUNK671228PH6|mi empresa|622|AAA010101AAA|Arlie Cassarubias|G03|01010101|1|1|ZZ|NA|cargo nuevo|12312|12312.000000|12312|002|Tasa||";
            var cfdiController = new CFDIv33( _MockRepository, _moqSatProvider);
            var xmlComprobante = "<?xml version=\"1.0\" encoding=\"utf-8\"?> " +
                                "<cfdi:Comprobante xmlns:xsi =\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.sat.gob.mx/cfd/3 http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv33.xsd\" Version=\"3.3\" Serie=\"12\" Folio=\"12123\" Fecha=\"2017-06-29T12:00:00\" Sello=\"LOdIkXGAvPZ8rBfdvr+BgEfcKfeIwM1f1Rn2MajYRNXRzFApO/sFwM3RZZIp374ToFK036TFGmqhaCyRw6Ty6sy7v2+jrGAQ5nXrtspAFe91ur5nP+T12Vikh7evMx36tS8/UM2hDHPkayTSwJWduu/yD1/1DmP1kLFbDZYZAHgrgUoI8Wsq3Qm6bDBWBnOHxpvUj2fDUI0+caB/vlSCy3KPlzUHDE0V5hMxNE43OV+k6CUQsPxxgY2hbCB9yeyh9WZ76gxnRSyKa31ighbFSDXI+tEIByfrtwy7efMCbtiUf2SC5ri3YpJXgKQOcHt6pM/27ur9GNlP4D6GSCjNjw==\" FormaPago=\"03\" NoCertificado=\"20001000000300022815\" Certificado=\"MIIFxTCCA62gAwIBAgIUMjAwMDEwMDAwMDAzMDAwMjI4MTUwDQYJKoZIhvcNAQELBQAwggFmMSAwHgYDVQQDDBdBLkMuIDIgZGUgcHJ1ZWJhcyg0MDk2KTEvMC0GA1UECgwmU2VydmljaW8gZGUgQWRtaW5pc3RyYWNpw7NuIFRyaWJ1dGFyaWExODA2BgNVBAsML0FkbWluaXN0cmFjacOzbiBkZSBTZWd1cmlkYWQgZGUgbGEgSW5mb3JtYWNpw7NuMSkwJwYJKoZIhvcNAQkBFhphc2lzbmV0QHBydWViYXMuc2F0LmdvYi5teDEmMCQGA1UECQwdQXYuIEhpZGFsZ28gNzcsIENvbC4gR3VlcnJlcm8xDjAMBgNVBBEMBTA2MzAwMQswCQYDVQQGEwJNWDEZMBcGA1UECAwQRGlzdHJpdG8gRmVkZXJhbDESMBAGA1UEBwwJQ295b2Fjw6FuMRUwEwYDVQQtEwxTQVQ5NzA3MDFOTjMxITAfBgkqhkiG9w0BCQIMElJlc3BvbnNhYmxlOiBBQ0RNQTAeFw0xNjEwMjUyMTUyMTFaFw0yMDEwMjUyMTUyMTFaMIGxMRowGAYDVQQDExFDSU5ERU1FWCBTQSBERSBDVjEaMBgGA1UEKRMRQ0lOREVNRVggU0EgREUgQ1YxGjAYBgNVBAoTEUNJTkRFTUVYIFNBIERFIENWMSUwIwYDVQQtExxMQU43MDA4MTczUjUgLyBGVUFCNzcwMTE3QlhBMR4wHAYDVQQFExUgLyBGVUFCNzcwMTE3TURGUk5OMDkxFDASBgNVBAsUC1BydWViYV9DRkRJMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAgvvCiCFDFVaYX7xdVRhp/38ULWto/LKDSZy1yrXKpaqFXqERJWF78YHKf3N5GBoXgzwFPuDX+5kvY5wtYNxx/Owu2shNZqFFh6EKsysQMeP5rz6kE1gFYenaPEUP9zj+h0bL3xR5aqoTsqGF24mKBLoiaK44pXBzGzgsxZishVJVM6XbzNJVonEUNbI25DhgWAd86f2aU3BmOH2K1RZx41dtTT56UsszJls4tPFODr/caWuZEuUvLp1M3nj7Dyu88mhD2f+1fA/g7kzcU/1tcpFXF/rIy93APvkU72jwvkrnprzs+SnG81+/F16ahuGsb2EZ88dKHwqxEkwzhMyTbQIDAQABox0wGzAMBgNVHRMBAf8EAjAAMAsGA1UdDwQEAwIGwDANBgkqhkiG9w0BAQsFAAOCAgEAJ/xkL8I+fpilZP+9aO8n93+20XxVomLJjeSL+Ng2ErL2GgatpLuN5JknFBkZAhxVIgMaTS23zzk1RLtRaYvH83lBH5E+M+kEjFGp14Fne1iV2Pm3vL4jeLmzHgY1Kf5HmeVrrp4PU7WQg16VpyHaJ/eonPNiEBUjcyQ1iFfkzJmnSJvDGtfQK2TiEolDJApYv0OWdm4is9Bsfi9j6lI9/T6MNZ+/LM2L/t72Vau4r7m94JDEzaO3A0wHAtQ97fjBfBiO5M8AEISAV7eZidIl3iaJJHkQbBYiiW2gikreUZKPUX0HmlnIqqQcBJhWKRu6Nqk6aZBTETLLpGrvF9OArV1JSsbdw/ZH+P88RAt5em5/gjwwtFlNHyiKG5w+UFpaZOK3gZP0su0sa6dlPeQ9EL4JlFkGqQCgSQ+NOsXqaOavgoP5VLykLwuGnwIUnuhBTVeDbzpgrg9LuF5dYp/zs+Y9ScJqe5VMAagLSYTShNtN8luV7LvxF9pgWwZdcM7lUwqJmUddCiZqdngg3vzTactMToG16gZA4CWnMgbU4E+r541+FNMpgAZNvs2CiW/eApfaaQojsZEAHDsDv4L5n3M1CC7fYjE/d61aSng1LaO6T1mh+dEfPvLzp7zyzz+UgWMhi5Cs4pcXx1eic5r7uxPoBwcCTt3YI1jKVVnV7/w=\" SubTotal=\"12312\" Moneda=\"MXN\" Total=\"14281.92\" TipoDeComprobante=\"I\" MetodoPago=\"PUE\" LugarExpedicion=\"99100\" xmlns:cfdi=\"http://www.sat.gob.mx/cfd/3\">" +
                                "<cfdi:Emisor Rfc =\"FUNK671228PH6\" Nombre=\"mi empresa\" RegimenFiscal=\"622\" />" +
                                "<cfdi:Receptor Rfc =\"AAA010101AAA\" Nombre=\"Arlie Cassarubias\" UsoCFDI=\"G03\" />" +
                                "<cfdi:Conceptos>" +
                                "<cfdi:Concepto ClaveProdServ =\"01010101\" NoIdentificacion=\"1\" Cantidad=\"1\" ClaveUnidad=\"ZZ\" Unidad=\"NA\" Descripcion=\"cargo nuevo\" ValorUnitario=\"12312\" Importe=\"12312.000000\">" +
                                "<cfdi:Impuestos>" +
                                "<cfdi:Traslados>" +
                                "<cfdi:Traslado Base =\"12312\" Impuesto=\"002\" TipoFactor=\"Tasa\" />" +
                                "</cfdi:Traslados>" +
                                "</cfdi:Impuestos>" +
                                "</cfdi:Concepto>" +
                                "</cfdi:Conceptos>" +
                                "<cfdi:Complemento/>" +
                                "</cfdi:Comprobante>";
            var output = cfdiController.GetOriginalChain(xmlComprobante);
            Assert.AreEqual(output, ExpectedOutput);
        }

        [TestMethod]
        public void GetSealTest()
        {

            var cfdiController = new CFDIv33( _MockRepository, _moqSatProvider);
            ICertificate certificate = _MockRepository.GetCertificate("30001000000400002321");
            string OriginalChain = "||3.3|12|12123|2017-06-29T12:00:00|03|20001000000300022815|12312|MXN|14281.92|I|PUE|99100|LAN7008173R5|mi empresa|622|AAA010101AAA|Arlie Cassarubias|G03|01010101|1|1|ZZ|NA|cargo nuevo|12312|12312.000000||";

            byte[] SHA256Hash = cfdiController.GetSHA256(OriginalChain);

            string PassKey = certificate.Pwd;
            System.Security.SecureString secPassPhrase = new System.Security.SecureString();
            foreach (char passChar in PassKey.ToCharArray())
                secPassPhrase.AppendChar(passChar);

            System.Security.Cryptography.RSACryptoServiceProvider privateKey = cfdiController.LoadPrivateKeyFromString(secPassPhrase, certificate.KeyFile);

            var expected = "Z+M785qEHd2S8FYfLxrBY9pU0QydYmAX+/8HCXm0VF34z+14+ItblvjjeuJ3PwXI2TlUZldJrLo3FnHtpCavtLORfLCGioxr01gBSjMHhKGJoUa+gdQTrgGIpE+YVnwvk/3xKe/UBsdqb3hInRl6K0aT5IygoWZKqm64kJAJerNCebadHZnkxlM4zoyCEKu4QY8vtp9gZAsAtREz0e490e5xSSpcK05ZPqVnTj8n51Y5cJVpMdGUJSHg1/rDv37R/che1wZR1aj+MHH2GJvXYmhWPxnBPVXvnTWxvQFJjaiqqrHPKvKsjBvZ2cSxXOZQ2KcHm9rm+rRPQAOrZslgag==";

            var sello = cfdiController .GetSeal(SHA256Hash, privateKey);
            Assert.AreEqual(sello, expected);

        }
    }
}
