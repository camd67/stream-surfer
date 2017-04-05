using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StreamSurfer.Services;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace StreamSurfer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebRequestHandler webRequest;
        private readonly IShowService showService;
        private readonly ILogger logger;

        public HomeController(IWebRequestHandler webRequest, IShowService showService, ILogger<HomeController> logger)
        {
            this.logger = logger;
            this.webRequest = webRequest;
            this.showService = showService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
        
        public IActionResult Error()
        {
            return View();
        }
    }
}
