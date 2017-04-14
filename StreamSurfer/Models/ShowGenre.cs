using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models
{
    public class ShowGenre
    {
        public int Id { get; set; }
        public int ShowID { get; set; }
        public int GenreID { get; set; }

        public Show Show { get; set; }
        public Genre Genre { get; set; }
    }
}
