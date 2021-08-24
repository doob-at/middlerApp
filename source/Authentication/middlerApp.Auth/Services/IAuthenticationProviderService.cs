using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using middlerApp.Auth.Entities;
using middlerApp.Auth.Models.DTO;
using middlerApp.Events;

namespace middlerApp.Auth.Services
{
    public interface IAuthenticationProviderService
    {
        DataEventDispatcher EventDispatcher { get; }
        Task<List<AuthenticationProviderListDto>> GetAllListDtos();
        Task<List<AuthenticationProvider>> GetAll();
        Task<AuthenticationProvider> GetSingleAsync(Guid id);

        Task<AuthenticationProvider> GetByNameAsync(string name);
        Task Create(AuthenticationProvider authenticationProvider);
        Task Delete(params Guid[] id);
        Task Update(AuthenticationProvider authenticationProvider);
    }
}