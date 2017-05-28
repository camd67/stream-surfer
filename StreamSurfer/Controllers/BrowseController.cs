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
    public class BrowseController : Controller
    {
        private readonly PostgresDataContext _context;
        private readonly IWebRequestHandler webRequest;
        private readonly IShowService showService;

        public BrowseController(PostgresDataContext context, IWebRequestHandler webRequest, IShowService showService)
        {
            this._context = context;
            this.webRequest = webRequest;
            this.showService = showService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Shows()
        {
            //get all sources
            /*
            var sources = await webRequest.Get(showService.GetSources());
            if (!sources.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var sourceContent = await sources.Content.ReadAsStringAsync();
            var sourceJson = JObject.Parse(sourceContent);
            List<Service> serviceList = sourceJson["results"]
                .Children()
                .Select(x => new Service()
                {
                    ID = (int)JObject.Parse(x.ToString())["id"],
                    Name = (string)JObject.Parse(x.ToString())["display_name"]
                })
                .ToList();

            foreach (var ser in serviceList)
            {
                Service getService = _context.Services.SingleOrDefault(s => s.ID == ser.ID);
                if (getService == null)
                {
                    _context.Add(ser);
                }
            }

            _context.SaveChanges();
            */

            var getShowGenre = await _context.ShowGenre
                .Include(m => m.Genre)
                .Include(m => m.Show)
                .ToListAsync();
            SortedDictionary<String, List<Show>> genreDictionary = new SortedDictionary<String, List<Show>>();
            if (getShowGenre.Count > 0)
            {
                foreach (var sg in getShowGenre)
                {
                    if (!genreDictionary.ContainsKey(sg.Genre.Title))
                    {
                        genreDictionary.Add(sg.Genre.Title, new List<Show>{sg.Show});
                    } else
                    {
                        var showList = genreDictionary[sg.Genre.Title];
                        showList.Add(sg.Show);
                        genreDictionary.Remove(sg.Genre.Title);
                        genreDictionary.Add(sg.Genre.Title, showList);
                    }
                }
            }
            return View(genreDictionary);
        }

        public async Task<IActionResult> Services()
        {
            var getShowService = await _context.ShowServices
                .ToListAsync();
            SortedDictionary<String, List<Show>> genreDictionary = new SortedDictionary<String, List<Show>>();
            return View(genreDictionary);
        }

        public async Task<IActionResult> Movies()
        {
            return View();
        }
    }
}