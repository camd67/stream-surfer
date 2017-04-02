using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models
{
    public class Genre
    {
        public int ID { get; set; }
        public string Title { get; set; }

        public int ShowID { get; set; }
        public Show ParentShow { get; set; }
    }
}
