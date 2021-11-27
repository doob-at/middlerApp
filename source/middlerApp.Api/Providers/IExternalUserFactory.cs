using System;
using middlerApp.Auth.Entities;

namespace middlerApp.Api.Providers
{
    public interface IExternalUserFactory
    {
        Guid GetId();

        MUser BuildUser();

        void UpdateClaims(MUser existingUser);

    }
}