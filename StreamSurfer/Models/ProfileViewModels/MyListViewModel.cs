using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models.ProfileViewModels
{
    public class MyListViewModel
    {
        public string Username { get; set; }
        public List<MyListShows> Watching { get; set; }
        public List<MyListShows> Complete { get; set; }
        public List<MyListShows> WantToWatch { get; set; }
    }
}
