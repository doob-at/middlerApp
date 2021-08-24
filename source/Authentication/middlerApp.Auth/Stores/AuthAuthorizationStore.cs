using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
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
    public class AuthAuthorizationStore: IOpenIddictAuthorizationStore<AuthAuthorization>
    {
        private readonly IMemoryCache _cache;
        private readonly AuthDbContext _dbContext;

        public AuthAuthorizationStore(IMemoryCache cache, AuthDbContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        private DbSet<Client> Applications => _dbContext.Set<Client>();
        private DbSet<AuthAuthorization> Authorizations => _dbContext.Set<AuthAuthorization>();
        private DbSet<AuthToken> Tokens => _dbContext.Set<AuthToken>();

        public async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            return await Authorizations.AsQueryable().LongCountAsync(cancellationToken);
        }

        public async ValueTask<long> CountAsync<TResult>(Func<IQueryable<AuthAuthorization>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return await query(Authorizations).LongCountAsync(cancellationToken);
        }

        public async ValueTask CreateAsync(AuthAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            _dbContext.Add(authorization);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async ValueTask DeleteAsync(AuthAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
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
                        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable,
                            cancellationToken);
                    }

                    catch
                    {
                        return null;
                    }
                }

                return null;
            }
            Task<List<AuthToken>> ListTokensAsync()
                => (from token in Tokens.AsTracking()
                    join element in Authorizations.AsTracking() on token.Authorization!.Id equals element.Id
                    where element.Id!.Equals(authorization.Id)
                    select token).ToListAsync(cancellationToken);

            // To prevent an SQL exception from being thrown if a new associated entity is
            // created after the existing entries have been listed, the following logic is
            // executed in a serializable transaction, that will lock the affected tables.
            using var transaction = await CreateTransactionAsync();

            // Remove all the tokens associated with the authorization.
            var tokens = await ListTokensAsync();
            foreach (var token in tokens)
            {
                _dbContext.Remove(token);
            }

            _dbContext.Remove(authorization);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                transaction?.Commit();
            }

            catch (DbUpdateConcurrencyException exception)
            {
                // Reset the state of the entity to prevents future calls to SaveChangesAsync() from failing.
                _dbContext.Entry(authorization).State = EntityState.Unchanged;

                foreach (var token in tokens)
                {
                    _dbContext.Entry(token).State = EntityState.Unchanged;
                }

                throw new OpenIddictExceptions.ConcurrencyException("The authorization was concurrently updated and cannot be persisted in its current state. Reload the authorization from the database and retry the operation.", exception);
            }
        }



        public IAsyncEnumerable<AuthAuthorization> FindAsync(string subject, string client, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.", nameof(client));
            }

            // Note: due to a bug in Entity Framework Core's query visitor, the authorizations can't be
            // filtered using authorization.Application.Id.Equals(key). To work around this issue,
            // this method is overriden to use an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            var key = ConvertIdentifierFromString<Guid>(client);

            return (from authorization in Authorizations.Include(authorization => authorization.Application).AsTracking()
                where authorization.Subject == subject
                join application in Applications.AsTracking() on authorization.Application!.Id equals application.Id
                where application.Id!.Equals(key)
                select authorization).AsAsyncEnumerable(cancellationToken);
        }

        public IAsyncEnumerable<AuthAuthorization> FindAsync(string subject, string client, string status, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.", nameof(client));
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(status));
            }

            // Note: due to a bug in Entity Framework Core's query visitor, the authorizations can't be
            // filtered using authorization.Application.Id.Equals(key). To work around this issue,
            // this method is overriden to use an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            var key = ConvertIdentifierFromString<Guid>(client);

            return (from authorization in Authorizations.Include(authorization => authorization.Application).AsTracking()
                where authorization.Subject == subject && authorization.Status == status
                join application in Applications.AsTracking() on authorization.Application!.Id equals application.Id
                where application.Id!.Equals(key)
                select authorization).AsAsyncEnumerable(cancellationToken);
        }

        public IAsyncEnumerable<AuthAuthorization> FindAsync(string subject, string client, string status, string type,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.", nameof(client));
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(status));
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("The type cannot be null or empty.", nameof(type));
            }

            // Note: due to a bug in Entity Framework Core's query visitor, the authorizations can't be
            // filtered using authorization.Application.Id.Equals(key). To work around this issue,
            // this method is overriden to use an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            var key = ConvertIdentifierFromString<Guid>(client);

            return (from authorization in Authorizations.Include(authorization => authorization.Application).AsTracking()
                where authorization.Subject == subject &&
                      authorization.Status == status &&
                      authorization.Type == type
                join application in Applications.AsTracking() on authorization.Application!.Id equals application.Id
                where application.Id!.Equals(key)
                select authorization).AsAsyncEnumerable(cancellationToken);
        }

        public IAsyncEnumerable<AuthAuthorization> FindAsync(string subject, string client, string status, string type, ImmutableArray<string> scopes,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.", nameof(client));
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(status));
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("The type cannot be null or empty.", nameof(type));
            }


            return ExecuteAsync(cancellationToken);

            async IAsyncEnumerable<AuthAuthorization> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
            {
                // Note: due to a bug in Entity Framework Core's query visitor, the authorizations can't be
                // filtered using authorization.Application.Id.Equals(key). To work around this issue,
                // this method is overriden to use an explicit join before applying the equality check.
                // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

                var key = ConvertIdentifierFromString<Guid>(client);

                var authorizations = (from authorization in Authorizations.Include(authorization => authorization.Application).AsTracking()
                                      where authorization.Subject == subject &&
                                            authorization.Status == status &&
                                            authorization.Type == type
                                      join application in Applications.AsTracking() on authorization.Application!.Id equals application.Id
                                      where application.Id!.Equals(key)
                                      select authorization).AsAsyncEnumerable(cancellationToken);

                await foreach (var authorization in authorizations)
                {
                    if (new HashSet<string>(await GetScopesAsync(authorization, cancellationToken), StringComparer.Ordinal).IsSupersetOf(scopes))
                    {
                        yield return authorization;
                    }
                }
            }
        }

        public IAsyncEnumerable<AuthAuthorization> FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            // Note: due to a bug in Entity Framework Core's query visitor, the authorizations can't be
            // filtered using authorization.Application.Id.Equals(key). To work around this issue,
            // this method is overriden to use an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            var key = ConvertIdentifierFromString<Guid>(identifier);

            return (from authorization in Authorizations.Include(authorization => authorization.Application).AsTracking()
                join application in Applications.AsTracking() on authorization.Application!.Id equals application.Id
                where application.Id!.Equals(identifier)
                select authorization).AsAsyncEnumerable(cancellationToken);
        }

        public async ValueTask<AuthAuthorization?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            var key = ConvertIdentifierFromString<Guid>(identifier);

            return await (from authorization in Authorizations.Include(authorization => authorization.Application).AsTracking()
                where authorization.Id!.Equals(key)
                select authorization).FirstOrDefaultAsync(cancellationToken);
        }

        public IAsyncEnumerable<AuthAuthorization> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            return (from authorization in Authorizations.Include(authorization => authorization.Application).AsTracking()
                where authorization.Subject == subject
                select authorization).AsAsyncEnumerable(cancellationToken);
        }

        public async ValueTask<string?> GetApplicationIdAsync(AuthAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            // If the application is not attached to the authorization, try to load it manually.
            if (authorization.Application is null)
            {
                var reference = _dbContext.Entry(authorization).Reference(entry => entry.Application);
                if (reference.EntityEntry.State == EntityState.Detached)
                {
                    return null;
                }

                await reference.LoadAsync(cancellationToken);
            }

            if (authorization.Application is null)
            {
                return null;
            }

            return ConvertIdentifierToString(authorization.Application.Id);
        }

        public async ValueTask<TResult> GetAsync<TState, TResult>(Func<IQueryable<AuthAuthorization>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return await query(
                Authorizations.Include(authorization => authorization.Application)
                    .AsTracking(), state).FirstOrDefaultAsync(cancellationToken);
        }

        public ValueTask<DateTimeOffset?> GetCreationDateAsync(AuthAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            if (authorization.CreationDate == null)
            {
                return new ValueTask<DateTimeOffset?>(result: null);
            }

            return new ValueTask<DateTimeOffset?>(DateTime.SpecifyKind(authorization.CreationDate.Value, DateTimeKind.Utc));
        }

        public ValueTask<string?> GetIdAsync(AuthAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return new ValueTask<string?>(ConvertIdentifierToString(authorization.Id));
        }

        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(AuthAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            if (string.IsNullOrEmpty(authorization.Properties))
            {
                return new ValueTask<ImmutableDictionary<string, JsonElement>>(ImmutableDictionary.Create<string, JsonElement>());
            }

            // Note: parsing the stringified properties is an expensive operation.
            // To mitigate that, the resulting object is stored in the memory cache.
            var key = string.Concat("68056e1a-dbcf-412b-9a6a-d791c7dbe726", "\x1e", authorization.Properties);
            var properties = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(authorization.Properties);
                var builder = ImmutableDictionary.CreateBuilder<string, JsonElement>();

                foreach (var property in document.RootElement.EnumerateObject())
                {
                    builder[property.Name] = property.Value.Clone();
                }

                return builder.ToImmutable();
            });

            return new ValueTask<ImmutableDictionary<string, JsonElement>>(properties);
        }

        public ValueTask<ImmutableArray<string>> GetScopesAsync(AuthAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            if (string.IsNullOrEmpty(authorization.Scopes))
            {
                return new ValueTask<ImmutableArray<string>>(ImmutableArray.Create<string>());
            }

            // Note: parsing the stringified scopes is an expensive operation.
            // To mitigate that, the resulting array is stored in the memory cache.
            var key = string.Concat("2ba4ab0f-e2ec-4d48-b3bd-28e2bb660c75", "\x1e", authorization.Scopes);
            var scopes = _cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(authorization.Scopes);
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

            return new ValueTask<ImmutableArray<string>>(scopes);
        }

        public ValueTask<string?> GetStatusAsync(AuthAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return new ValueTask<string?>(authorization.Status);
        }

        public ValueTask<string?> GetSubjectAsync(AuthAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return new ValueTask<string?>(authorization.Subject);
        }

        public ValueTask<string?> GetTypeAsync(AuthAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return new ValueTask<string?>(authorization.Type);
        }

        public ValueTask<AuthAuthorization> InstantiateAsync(CancellationToken cancellationToken)
        {
            try
            {
                return new ValueTask<AuthAuthorization>(Activator.CreateInstance<AuthAuthorization>());
            }

            catch (MemberAccessException exception)
            {
                return new ValueTask<AuthAuthorization>(Task.FromException<AuthAuthorization>(
                    new InvalidOperationException("An error occurred while trying to create a new authorization instance. Make sure that the authorization entity is not abstract and has a public parameterless constructor or create a custom authorization store that overrides 'InstantiateAsync()' to use a custom factory.", exception)));
            }
        }

        public IAsyncEnumerable<AuthAuthorization> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            var query = Authorizations.Include(authorization => authorization.Application)
                .OrderBy(authorization => authorization.Id!)
                .AsTracking();

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

        public IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<AuthAuthorization>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query(
                Authorizations.Include(authorization => authorization.Application)
                    .AsTracking(), state).AsAsyncEnumerable(cancellationToken);
        }

        public async ValueTask PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
        {
            // Note: Entity Framework Core doesn't support set-based deletes, which prevents removing
            // entities in a single command without having to retrieve and materialize them first.
            // To work around this limitation, entities are manually listed and deleted using a batch logic.

            List<Exception>? exceptions = null;

            async ValueTask<IDbContextTransaction?> CreateTransactionAsync()
            {
                // Note: transactions that specify an explicit isolation level are only supported by
                // relational providers and trying to use them with a different provider results in
                // an invalid operation exception being thrown at runtime. To prevent that, a manual
                // check is made to ensure the underlying transaction manager is relational.
                var manager = _dbContext.Database.GetService<IDbContextTransactionManager>();
                if (manager is IRelationalTransactionManager)
                {
                    // Note: relational providers like Sqlite are known to lack proper support
                    // for repeatable read transactions. To ensure this method can be safely used
                    // with such providers, the database transaction is created in a try/catch block.
                    try
                    {
                        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
                    }

                    catch
                    {
                        return null;
                    }
                }

                return null;
            }

            // Note: to avoid sending too many queries, the maximum number of elements
            // that can be removed by a single call to PruneAsync() is deliberately limited.
            for (var index = 0; index < 1_000; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // To prevent concurrency exceptions from being thrown if an entry is modified
                // after it was retrieved from the database, the following logic is executed in
                // a repeatable read transaction, that will put a lock on the retrieved entries
                // and thus prevent them from being concurrently modified outside this block.
                using var transaction = await CreateTransactionAsync();

                // Note: the Oracle MySQL provider doesn't support DateTimeOffset and is unable
                // to create a SQL query with an expression calling DateTimeOffset.UtcDateTime.
                // To work around this limitation, the threshold represented as a DateTimeOffset
                // instance is manually converted to a UTC DateTime instance outside the query.
                var date = threshold.UtcDateTime;

                var authorizations =
                    await (from authorization in Authorizations.Include(authorization => authorization.Tokens).AsTracking()
                           where authorization.CreationDate < date
                           where authorization.Status != OpenIddictConstants.Statuses.Valid ||
                                (authorization.Type == OpenIddictConstants.AuthorizationTypes.AdHoc && !authorization.Tokens.Any())
                           orderby authorization.Id
                           select authorization).Take(1_000).ToListAsync(cancellationToken);

                if (authorizations.Count == 0)
                {
                    break;
                }

                // Note: new tokens may be attached after the authorizations were retrieved
                // from the database since the transaction level is deliberately limited to
                // repeatable read instead of serializable for performance reasons). In this
                // case, the operation will fail, which is considered an acceptable risk.
                _dbContext.RemoveRange(authorizations);

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    transaction?.Commit();
                }

                catch (Exception exception)
                {
                    exceptions ??= new List<Exception>(capacity: 1);
                    exceptions.Add(exception);
                }
            }

            if (exceptions is not null)
            {
                throw new AggregateException("An error occurred while pruning authorizations.", exceptions);
            }
        }

        public async ValueTask SetApplicationIdAsync(AuthAuthorization authorization, string? identifier,
            CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            if (!string.IsNullOrEmpty(identifier))
            {
                var key = ConvertIdentifierFromString<Guid>(identifier);

                // Warning: FindAsync() is deliberately not used to work around a breaking change introduced
                // in Entity Framework Core 3.x (where a ValueTask instead of a Task is now returned).
                var application =
                    await Applications.AsQueryable()
                        .AsTracking()
                        .FirstOrDefaultAsync(application => application.Id!.Equals(key), cancellationToken);

                if (application is null)
                {
                    throw new InvalidOperationException("The application associated with the authorization cannot be found.");
                }

                authorization.Application = application;
            }

            else
            {
                // If the application is not attached to the authorization, try to load it manually.
                if (authorization.Application is null)
                {
                    var reference = _dbContext.Entry(authorization).Reference(entry => entry.Application);
                    if (reference.EntityEntry.State == EntityState.Detached)
                    {
                        return;
                    }

                    await reference.LoadAsync(cancellationToken);
                }

                authorization.Application = null;
            }
        }

        public ValueTask SetCreationDateAsync(AuthAuthorization authorization, DateTimeOffset? date,
            CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.CreationDate = date?.UtcDateTime;

            return default;
        }

        public ValueTask SetPropertiesAsync(AuthAuthorization authorization, ImmutableDictionary<string, JsonElement> properties,
            CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            if (properties is null || properties.IsEmpty)
            {
                authorization.Properties = null;

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

            authorization.Properties = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public ValueTask SetScopesAsync(AuthAuthorization authorization, ImmutableArray<string> scopes, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            if (scopes.IsDefaultOrEmpty)
            {
                authorization.Scopes = null;

                return default;
            }

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = false
            });

            writer.WriteStartArray();

            foreach (var scope in scopes)
            {
                writer.WriteStringValue(scope);
            }

            writer.WriteEndArray();
            writer.Flush();

            authorization.Scopes = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        public ValueTask SetStatusAsync(AuthAuthorization authorization, string? status, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.Status = status;

            return default;
        }

        public ValueTask SetSubjectAsync(AuthAuthorization authorization, string? subject, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.Subject = subject;

            return default;
        }

        public ValueTask SetTypeAsync(AuthAuthorization authorization, string? type, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.Type = type;

            return default;
        }

        public async ValueTask UpdateAsync(AuthAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization is null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            _dbContext.Attach(authorization);

            // Generate a new concurrency token and attach it
            // to the authorization before persisting the changes.
            authorization.ConcurrencyToken = Guid.NewGuid().ToString();

            _dbContext.Update(authorization);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            catch (DbUpdateConcurrencyException exception)
            {
                // Reset the state of the entity to prevents future calls to SaveChangesAsync() from failing.
                _dbContext.Entry(authorization).State = EntityState.Unchanged;

                throw new OpenIddictExceptions.ConcurrencyException("The authorization was concurrently updated and cannot be persisted in its current state. Reload the authorization from the database and retry the operation.", exception);
            }
        }


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
