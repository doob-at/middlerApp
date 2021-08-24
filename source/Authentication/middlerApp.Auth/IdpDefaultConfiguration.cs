using System;
using middlerApp.Auth.Entities;

namespace middlerApp.Auth
{
    internal static class IdpDefaultIdentifier
    {
        public static Guid IdpClient { get; } = new Guid("00000001-0000-0000-0000-000000000000");

        public static Guid Role_IdentityServer_Administrators { get; } = new Guid("00000002-0001-0000-0000-000000000000");



        public static Guid Scope_OpenID_Id { get; } = new Guid("00000003-0001-0000-0000-000000000000");
        public static Guid Scope_Roles_Id { get; } = new Guid("00000003-0002-0000-0000-000000000000");
        public static Guid Scope_MiddlerAppApi_Id { get; } = new Guid("00000003-0003-0000-0000-000000000000");

        public static Guid Resource_MiddlerApi_Id { get; } = new Guid("00000004-0000-0000-0000-000000000000");

    }

    internal static class IdpDefaultResources
    {

        public static MRole Role_Idp_Administrator { get; } = new MRole()
        {
            Id = IdpDefaultIdentifier.Role_IdentityServer_Administrators,
            //BuiltIn = true,
            Name = "Administrators",
            Description = "BUILTIN Administrator Role",
            DisplayName = "IdentityServer Administrators",

        };


        //public static AsnWriter.Scope Scope_OpenID { get; } = new AsnWriter.Scope()
        //{
        //    Id = IdpDefaultIdentifier.Scope_OpenID_Id,
        //    Name = "openid",
        //    Type = ScopeType.IdentityResource,
        //    UserClaims = new List<ScopeClaim>()
        //    {
        //        new ScopeClaim()
        //        {
        //            Id = Guid.NewGuid(),
        //            Type = "name"
        //        }
        //    }
        //};

        //public static AsnWriter.Scope Scope_Roles { get; } = new AsnWriter.Scope()
        //{
        //    Id = IdpDefaultIdentifier.Scope_Roles_Id,
        //    Name = "roles",
        //    UserClaims = new List<ScopeClaim>()
        //    {
        //        new ScopeClaim()
        //        {
        //            Id = Guid.NewGuid(),
        //            Type = "role"
        //        }
        //    },
        //    Type = ScopeType.IdentityResource
        //};

        //public static AsnWriter.Scope Scope_MiddlerAppApi { get; } = new AsnWriter.Scope()
        //{
        //    Id = IdpDefaultIdentifier.Scope_MiddlerAppApi_Id,
        //    Name = "IdentityServerApi",
        //    Type = ScopeType.ApiScope
        //};


        //public static ApiResource Resource_MiddlerApi { get; } = new ApiResource()
        //{
        //    Id = IdpDefaultIdentifier.Resource_MiddlerApi_Id,
        //    DisplayName = "Middler API Resource",
        //    Name = "middlerApi",
        //    Enabled = true,
        //    NonEditable = true,
        //    ShowInDiscoveryDocument = true
        //};

    }
}
