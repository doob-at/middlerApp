using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using doob.Reflectensions;
using Microsoft.EntityFrameworkCore;
using middlerApp.Auth.Context;
using middlerApp.Auth.Entities;
using OpenIddict.Abstractions;

namespace middlerApp.Auth
{
    public class DefaultResourcesManager
    {
        private readonly IdpConfiguration _idpConfiguration;
        public AuthDbContext DbContext { get; }



        public DefaultResourcesManager(AuthDbContext dbContext, IdpConfiguration idpConfiguration)
        {
            _idpConfiguration = idpConfiguration;
            DbContext = dbContext;
        }


        public void EnsureAllResourcesExists()
        {
            Console.WriteLine("EnsureAllResourcesExists");
            EnsureAdminClientExists().GetAwaiter().GetResult();
            EnsureAdminRoleExists().GetAwaiter().GetResult();
        }

        public async Task EnsureAdminClientExists()
        {

            var adminClient = DbContext.Clients
                .Include(c => c.RedirectUris)

                .FirstOrDefault(c => c.Id == IdpDefaultIdentifier.IdpClient);

            if (adminClient != null)
            {
                UpdateAdminClient(adminClient);
                return;
            }


            //await EnsureDefaultScopesExists();

            var client = new Client();
            client.Id = IdpDefaultIdentifier.IdpClient;
            client.ClientId = "scsmportal";
            client.DisplayName = "Scsm Portal UI";
            client.Type = OpenIddictConstants.ClientTypes.Public;
            client.Permissions = Json.Converter.ToJson(new HashSet<string>()
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Logout,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Revocation,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Roles,
                OpenIddictConstants.Permissions.Prefixes.Scope + "dataEventRecords"
            });
            
            SetUris(client);
           

            await DbContext.Clients.AddAsync(client);
            await DbContext.SaveChangesAsync();

        }

        //public async Task EnsureAdminApiExists()
        //{
        //    var apiResource = await
        //        DbContext.ApiResources
        //            .Include(r => r.Scopes)
        //            .Include(r => r.Secrets)
        //            .FirstOrDefaultAsync(r => r.Id == IdpDefaultIdentifier.Resource_MiddlerApi_Id);

        //    if(apiResource != null)
        //        return;

        //    await EnsureDefaultScopesExists();

        //    apiResource = IdpDefaultResources.Resource_MiddlerApi;
        //    apiResource.Secrets = new List<ApiResourceSecret>
        //    {
        //        new ApiResourceSecret()
        //        {
        //            ApiResourceId = apiResource.Id,
        //            Value = "ABC12abc!".Sha256()
        //        }
        //    };
        //    apiResource.Scopes = new List<ApiResourceScope>
        //    {
        //        new ApiResourceScope()
        //        {
        //            ApiResourceId = apiResource.Id,
        //            ScopeId = IdpDefaultResources.Scope_OpenID.Id
        //        },
        //        new ApiResourceScope()
        //        {
        //            ApiResourceId = apiResource.Id,
        //            ScopeId = IdpDefaultResources.Scope_Roles.Id
        //        },
        //        new ApiResourceScope()
        //        {
        //            ApiResourceId = apiResource.Id,
        //            ScopeId = IdpDefaultResources.Scope_MiddlerAppApi.Id
        //        },
        //    };

        //    await DbContext.ApiResources.AddAsync(apiResource);
        //    await DbContext.SaveChangesAsync();

        //}
        private void UpdateAdminClient(Client client)
        {
            SetUris(client);
            DbContext.SaveChanges();
        }


        private void SetUris(Client client)
        {
            SetRedirectUris(client);
            //SetCorsUris(client);
        }

        //private string GenerateIdpRedirectUri()
        //{
        //    var conf = Static.StartUpConfiguration.IdpSettings;
        //    var idpListenIp = IPAddress.Parse(conf.ListeningIP);
        //    var isLocalhost = IPAddress.IsLoopback(idpListenIp) || idpListenIp.ToString() == IPAddress.Any.ToString();

        //    if (isLocalhost)
        //    {
        //        return conf.HttpsPort == 443 ? $"https://localhost" : $"https://localhost:{conf.HttpsPort}";
        //    }
        //    else
        //    {
        //        return conf.HttpsPort == 443
        //            ? $"https://{conf.ListeningIP}"
        //            : $"https://{conf.ListeningIP}:{conf.HttpsPort}";
        //    }
        //}

        //private string GenerateAdminRedirectUri()
        //{
        //    var conf = Static.StartUpConfiguration.AdminSettings;
        //    var idpListenIp = IPAddress.Parse(conf.ListeningIP);
        //    var isLocalhost = IPAddress.IsLoopback(idpListenIp) || idpListenIp.ToString() == IPAddress.Any.ToString();

        //    if (isLocalhost)
        //    {
        //        return conf.HttpsPort == 443 ? $"https://localhost" : $"https://localhost:{conf.HttpsPort}";
        //    }
        //    else
        //    {
        //        return conf.HttpsPort == 443
        //            ? $"https://{conf.ListeningIP}"
        //            : $"https://{conf.ListeningIP}:{conf.HttpsPort}";
        //    }
        //}

        private void SetRedirectUris(Client client)
        {
            var uris = client.RedirectUris.Select(u => u.RedirectUri).ToList();

            foreach (var uri in _idpConfiguration.AdminUIRedirectUris)
            {
                if (!uris.Contains(uri))
                {
                    client.RedirectUris.Add(new ClientRedirectUri()
                    {
                        ClientId = client.Id,
                        RedirectUri = uri
                    });
                }
            }

            foreach (var uri in _idpConfiguration.AdminUIRedirectUris)
            {
                if (!uris.Contains(uri))
                {
                    client.PostLogoutRedirectUris.Add(new ClientPostLogoutRedirectUri()
                    {
                        ClientId = client.Id,
                        PostLogoutRedirectUri = uri
                    });
                }
            }

        }

        //private void SetCorsUris(Client client)
        //{
        //    var corsUris = client.AllowedCorsOrigins.Select(u => u.Origin).ToList();

        //    foreach (var uri in _idpConfiguration.AdminUIRedirectUris)
        //    {
        //        if (!corsUris.Contains(uri))
        //        {
        //            client.AllowedCorsOrigins.Add(new ClientCorsOrigin
        //            {
        //                ClientId = client.Id,
        //                Origin = uri
        //            });
        //        }
        //    }



        //}


        public async Task<MRole> EnsureAdminRoleExists()
        {
            var adminRole = await DbContext
                .Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Id == IdpDefaultIdentifier.Role_IdentityServer_Administrators);
            if (adminRole == null)
            {
                await DbContext.Roles.AddAsync(IdpDefaultResources.Role_Idp_Administrator);
                await DbContext.SaveChangesAsync();
            }
            else
            {
                return adminRole;
            }

            return await EnsureAdminRoleExists();
        }



        public async Task<bool> AtLeastOneAdminUserExistsAsync()
        {
            var adminRole = await EnsureAdminRoleExists();
            return adminRole.Users.Any();

        }

        //public async Task EnsureDefaultScopesExists()
        //{

        //    await EnsureScopeExists(IdpDefaultResources.Scope_OpenID);
        //    await EnsureScopeExists(IdpDefaultResources.Scope_Roles);
        //    await EnsureScopeExists(IdpDefaultResources.Scope_MiddlerAppApi);
        //    await DbContext.SaveChangesAsync();

        //}

        //private async Task EnsureScopeExists(Scope scope)
        //{
        //    var foundScope = await DbContext.Scopes.FirstOrDefaultAsync(s => s.Name == scope.Name);
        //    if (foundScope == null)
        //    {
        //        await DbContext.Scopes.AddAsync(scope);
        //    }
        //}


    }
}
