using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Attributes;
using middlerApp.Api.Models.Idp;
using middlerApp.Auth.Entities;
using middlerApp.Auth.Models.DTO;
using middlerApp.Auth.Services;

namespace middlerApp.Api.Controllers.Admin.Auth
{
    [ApiController]
    [Route("_api/idp/users")]
    [AdminController]
    [Authorize(Policy = "Admin")]

    public class UsersController : Controller
    {

        private readonly IMapper _mapper;
        private readonly ILocalUserService _localUserService;


        public UsersController(ILocalUserService localUserService,
            IMapper mapper)
        {
            _mapper = mapper;
            _localUserService = localUserService;
        }

        [HttpGet]
        public async Task<ActionResult<List<MUserListDto>>> GetAllUsers()
        {

            var users = await _localUserService.GetAllUserListDtosAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MUserDto>> GetUser(string id)
        {

            if (id == "create")
            {
                return Ok(new MUserDto());
            }

            if (!Guid.TryParse(id, out var guid))
                return NotFound();

            var user = await _localUserService.GetUserDtoAsync(guid);

            
            if (user == null)
                return NotFound();


            return Ok(user);


        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(MUserDto createUserDto)
        {

            var userModel = _mapper.Map<MUser>(createUserDto);
            userModel.Subject = Guid.NewGuid().ToString();

            await _localUserService.AddUserAsync(userModel);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(MUserDto updateUserDto)
        {
            await _localUserService.UpdateUserAsync(updateUserDto);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {

            await _localUserService.DeleteUser(id);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUsers([FromBody] List<Guid> ids)
        {

            await _localUserService.DeleteUser(ids.ToArray());
            return NoContent();
        }


        [HttpPost("{id}/password")]
        public async Task<IActionResult> SetPassword(Guid id, SetPasswordDto passwordDto)
        {
            await _localUserService.SetPassword(id, passwordDto.Password);
            return Ok();
        }

        [HttpDelete("{id}/password")]
        public async Task<IActionResult> ClearPassword(Guid id)
        {
            await _localUserService.ClearPassword(id);
            return Ok();
        }
    }
}
