using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace StreamSurfer.Services
{
    public class HttpClientRequestHandler : IWebRequestHandler
    {
        private static HttpClient Client;
        public HttpClientRequestHandler()
        {
            Client = new HttpClient();
        }

        public async Task<HttpResponseMessage> Get(string url)
        {
            return await Client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> Post(string url, HttpContent content)
        {
            return await Client.PostAsync(url, content);
        }
    }
}
