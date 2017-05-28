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
        private readonly RotatingCache<List<Show>> searchCache;
        private readonly UserManager<AppUser> _userManager;
        private readonly PostgresDataContext _context;

        public HomeController(IWebRequestHandler webRequest,
            IShowService showService,
            ILogger<HomeController> logger,
            RotatingCache<List<Show>> searchCache,
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

        public async Task<IActionResult> Search(string query, string offline)
        {
            // ugly string sanitize... at least the query is usually short
            query = query.Replace("<", "").Replace(">", "");
            ViewData["search_query"] = query.ToUpper();
            var cacheResult = searchCache.Get(query);
            List<SearchViewModel> vm = new List<SearchViewModel>();
            MyList myList = null;
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                myList = _context.MyList
                    .Include(x => x.MyListShows)
                    .FirstOrDefault(x => x.User.Id == user.Id);
            }

            if (cacheResult == null)
            {
                logger.LogDebug("Search cache miss, adding: " + query);
                
                if (offline == "t")
                {
                    var shows = _context.Shows
                        .Where(x => x.Title.ToLower().Contains(query.ToLower()))
                        .ToList();
                    foreach (Show s in shows)
                    {

                        bool inList = false;
                        if (myList != null)
                        {
                            var listCheck = myList.MyListShows?
                                .FirstOrDefault(x => x.ShowId == s.ID);
                            inList = listCheck == null ? false : true;
                        }
                        vm.Add(new SearchViewModel()
                        {
                            Show = s,
                            IsInList = inList
                        });
                    }
                }
                else
                {
                    // build the API request string, and get it
                    var response = await webRequest.Get(showService.ConvertToShowSearch(query));
                    if (!response.IsSuccessStatusCode)
                    {
                        return Error(500);
                    }
                    // actually download the content
                    var content = await response.Content.ReadAsStringAsync();
                    // convert httpResponse into JSON
                    var json = JObject.Parse(content);
                    // get all the results as a list
                    IList<JToken> results = json["results"].Children().ToList();
                    List<Show> showList = new List<Show>();
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
                        if (myList != null)
                        {
                            var listCheck = myList.MyListShows?.FirstOrDefault(x => x.ShowId == showResult.ID);
                            inList = listCheck == null ? false : true;
                        }
                        showList.Add(showResult);
                        vm.Add(new SearchViewModel()
                        {
                            IsInList = inList,
                            Show = showResult
                        });
                    }
                    // ONLY add to cache if we had to download it
                    // otherwise we may add a shortened version of the list
                    searchCache.Add(query, showList);
                }
            }
            else
            {
                foreach(var s in cacheResult)
                {
                    bool inList = false;
                    if (myList != null)
                    {
                        var listCheck = myList.MyListShows?
                            .FirstOrDefault(x => x.ShowId == s.ID);
                        inList = listCheck == null ? false : true;
                    }
                    vm.Add(new SearchViewModel()
                    {
                        Show = s,
                        IsInList = inList
                    });
                }
            }
            return View(vm);
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
        
        public IActionResult Error(int id)
        {
            // This isn't how you're supposed to do it, but it works...
            if(HttpContext.Response.StatusCode == 404 || id == 404)
            {
                return View("Error404");
            }
            if ((HttpContext.Response.StatusCode >= 500 && HttpContext.Response.StatusCode < 600)
                || id == 500)
            {
                return View("Error500");
            }
            return View();
        }

        private Task<AppUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}