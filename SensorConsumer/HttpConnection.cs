using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SensorConsumer
{
    class HttpConnection
    {
        private readonly HttpClient _httpClient;
        public HttpConnection()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler);
            var byteArray = Encoding.ASCII.GetBytes($"{Consts.ELASTIC_USERNAME}:{Consts.ELASTIC_PASSWORD}");
            _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(Consts.ELASTIC_POST_AUTHENTICANTION, Convert.ToBase64String(byteArray));
        }
        public async Task SendRequestAsync(string json)
        {
            var content = new StringContent(json, Encoding.UTF8, Consts.ELASTIC_POST_TYPE);
            await _httpClient.PostAsync(Consts.ELASTIC_URL, content);
        }
    }
}
