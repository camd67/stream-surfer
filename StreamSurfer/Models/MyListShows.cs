using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models
{
    public class MyListShows
    {
        public int ID { get; set; }

        public MyList MyList { get; set; }
        public int MyListId { get; set; }

        public Movie Movie { get; set; }
        public int? MovieId { get; set; }

        public Show Show { get; set; }
        public int? ShowId { get; set; }

        public int Rating { get; set; }
        public int Status { get; set; }
        public DateTime LastChange { get; set; }

        public MyListShows() { }
        public MyListShows(int MyListId, MyList MyListRef,
                           int ShowId, Show ShowInList)
        {
            this.MyListId = MyListId;
            this.ShowId = ShowId;
            this.Show = ShowInList;
            this.MyList = MyListRef;
        }
        public MyListShows(int MyListId, MyList MyListRef,
                           int MovieId, Movie MovieInList)
        {
            this.MyListId = MyListId;
            this.MovieId = MovieId;
            this.Movie = MovieInList;
            this.MyList = MyListRef;
        }

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
            return MovieId == null ? ShowId == id : MovieId == id;
        }
        public bool SafeCompareId(string type, int id)
        {
            return type == "show" ? ShowId == id : MovieId == id;
        }
    }
}
