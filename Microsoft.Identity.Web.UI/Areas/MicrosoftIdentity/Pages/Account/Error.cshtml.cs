using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace Microsoft.Identity.Web.UI.Areas.MicrosoftIdentity.Pages.Account
{
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        /// <summary>
        /// This API supports infrastructure and is not intended to be used
        /// directly from your code.This API may change or be removed in future releases
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// This API supports infrastructure and is not intended to be used
        /// directly from your code.This API may change or be removed in future releases
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// This API supports infrastructure and is not intended to be used
        /// directly from your code.This API may change or be removed in future releases
        /// </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}
