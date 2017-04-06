using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace StreamSurfer.Services
{
    public class GuideboxService : IShowService
    {
        private const string BASE_URL = "http://api-public.guidebox.com/v2/";
        private readonly string API_KEY;

        public GuideboxService(IOptions<Settings> settings)
        {
            API_KEY = settings.Value.GuideboxKey;

        }
        private string BuildQuery(string endpoint, string queryParams)
        {
            return BASE_URL + endpoint + "?api_key=" + API_KEY + "&" + queryParams;
        }

        public string ConvertToShowSearch(string query)
        {
            return BuildQuery("search", "type=show&query=" + query);
        }
    }
}
