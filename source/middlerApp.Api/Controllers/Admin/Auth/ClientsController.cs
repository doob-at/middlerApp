using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Attributes;
using middlerApp.Auth.Managers;
using middlerApp.Auth.Models.DTO;
using middlerApp.Auth.Stores;
using OpenIddict.Abstractions;

namespace middlerApp.Api.Controllers.Admin.Auth
{
    [ApiController]
    [Route("_api/idp/clients")]
    [AdminController]
    [IdPController]
    [Authorize(Policy = "Admin")]
    public class ClientsController: Controller
    {
        
        private readonly ClientsStore _clientsStore;
        private readonly IMapper _mapper;

        public ClientsController(ClientsStore clientsStore, IMapper mapper)
        {
            _clientsStore = clientsStore;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetClients(int? count, int? offset)
        {
            var applications = await _clientsStore.ListAsync(count, offset, HttpContext.RequestAborted).ToListAsync();
            return Ok(applications);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(Guid id)
        {
            var client = await _clientsStore.FindByIdAsync(id.ToString(), HttpContext.RequestAborted);
            if (client == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<MClientDto>(client));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateClient(MClientDto clientDto)
        {
            var clientInDB = await _clientsStore.FindByIdAsync(clientDto.Id.ToString(), HttpContext.RequestAborted);

          

            var updated = _mapper.Map(clientDto, clientInDB);
          

            await _clientsStore.UpdateAsync(updated, HttpContext.RequestAborted);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var clientInDB = await _clientsStore.FindByIdAsync(id.ToString(), HttpContext.RequestAborted);

            await _clientsStore.DeleteAsync(clientInDB, HttpContext.RequestAborted);
            return NoContent();
        }

        
    }
}
