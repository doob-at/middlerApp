using System.Security.Claims;
using middlerApp.Auth.Entities;

namespace middlerApp.Api.Providers
{
    public interface IAuthHandler
    {

        void Register(AuthenticationProvider provider);

        void UnRegister();

        IExternalUserFactory GetUserFactory(ClaimsPrincipal claimsPrincipal);

    }
}