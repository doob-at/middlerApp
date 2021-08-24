using System;
using System.Collections.Generic;

namespace middlerApp.Auth.Entities
{
    public class Client
    { 
        /// <summary>
        /// Gets the list of the authorizations associated with this application.
        /// </summary>
        public virtual ICollection<AuthAuthorization> Authorizations { get; } = new HashSet<AuthAuthorization>();

        /// <summary>
        /// Gets or sets the client identifier associated with the current application.
        /// </summary>
        public virtual string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret associated with the current application.
        /// Note: depending on the application manager used to create this instance,
        /// this property may be hashed or encrypted for security reasons.
        /// </summary>
        public virtual string? ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the concurrency token.
        /// </summary>
        public virtual string? ConcurrencyToken { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the consent type associated with the current application.
        /// </summary>
        public virtual string? ConsentType { get; set; }

        /// <summary>
        /// Gets or sets the display name associated with the current application.
        /// </summary>
        public virtual string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the localized display names
        /// associated with the current application,
        /// serialized as a JSON object.
        /// </summary>
        public virtual string? DisplayNames { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier associated with the current application.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the permissions associated with the
        /// current application, serialized as a JSON array.
        /// </summary>
        public virtual string? Permissions { get; set; }

        /// <summary>
        /// Gets or sets the logout callback URLs associated with
        /// the current application, serialized as a JSON array.
        /// </summary>
        public virtual ICollection<ClientPostLogoutRedirectUri> PostLogoutRedirectUris { get; set; } = new List<ClientPostLogoutRedirectUri>();

        /// <summary>
        /// Gets or sets the additional properties serialized as a JSON object,
        /// or <c>null</c> if no bag was associated with the current application.
        /// </summary>
        public virtual string? Properties { get; set; }

        /// <summary>
        /// Gets or sets the callback URLs associated with the
        /// current application, serialized as a JSON array.
        /// </summary>
        public virtual ICollection<ClientRedirectUri> RedirectUris { get; set; } = new List<ClientRedirectUri>();

        /// <summary>
        /// Gets or sets the requirements associated with the
        /// current application, serialized as a JSON array.
        /// </summary>
        public virtual string? Requirements { get; set; }

        /// <summary>
        /// Gets the list of the tokens associated with this application.
        /// </summary>
        public virtual ICollection<AuthToken> Tokens { get; } = new HashSet<AuthToken>();

        /// <summary>
        /// Gets or sets the application type associated with the current application.
        /// </summary>
        public virtual string? Type { get; set; }
        public int? AccessTokenLifeTime { get; set; }
        public int? RefreshTokenLifeTime { get; set; }
    }
}