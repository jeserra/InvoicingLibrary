using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Invoicing.Interfaces;
using ProcessCFDI.Utils;

namespace Invoicing.QRProviders
{
    public class EcodexQRProvider : IQRProvider
    {
        static readonly HttpClient client = new HttpClient();
        readonly string baseUrl;
        readonly string integratorId;
        static string TokenPath = "/token?version=2";
        static string QRPath = "/api/documentos/qr/";

        public EcodexQRProvider(string baseUrl, string integratorId)
        {
            this.baseUrl = baseUrl;
            this.integratorId = integratorId;
        }

        private string ObtenerHash(string serviceToken)
        {
            var toHash = String.Format("{0}|{1}", integratorId, serviceToken);
            return Security.Hash(toHash);
        }

        public async Task<tokenmodel> getTokenAsync<TResult>(string rfc)
        {
            try
            {
                var path = baseUrl + TokenPath;
                tokenmodel result = new tokenmodel();

                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("rfc", rfc));
                nvc.Add(new KeyValuePair<string, string>("grant_type", "authorization_token"));

                HttpResponseMessage response = await client.PostAsync(path, new FormUrlEncodedContent(nvc), CancellationToken.None);
                if (response.IsSuccessStatusCode)
                {
                    String token = await response.Content.ReadAsStringAsync();
                    result = JsonSerializer.Deserialize<tokenmodel>(token);
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<byte[]> GenerateQR(string rfc, string UUID)
        {
            try
            {
                var path = baseUrl + QRPath + UUID;

                var token = await getTokenAsync<tokenmodel>(rfc);
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
                requestMessage.Headers.Add("X-Auth-Token", ObtenerHash(token.service_token));
                HttpResponseMessage response = await client.SendAsync(requestMessage, CancellationToken.None);
                if (response.IsSuccessStatusCode)
                {
                    var imageQR = await response.Content.ReadAsByteArrayAsync();
                    return imageQR;
                }
                else
                {
                    var messageError = await response.Content.ReadAsStringAsync();
                    var error = JsonSerializer.Deserialize<errorQr>(messageError);
                    throw new SystemException("Error al obtener el QR " + error.error_description);
                }
            }
            catch (Exception ex)
            {
                Console.Write("Error al impriimir {0}", ex.Message);
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
