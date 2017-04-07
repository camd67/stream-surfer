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
            /*
            // DB calls, uncomment when data is added to DB
            var service = await _context.Services
                .Include(m => m.ShowService)
                .ToListAsync();
            var show = await _context.Shows
                .Include(m => m.ShowService)
                .SingleOrDefaultAsync(m => m.ID == id);
            */
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
                .Select(x => new Genre() { ShowID = id.Value, Title = x.ToString() })
                .ToList();
            // TODO: support more than just web links (such as ios + android)
            List<ShowService> services = json["channels"][0]["live_stream"]["web"]
                .Children()
                .Select(x => new ShowService() { ShowID = id.Value})
                .ToList();
            Show show = new Show()
            {
                ID = (int)json["id"],
                Title = (string)json["title"],
                Picture = (string)json["artwork_448x252"],
                Desc = (string)json["overview"],
                Synonyms = synonyms,
                Genres = genres
                //ShowService = services
            };
            return View(show);
        }
    }
}