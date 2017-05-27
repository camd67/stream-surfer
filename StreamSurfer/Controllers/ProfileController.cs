using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StreamSurfer.Models;
using StreamSurfer.Models.ProfileViewModels;
using StreamSurfer.Services;
using System.IO;

namespace StreamSurfer.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly string _externalCookieScheme;
        private readonly IMessageService _messageSender;
        private readonly ILogger _logger;
        private readonly PostgresDataContext _context;

        public ProfileController(
          PostgresDataContext context,
          UserManager<AppUser> userManager,
          SignInManager<AppUser> signInManager,
          IOptions<IdentityCookieOptions> identityCookieOptions,
          IMessageService messageSender,
          ILoggerFactory loggerFactory)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            _messageSender = messageSender;
            _logger = loggerFactory.CreateLogger<ProfileController>();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Overview(string id)
        {
            var user = await GetCurrentUserAsync();
            
            if((id == null || id == "") && user == null)
            {
                return NotFound();
            }
            string path = user.ProfilePicture == null || user.ProfilePicture == ""
                ? "default-avatar.png"
                : user.ProfilePicture;
            string bio =  user.Bio == null || user.Bio == ""
                ? "This user hasn't set a bio yet!"
                : user.Bio;
            var model = new OverviewViewModel()
            {
                Username = user.UserName,
                RegisterDate = user.RegisterDate,
                ProfilePicture = path,
                Bio = bio
            };
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult List(int id)
        {
            //var list = _context.MyList.Where(x => x.Id == id);
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> AddToList(int id)
        {
            var toAdd = _context.Shows.SingleOrDefault(x => x.ID == id);
            var user = await GetCurrentUserAsync();
            if(toAdd != null)
            {
                var myList = _context.MyList.FirstOrDefault(x => x.User.Id == user.Id);
                if(myList.MyListShows == null)
                {
                    myList.MyListShows = new List<MyListShows>();
                }
                _context.Add(new MyListShows()
                    {
                        ShowId = id,
                        MyListId = myList.Id
                    });
                _context.SaveChanges();
            }
            return Json("Added to list");
        }

        private Task<AppUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}