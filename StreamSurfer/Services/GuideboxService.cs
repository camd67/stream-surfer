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
        public string ConvertToShowSearch(string query)
        {
            return BASE_URL + "search?api_key=" + API_KEY + "&type=show&query=" + query;
        }
    }
}
