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
                        genreDictionary.Add(sg.Genre.Title, new List<Show> { sg.Show });
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
                .Include(s => s.Service)
                .Include(s => s.Show)
                .ToListAsync();
            var getMovieService = await _context.MovieService
                .Include(m => m.Service)
                .Include(m => m.Movie)
                .ToListAsync();
            var getServices = await _context.Services
                .ToListAsync();
            var getGenres = await _context.Genres
                .ToListAsync();

            if (getServices.Count == 0)
            {
                //get all sources
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
                        Name = (string)JObject.Parse(x.ToString())["display_name"],
                        Source = (string)JObject.Parse(x.ToString())["source"],
                        Link = (string)JObject.Parse(x.ToString())["info"]
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
            }

            if (getGenres.Count == 0)
            {
                var genres = await webRequest.Get(showService.GetGenres());
                if (!genres.IsSuccessStatusCode)
                {
                    return NotFound();
                }
                var genreContent = await genres.Content.ReadAsStringAsync();
                var genreJson = JObject.Parse(genreContent);

                List<Genre> genreList = genreJson["results"]
                    .Children()
                    .Select(x => new Genre()
                    {
                        ID = (int)JObject.Parse(x.ToString())["id"],
                        Title = (string)JObject.Parse(x.ToString())["genre"]
                    })
                    .ToList();
                foreach (var gen in genreList)
                {
                    Genre getGenre = _context.Genres.SingleOrDefault(s => s.ID == gen.ID);
                    if (getGenre == null)
                    {
                        _context.Add(gen);
                    }
                }
                _context.SaveChanges();
            }

            SortedDictionary<String, HashSet<Object>> serviceDictionary = new SortedDictionary<String, HashSet<Object>>();
            foreach (var ser in getShowService)
            {
                string key = ser.Service.Name;
                if (!serviceDictionary.ContainsKey(key))
                {
                    serviceDictionary.Add(key, new HashSet<Object>() { ser.Show });
                }
                else
                {
                    HashSet<Object> getList = serviceDictionary[key];
                    if (!getList.Contains(ser.Show))
                    {
                        getList.Add(ser.Show);
                        serviceDictionary.Remove(key);
                        serviceDictionary.Add(key, getList);
                    }
                }
            }
            foreach (var mov in getMovieService)
            {
                string key = mov.Service.Name;
                if (!serviceDictionary.ContainsKey(key))
                {
                    serviceDictionary.Add(key, new HashSet<Object>() { mov.Movie });
                }
                else
                {
                    HashSet<Object> getList = serviceDictionary[key];
                    if (!getList.Contains(mov.Movie))
                    {
                        getList.Add(mov.Movie);
                        serviceDictionary.Remove(key);
                        serviceDictionary.Add(key, getList);
                    }
                }
            }
            return View(serviceDictionary);
        }

        public async Task<IActionResult> Movies()
        {
            var getMovieGenre = await _context.MovieGenre
                .Include(m => m.Genre)
                .Include(m => m.Movie)
                .ToListAsync();
            SortedDictionary<String, List<Movie>> genreDictionary = new SortedDictionary<String, List<Movie>>();
            if (getMovieGenre.Count > 0)
            {
                foreach (var mg in getMovieGenre)
                {
                    if (!genreDictionary.ContainsKey(mg.Genre.Title))
                    {
                        genreDictionary.Add(mg.Genre.Title, new List<Movie> { mg.Movie });
                    }
                    else
                    {
                        var movieList = genreDictionary[mg.Genre.Title];
                        movieList.Add(mg.Movie);
                        genreDictionary.Remove(mg.Genre.Title);
                        genreDictionary.Add(mg.Genre.Title, movieList);
                    }
                }
            }
            return View(genreDictionary);
        }
    }
}