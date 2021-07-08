using System;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace middlerApp.Auth
{
    public class CustomValidateResourceOwnerCredentialsParameters : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateTokenRequestContext>
    {
        /// <summary>
        /// Gets the default descriptor definition assigned to this handler.
        /// </summary>
        public static OpenIddictServerHandlerDescriptor Descriptor { get; }
            = OpenIddictServerHandlerDescriptor.CreateBuilder<OpenIddictServerEvents.ValidateTokenRequestContext>()
                .UseSingletonHandler<CustomValidateResourceOwnerCredentialsParameters>()
                .SetOrder(OpenIddictServerHandlers.Exchange.ValidateRefreshTokenParameter.Descriptor.Order + 900)
                .SetType(OpenIddictServerHandlerType.Custom)
                .Build();

        /// <inheritdoc/>
        public ValueTask HandleAsync(OpenIddictServerEvents.ValidateTokenRequestContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
          
            if (context.Request.IsPasswordGrantType() && (string.IsNullOrEmpty(context.Request.Username) ||
                                                          string.IsNullOrEmpty(context.Request.Password)))
            {
                
                if (string.IsNullOrEmpty(context.Request.Username))
                {
                    context.Request.Username = $"{Environment.UserDomainName}\\";
                    context.Request.Password = "empty";
                    return default;
                }
                
               
                //context.Logger.LogError(SR.GetResourceString(SR.ID6079));

                //context.Reject(
                //    error: OpenIddictConstants.Errors.InvalidRequest,
                //    description: SR.FormatID2059(OpenIddictConstants.Parameters.Username, OpenIddictConstants.Parameters.Password),
                //    uri: SR.FormatID8000(SR.ID2059));

                //return default;
            }

            return default;
        }
    }
}
