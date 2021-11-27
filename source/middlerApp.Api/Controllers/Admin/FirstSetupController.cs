using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Attributes;
using middlerApp.Api.Models;
using middlerApp.Auth;
using middlerApp.Auth.Entities;
using middlerApp.Auth.Services;

namespace middlerApp.Api.Controllers.Admin
{
    [ApiController]
    [Route("_api/first-setup")]
    [AdminController]
    [AllowAnonymous]
    public class FirstSetupController: Controller
    {

        private readonly DefaultResourcesManager _defaultResourcesManager;
        private readonly UserManager<MUser> _userManager;


        public FirstSetupController(DefaultResourcesManager defaultResourcesManager, UserManager<MUser> userManager)
        {
            _defaultResourcesManager = defaultResourcesManager;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> GetSetupViewModel()
        {
            var vm = new FirstSetupViewModel();
            vm.AdminUserExists = await _defaultResourcesManager.AtLeastOneAdminUserExistsAsync();

            return Ok(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFirstUser([FromBody] FirstSetupModel firstSetupModel)
        {

            var exists = await _defaultResourcesManager.AtLeastOneAdminUserExistsAsync();
            if (exists)
            {
                return BadRequest("Admin User already exists!");
            }

            var adminRole = await _defaultResourcesManager.EnsureAdminRoleExists();

            var user = new MUser();
            user.UserName = firstSetupModel.Username;
            user.Roles.Add(adminRole);
            user.Active = true;

            var result = await _userManager.CreateAsync(user, firstSetupModel.Password);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
            //await _defaultResourcesManager.EnsureAdminClientExists(firstSetupModel.RedirectUri);


        }
    }

    public class FirstSetupViewModel
    {
        public bool AdminUserExists { get; set; }
    }
}
