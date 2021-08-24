using middlerApp.Auth.Entities;

namespace middlerApp.Api.Providers
{
    public interface IExternalUserFactory
    {
        string GetSubject();

        MUser BuildUser();

        void UpdateClaims(MUser existingUser);

    }
}