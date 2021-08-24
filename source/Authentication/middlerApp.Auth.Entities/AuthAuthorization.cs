using System;
using OpenIddict.EntityFrameworkCore.Models;

namespace middlerApp.Auth.Entities
{
    public class AuthAuthorization : OpenIddictEntityFrameworkCoreAuthorization<Guid, Client, AuthToken> { }
}