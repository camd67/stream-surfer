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
            // only add query params if they exist
            return BASE_URL + endpoint + "?api_key=" + API_KEY + (queryParams != null ? "&" + queryParams : "");
        }

        public string ConvertToShowDetail(int id)
        {
            return BuildQuery("shows/" + id, null);
        }

        public string ConvertToMovieDetail(int id)
        {
            return BuildQuery("movies/" + id, null);
        }

        public string ConvertToShowSearch(string query)
        {
            return BuildQuery("search", "type=show&query=" + query);
        }

        public string ConvertToMovieSearch(string query)
        {
            return BuildQuery("search", "type=movie&query=" + query);
        }

        public string ConvertToServices(int id)
        {
            return BuildQuery("shows/" + id + "/available_content", null);
        }

        public string GetShows(int limit)
        {
            return BuildQuery("shows", "limit=" + limit);
        }

        public string GetEpisodes(int id, int limit, int offset)
        {
            return BuildQuery("shows/" + id + "/episodes", "include_links=true&reverse_ordering=true&platform=web&limit=" + limit + "&offset=" + offset);
        }

        public string GetSources()
        {
            return BuildQuery("sources", null);
        }
    }
}
