using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models
{
    public class Service
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string Picture { get; set; }

        public int ShowID { get; set; }
        public Show ParentShow { get; set; }
    }
}
