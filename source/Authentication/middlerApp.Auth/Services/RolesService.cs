using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using middlerApp.Auth.Context;
using middlerApp.Auth.Entities;
using middlerApp.Auth.Models.DTO;
using middlerApp.Events;

namespace middlerApp.Auth.Services
{
    public class RolesService : IRolesService
    {
        private readonly IMapper _mapper;
        private AuthDbContext DbContext { get; }
        public DataEventDispatcher EventDispatcher { get; }


        public RolesService(AuthDbContext dbContext, IMapper mapper, DataEventDispatcher eventDispatcher)
        {
            _mapper = mapper;
            DbContext = dbContext;
            EventDispatcher = eventDispatcher;
        }

        public async Task<List<MRoleListDto>> GetAllRoleListDtosAsync()
        {
            var roles = await DbContext.Roles.AsQueryable().ToListAsync();
            return _mapper.Map<List<MRoleListDto>>(roles);
        }

        public async Task<MRole> GetRoleAsync(Guid id)
        {
            return await DbContext.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(c => c.Id == id);
        }


        public async Task<MRoleDto> GetRoleDtoAsync(Guid id)
        {
            var role = await GetRoleAsync(id);
            return _mapper.Map<MRoleDto>(role);
        }

        public async Task CreateRoleAsync(MRoleDto roleDto)
        {
            var role = _mapper.Map<MRole>(roleDto);
            await DbContext.Roles.AddAsync(role);
            await DbContext.SaveChangesAsync();

            EventDispatcher.DispatchCreatedEvent("IDPRoles", _mapper.Map<MRoleListDto>(role));
        }


        public async Task DeleteRole(params Guid[] id)
        {
            var roles = await DbContext.Roles.AsQueryable().Where(u => id.Contains(u.Id)).ToListAsync();
            DbContext.Roles.RemoveRange(roles);
            await DbContext.SaveChangesAsync();

            EventDispatcher.DispatchDeletedEvent("IDPRoles", roles.Select(r => r.Id));
        }

        //public async Task UpdateRoleAsync(MRole updated)
        //{
        //    //var roleModel = _mapper.Map<MRole>(updated);
        //    var userIds = updated.Users.Select(ur => ur.Id).ToList();
        //    var availableUsers = DbContext.Users.AsQueryable().Where(r => userIds.Contains(r.Id)).Select(r => r.Id).ToList();

        //    updated.Users = updated.Users.Where(ur => availableUsers.Contains(ur.Id)).ToList();

        //    await DbContext.SaveChangesAsync();

        //    EventDispatcher.DispatchUpdatedEvent("IDPRoles", _mapper.Map<MRoleListDto>(updated));
        //}

        public async Task UpdateRoleAsync(MRoleDto updated)
        {
            var roleInDb = await GetRoleAsync(updated.Id);
            _mapper.Map(updated, roleInDb);
            //var roleModel = _mapper.Map<MRole>(updated);
            var userIds = updated.Users.Select(ur => ur.Id).ToList();
            var availableUsers = DbContext.Users.Where(r => userIds.Contains(r.Id)).ToList();

            roleInDb.Users = availableUsers;

            await DbContext.SaveChangesAsync();

            EventDispatcher.DispatchUpdatedEvent("IDPRoles", _mapper.Map<MRoleListDto>(roleInDb));
        }

       
    }
}