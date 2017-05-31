using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using StreamSurfer.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace StreamSurfer.Services
{
    public class GuideboxService : IShowService
    {
        private const string BASE_URL = "http://api-public.guidebox.com/v2/";
        private readonly string API_KEY;

        public GuideboxService(IOptions<Settings> settings)
        {
            API_KEY = settings.Value.GuideboxKey;

        }
        private string BuildQuery(string endpoint, string queryParams)
        {
            // only add query params if they exist
            return BASE_URL + endpoint + "?api_key=" + API_KEY + (queryParams != null ? "&" + queryParams : "");
        }

        public string ConvertToShowDetail(int id)
        {
            return BuildQuery("shows/" + id, null);
        }

        public string ConvertToMovieDetail(int id)
        {
            return BuildQuery("movies/" + id, null);
        }

        public string ConvertToShowSearch(string query)
        {
            return BuildQuery("search", "type=show&query=" + query);
        }

        public string ConvertToMovieSearch(string query)
        {
            return BuildQuery("search", "type=movie&query=" + query);
        }

        public string ConvertToServices(int id)
        {
            return BuildQuery("shows/" + id + "/available_content", null);
        }

        public string GetShows(int limit)
        {
            return BuildQuery("shows", "limit=" + limit);
        }

        public string GetEpisodes(int id, int limit, int offset)
        {
            return BuildQuery("shows/" + id + "/episodes", "include_links=true&reverse_ordering=true&platform=web&limit=" + limit + "&offset=" + offset);
        }

        public string GetSources()
        {
            return BuildQuery("sources", null);
        }

        public string GetGenres()
        {
            return BuildQuery("genres", null);
        }

        public string ConvertToMovieImage(int id)
        {
            return BuildQuery("movies/" + id + "/images", null);
        }

        public async Task<Show> GetShowDetails(int? id, PostgresDataContext _context, IWebRequestHandler webRequest)
        {
            if (id == null)
            {
                return (Show)NotFound();
            }
            var show = await _context.Shows
                .Include(m => m.ShowService)
                .Include(m => m.ShowGenre)
                .SingleOrDefaultAsync(m => m.ID == id);
            var loadGenres = _context.Genres
                .Include(m => m.ShowGenre)
                .ToDictionary(x => x.ID, x => x);
            var loadServices = _context.Services
                .Include(m => m.ShowService)
                .ToDictionary(x => x.Source, x => x);
            //switch cast to string separated by ;
            if (show == null)
            {
                //get show details
                var response = await webRequest.Get(this.ConvertToShowDetail(id.Value));
                if (!response.IsSuccessStatusCode)
                {
                    return (Show)NotFound();
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

                //get service details
                var serviceResponse = await webRequest.Get(this.ConvertToServices(id.Value));
                if (!serviceResponse.IsSuccessStatusCode)
                {
                    return (Show)NotFound();
                }
                var serviceContent = await serviceResponse.Content.ReadAsStringAsync();
                var serviceJson = JObject.Parse(serviceContent);
                // TODO: support more than just web links (such as ios + android)
                List<Service> services = serviceJson["results"]["web"]["episodes"]["all_sources"]
                    .Children()
                    .Select(x => new Service()
                    {
                        ID = (int)JObject.Parse(x.ToString())["id"],
                        Name = (string)JObject.Parse(x.ToString())["display_name"],
                        Source = (string)JObject.Parse(x.ToString())["source"]
                    })
                    .ToList();

                //get episodes
                var episodeResponse = await webRequest.Get(this.GetEpisodes(id.Value, 1, 0));
                if (!episodeResponse.IsSuccessStatusCode)
                {
                    return (Show)NotFound();
                }
                var episodeContent = await episodeResponse.Content.ReadAsStringAsync();
                JObject episodeJson = JObject.Parse(episodeContent);
                Dictionary<string, string> freeWeb = null;
                Dictionary<string, string> tvEverywhereWeb = null;
                Dictionary<string, string> subscriptionWeb = null;
                Dictionary<string, string> purchaseWeb = null;
                JArray results = (JArray)episodeJson["results"];
                if (results.Count > 0)
                {
                    freeWeb = getDictionary(episodeJson, "free_web_sources");
                    tvEverywhereWeb = getDictionary(episodeJson, "tv_everywhere_web_sources");
                    subscriptionWeb = getDictionary(episodeJson, "subscription_web_sources");
                    purchaseWeb = getDictionary(episodeJson, "purchase_web_sources");
                }

                //TODO services displayed multiple times
                List<ShowService> showServices = new List<ShowService>();
                HashSet<string> check = new HashSet<string>();
                foreach (var service in services)
                {
                    loadServices.TryGetValue(service.Source, out Service getService);
                    if (getService == null)
                    {
                        getService = service;
                    }
                    string link = "";
                    if (freeWeb.ContainsKey(getService.Name))
                    {
                        link = freeWeb[getService.Name];
                    }
                    else if (tvEverywhereWeb.ContainsKey(getService.Name))
                    {
                        link = tvEverywhereWeb[getService.Name];
                    }
                    else if (subscriptionWeb.ContainsKey(getService.Name))
                    {
                        link = subscriptionWeb[getService.Name];
                    }
                    else if (purchaseWeb.ContainsKey(getService.Name))
                    {
                        link = purchaseWeb[getService.Name];
                    }
                    if (!check.Contains(getService.Name))
                    {
                        check.Add(getService.Name);
                        showServices.Add(new ShowService(id.Value, getService.ID, null, getService, link));
                    }
                }
                List<ShowGenre> showGenres = new List<ShowGenre>();
                foreach (var genre in genres)
                {
                    loadGenres.TryGetValue(genre.ID, out Genre getGenre);
                    if (getGenre == null)
                    {
                        getGenre = genre;
                    }
                    showGenres.Add(new ShowGenre(id.Value, getGenre.ID, null, getGenre));
                }

                string poster = (string)json["poster"];
                poster.Replace("http://", "https://");
                string artwork = (string)json["artwork_304x171"];
                artwork.Replace("http://", "https://");

                show = new Show()
                {
                    ID = (int)json["id"],
                    Title = (string)json["title"],
                    Poster = poster,
                    Artwork = artwork,
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
            return show;
        }

        public async Task<Movie> GetMovieDetails(int? id, PostgresDataContext _context, IWebRequestHandler webRequest)
        {
            if (id == null)
            {
                return (Movie)NotFound();
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
                var response = await webRequest.Get(this.ConvertToMovieDetail(id.Value));
                if (!response.IsSuccessStatusCode)
                {
                    return (Movie)NotFound();
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
                HashSet<string> check = new HashSet<string>();
                foreach (var ser in services)
                {
                    loadServices.TryGetValue(ser.Source, out Service getService);
                    if (getService != null && !check.Contains(ser.Name))
                    {
                        check.Add(ser.Name);
                        movieServices.Add(new MovieService(id.Value, getService.ID, null, getService, links[getService.Source]));
                    }
                }

                string poster = (string)json["poster_400x570"];
                poster.Replace("http://", "https://");
                string artwork = poster;

                var movieResponse = await webRequest.Get(this.ConvertToMovieImage(id.Value));
                if (!movieResponse.IsSuccessStatusCode)
                {
                    return (Movie)NotFound();
                }
                var movieContent = await movieResponse.Content.ReadAsStringAsync();
                var movieJson = JObject.Parse(movieContent);

                JToken thumbnails = movieJson["results"]["thumbnails"];
                if (thumbnails != null)
                {
                    var artworkToken = thumbnails.First;
                    artwork = artworkToken["medium"]["url"].ToString();
                    artwork.Replace("http://", "https://");
                }

                movie = new Movie()
                {
                    ID = (int)json["id"],
                    Title = (string)json["title"],
                    Poster = poster,
                    Artwork = artwork,
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
            return movie;
        }

        private Object NotFound()
        {
            throw new NotImplementedException();
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
