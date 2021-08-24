using System;
using Microsoft.EntityFrameworkCore;
using middlerApp.Auth.Entities;

namespace middlerApp.Auth.Context
{
    public class AuthDbContext: DbContext
    {

        public DbSet<Client> Clients { get; set; }
        public DbSet<AuthAuthorization> AuthAuthorizations { get; set; }
        public DbSet<AuthScope> AuthScopes { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }

        public DbSet<MUser> Users { get; set; }
        public DbSet<MRole> Roles { get; set; }

        public DbSet<MUserClaim> UserClaims { get; set; }
        public DbSet<MExternalClaim> ExternalClaims { get; set; }

        public DbSet<MUserLogin> UserLogins { get; set; }

        public DbSet<MUserSecret> UserSecrets { get; set; }

        public DbSet<AuthenticationProvider> AuthenticationProviders { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) :base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<AuthAuthorization>().ToTable("AuthAuthorizations");
            modelBuilder.Entity<AuthScope>().ToTable("AuthScopes");
            modelBuilder.Entity<AuthToken>().ToTable("AuthTokens");

            modelBuilder
                .Entity<AuthenticationProvider>()
                .Property(p => p.Parameters);
        }
    }
}
