using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using middlerApp.Auth.Entities;
using middlerApp.Auth.Stores;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace middlerApp.Auth.Resolvers
{
    public class AuthApplicationStoreResolver: IOpenIddictApplicationStoreResolver
    {
        private readonly TypeResolutionCache _cache;
        private readonly IServiceProvider _provider;

        public AuthApplicationStoreResolver(
            TypeResolutionCache cache,
            IServiceProvider provider)
        {
            _cache = cache;
            _provider = provider;
        }

        /// <summary>
        /// Returns an application store compatible with the specified application type or throws an
        /// <see cref="InvalidOperationException"/> if no store can be built using the specified type.
        /// </summary>
        /// <typeparam name="TApplication">The type of the Application entity.</typeparam>
        /// <returns>An <see cref="IOpenIddictApplicationStore{TApplication}"/>.</returns>
        public IOpenIddictApplicationStore<TApplication> Get<TApplication>() where TApplication : class
        {
            var store = _provider.GetService<IOpenIddictApplicationStore<TApplication>>();
            if (store is not null)
            {
                return store;
            }

            var type = _cache.GetOrAdd(typeof(TApplication), key => typeof(AuthApplicationStore));

            return (IOpenIddictApplicationStore<TApplication>)_provider.GetRequiredService(type);
        }

        // Note: Entity Framework Core resolvers are registered as scoped dependencies as their inner
        // service provider must be able to resolve scoped services (typically, the store they return).
        // To avoid having to declare a static type resolution cache, a special cache service is used
        // here and registered as a singleton dependency so that its content persists beyond the scope.
        public class TypeResolutionCache : ConcurrentDictionary<Type, Type> { }
    }
}
