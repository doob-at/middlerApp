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
using middlerApp.Auth.Context;
using middlerApp.Auth.Entities;
using middlerApp.Auth.ExtensionMethods;
using OpenIddict.Abstractions;

namespace middlerApp.Auth.Stores
{
    public class ClientsStore : IOpenIddictApplicationStore<Client>
    {
        private readonly IMemoryCache _cache;
        private readonly AuthDbContext _dbContext;

        public ClientsStore(IMemoryCache cache, AuthDbContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        public async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Clients.AsQueryable().LongCountAsync(cancellationToken);
        }

        public async ValueTask<long> CountAsync<TResult>(Func<IQueryable<Client>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return await query(_dbContext.Clients).LongCountAsync(cancellationToken);

        }

        public async ValueTask CreateAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _dbContext.Add(client);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async ValueTask DeleteAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
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
                    join element in _dbContext.Clients.AsTracking() on authorization.Application!.Id equals element.Id
                    where element.Id!.Equals(client.Id)
                    select authorization).ToListAsync(cancellationToken);

            // Note: due to a bug in Entity Framework Core's query visitor, the tokens can't be
            // filtered using token.Application.Id.Equals(key). To work around this issue,
            // this local method uses an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            Task<List<AuthToken>> ListTokensAsync()
                => (from token in _dbContext.AuthTokens.AsTracking()
                    where token.Authorization == null
                    join element in _dbContext.Clients.AsTracking() on token.Application!.Id equals element.Id
                    where element.Id!.Equals(client.Id)
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

            // Remove all the tokens associated with the client.
            var tokens = await ListTokensAsync();
            foreach (var token in tokens)
            {
                _dbContext.Remove(token);
            }

            _dbContext.Remove(client);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                transaction?.Commit();
            }

            catch (DbUpdateConcurrencyException exception)
            {
                // Reset the state of the entity to prevents future calls to SaveChangesAsync() from failing.
                _dbContext.Entry(client).State = EntityState.Unchanged;

                foreach (var authorization in authorizations)
                {
                    _dbContext.Entry(authorization).State = EntityState.Unchanged;
                }

                foreach (var token in tokens)
                {
                    _dbContext.Entry(token).State = EntityState.Unchanged;
                }

                throw new OpenIddictExceptions.ConcurrencyException("The client was concurrently updated and cannot be persisted in its current state. Reload the client from the database and retry the operation.", exception);
            }

        }

        public async ValueTask<Client> FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return await(from client in _dbContext.Clients.Include(x => x.RedirectUris).AsTracking()
                where client.ClientId == identifier
                select client).FirstOrDefaultAsync(cancellationToken);
        }

        public async ValueTask<Client> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            var key = ConvertIdentifierFromString<Guid>(identifier);

            return await(from client in _dbContext.Clients
                    .Include(x => x.RedirectUris)
                    .Include(x => x.PostLogoutRedirectUris).AsTracking()
                where client.Id!.Equals(key)
                select client).FirstOrDefaultAsync(cancellationToken);
        }

        public IAsyncEnumerable<Client> FindByPostLogoutRedirectUriAsync(string address, CancellationToken cancellationToken)
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

            async IAsyncEnumerable<Client> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
            {
                var applications = (from client in _dbContext.Clients.Include(x => x.PostLogoutRedirectUris).AsTracking()
                    where client.PostLogoutRedirectUris.Any(r => r.PostLogoutRedirectUri == address)//.Contains(address)
                    select client).AsAsyncEnumerable(cancellationToken);

                await foreach (var client in applications)
                {
                    var addresses = await GetPostLogoutRedirectUrisAsync(client, cancellationToken);
                    if (addresses.Contains(address, StringComparer.Ordinal))
                    {
                        yield return client;
                    }
                }
            }
        }

        public IAsyncEnumerable<Client> FindByRedirectUriAsync(string address, CancellationToken cancellationToken)
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

            async IAsyncEnumerable<Client> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
            {
                ///TODO: Collection
                var applications = (from client in _dbContext.Clients.Include(x => x.RedirectUris).AsTracking()
                    where client.RedirectUris.Any(r => r.RedirectUri == address) //.Contains(address)
                    select client).AsAsyncEnumerable(cancellationToken);

                await foreach (var client in applications)
                {
                    var addresses = await GetRedirectUrisAsync(client, cancellationToken);
                    if (addresses.Contains(address, StringComparer.Ordinal))
                    {
                        yield return client;
                    }
                }
            }
        }

        public async ValueTask<TResult> GetAsync<TState, TResult>(Func<IQueryable<Client>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return await query(_dbContext.Clients.Include(x => x.RedirectUris).AsTracking(), state).FirstOrDefaultAsync(cancellationToken);
        }

        public ValueTask<string> GetClientIdAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return new ValueTask<string?>(client.ClientId);
        }

        public ValueTask<string> GetClientSecretAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return new ValueTask<string?>(client.ClientSecret);
        }

        public ValueTask<string> GetClientTypeAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return new ValueTask<string?>(client.Type);
        }

        public ValueTask<string> GetConsentTypeAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return new ValueTask<string?>(client.ConsentType);
        }

        public ValueTask<string> GetDisplayNameAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return new ValueTask<string?>(client.DisplayName);
        }

        public ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrEmpty(client.DisplayNames))
            {
                return new ValueTask<ImmutableDictionary<CultureInfo, string>>(ImmutableDictionary.Create<CultureInfo, string>());
            }

            // Note: parsing the stringified display names is an expensive operation.
            // To mitigate that, the resulting object is stored in the memory cache.
            var key = string.Concat("7762c378-c113-4564-b14b-1402b3949aaa", "\x1e", client.DisplayNames);
            var names = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(client.DisplayNames);
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

        public ValueTask<string> GetIdAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return new ValueTask<string?>(ConvertIdentifierToString(client.Id));
        }

        public ValueTask<ImmutableArray<string>> GetPermissionsAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrEmpty(client.Permissions))
            {
                return new ValueTask<ImmutableArray<string>>(ImmutableArray.Create<string>());
            }

            // Note: parsing the stringified permissions is an expensive operation.
            // To mitigate that, the resulting array is stored in the memory cache.
            var key = string.Concat("0347e0aa-3a26-410a-97e8-a83bdeb21a1f", "\x1e", client.Permissions);
            var permissions = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(client.Permissions);
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

        public ValueTask<ImmutableArray<string>> GetPostLogoutRedirectUrisAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (!client.PostLogoutRedirectUris.Any())
            {
                return new ValueTask<ImmutableArray<string>>(ImmutableArray.Create<string>());
            }

            // Note: parsing the stringified addresses is an expensive operation.
            // To mitigate that, the resulting array is stored in the memory cache.
            var key = string.Concat("fb14dfb9-9216-4b77-bfa9-7e85f8201ff4", "\x1e", client.PostLogoutRedirectUris);
            var addresses = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                //using var document = JsonDocument.Parse(client.PostLogoutRedirectUris);
                //var builder = ImmutableArray.CreateBuilder<string>(document.RootElement.GetArrayLength());

                //foreach (var element in document.RootElement.EnumerateArray())
                //{
                //    var value = element.GetString();
                //    if (string.IsNullOrEmpty(value))
                //    {
                //        continue;
                //    }

                //    builder.Add(value);
                //}

                return client.PostLogoutRedirectUris.Select(r => r.PostLogoutRedirectUri).ToImmutableArray();
            });

            return new ValueTask<ImmutableArray<string>>(addresses);
        }

        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrEmpty(client.Properties))
            {
                return new ValueTask<ImmutableDictionary<string, JsonElement>>(ImmutableDictionary.Create<string, JsonElement>());
            }

            // Note: parsing the stringified properties is an expensive operation.
            // To mitigate that, the resulting object is stored in the memory cache.
            var key = string.Concat("2e3e9680-5654-48d8-a27d-b8bb4f0f1d50", "\x1e", client.Properties);
            var properties = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(client.Properties);
                var builder = ImmutableDictionary.CreateBuilder<string, JsonElement>();

                foreach (var property in document.RootElement.EnumerateObject())
                {
                    builder[property.Name] = property.Value.Clone();
                }

                return builder.ToImmutable();
            });

            return new ValueTask<ImmutableDictionary<string, JsonElement>>(properties);
        }

        public ValueTask<ImmutableArray<string>> GetRedirectUrisAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (!client.RedirectUris.Any())
            {
                return new ValueTask<ImmutableArray<string>>(ImmutableArray.Create<string>());
            }

            // Note: parsing the stringified addresses is an expensive operation.
            // To mitigate that, the resulting array is stored in the memory cache.
            var key = string.Concat("851d6f08-2ee0-4452-bbe5-ab864611ecaa", "\x1e", client.RedirectUris);
            var addresses = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                //using var document = JsonDocument.Parse(client.RedirectUris);
                //var builder = ImmutableArray.CreateBuilder<string>(document.RootElement.GetArrayLength());

                //foreach (var element in document.RootElement.EnumerateArray())
                //{
                //    var value = element.GetString();
                //    if (string.IsNullOrEmpty(value))
                //    {
                //        continue;
                //    }

                //    builder.Add(value);
                //}

                //return builder.ToImmutable();

                return client.RedirectUris.Select(r => r.RedirectUri).ToImmutableArray();
            });

            return new ValueTask<ImmutableArray<string>>(addresses);
        }

        public ValueTask<ImmutableArray<string>> GetRequirementsAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrEmpty(client.Requirements))
            {
                return new ValueTask<ImmutableArray<string>>(ImmutableArray.Create<string>());
            }

            // Note: parsing the stringified requirements is an expensive operation.
            // To mitigate that, the resulting array is stored in the memory cache.
            var key = string.Concat("b4808a89-8969-4512-895f-a909c62a8995", "\x1e", client.Requirements);
            var requirements = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(client.Requirements);
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

        public ValueTask<Client> InstantiateAsync(CancellationToken cancellationToken)
        {
            try
            {
                return new ValueTask<Client>(Activator.CreateInstance<Client>());
            }

            catch (MemberAccessException exception)
            {
                return new ValueTask<Client>(Task.FromException<Client>(
                    new InvalidOperationException("An error occurred while trying to create a new client instance. Make sure that the client entity is not abstract and has a public parameterless constructor or create a custom client store that overrides 'InstantiateAsync()' to use a custom factory.", exception)));
            }
        }

        public IAsyncEnumerable<Client> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            var query = _dbContext.Clients.Include(x => x.RedirectUris).AsQueryable().OrderBy(client => client.Id!).AsTracking();

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

        public IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<Client>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query(_dbContext.Clients.Include(x => x.RedirectUris).AsTracking(), state).AsAsyncEnumerable(cancellationToken);
        }

        public ValueTask SetClientIdAsync(Client client, string identifier, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            client.ClientId = identifier;

            return default;
        }

        public ValueTask SetClientSecretAsync(Client client, string secret, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            client.ClientSecret = secret;

            return default;
        }

        public ValueTask SetClientTypeAsync(Client client, string type, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            client.Type = type;

            return default;
        }

        public ValueTask SetConsentTypeAsync(Client client, string type, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            client.ConsentType = type;

            return default;
        }

        public ValueTask SetDisplayNameAsync(Client client, string name, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            client.DisplayName = name;

            return default;
        }

        public ValueTask SetDisplayNamesAsync(Client client, ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (names is null || names.IsEmpty)
            {
                client.DisplayNames = null;

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

            client.DisplayNames = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public ValueTask SetPermissionsAsync(Client client, ImmutableArray<string> permissions, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (permissions.IsDefaultOrEmpty)
            {
                client.Permissions = null;

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

            client.Permissions = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public ValueTask SetPostLogoutRedirectUrisAsync(Client client, ImmutableArray<string> addresses, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (addresses.IsDefaultOrEmpty)
            {
                client.PostLogoutRedirectUris = null;

                return default;
            }

            client.PostLogoutRedirectUris = addresses.Select(a => new ClientPostLogoutRedirectUri()
            {
                Client = client,
                PostLogoutRedirectUri = a
            }).ToList();

           
            return default;
        }

        public ValueTask SetPropertiesAsync(Client client, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (properties is null || properties.IsEmpty)
            {
                client.Properties = null;

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

            client.Properties = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public ValueTask SetRedirectUrisAsync(Client client, ImmutableArray<string> addresses, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (addresses.IsDefaultOrEmpty)
            {
                client.RedirectUris = null;

                return default;
            }

            client.RedirectUris = addresses.Select(a => new ClientRedirectUri()
            {
                Client = client,
                RedirectUri = a
            }).ToList();
            //using var stream = new MemoryStream();
            //using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            //{
            //    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //    Indented = false
            //});

            //writer.WriteStartArray();

            //foreach (var address in addresses)
            //{
            //    writer.WriteStringValue(address);
            //}

            //writer.WriteEndArray();
            //writer.Flush();

            //client.RedirectUris = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public ValueTask SetRequirementsAsync(Client client, ImmutableArray<string> requirements, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (requirements.IsDefaultOrEmpty)
            {
                client.Requirements = null;

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

            client.Requirements = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public async ValueTask UpdateAsync(Client client, CancellationToken cancellationToken)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _dbContext.Attach(client);

            // Generate a new concurrency token and attach it
            // to the client before persisting the changes.
            client.ConcurrencyToken = Guid.NewGuid().ToString();

            _dbContext.Update(client);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            catch (DbUpdateConcurrencyException exception)
            {
                // Reset the state of the entity to prevents future calls to SaveChangesAsync() from failing.
                _dbContext.Entry(client).State = EntityState.Unchanged;

                throw new OpenIddictExceptions.ConcurrencyException("The client was concurrently updated and cannot be persisted in its current state. Reload the client from the database and retry the operation.", exception);
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
