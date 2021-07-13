using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Models;
using WebApp_OpenIDConnect_DotNet.Services.Arm;
using WebApp_OpenIDConnect_DotNet.Services.GraphOperations;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        readonly ITokenAcquisition tokenAcquisition;
        private readonly IGraphApiOperations graphApiOperations;
        private readonly IArmOperations armOperations;
        private readonly IArmOperationsWithImplicitAuth armOperationsWithImplicitAuth;

        public HomeController(ITokenAcquisition tokenAcquisition,
                              IGraphApiOperations graphApiOperations,
                              IArmOperations armOperations,
                              IArmOperationsWithImplicitAuth armOperationsWithImplicitAuth)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.graphApiOperations = graphApiOperations;
            this.armOperations = armOperations;
            this.armOperationsWithImplicitAuth = armOperationsWithImplicitAuth;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AuthorizeForScopes(Scopes = new[] { WebApp_OpenIDConnect_DotNet.Infrastructure.Constants.ScopeUserRead })]
        public async Task<IActionResult> Profile()
        {
            var accessToken =
                await tokenAcquisition.GetAccessTokenForUserAsync(new[] { WebApp_OpenIDConnect_DotNet.Infrastructure.Constants.ScopeUserRead });

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

        // Requires that the app has added the Azure Service Management / user_impersonation scope, and that
        // the admin tenant does not require admin consent for ARM.
        [AuthorizeForScopes(Scopes = new[] { "https://management.core.windows.net/user_impersonation" })]
        public async Task<IActionResult> TenantsWithImplicitAuth()
        {
            var tenantIds = await armOperationsWithImplicitAuth.EnumerateTenantsIds();
            /*
                        var tenantsIdsAndNames =  await graphApiOperations.EnumerateTenantsIdAndNameAccessibleByUser(tenantIds,
                            async tenantId => { return await tokenAcquisition.GetAccessTokenForUserAsync(new string[] { "Directory.Read.All" }, tenantId); });
            */
            ViewData["tenants"] = tenantIds;

            return View(nameof(Tenants));
        }

        [AuthorizeForScopes(Scopes = new[] { "https://storage.azure.com/user_impersonation" })]
        public async Task<IActionResult> Blob()
        {
            string message = "Blob failed to create";
            // replace the URL below with your storage account URL
            Uri blobUri = new Uri("https://aadsamplesstorageaccount.blob.core.windows.net/sample-apiaccess/sampleblob1.txt");
            BlobClient blobClient = new BlobClient(blobUri, new TokenAcquisitionTokenCredential(tokenAcquisition));

            string blobContents = "Blob created by Azure AD authenticated user.";
            byte[] byteArray = Encoding.ASCII.GetBytes(blobContents);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                try
                {
                    await blobClient.UploadAsync(stream);
                    message = "Blob successfully created";
                }
	            catch (MicrosoftIdentityWebChallengeUserException ex)
                {
                    throw ex;
                }	
                catch (MsalUiRequiredException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    try
                    {
                        message += $". Reason - {((Azure.RequestFailedException)ex).ErrorCode}";
                    }
                    catch (Exception)
                    {
                        message += $". Reason - {ex.Message}";
                    }
                }
            }

            ViewData["Message"] = message;
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