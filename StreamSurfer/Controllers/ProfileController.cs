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

        public ProfileController(
          UserManager<AppUser> userManager,
          SignInManager<AppUser> signInManager,
          IOptions<IdentityCookieOptions> identityCookieOptions,
          IMessageService messageSender,
          ILoggerFactory loggerFactory)
        {
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
            string path = "default-avatar.png";
            var model = new OverviewViewModel()
            {
                Username = await _userManager.GetUserNameAsync(user),
                RegisterDate = user.RegisterDate,
                ProfilePicture = path
            };
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult List(int? id)
        {
            return View();
        }

        private Task<AppUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}