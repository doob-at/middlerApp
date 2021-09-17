using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Models;
using middlerApp.Auth;

namespace middlerApp.Api.Controllers.Admin
{
    [Route("_api/uiconfig")]
    public class UIConfigController: Controller
    {
        private readonly StartUpConfiguration _startUpConfiguration;

        public UIConfigController(StartUpConfiguration startUpConfiguration)
        {
            _startUpConfiguration = startUpConfiguration;
        }

        [HttpGet]
        public IActionResult GetConfig()
        {
            var model = new UIConfigModel();
            
            model.IdpBaseUrl = IdpUriGenerator.GenerateRedirectUri(_startUpConfiguration.IdpSettings.ListeningIP,
                _startUpConfiguration.IdpSettings.HttpsPort);

            return Ok(model);
        }
    }
}
