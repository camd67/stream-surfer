using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models
{
    public class MovieService
    {
        public int MovieID { get; set; }
        public int ServiceID { get; set; }

        public Movie Movie { get; set; }
        public Service Service { get; set; }
        public string MovieLink { get; set; }

        public MovieService() { }

        public MovieService(int MovieID, int ServiceID, Movie Movie, Service Service, string MovieLink)
        {
            this.MovieID = MovieID;
            this.ServiceID = ServiceID;
            this.Movie = Movie;
            this.Service = Service;
            this.MovieLink = MovieLink;
        }
    }
}
