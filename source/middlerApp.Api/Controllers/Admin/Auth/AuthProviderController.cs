using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Attributes;
using middlerApp.Api.Providers;
using middlerApp.Auth.Entities;
using middlerApp.Auth.ExtensionMethods;
using middlerApp.Auth.Models.DTO;
using middlerApp.Auth.Services;

namespace middlerApp.Api.Controllers.Admin.Auth
{
    [ApiController]
    [Route("_api/idp/auth-provider")]
    [AdminController]
    [Authorize(Policy = "Admin")]
    public class AuthProviderController: Controller
    {

        private readonly IAuthenticationProviderService _authenticationProviderService;
        private readonly IMapper _mapper;
        private readonly AuthenticationProviderContextService _authenticationProvider;


        public AuthProviderController(IAuthenticationProviderService authenticationProviderService, IMapper mapper, AuthenticationProviderContextService authenticationProvider)
        {
            _authenticationProviderService = authenticationProviderService;
            _mapper = mapper;
            _authenticationProvider = authenticationProvider;
        }


        [HttpGet]
        public async Task<ActionResult<List<AuthenticationProviderListDto>>> GetAllListDtos()
        {
            var providers = await _authenticationProviderService.GetAllListDtos();
            return Ok(providers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuthenticationProvider>> GetProvider(string id)
        {

            if (id == "create")
            {
                return Ok(new AuthenticationProvider());
            }

            if (!Guid.TryParse(id, out var guid))
                return NotFound();

            var client = await _authenticationProviderService.GetSingleAsync(guid);

            if (client == null)
                return NotFound();

            return Ok(client);
            
        }

        [HttpPost]
        public async Task<IActionResult> Create(AuthenticationProvider dto)
        {
            await _authenticationProviderService.Create(dto);
            _authenticationProvider.RegisterProvider(dto);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update(AuthenticationProvider dto)
        {
            var inDB = await _authenticationProviderService.GetSingleAsync(dto.Id);

            if (inDB == null)
                return NotFound();

            inDB.Map(dto);


            await _authenticationProviderService.Update(inDB);

            _authenticationProvider.UpdateProvider(inDB);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var prov = await _authenticationProviderService.GetSingleAsync(id);
            if (prov == null)
                return NotFound();

            await _authenticationProviderService.Delete(id);
            _authenticationProvider.UnRegisterProvider(prov.Name);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] List<Guid> ids)
        {
            var provs = await _authenticationProviderService.GetAll();
            var existing = provs.Where(p => ids.Contains(p.Id)).ToList();
            await _authenticationProviderService.Delete(existing.Select(e => e.Id).ToArray());

            foreach (var authenticationProvider in existing)
            {
                _authenticationProvider.UnRegisterProvider(authenticationProvider.Name);
            }

            return NoContent();
        }
    }
}
