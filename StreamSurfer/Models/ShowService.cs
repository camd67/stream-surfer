using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models
{
    public class ShowService
    {
        public int ShowServiceID { get; set; }
        public int ShowID { get; set; }
        public int ServiceID { get; set; }

        public Show Show { get; set; }
        public Service Service { get; set; }
    }
}
