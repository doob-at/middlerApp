using System;
using OpenIddict.EntityFrameworkCore.Models;

namespace middlerApp.Auth.Entities
{
    public class AuthToken : OpenIddictEntityFrameworkCoreToken<Guid, Client, AuthAuthorization> { }
}