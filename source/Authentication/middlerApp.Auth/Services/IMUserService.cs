using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using middlerApp.Auth.Entities;

namespace middlerApp.Auth.Services
{
    public interface IMUserService
    {
        

        public Task<List<MUser>> GetAllUsersAsync();


        public Task DeleteUserAsync(params Guid[] id);

    }
}
