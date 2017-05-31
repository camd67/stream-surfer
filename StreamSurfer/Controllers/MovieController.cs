using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StreamSurfer.Models;
using StreamSurfer.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using StreamSurfer.Models.MovieViewModels;
using Microsoft.AspNetCore.Identity;

namespace StreamSurfer.Controllers
{
    public class MovieController : Controller
    {
        private readonly PostgresDataContext _context;
        private readonly IWebRequestHandler webRequest;
        private readonly IShowService showService;
        private readonly ILogger logger;
        private readonly UserManager<AppUser> _userManager;

        public MovieController(PostgresDataContext context, IWebRequestHandler webRequest, IShowService showService, ILogger<HomeController> logger, UserManager<AppUser> _userManager)
        {
            this._userManager = _userManager;
            this._context = context;
            this.logger = logger;
            this.webRequest = webRequest;
            this.showService = showService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Detail(int? id)
        {
            Movie movie = await showService.GetMovieDetails(id, _context, webRequest);
            var user = await GetCurrentUserAsync();
            MyListShows myListShow = null;
            bool isLoggedIn = true;
            bool isInList = false;
            if (user == null)
            {
                isLoggedIn = false;
                myListShow = null;
            }
            else
            {
                myListShow = await _context.MyListShows
                        .Include(x => x.MyList)
                        .Where(x => x.MyList.User.Id == user.Id)
                        .SingleOrDefaultAsync(x => x.SafeCompareId(movie.ID));
                isInList = myListShow == null ? false : true;
            }

            var vm = new DetailShowViewModel()
            {
                Movie = movie,
                MyListShow = myListShow,
                IsLoggedIn = isLoggedIn,
                IsInList = isInList
            };
            return View(vm);
        }

        private Task<AppUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}