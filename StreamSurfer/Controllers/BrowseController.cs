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

        public async Task<IActionResult> Genres()
        {
            var getShowGenre = await _context.ShowGenre
                .Include(m => m.Genre)
                .Include(m => m.Show)
                .ToListAsync();
            SortedDictionary<String, List<Show>> genreDictionary = new SortedDictionary<String, List<Show>>();
            if (getShowGenre == null)
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
                    .Select(x => new Show()
                    {
                        ID = (int)JObject.Parse(x.ToString())["id"],
                        Title = (string)JObject.Parse(x.ToString())["title"],
                        Artwork = (string)JObject.Parse(x.ToString())["artwork_304x171"],
                    })
                    .ToList();
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
                        }
                        else
                        {
                            tempList = new List<Show>();
                        }
                        tempList.Add(show);
                        genreDictionary.Add(genre, tempList);
                    }
                    _context.Add(show);
                    _context.SaveChanges();
                }
            }
            else
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
    }
}