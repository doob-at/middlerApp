using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Attributes;
using middlerApp.Auth.Managers;
using OpenIddict.Abstractions;

namespace ScsmPortal.API.Controllers.Admin.Auth
{
    [ApiController]
    [Route("_api/idp/applications")]
    [AdminController]
    [Authorize(Policy = "Admin")]
    public class AuthApplicationController: Controller
    {
        
        private readonly AuthApplicationManager _applicationManager;

        public AuthApplicationController(IOpenIddictApplicationManager applicationManager)
        {
            
            _applicationManager = (AuthApplicationManager)applicationManager;
        }


        [HttpGet]
        public async Task<IActionResult> GetApplications(int? count, int? offset)
        {

            return Ok(_applicationManager.GetApplicationsAsync(count, offset, HttpContext.RequestAborted));
        }
    }
}
