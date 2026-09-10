using System;
using TRSF.Invoicing.srvSeguridad;
using TRSF.Invoicing.srvTimbrado;
using TRSF.Invoicing.Utils;
using System.ServiceModel;
using TRSF.Invoicing.Interfaces;

namespace TRSF.Invoicing.CFDIProviders
{
    public class EcodexProvider : ISATProvider
    {
        private readonly string integratorId;
        private readonly Uri timbradoEndpoint;
        private readonly Uri seguridadEndpoint;

        public EcodexProvider(string integratorId, Uri timbradoEndpoint, Uri seguridadEndpoint)
        {
            this.integratorId = integratorId;
            this.timbradoEndpoint = timbradoEndpoint;
            this.seguridadEndpoint = seguridadEndpoint;
        }

        public string INTEGRATOR_ID => integratorId;

        private static BasicHttpBinding CreateSecureBinding() =>
            new BasicHttpBinding(BasicHttpSecurityMode.Transport);

        private TimbradoClient CreateTimbradoClient() =>
            new TimbradoClient(CreateSecureBinding(), new EndpointAddress(timbradoEndpoint));

        private SeguridadClient CreateSeguridadClient() =>
            new SeguridadClient(CreateSecureBinding(), new EndpointAddress(seguridadEndpoint));

        public byte[] ObtenerQR(string RFC, String UUID, long transactionId)
        {
            try
            {
                TimbradoClient client = CreateTimbradoClient();

                var response = client.ObtenerQRTimbrado(RFC, ObtenerToken(RFC, transactionId), ref transactionId, UUID);
                return response.Imagen;
            }
            catch (FaultException<srvTimbrado.FallaServicio> serviceFault)
            {
                throw new Exception(String.Format("Error al timbrar  {0} {1}", serviceFault.Message, serviceFault.Detail.Numero));
            }
            catch (FaultException<srvTimbrado.FallaSesion> serviceSesion)
            {
                throw new Exception(String.Format("Error al timbrar {0}", serviceSesion.Message));
            }
            catch (FaultException<srvTimbrado.FallaValidacion> faultvalidation)
            {
                throw new Exception(String.Format("Error al timbrar {0}", faultvalidation.Message));
            }
        }

        public String Timbrar(String RFC, String Comprobante, long transactionId)
        {
            try
            {
                TimbradoClient client = CreateTimbradoClient();

                ComprobanteXML commprobante = new ComprobanteXML();
                commprobante.DatosXML = Comprobante;
                var timbrado = client.TimbraXML(ref commprobante, RFC, ObtenerToken(RFC, transactionId), ref transactionId);
                return commprobante.DatosXML;
            }
            catch (FaultException<srvTimbrado.FallaServicio> serviceFault)
            {
                throw new Exception(String.Format("Error al timbrar  {0} {1}", serviceFault.Message, serviceFault.Detail.Numero));
            }
            catch (FaultException<srvTimbrado.FallaSesion> serviceSesion)
            {
                throw new Exception(String.Format("Error al timbrar {0}", serviceSesion.Message));
            }
            catch (FaultException<srvTimbrado.FallaValidacion> faultvalidation)
            {
                throw new Exception(String.Format("Error al timbrar {0}", faultvalidation.Message));
            }
        }

        public string ObtenerToken(string RFC, long transactionID)
        {
            SeguridadClient srv = CreateSeguridadClient();
            var serviceToken = srv.ObtenerToken(RFC, ref transactionID);

            return ObtenerHash(serviceToken);
        }

        public string ObtenerHash(string serviceToken)
        {
            var toHash = String.Format("{0}|{1}", INTEGRATOR_ID, serviceToken);
            var token = Security.Hash(toHash);
            return token;
        }
    }
}
