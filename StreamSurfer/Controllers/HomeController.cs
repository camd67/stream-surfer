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

        public HomeController(IWebRequestHandler webRequest, IShowService showService, ILogger<HomeController> logger)
        {
            this.logger = logger;
            this.webRequest = webRequest;
            this.showService = showService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(string query)
        {
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
            IList<Show> showResults = new List<Show>();
            foreach (JToken r in results)
            {
                Show showResult = r.ToObject<Show>();
                showResults.Add(showResult);
            }
            return View(showResults);
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
