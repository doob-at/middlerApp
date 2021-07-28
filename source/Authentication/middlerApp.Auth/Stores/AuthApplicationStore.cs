using middlerApp.Auth.Context;
using middlerApp.Auth.Entities;
using OpenIddict.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using middlerApp.Auth.ExtensionMethods;

namespace middlerApp.Auth
{
    public class AuthApplicationStore : IOpenIddictApplicationStore<AuthApplication>
    {
        private readonly IMemoryCache _cache;
        private readonly AuthDbContext _dbContext;

        public AuthApplicationStore(IMemoryCache cache, AuthDbContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        public async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.AuthApplications.AsQueryable().LongCountAsync(cancellationToken);
        }

        public async ValueTask<long> CountAsync<TResult>(Func<IQueryable<AuthApplication>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return await query(_dbContext.AuthApplications).LongCountAsync(cancellationToken);

        }

        public async ValueTask CreateAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            _dbContext.Add(application);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async ValueTask DeleteAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            async ValueTask<IDbContextTransaction?> CreateTransactionAsync()
            {
                // Note: transactions that specify an explicit isolation level are only supported by
                // relational providers and trying to use them with a different provider results in
                // an invalid operation exception being thrown at runtime. To prevent that, a manual
                // check is made to ensure the underlying transaction manager is relational.
                var manager = _dbContext.Database.GetService<IDbContextTransactionManager>();
                if (manager is IRelationalTransactionManager)
                {
                    try
                    {
                        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                    }

                    catch
                    {
                        return null;
                    }
                }

                return null;
            }

            // Note: due to a bug in Entity Framework Core's query visitor, the authorizations can't be
            // filtered using authorization.Application.Id.Equals(key). To work around this issue,
            // this local method uses an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            Task<List<AuthAuthorization>> ListAuthorizationsAsync()
                => (from authorization in _dbContext.AuthAuthorizations.Include(authorization => authorization.Tokens).AsTracking()
                    join element in _dbContext.AuthApplications.AsTracking() on authorization.Application!.Id equals element.Id
                    where element.Id!.Equals(application.Id)
                    select authorization).ToListAsync(cancellationToken);

            // Note: due to a bug in Entity Framework Core's query visitor, the tokens can't be
            // filtered using token.Application.Id.Equals(key). To work around this issue,
            // this local method uses an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            Task<List<AuthToken>> ListTokensAsync()
                => (from token in _dbContext.AuthTokens.AsTracking()
                    where token.Authorization == null
                    join element in _dbContext.AuthApplications.AsTracking() on token.Application!.Id equals element.Id
                    where element.Id!.Equals(application.Id)
                    select token).ToListAsync(cancellationToken);

            // To prevent an SQL exception from being thrown if a new associated entity is
            // created after the existing entries have been listed, the following logic is
            // executed in a serializable transaction, that will lock the affected tables.
            using var transaction = await CreateTransactionAsync();

            var authorizations = await ListAuthorizationsAsync();
            foreach (var authorization in authorizations)
            {
                foreach (var token in authorization.Tokens)
                {
                    _dbContext.Remove(token);
                }

                _dbContext.Remove(authorization);
            }

            // Remove all the tokens associated with the application.
            var tokens = await ListTokensAsync();
            foreach (var token in tokens)
            {
                _dbContext.Remove(token);
            }

            _dbContext.Remove(application);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                transaction?.Commit();
            }

            catch (DbUpdateConcurrencyException exception)
            {
                // Reset the state of the entity to prevents future calls to SaveChangesAsync() from failing.
                _dbContext.Entry(application).State = EntityState.Unchanged;

                foreach (var authorization in authorizations)
                {
                    _dbContext.Entry(authorization).State = EntityState.Unchanged;
                }

                foreach (var token in tokens)
                {
                    _dbContext.Entry(token).State = EntityState.Unchanged;
                }

                throw new OpenIddictExceptions.ConcurrencyException("The application was concurrently updated and cannot be persisted in its current state. Reload the application from the database and retry the operation.", exception);
            }

        }

        public async ValueTask<AuthApplication> FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return await(from application in _dbContext.AuthApplications.AsTracking()
                where application.ClientId == identifier
                select application).FirstOrDefaultAsync(cancellationToken);
        }

        public async ValueTask<AuthApplication> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            var key = ConvertIdentifierFromString<Guid>(identifier);

            return await(from application in _dbContext.AuthApplications.AsTracking()
                where application.Id!.Equals(key)
                select application).FirstOrDefaultAsync(cancellationToken);
        }

        public IAsyncEnumerable<AuthApplication> FindByPostLogoutRedirectUriAsync(string address, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException("The address cannot be null or empty.", nameof(address));
            }

            // To optimize the efficiency of the query a bit, only applications whose stringified
            // PostLogoutRedirectUris contains the specified URL are returned. Once the applications
            // are retrieved, a second pass is made to ensure only valid elements are returned.
            // Implementers that use this method in a hot path may want to override this method
            // to use SQL Server 2016 functions like JSON_VALUE to make the query more efficient.

            return ExecuteAsync(cancellationToken);

            async IAsyncEnumerable<AuthApplication> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
            {
                var applications = (from application in _dbContext.AuthApplications.AsTracking()
                    where application.PostLogoutRedirectUris!.Contains(address)
                    select application).AsAsyncEnumerable(cancellationToken);

                await foreach (var application in applications)
                {
                    var addresses = await GetPostLogoutRedirectUrisAsync(application, cancellationToken);
                    if (addresses.Contains(address, StringComparer.Ordinal))
                    {
                        yield return application;
                    }
                }
            }
        }

        public IAsyncEnumerable<AuthApplication> FindByRedirectUriAsync(string address, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException("The address cannot be null or empty.", nameof(address));
            }

            // To optimize the efficiency of the query a bit, only applications whose stringified
            // RedirectUris property contains the specified URL are returned. Once the applications
            // are retrieved, a second pass is made to ensure only valid elements are returned.
            // Implementers that use this method in a hot path may want to override this method
            // to use SQL Server 2016 functions like JSON_VALUE to make the query more efficient.

            return ExecuteAsync(cancellationToken);

            async IAsyncEnumerable<AuthApplication> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
            {
                var applications = (from application in _dbContext.AuthApplications.AsTracking()
                    where application.RedirectUris!.Contains(address)
                    select application).AsAsyncEnumerable(cancellationToken);

                await foreach (var application in applications)
                {
                    var addresses = await GetRedirectUrisAsync(application, cancellationToken);
                    if (addresses.Contains(address, StringComparer.Ordinal))
                    {
                        yield return application;
                    }
                }
            }
        }

        public async ValueTask<TResult> GetAsync<TState, TResult>(Func<IQueryable<AuthApplication>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return await query(_dbContext.AuthApplications.AsTracking(), state).FirstOrDefaultAsync(cancellationToken);
        }

        public ValueTask<string> GetClientIdAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string?>(application.ClientId);
        }

        public ValueTask<string> GetClientSecretAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string?>(application.ClientSecret);
        }

        public ValueTask<string> GetClientTypeAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string?>(application.Type);
        }

        public ValueTask<string> GetConsentTypeAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string?>(application.ConsentType);
        }

        public ValueTask<string> GetDisplayNameAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string?>(application.DisplayName);
        }

        public ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (string.IsNullOrEmpty(application.DisplayNames))
            {
                return new ValueTask<ImmutableDictionary<CultureInfo, string>>(ImmutableDictionary.Create<CultureInfo, string>());
            }

            // Note: parsing the stringified display names is an expensive operation.
            // To mitigate that, the resulting object is stored in the memory cache.
            var key = string.Concat("7762c378-c113-4564-b14b-1402b3949aaa", "\x1e", application.DisplayNames);
            var names = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(application.DisplayNames);
                var builder = ImmutableDictionary.CreateBuilder<CultureInfo, string>();

                foreach (var property in document.RootElement.EnumerateObject())
                {
                    var value = property.Value.GetString();
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    builder[CultureInfo.GetCultureInfo(property.Name)] = value;
                }

                return builder.ToImmutable();
            });

            return new ValueTask<ImmutableDictionary<CultureInfo, string>>(names);
        }

        public ValueTask<string> GetIdAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string?>(ConvertIdentifierToString(application.Id));
        }

        public ValueTask<ImmutableArray<string>> GetPermissionsAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (string.IsNullOrEmpty(application.Permissions))
            {
                return new ValueTask<ImmutableArray<string>>(ImmutableArray.Create<string>());
            }

            // Note: parsing the stringified permissions is an expensive operation.
            // To mitigate that, the resulting array is stored in the memory cache.
            var key = string.Concat("0347e0aa-3a26-410a-97e8-a83bdeb21a1f", "\x1e", application.Permissions);
            var permissions = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(application.Permissions);
                var builder = ImmutableArray.CreateBuilder<string>(document.RootElement.GetArrayLength());

                foreach (var element in document.RootElement.EnumerateArray())
                {
                    var value = element.GetString();
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    builder.Add(value);
                }

                return builder.ToImmutable();
            });

            return new ValueTask<ImmutableArray<string>>(permissions);
        }

        public ValueTask<ImmutableArray<string>> GetPostLogoutRedirectUrisAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (string.IsNullOrEmpty(application.PostLogoutRedirectUris))
            {
                return new ValueTask<ImmutableArray<string>>(ImmutableArray.Create<string>());
            }

            // Note: parsing the stringified addresses is an expensive operation.
            // To mitigate that, the resulting array is stored in the memory cache.
            var key = string.Concat("fb14dfb9-9216-4b77-bfa9-7e85f8201ff4", "\x1e", application.PostLogoutRedirectUris);
            var addresses = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(application.PostLogoutRedirectUris);
                var builder = ImmutableArray.CreateBuilder<string>(document.RootElement.GetArrayLength());

                foreach (var element in document.RootElement.EnumerateArray())
                {
                    var value = element.GetString();
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    builder.Add(value);
                }

                return builder.ToImmutable();
            });

            return new ValueTask<ImmutableArray<string>>(addresses);
        }

        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (string.IsNullOrEmpty(application.Properties))
            {
                return new ValueTask<ImmutableDictionary<string, JsonElement>>(ImmutableDictionary.Create<string, JsonElement>());
            }

            // Note: parsing the stringified properties is an expensive operation.
            // To mitigate that, the resulting object is stored in the memory cache.
            var key = string.Concat("2e3e9680-5654-48d8-a27d-b8bb4f0f1d50", "\x1e", application.Properties);
            var properties = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(application.Properties);
                var builder = ImmutableDictionary.CreateBuilder<string, JsonElement>();

                foreach (var property in document.RootElement.EnumerateObject())
                {
                    builder[property.Name] = property.Value.Clone();
                }

                return builder.ToImmutable();
            });

            return new ValueTask<ImmutableDictionary<string, JsonElement>>(properties);
        }

        public ValueTask<ImmutableArray<string>> GetRedirectUrisAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (string.IsNullOrEmpty(application.RedirectUris))
            {
                return new ValueTask<ImmutableArray<string>>(ImmutableArray.Create<string>());
            }

            // Note: parsing the stringified addresses is an expensive operation.
            // To mitigate that, the resulting array is stored in the memory cache.
            var key = string.Concat("851d6f08-2ee0-4452-bbe5-ab864611ecaa", "\x1e", application.RedirectUris);
            var addresses = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(application.RedirectUris);
                var builder = ImmutableArray.CreateBuilder<string>(document.RootElement.GetArrayLength());

                foreach (var element in document.RootElement.EnumerateArray())
                {
                    var value = element.GetString();
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    builder.Add(value);
                }

                return builder.ToImmutable();
            });

            return new ValueTask<ImmutableArray<string>>(addresses);
        }

        public ValueTask<ImmutableArray<string>> GetRequirementsAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (string.IsNullOrEmpty(application.Requirements))
            {
                return new ValueTask<ImmutableArray<string>>(ImmutableArray.Create<string>());
            }

            // Note: parsing the stringified requirements is an expensive operation.
            // To mitigate that, the resulting array is stored in the memory cache.
            var key = string.Concat("b4808a89-8969-4512-895f-a909c62a8995", "\x1e", application.Requirements);
            var requirements = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(application.Requirements);
                var builder = ImmutableArray.CreateBuilder<string>(document.RootElement.GetArrayLength());

                foreach (var element in document.RootElement.EnumerateArray())
                {
                    var value = element.GetString();
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    builder.Add(value);
                }

                return builder.ToImmutable();
            });

            return new ValueTask<ImmutableArray<string>>(requirements);
        }

        public ValueTask<AuthApplication> InstantiateAsync(CancellationToken cancellationToken)
        {
            try
            {
                return new ValueTask<AuthApplication>(Activator.CreateInstance<AuthApplication>());
            }

            catch (MemberAccessException exception)
            {
                return new ValueTask<AuthApplication>(Task.FromException<AuthApplication>(
                    new InvalidOperationException("An error occurred while trying to create a new application instance. Make sure that the application entity is not abstract and has a public parameterless constructor or create a custom application store that overrides 'InstantiateAsync()' to use a custom factory.", exception)));
            }
        }

        public IAsyncEnumerable<AuthApplication> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            var query = _dbContext.AuthApplications.AsQueryable().OrderBy(application => application.Id!).AsTracking();

            if (offset.HasValue)
            {
                query = query.Skip(offset.Value);
            }

            if (count.HasValue)
            {
                query = query.Take(count.Value);
            }

            return query.AsAsyncEnumerable(cancellationToken);
        }

        public IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<AuthApplication>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query(_dbContext.AuthApplications.AsTracking(), state).AsAsyncEnumerable(cancellationToken);
        }

        public ValueTask SetClientIdAsync(AuthApplication application, string identifier, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.ClientId = identifier;

            return default;
        }

        public ValueTask SetClientSecretAsync(AuthApplication application, string secret, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.ClientSecret = secret;

            return default;
        }

        public ValueTask SetClientTypeAsync(AuthApplication application, string type, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.Type = type;

            return default;
        }

        public ValueTask SetConsentTypeAsync(AuthApplication application, string type, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.ConsentType = type;

            return default;
        }

        public ValueTask SetDisplayNameAsync(AuthApplication application, string name, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.DisplayName = name;

            return default;
        }

        public ValueTask SetDisplayNamesAsync(AuthApplication application, ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (names is null || names.IsEmpty)
            {
                application.DisplayNames = null;

                return default;
            }

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = false
            });

            writer.WriteStartObject();

            foreach (var pair in names)
            {
                writer.WritePropertyName(pair.Key.Name);
                writer.WriteStringValue(pair.Value);
            }

            writer.WriteEndObject();
            writer.Flush();

            application.DisplayNames = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public ValueTask SetPermissionsAsync(AuthApplication application, ImmutableArray<string> permissions, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (permissions.IsDefaultOrEmpty)
            {
                application.Permissions = null;

                return default;
            }

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = false
            });

            writer.WriteStartArray();

            foreach (var permission in permissions)
            {
                writer.WriteStringValue(permission);
            }

            writer.WriteEndArray();
            writer.Flush();

            application.Permissions = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public ValueTask SetPostLogoutRedirectUrisAsync(AuthApplication application, ImmutableArray<string> addresses, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (addresses.IsDefaultOrEmpty)
            {
                application.PostLogoutRedirectUris = null;

                return default;
            }

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = false
            });

            writer.WriteStartArray();

            foreach (var address in addresses)
            {
                writer.WriteStringValue(address);
            }

            writer.WriteEndArray();
            writer.Flush();

            application.PostLogoutRedirectUris = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public ValueTask SetPropertiesAsync(AuthApplication application, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (properties is null || properties.IsEmpty)
            {
                application.Properties = null;

                return default;
            }

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = false
            });

            writer.WriteStartObject();

            foreach (var property in properties)
            {
                writer.WritePropertyName(property.Key);
                property.Value.WriteTo(writer);
            }

            writer.WriteEndObject();
            writer.Flush();

            application.Properties = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public ValueTask SetRedirectUrisAsync(AuthApplication application, ImmutableArray<string> addresses, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (addresses.IsDefaultOrEmpty)
            {
                application.RedirectUris = null;

                return default;
            }

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = false
            });

            writer.WriteStartArray();

            foreach (var address in addresses)
            {
                writer.WriteStringValue(address);
            }

            writer.WriteEndArray();
            writer.Flush();

            application.RedirectUris = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public ValueTask SetRequirementsAsync(AuthApplication application, ImmutableArray<string> requirements, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (requirements.IsDefaultOrEmpty)
            {
                application.Requirements = null;

                return default;
            }

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = false
            });

            writer.WriteStartArray();

            foreach (var requirement in requirements)
            {
                writer.WriteStringValue(requirement);
            }

            writer.WriteEndArray();
            writer.Flush();

            application.Requirements = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public async ValueTask UpdateAsync(AuthApplication application, CancellationToken cancellationToken)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            _dbContext.Attach(application);

            // Generate a new concurrency token and attach it
            // to the application before persisting the changes.
            application.ConcurrencyToken = Guid.NewGuid().ToString();

            _dbContext.Update(application);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            catch (DbUpdateConcurrencyException exception)
            {
                // Reset the state of the entity to prevents future calls to SaveChangesAsync() from failing.
                _dbContext.Entry(application).State = EntityState.Unchanged;

                throw new OpenIddictExceptions.ConcurrencyException("The application was concurrently updated and cannot be persisted in its current state. Reload the application from the database and retry the operation.", exception);
            }
        }


        /// <summary>
        /// Converts the provided identifier to a strongly typed key object.
        /// </summary>
        /// <param name="identifier">The identifier to convert.</param>
        /// <returns>An instance of <typeparamref name="TKey"/> representing the provided identifier.</returns>
        public virtual TKey? ConvertIdentifierFromString<TKey>(string? identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return default;
            }

            return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(identifier);
        }

        public virtual string? ConvertIdentifierToString<TKey>(TKey? identifier)
        {
            if (Equals(identifier, default(TKey)))
            {
                return null;
            }

            return TypeDescriptor.GetConverter(typeof(TKey)).ConvertToInvariantString(identifier);
        }
    }
}
