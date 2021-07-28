using System;
using Microsoft.EntityFrameworkCore;
using middlerApp.Auth.Entities;

namespace middlerApp.Auth.Context
{
    public class AuthDbContext: DbContext
    {

        public DbSet<AuthApplication> AuthApplications { get; set; }
        public DbSet<AuthAuthorization> AuthAuthorizations { get; set; }
        public DbSet<AuthScope> AuthScopes { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) :base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.UseOpenIddict<AuthApplication, AuthAuthorization, AuthScope, AuthToken, Guid>();

            modelBuilder.Entity<AuthApplication>().ToTable("AuthApplications");
            modelBuilder.Entity<AuthAuthorization>().ToTable("AuthAuthorizations");
            modelBuilder.Entity<AuthScope>().ToTable("AuthScopes");
            modelBuilder.Entity<AuthToken>().ToTable("AuthTokens");
        }
    }
}
