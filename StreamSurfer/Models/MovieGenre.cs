using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models
{
    public class MovieGenre
    {
        public int MovieID { get; set; }
        public int GenreID { get; set; }

        public Movie Movie { get; set; }
        public Genre Genre { get; set; }

        public MovieGenre() { }

        public MovieGenre(int MovieID, int GenreID, Movie Movie, Genre Genre)
        {
            this.MovieID = MovieID;
            this.GenreID = GenreID;
            this.Movie = Movie;
            this.Genre = Genre;
        }
    }
}
