using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace StreamSurfer.Services
{
    public interface IWebRequestHandler
    {
        Task<HttpResponseMessage> Get(string url);
        Task<HttpResponseMessage> Post(string url, HttpContent content);
    }
}
