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

        public async Task<IActionResult> Search(string query, string offline)
        {
            if (query == null || query.Trim() == "")
            {
                return View(new List<SearchViewModel>());
            }
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
                    var showSearch = SearchShows(query, myList);
                    var movieSearch = SearchMovies(query, myList);
                    await Task.WhenAll(showSearch, movieSearch);

                    // Weave results together, since there's no other good way to get a "results rating"
                    var shows = showSearch.Result;
                    var movies = movieSearch.Result;
                    int smallerSize = Math.Min(shows.Count, movies.Count);
                    for(int i = 0; i < smallerSize; i++)
                    {
                        vm.Add(shows[i]);
                        vm.Add(movies[i]);
                    }
                    if(shows.Count > smallerSize)
                    {
                        vm.AddRange(shows.Skip(smallerSize));
                    }
                    else if(movies.Count > smallerSize)
                    {
                        vm.AddRange(movies.Skip(smallerSize));
                    }
                    // ONLY add to cache if we had to download it
                    // otherwise we may add a shortened version of the list
                    searchCache.Add(query, vm);
                }
            }
            else
            {
                foreach(var s in cacheResult)
                {
                    bool inList = false;
                    if (myList != null)
                    {
                        var listCheck = myList.MyListShows?.FirstOrDefault(x => x.SafeCompareId(s.SafeGetId()));
                        inList = listCheck == null ? false : true;
                    }
                    vm.Add(new SearchViewModel()
                    {
                        Show = s.Show,
                        Movie = s.Movie,
                        IsInList = inList
                    });
                }
            }
            return View(vm);
        }

        private async Task<List<SearchViewModel>> SearchMovies(string query, MyList myList)
        {
            var response = await webRequest.Get(showService.ConvertToMovieSearch(query));
            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException("Query request failed");
            }
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            IList<JToken> results = json["results"].Children().ToList();
            List<SearchViewModel> movieList = new List<SearchViewModel>();
            foreach (JToken r in results)
            {
                Movie movieResult = r.ToObject<Movie>();
                try
                {
                    movieResult.Aired = DateTime.ParseExact(r["release_date"].ToString(), "yyyy-M-d", null).ToString("y");
                }
                catch (FormatException)
                {
                    movieResult.Aired = "Unknown Start Date";
                }
                movieResult.Artwork = r["poster_240x342"].ToString();
                bool inList = false;
                if (myList != null)
                {
                    var listCheck = myList.MyListShows?.FirstOrDefault(x => x.SafeCompareId(movieResult.ID));
                    inList = listCheck == null ? false : true;
                }
                movieList.Add(new SearchViewModel()
                {
                    IsInList = inList,
                    Movie = movieResult
                });
            }
            return movieList;
        }

        private async Task<List<SearchViewModel>> SearchShows(string query, MyList myList)
        {
            // build the API request string, and get it
            var response = await webRequest.Get(showService.ConvertToShowSearch(query));
            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException("Query request failed");
            }
            // actually download the content
            var content = await response.Content.ReadAsStringAsync();
            // convert httpResponse into JSON
            var json = JObject.Parse(content);
            // get all the results as a list
            IList<JToken> results = json["results"].Children().ToList();
            List<SearchViewModel> showList = new List<SearchViewModel>();
            foreach (JToken r in results)
            {
                Show showResult = r.ToObject<Show>();
                try
                {
                    showResult.Started = DateTime.ParseExact(r["first_aired"].ToString(), "yyyy-M-d", null).ToString("y");
                }
                catch (FormatException)
                {
                    showResult.Started = "Unknown Start Date";
                }
                showResult.Artwork = r["artwork_304x171"].ToString();
                bool inList = false;
                if (myList != null)
                {
                    var listCheck = myList.MyListShows?
                        .FirstOrDefault(x => x.ShowId == showResult.ID);
                    inList = listCheck == null ? false : true;
                }
                showList.Add(new SearchViewModel()
                {
                    IsInList = inList,
                    Show = showResult
                });
            }
            return showList;
        }

        public IActionResult AdvancedSearch()
        {
            return View();
        }
        public IActionResult About()
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