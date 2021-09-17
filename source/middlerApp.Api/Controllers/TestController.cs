using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Attributes;

namespace middlerApp.Api.Controllers
{
    [Route("_test")]
    [Authorize]
    [IdPController]
    [AdminController]
    public class TestController: Controller
    {
        public IActionResult Get()
        {
            return Ok("Test");
        }
    }
}
