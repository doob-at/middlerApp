using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    [IdPController]
    [Authorize(Policy = "Admin")]

    public class UsersController : Controller
    {

        private readonly IMapper _mapper;
        private readonly IMUserService _mUserService;
        private readonly UserManager<MUser> _userManager;


        public UsersController(IMapper mapper, IMUserService mUserService, UserManager<MUser> userManager)
        {
            _mapper = mapper;
            _mUserService = mUserService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<MUserListDto>>> GetAllUsers()
        {
            var users = await _mUserService.GetAllUsersAsync();
            var dtos = users.Select(u => _mapper.Map<MUserListDto>(u));
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MUserDto>> GetUser(string id)
        {

            if (id == "create")
            {
                return Ok(new MUserDto());
            }

            var user = await _userManager.FindByIdAsync(id);
            
            if (user == null)
                return NotFound();

            var dto = _mapper.Map<MUserDto>(user);
            return Ok(dto);


        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(MUserDto createUserDto)
        {
            var userModel = _mapper.Map<MUser>(createUserDto);
            await _userManager.CreateAsync(userModel);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(MUserDto updateUserDto)
        {

            
            var user = await _userManager.FindByIdAsync(updateUserDto.Id.ToString());

            user = _mapper.Map(updateUserDto, user);
            
            await _userManager.UpdateAsync(user);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {

            await _mUserService.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUsers([FromBody] List<Guid> ids)
        {

            await _mUserService.DeleteUserAsync(ids.ToArray());
            return NoContent();
        }


        [HttpPost("{id}/password")]
        public async Task<IActionResult> SetPassword(Guid id, SetPasswordDto passwordDto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            var passwordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, passwordToken, passwordDto.Password);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        [HttpDelete("{id}/password")]
        public async Task<IActionResult> ClearPassword(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            await _userManager.RemovePasswordAsync(user);
            return Ok();
        }
    }
}
