using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Models
{
    public class MyListShows
    {
        public MyList MyList { get; set; }
        public Show Show { get; set; }

        public int MyListId { get; set; }
        public int ShowId { get; set; }

        public MyListShows() { }
        public MyListShows(int MyListId, int ShowId, MyList MyListRef, Show ShowInList)
        {
            this.MyListId = MyListId;
            this.ShowId = ShowId;
            this.Show = ShowInList;
            this.MyList = MyListRef;
        }
    }
}
