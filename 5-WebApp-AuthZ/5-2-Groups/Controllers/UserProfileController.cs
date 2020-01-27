﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Services.MicrosoftGraph;
using Constants = WebApp_OpenIDConnect_DotNet.Infrastructure.Constants;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    // [Authorize(Roles = "8873daa2-17af-4e72-973e-930c94ef7549")] // Using groups ids in the Authorize attribute
    public class UserProfileController : Controller
    {
        private readonly ITokenAcquisition tokenAcquisition;
        private readonly IMSGraphService graphService;

        public UserProfileController(ITokenAcquisition tokenAcquisition, IMSGraphService MSGraphService)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.graphService = MSGraphService;
        }

        [AuthorizeForScopes(Scopes = new[] { Constants.ScopeUserRead, Constants.ScopeDirectoryReadAll })]        
        public async Task<IActionResult> Index()
        {
            // Using group ids/names in the IsInRole method
            // var isinrole = User.IsInRole("8873daa2-17af-4e72-973e-930c94ef7549");

            string accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(new[] { Constants.ScopeUserRead, Constants.ScopeDirectoryReadAll });

            User me = await graphService.GetMeAsync(accessToken);
            ViewData["Me"] = me;

            try
            {
                var photo = await graphService.GetMyPhotoAsync(accessToken);
                ViewData["Photo"] = photo;
            }
            catch
            {
                //swallow
            }
            
            IList<Group> groups = await graphService.GetMyMemberOfGroupsAsync(accessToken);            
            ViewData["Groups"] = groups;

            return View();
        }
    }
}