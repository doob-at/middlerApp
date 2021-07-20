using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Attributes;
using middlerApp.Api.ExtensionMethods;
using middlerApp.Api.Models;
using OpenIddict.Validation.AspNetCore;

namespace middlerApp.Api.Controllers
{
    [Route("_api/status")]
    [AdminController]
    public class StatusController: Controller
    {

        private static DateTime ServiceStart = Process.GetCurrentProcess().StartTime;
        
        [HttpGet]
        public IActionResult GetStatus()
        {
            var status = new Status()
                {
                    ServiceName = this.GetType().Assembly.GetName().Name,
                    CurrentDateTime = DateTime.Now,
                    ClientIp = Request.FindSourceIp().FirstOrDefault()?.ToString(),
                    Version = this.GetType().Assembly.GetName().Version?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),

                    ProxyServers = Request.FindSourceIp().Skip(1).Select(ip => ip.ToString()).ToArray(),
                    CurrentUser = this.User.Identity?.Name ?? "Anonymous",
                    HostName = Environment.MachineName,
                    ServiceStart = ServiceStart,
                    ServiceRunningSince = DateTime.Now - ServiceStart
                };

                return Ok(status);
            
        }


        [HttpGet("userinfo")]
        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        public IActionResult GetAccountInfo()
        {
            



            if (!String.IsNullOrWhiteSpace(this.User.Identity?.Name))
            {
                var loggedInUser = new LoggedInUser();
                loggedInUser.UserName = this.User.Identity?.Name;
                return Ok(loggedInUser);
            }


            return Unauthorized();
        }
    }

}
