using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Services
{
    public interface IShowService
    {
        string ConvertToShowSearch(string query);
    }
}
