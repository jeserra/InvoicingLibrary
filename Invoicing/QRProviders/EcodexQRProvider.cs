using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Invoicing.Interfaces;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading;
using Invoicing.CFDIProviders;
using System.ServiceModel;
using Invoicing.srvSeguridad;
using ProcessCFDI.Utils;
using System.Configuration;

namespace Invoicing.QRProviders
{
    public class EcodexQRProvider:IQRProvider
    {
        static readonly HttpClient client = new HttpClient();
        readonly string baseUrl;
        static string TokenPath = "/token?version=2";
        static string QRPath = "/api/documentos/qr/";

        public EcodexQRProvider()
        {
            baseUrl = System.Configuration.ConfigurationManager.AppSettings["APIEcodexUrl"].ToString();
        }
        public  async Task<tokenmodel> getTokenAsync<TResult>(string rfc)
        {
            try
            {
                var path = baseUrl + TokenPath;
                tokenmodel result = new tokenmodel();
                tokenRequestModel request = new tokenRequestModel();
                request.rfc = rfc;
                request.grant_type = "grant_type";

                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("rfc", rfc));
                nvc.Add(new KeyValuePair<string, string>("grant_type", "authorization_token"));


                HttpResponseMessage response = await client.PostAsync(path, new FormUrlEncodedContent(nvc), CancellationToken.None);
                if (response.IsSuccessStatusCode)
                {
                    String token = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<tokenmodel>(token);
                }
                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;

            }
        }

        public async Task <byte[]> GenerateQR(string rfc, string UUID)
        {
            try
            {
                var ecodexprovider = new EcodexProvider();

                var path = baseUrl + QRPath + UUID;

                var token = await getTokenAsync<tokenmodel>(rfc);
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
                requestMessage.Headers.Add("X-Auth-Token", ecodexprovider.ObtenerHash(token.service_token));
                HttpResponseMessage response = await client.SendAsync(requestMessage, CancellationToken.None);
                if (response.IsSuccessStatusCode)
                {
                    var imageQR = await response.Content.ReadAsByteArrayAsync();
                    return imageQR;
                    //Console.WriteLine(imageQR);
                }
                else
                {
                    var messageError = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<errorQr>(messageError);
                    throw new SystemException("Error al obtener el QR " + error.error_description);
                    //Console.WriteLine(error);
                }
            }
            catch(Exception ex)
            {
                System.Console.Write("Error al impriimir {0}", ex.Message);
                throw;
            }

        }


         

        public class tokenmodel
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string expires_in { get; set; }
            public string service_datetime { get; set; }
            public string service_token { get; set; }
        
        }

        public class tokenRequestModel
        {
            public string grant_type { get; set; }
            public string rfc { get; set; }
        }

        public class errorQr
        {
            public string error { get; set; }
            public string error_code { get; set; }
            public string error_description { get; set; }
            public string error_suggestion { get; set; }
        }
        
    }
}
