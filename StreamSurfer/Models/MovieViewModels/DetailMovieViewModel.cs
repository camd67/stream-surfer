using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models.MovieViewModels
{
    public class DetailMovieViewModel
    {
        public Movie Movie { get; set; }
        public MyListShows MyListShow { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool IsInList { get; set; }
    }
}
