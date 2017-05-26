using System.Collections.Generic;

namespace StreamSurfer.Models
{
    public class MyList
    {
        public int Id {get; set; }
        public AppUser User { get; set; }

        public List<MyListShows> MyListShows { get; set; }
    }
}
