using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StreamSurfer.Models;
using Microsoft.EntityFrameworkCore;
using StreamSurfer.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace StreamSurfer.Controllers
{
    public class ShowController : Controller
    {
        private readonly PostgresDataContext _context;
        private readonly IWebRequestHandler webRequest;
        private readonly IShowService showService;
        private readonly ILogger logger;

        public ShowController(PostgresDataContext context, IWebRequestHandler webRequest, IShowService showService, ILogger<HomeController> logger)
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
            var show = await _context.Shows
                .Include(m => m.ShowService)
                .Include(m => m.ShowGenre)
                .SingleOrDefaultAsync(m => m.ID == id);
            var loadGenres = await _context.Genres
                .Include(m => m.ShowGenre)
                .ToListAsync();
            var loadServices = await _context.Services
                .Include(m => m.ShowService)
                .ToListAsync();
            //switch cast to string separated by ;
            if (show == null)
            {
                var response = await webRequest.Get(showService.ConvertToDetail(id.Value));
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
                    .Select(x => new Genre() {
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
                var serviceResponse = await webRequest.Get(showService.ConvertToServices(id.Value));
                var serviceContent = await serviceResponse.Content.ReadAsStringAsync();
                var serviceJson = JObject.Parse(serviceContent);
                // TODO: support more than just web links (such as ios + android)
                List<Service> services = serviceJson["results"]["web"]["episodes"]["all_sources"]
                    .Children()
                    .Select(x => new Service()
                    {
                        ID = (int)JObject.Parse(x.ToString())["id"],
                        Name = (string)JObject.Parse(x.ToString())["display_name"]
                    })
                    .ToList();
                List<ShowService> showServices = new List<ShowService>();
                foreach (var service in services)
                {
                    Service getService = _context.Services.SingleOrDefault(s => s.ID == service.ID);
                    if (getService == null)
                    {
                        getService = service;
                    }
                    showServices.Add(new ShowService(id.Value, getService.ID, null, getService));
                }
                List<ShowGenre> showGenres = new List<ShowGenre>();
                foreach (var genre in genres)
                {
                    Genre getGenre = _context.Genres.SingleOrDefault(g => g.ID == genre.ID);
                    if (getGenre == null)
                    {
                        getGenre = genre;
                    }
                    showGenres.Add(new ShowGenre(id.Value, getGenre.ID, null, getGenre));
                }
                show = new Show()
                {
                    ID = (int)json["id"],
                    Title = (string)json["title"],
                    Picture = (string)json["artwork_304x171"],
                    Desc = (string)json["overview"],
                    Started = json["first_aired"].ToString().Substring(0, 4),
                    Rating = (string)json["rating"],
                    Cast = castString,
                    Synonyms = synonyms,
                    ShowGenre = showGenres,
                    ShowService = showServices
                };
                _context.Add(show);
                _context.SaveChanges();
            }
            return View(show);
        }
    }
}