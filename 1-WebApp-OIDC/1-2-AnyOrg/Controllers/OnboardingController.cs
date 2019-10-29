using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    public class OnboardingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}