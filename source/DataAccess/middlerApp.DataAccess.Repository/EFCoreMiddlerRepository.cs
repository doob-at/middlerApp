using System.Collections.Generic;
using System.Linq;
using doob.middler.Common.Interfaces;
using doob.middler.Common.SharedModels.Models;
using Microsoft.EntityFrameworkCore;
using middlerApp.DataAccess.Context;
using middlerApp.DataAccess.ExtensionMethods;

namespace middlerApp.DataAccess
{
    public class EFCoreMiddlerRepository : IMiddlerRepository
    {
        public AppDbContext AppDbContext { get; }

        public EFCoreMiddlerRepository(AppDbContext appDbContext)
        {
            AppDbContext = appDbContext;
        }

        public List<MiddlerRule> ProvideRules()
        {
            var rules = AppDbContext
                .EndpointRules.AsQueryable()
                .Where(er => er.Enabled)
                .Include(r => r.Actions)
                .Include(r => r.Permissions)
                .ToList()
                .Select(r => r.ToMiddlerRule())
                .ToList();

            return rules;

        }
    }
}
