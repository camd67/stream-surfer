using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models
{
    public class ShowGenre
    {
        public int ShowID { get; set; }
        public int GenreID { get; set; }

        public Show Show { get; set; }
        public Genre Genre { get; set; }

        public ShowGenre() { }

        public ShowGenre(int ShowID, int GenreID, Show Show, Genre Genre)
        {
            this.ShowID = ShowID;
            this.GenreID = GenreID;
            this.Show = Show;
            this.Genre = Genre;
        }
    }
}
