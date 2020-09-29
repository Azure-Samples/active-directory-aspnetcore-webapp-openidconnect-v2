using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Infrastructure;
using WebApp_OpenIDConnect_DotNet.Models;
using WebApp_OpenIDConnect_DotNet.Services.Arm;
using WebApp_OpenIDConnect_DotNet.Services.GraphOperations;
using Constants = WebApp_OpenIDConnect_DotNet.Infrastructure.Constants;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        readonly ITokenAcquisition tokenAcquisition;
        private readonly IGraphApiOperations graphApiOperations;
        private readonly IArmOperations armOperations;

        public HomeController(ITokenAcquisition   tokenAcquisition,
                              IGraphApiOperations graphApiOperations,
                              IArmOperations armOperations)
        {
            this.tokenAcquisition   = tokenAcquisition;
            this.graphApiOperations = graphApiOperations;
            this.armOperations = armOperations;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AuthorizeForScopes(Scopes = new[] {Constants.ScopeUserRead})]
        public async Task<IActionResult> Profile()
        {
            var accessToken =
                await tokenAcquisition.GetAccessTokenForUserAsync(new[] {Constants.ScopeUserRead});

            var me = await graphApiOperations.GetUserInformation(accessToken);
            var photo = await graphApiOperations.GetPhotoAsBase64Async(accessToken);

            ViewData["Me"] = me;
            ViewData["Photo"] = photo;

            return View();
        }

        // Requires that the app has added the Azure Service Management / user_impersonation scope, and that
        // the admin tenant does not require admin consent for ARM.
        [AuthorizeForScopes(Scopes = new[] { "https://management.core.windows.net/user_impersonation", "user.read", "directory.read.all" })]
        public async Task<IActionResult> Tenants()
        {
            var accessToken =
                await tokenAcquisition.GetAccessTokenForUserAsync(new[] { $"{ArmApiOperationService.ArmResource}user_impersonation" });

            var tenantIds = await armOperations.EnumerateTenantsIdsAccessibleByUser(accessToken);
            /*
                        var tenantsIdsAndNames =  await graphApiOperations.EnumerateTenantsIdAndNameAccessibleByUser(tenantIds,
                            async tenantId => { return await tokenAcquisition.GetAccessTokenForUserAsync(new string[] { "Directory.Read.All" }, tenantId); });
            */
            ViewData["tenants"] = tenantIds;

            return View();
        }


		
		[AuthorizeForScopes(Scopes = new[] { "https://storage.azure.com/user_impersonation" })]

        public async Task<IActionResult> Blob()
        {
            var scopes = new string[] { "https://storage.azure.com/user_impersonation" };

            var accessToken =
                await tokenAcquisition.GetAccessTokenForUserAsync(scopes);

            // create a blob on behalf of the user
            TokenCredential tokenCredential = new TokenCredential(accessToken);
            StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);

            // replace the URL below with your storage account URL
            Uri blobUri = new Uri("https://blobstorageazuread.blob.core.windows.net/sample-container/Blob1.txt");
            CloudBlockBlob blob = new CloudBlockBlob(blobUri, storageCredentials);
            await blob.UploadTextAsync("Blob created by Azure AD authenticated user.");

            ViewData["Message"] = "Blob successfully created";
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}