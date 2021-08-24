using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using middlerApp.Auth.Entities;
using middlerApp.Auth.Models.DTO;

namespace middlerApp.Auth.Services
{
    public interface IRolesService
    {
        Task<List<MRoleListDto>> GetAllRoleListDtosAsync();

        Task<MRole> GetRoleAsync(Guid id);

        Task<MRoleDto> GetRoleDtoAsync(Guid id);

        Task CreateRoleAsync(MRoleDto roleDto);

        Task DeleteRole(params Guid[] ids);
        Task UpdateRoleAsync(MRoleDto updated);
    }
}
