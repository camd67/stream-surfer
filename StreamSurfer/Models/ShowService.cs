using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models
{
    public class ShowService
    {
        public int ShowID { get; set; }
        public int ServiceID { get; set; }

        public Show Show { get; set; }
        public Service Service { get; set; }

        public ShowService(){ }

        public ShowService(int ShowID, int ServiceID, Show Show, Service Service)
        {
            this.ShowID = ShowID;
            this.ServiceID = ServiceID;
            this.Show = Show;
            this.Service = Service;
        }
    }
}
