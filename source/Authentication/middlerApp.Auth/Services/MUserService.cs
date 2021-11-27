using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using middlerApp.Auth.Context;
using middlerApp.Auth.Entities;

namespace middlerApp.Auth.Services
{
    public class MUserService: IMUserService
    {
        private readonly AuthDbContext _context;

        public MUserService( AuthDbContext context)
        {
            _context = context;
        }

        public Task<List<MUser>> GetAllUsersAsync()
        {
            return _context.Users.ToListAsync();
        }

        public Task DeleteUserAsync(params Guid[] id)
        {
            throw new NotImplementedException();
        }
    }
}
