using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models.ShowViewModels
{
    public class SearchViewModel
    {
        public Show Show { get; set; }
        public Movie Movie { get; set; }
        public bool IsInList { get; set; }

        public string SafeGetTitle()
        {
            return Movie == null ? Show.Title : Movie.Title;
        }
        public string SafeGetPoster()
        {
            return Movie == null ? Show.Poster : Movie.Poster;
        }
        public string SafeGetArtwork()
        {
            return Movie == null ? Show.Artwork : Movie.Artwork;
        }
        public string SafeGetDesc()
        {
            return Movie == null ? Show.Desc : Movie.Desc;
        }
        public string SafeGetAired()
        {
            return Movie == null ? Show.Started : Movie.Aired;
        }
        public string SafeGetRating()
        {
            return Movie == null ? Show.Rating : Movie.Rating;
        }
        public string SafeGetCast()
        {
            return Movie == null ? Show.Cast : Movie.Cast;
        }
        public int SafeGetId()
        {
            return Movie == null ? Show.ID : Movie.ID;
        }
        public bool SafeCompareId(int id)
        {
            return Movie == null ? Show.ID == id : Movie.ID == id;
        }
        public bool SafeCompareId(string type, int id)
        {
            return type == "show" ? Show.ID == id : Movie.ID == id;
        }
        public string GetTypeString()
        {
            return IsMovie() ? "movie" : "show";
        }
        public bool IsMovie()
        {
            return Movie != null;
        }
        public bool IsShow()
        {
            return Show != null;
        }
    }
}
