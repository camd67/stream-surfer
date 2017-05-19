using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Services
{
    public interface IShowService
    {
        string ConvertToShowSearch(string query);
        string ConvertToDetail(int id);
        string ConvertToServices(int id);
        string GetShows(int limit);
        string GetEpisodes(int id, int limit, int offset);
    }
}
