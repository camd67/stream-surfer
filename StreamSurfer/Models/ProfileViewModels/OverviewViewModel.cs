﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models.ProfileViewModels
{
    public class OverviewViewModel
    {
        public string Username { get; set; }
        public DateTime RegisterDate { get; set; }
        public string ProfilePicture { get; set; }
        public string Bio { get; set; }
        public MyList List { get; set; }
        public List<Show> RecentlyRated { get; set; }
        public List<Show> RecentlyWatched { get; set; }
    }
}
