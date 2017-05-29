using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models.ShowViewModels
{
    public class DetailShowViewModel
    {
        public Show Show { get; set; }
        public MyListShows MyListShow { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool IsInList { get; set; }
    }
}
