using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApp_OpenIDConnect_DotNet.Models;
using Microsoft.AspNetCore.Http;
using WebApp_OpenIDConnect_DotNet.Infrastructure;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.AssignmentToGroupAdminGroupRequired)]
    public class AdminController : Controller
    {
        public AdminController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}