using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StreamSurfer.Models;
using StreamSurfer.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using StreamSurfer.Models.MovieViewModels;
using Microsoft.AspNetCore.Identity;

namespace StreamSurfer.Controllers
{
    public class MovieController : Controller
    {
        private readonly PostgresDataContext _context;
        private readonly IWebRequestHandler webRequest;
        private readonly IShowService showService;
        private readonly ILogger logger;
        private readonly UserManager<AppUser> _userManager;

        public MovieController(PostgresDataContext context, IWebRequestHandler webRequest, IShowService showService, ILogger<HomeController> logger, UserManager<AppUser> _userManager)
        {
            this._userManager = _userManager;
            this._context = context;
            this.logger = logger;
            this.webRequest = webRequest;
            this.showService = showService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var movie = await _context.Movies
                .Include(m => m.MovieService)
                .Include(m => m.MovieGenre)
                .SingleOrDefaultAsync(m => m.ID == id);
            var loadGenres = _context.Genres
                .Include(m => m.MovieGenre)
                .ToDictionary(x => x.ID, x => x);
            var loadServices = _context.Services
                .Include(m => m.MovieService)
                .ToDictionary(x => x.Source, x => x);
            //switch cast to string separated by ;
            if (movie == null)
            {
                //get show details
                var response = await webRequest.Get(showService.ConvertToMovieDetail(id.Value));
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);
                // may want to come up with a new way to get these values out.
                // instead of pulling values out manually
                List<Synonym> synonyms = json["alternate_titles"]
                    .Children()
                    .Select(x => new Synonym() { ShowID = id.Value, Title = x.ToString() })
                    .ToList();
                List<Genre> genres = json["genres"]
                    .Children()
                    .Select(x => new Genre()
                    {
                        ID = (int)JObject.Parse(x.ToString())["id"],
                        Title = (string)JObject.Parse(x.ToString())["title"]
                    })
                    .ToList();
                List<MovieGenre> movieGenres = new List<MovieGenre>();
                foreach (var genre in genres)
                {
                    loadGenres.TryGetValue(genre.ID, out Genre getGenre);
                    if (getGenre == null)
                    {
                        getGenre = genre;
                    }
                    movieGenres.Add(new MovieGenre(id.Value, getGenre.ID, null, getGenre));
                }
                    
                List<string> cast = json["cast"]
                    .Children()
                    .Select(x => (string)JObject.Parse(x.ToString())["name"])
                    .ToList();
                string castString = "No cast available.";
                if (cast.Count > 0)
                {
                    castString = cast[0];
                    cast.Remove(castString);
                    foreach (var str in cast)
                    {
                        castString = castString + ";" + str;
                    }
                }

                //get service details
                List<Service> services = new List<Service>();
                Dictionary<string, string> links = new Dictionary<string, string>();
                foreach (var x in json["free_web_sources"].Children())
                {
                    var source = (string)JObject.Parse(x.ToString())["source"];
                    services.Add(new Service()
                    {
                        Name = (string)JObject.Parse(x.ToString())["display_name"],
                        Source = source
                    });
                    links.Add(source, (string)JObject.Parse(x.ToString())["link"]);
                }
                foreach (var x in json["tv_everywhere_web_sources"].Children())
                {
                    var source = (string)JObject.Parse(x.ToString())["source"];
                    services.Add(new Service()
                    {
                        Name = (string)JObject.Parse(x.ToString())["display_name"],
                        Source = source
                    });
                    links.Add(source, (string)JObject.Parse(x.ToString())["link"]);
                }
                foreach (var x in json["subscription_web_sources"].Children())
                {
                    var source = (string)JObject.Parse(x.ToString())["source"];
                    services.Add(new Service()
                    {
                        Name = (string)JObject.Parse(x.ToString())["display_name"],
                        Source = source
                    });
                    links.Add(source, (string)JObject.Parse(x.ToString())["link"]);
                }
                foreach (var x in json["purchase_web_sources"].Children())
                {
                    var source = (string)JObject.Parse(x.ToString())["source"];
                    services.Add(new Service()
                    {
                        Name = (string)JObject.Parse(x.ToString())["display_name"],
                        Source = source
                    });
                    links.Add(source, (string)JObject.Parse(x.ToString())["link"]);
                }
                List<MovieService> movieServices = new List<MovieService>();
                foreach (var ser in services)
                {
                    loadServices.TryGetValue(ser.Source, out Service getService);
                    if (getService != null)
                    {
                        movieServices.Add(new MovieService(id.Value, getService.ID, null, getService, links[getService.Source]));
                    }
                }

                string poster = (string)json["poster_400x570"];
                poster.Replace("http://", "https://");

                movie = new Movie()
                {
                    ID = (int)json["id"],
                    Title = (string)json["title"],
                    Poster = poster,
                    Desc = (string)json["overview"],
                    Aired = json["release_date"].ToString().Substring(0, 4),
                    Rating = (string)json["rating"],
                    Cast = castString,
                    MovieGenre = movieGenres,
                    MovieService = movieServices
                };
                _context.Add(movie);
                _context.SaveChanges();
            }
            var user = await GetCurrentUserAsync();
            MyListShows myListShow = null;
            bool isLoggedIn = true;
            bool isInList = false;
            if (user == null)
            {
                isLoggedIn = false;
                myListShow = null;
            }
            else
            {
                myListShow = await _context.MyListShows
                        .Include(x => x.MyList)
                        .Where(x => x.MyList.User.Id == user.Id)
                        .SingleOrDefaultAsync(x => x.SafeCompareId(movie.ID));
                isInList = myListShow == null ? false : true;
            }

            var vm = new DetailShowViewModel()
            {
                Movie = movie,
                MyListShow = myListShow,
                IsLoggedIn = isLoggedIn,
                IsInList = isInList
            };
            return View(vm);
        }
        private Task<AppUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}