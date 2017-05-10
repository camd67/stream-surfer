using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StreamSurfer.Models;
using StreamSurfer.Services;

namespace StreamSurfer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebRequestHandler webRequest;
        private readonly IShowService showService;
        private readonly ILogger logger;
        private RotatingCache<List<Show>> searchCache;

        public HomeController(IWebRequestHandler webRequest,
            IShowService showService,
            ILogger<HomeController> logger,
            RotatingCache<List<Show>> searchCache)
        {
            this.logger = logger;
            this.webRequest = webRequest;
            this.showService = showService;
            this.searchCache = searchCache;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(string query)
        {
            var cacheResult = searchCache.Get(query);
            if (cacheResult == null)
            {
                logger.LogDebug("Search cache miss, adding: " + query);
                // build the API request string, and get it
                var response = await webRequest.Get(showService.ConvertToShowSearch(query));
                if (!response.IsSuccessStatusCode)
                {
                    return Error();
                }
                // actually download the content
                var content = await response.Content.ReadAsStringAsync();
                // convert httpResponse into JSON
                var json = JObject.Parse(content);
                // get all the results as a list
                IList<JToken> results = json["results"].Children().ToList();
                List<Show> showResults = new List<Show>();
                foreach (JToken r in results)
                {
                    Show showResult = r.ToObject<Show>();
                    showResult.Desc = "First aired: " + r["first_aired"];
                    showResult.Picture = r["artwork_304x171"].ToString();
                    showResults.Add(showResult);
                }
                searchCache.Add(query, showResults);
                return View(showResults);
            }
            else
            {
                return View(cacheResult);
            }
        }

        public IActionResult AdvancedSearch()
        {
            return View();
        }
        
        public IActionResult Error()
        {
            return View();
        }
    }
}