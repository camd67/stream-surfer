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

        public async Task<IActionResult> Index()
        {
            var response = await webRequest.Get(showService.GetShows(5));
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            List<Show> showResults = json["results"]
                .Children()
                .Select(x => new Show() { ID = (int)JObject.Parse(x.ToString())["id"],
                                            Title = (string)JObject.Parse(x.ToString())["title"],
                                            Picture = (string)JObject.Parse(x.ToString())["artwork_304x171"]})
                .ToList();
            SortedDictionary<String, List<Show>> genreDictionary = new SortedDictionary<String, List<Show>>();
            foreach (var show in showResults)
            {
                response = await webRequest.Get(showService.ConvertToDetail(show.ID));
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                content = await response.Content.ReadAsStringAsync();
                json = JObject.Parse(content);
                List<String> showGenreList = json["genres"]
                    .Children()
                    .Select(x => (string)JObject.Parse(x.ToString())["title"])
                    .ToList();
                foreach (var genre in showGenreList)
                {
                    List<Show> tempList;
                    if (genreDictionary.ContainsKey(genre))
                    {
                        tempList = genreDictionary[genre];
                        genreDictionary.Remove(genre);
                    } else
                    {
                        tempList = new List<Show>();
                    }
                    tempList.Add(show);
                    genreDictionary.Add(genre, tempList);
                }
            }
            List<Show> shows = new List<Show>();
            return View(genreDictionary);
        }
    }
}