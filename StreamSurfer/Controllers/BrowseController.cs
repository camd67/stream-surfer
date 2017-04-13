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
            var genres = new List<string> { "Action", "Action and Adventure", "Adventure", "Animation", "Biography", "Children", "Comedy",
                                            "Crime", "Documentary", "Drama", "Family", "Fantasy", "Film-Noir", "Food", "Game Show",
                                            "History", "Home and Garden", "Horror", "Mini-Series", "Music", "Musical", "Mystery", "News",
                                            "Reality", "Romance", "Science-Fiction", "Soap", "Special Interest", "Sport", "Suspense",
                                            "Talk Show", "Thriller", "Travel", "Variety", "War", "Western" };
            var response = await webRequest.Get(showService.GetShows(1));
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
            List<ShowGenre> showGenres = new List<ShowGenre>();
            foreach (var show in showResults)
            {
                response = await webRequest.Get(showService.ConvertToDetail(show.ID));
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                content = await response.Content.ReadAsStringAsync();
                json = JObject.Parse(content);
                List<Genre> showGenreList = json["genres"]
                    .Children()
                    .Select(x => new Genre() { Title = (string)JObject.Parse(x.ToString())["title"] })
                    .ToList();
                foreach (var genre in showGenreList)
                {
                    showGenres.Add(new ShowGenre { Genre = genre, Show = show });
                }
            }
            /*
            var response = await webRequest.Get(showService.GetGenre());
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            List<Genre> genres = json["results"]
                .Children()
                .Select(x => new Genre() { Title = (string)JObject.Parse(x.ToString())["genre"] })
                .ToList();
            */
            List<Show> shows = new List<Show>();
            return View(showGenres);
        }
    }
}