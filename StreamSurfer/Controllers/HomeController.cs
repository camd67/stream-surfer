using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StreamSurfer.Models;
using StreamSurfer.Models.ShowViewModels;
using StreamSurfer.Services;

namespace StreamSurfer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebRequestHandler webRequest;
        private readonly IShowService showService;
        private readonly ILogger logger;
        private readonly RotatingCache<List<SearchViewModel>> searchCache;
        private readonly UserManager<AppUser> _userManager;
        private readonly PostgresDataContext _context;

        public HomeController(IWebRequestHandler webRequest,
            IShowService showService,
            ILogger<HomeController> logger,
            RotatingCache<List<SearchViewModel>> searchCache,
            UserManager<AppUser> userManager,
            PostgresDataContext context)
        {
            this.logger = logger;
            this.webRequest = webRequest;
            this.showService = showService;
            this.searchCache = searchCache;
            this._userManager = userManager;
            this._context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(string query)
        {
            // ugly string sanitize... at least the query is usually short
            query = query.Replace("<", "").Replace(">", "");
            ViewData["search_query"] = query.ToUpper();
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
                List<SearchViewModel> showResults = new List<SearchViewModel>();
                var user = await GetCurrentUserAsync();
                MyList myList = null;
                if(user != null)
                {
                    myList = _context.MyList
                        .Include(x => x.MyListShows)
                        .FirstOrDefault(x => x.User.Id == user.Id);
                }
                foreach (JToken r in results)
                {
                    Show showResult = r.ToObject<Show>();
                    try
                    {
                        showResult.Started = DateTime.ParseExact(r["first_aired"].ToString(), "yyyy-m-d", null).ToString("y");
                    }
                    catch (FormatException)
                    {
                        showResult.Started = "Unknown Start Date";
                    }
                    showResult.Artwork = r["artwork_304x171"].ToString();
                    bool inList = false;
                    if(myList != null)
                    {
                        var listCheck = myList.MyListShows?.FirstOrDefault(x => x.ShowId == showResult.ID);
                        inList = listCheck == null ? false : true;
                    }
                    showResults.Add(new SearchViewModel()
                    {
                        IsInList = inList,
                        Show = showResult
                    });
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

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Tos()
        {
            return View();
        }
        
        public IActionResult Error()
        {
            return View();
        }

        private Task<AppUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}