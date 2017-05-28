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

namespace StreamSurfer.Controllers
{
    public class MovieController : Controller
    {
        private readonly PostgresDataContext _context;
        private readonly IWebRequestHandler webRequest;
        private readonly IShowService showService;
        private readonly ILogger logger;

        public MovieController(PostgresDataContext context, IWebRequestHandler webRequest, IShowService showService, ILogger<HomeController> logger)
        {
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
                .ToList();
            var loadServices = _context.Services
                .Include(m => m.MovieService)
                .ToList();
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
                services.AddRange(json["free_web_sources"].Children().Select(x => new Service()
                {
                    Name = (string)JObject.Parse(x.ToString())["display_name"]
                }));
                services.AddRange(json["tv_everywhere_web_sources"].Children().Select(x => new Service()
                {
                    Name = (string)JObject.Parse(x.ToString())["display_name"]
                }));
                services.AddRange(json["subscription_web_sources"].Children().Select(x => new Service()
                {
                    Name = (string)JObject.Parse(x.ToString())["display_name"]
                }));
                services.AddRange(json["purchase_web_sources"].Children().Select(x => new Service()
                {
                    Name = (string)JObject.Parse(x.ToString())["display_name"]
                }));
                movie = new Movie()
                {
                    ID = (int)json["id"],
                    Title = (string)json["title"],
                    Poster = (string)json["poster_400x570"],
                    Desc = (string)json["overview"],
                    Aired = json["release_date"].ToString().Substring(0, 4),
                    Rating = (string)json["rating"],
                    Cast = castString
                };
                _context.Add(movie);
                _context.SaveChanges();
            }
            return View(movie);
        }

        private Dictionary<string, string> getDictionary(JObject episodeJson, string source)
        {
            Dictionary<string, string> dictionary = episodeJson["results"][0][source]
                .Children()
                .ToDictionary(x => (string)JObject.Parse(x.ToString())["display_name"], y => (string)JObject.Parse(y.ToString())["link"]);
            return dictionary;
        }
    }
}