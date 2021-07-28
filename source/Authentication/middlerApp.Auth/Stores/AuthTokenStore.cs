using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using middlerApp.Auth.Context;
using middlerApp.Auth.Entities;
using middlerApp.Auth.ExtensionMethods;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore;

namespace middlerApp.Auth.Stores
{
    public class AuthTokenStore: IOpenIddictTokenStore<AuthToken>
    {
        /// <summary>
        /// Gets the memory cache associated with the current store.
        /// </summary>
        protected IMemoryCache Cache { get; }

        /// <summary>
        /// Gets the database context associated with the current store.
        /// </summary>
        protected AuthDbContext Context { get; }

        public AuthTokenStore(IMemoryCache cache, AuthDbContext dbContext)
        {
            Cache = cache;
            Context = dbContext;
        }

        

        /// <summary>
        /// Gets the options associated with the current store.
        /// </summary>
        protected IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> Options { get; }

        /// <summary>
        /// Gets the database set corresponding to the <typeparamref name="AuthApplication"/> entity.
        /// </summary>
        private DbSet<AuthApplication> Applications => Context.Set<AuthApplication>();

        /// <summary>
        /// Gets the database set corresponding to the <typeparamref name="AuthAuthorization"/> entity.
        /// </summary>
        private DbSet<AuthAuthorization> Authorizations => Context.Set<AuthAuthorization>();

        /// <summary>
        /// Gets the database set corresponding to the <typeparamref name="AuthToken"/> entity.
        /// </summary>
        private DbSet<AuthToken> Tokens => Context.Set<AuthToken>();

        /// <inheritdoc/>
        public virtual async ValueTask<long> CountAsync(CancellationToken cancellationToken)
            => await Tokens.AsQueryable().LongCountAsync(cancellationToken);

        /// <inheritdoc/>
        public virtual async ValueTask<long> CountAsync<TResult>(Func<IQueryable<AuthToken>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return await query(Tokens).LongCountAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async ValueTask CreateAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            Context.Add(token);

            await Context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async ValueTask DeleteAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            Context.Remove(token);

            try
            {
                await Context.SaveChangesAsync(cancellationToken);
            }

            catch (DbUpdateConcurrencyException exception)
            {
                // Reset the state of the entity to prevents future calls to SaveChangesAsync() from failing.
                Context.Entry(token).State = EntityState.Unchanged;

                throw new OpenIddictExceptions.ConcurrencyException("The token was concurrently updated and cannot be persisted in its current state. Reload the token from the database and retry the operation.", exception);
            }
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<AuthToken> FindAsync(string subject, string client, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client cannot be null or empty.", nameof(client));
            }

            // Note: due to a bug in Entity Framework Core's query visitor, the authorizations can't be
            // filtered using token.Application.Id.Equals(key). To work around this issue,
            // this compiled query uses an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            var key = ConvertIdentifierFromString<Guid>(client);

            return (from token in Tokens.Include(token => token.Application).Include(token => token.Authorization).AsTracking()
                    where token.Subject == subject
                    join application in Applications.AsTracking() on token.Application!.Id equals application.Id
                    where application.Id!.Equals(key)
                    select token).AsAsyncEnumerable(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<AuthToken> FindAsync(
            string subject, string client,
            string status, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client cannot be null or empty.", nameof(client));
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(status));
            }

            // Note: due to a bug in Entity Framework Core's query visitor, the authorizations can't be
            // filtered using token.Application.Id.Equals(key). To work around this issue,
            // this compiled query uses an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            var key = ConvertIdentifierFromString<Guid>(client);

            return (from token in Tokens.Include(token => token.Application).Include(token => token.Authorization).AsTracking()
                    where token.Subject == subject &&
                          token.Status == status
                    join application in Applications.AsTracking() on token.Application!.Id equals application.Id
                    where application.Id!.Equals(key)
                    select token).AsAsyncEnumerable(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<AuthToken> FindAsync(
            string subject, string client,
            string status, string type, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client cannot be null or empty.", nameof(client));
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
            // filtered using token.Application.Id.Equals(key). To work around this issue,
            // this compiled query uses an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            var key = ConvertIdentifierFromString<Guid>(client);

            return (from token in Tokens.Include(token => token.Application).Include(token => token.Authorization).AsTracking()
                    where token.Subject == subject &&
                          token.Status == status &&
                          token.Type == type
                    join application in Applications.AsTracking() on token.Application!.Id equals application.Id
                    where application.Id!.Equals(key)
                    select token).AsAsyncEnumerable(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<AuthToken> FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            // Note: due to a bug in Entity Framework Core's query visitor, the tokens can't be
            // filtered using token.Application.Id.Equals(key). To work around this issue,
            // this method is overriden to use an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            var key = ConvertIdentifierFromString<Guid>(identifier);

            return (from token in Tokens.Include(token => token.Application).Include(token => token.Authorization).AsTracking()
                    join application in Applications.AsTracking() on token.Application!.Id equals application.Id
                    where application.Id!.Equals(key)
                    select token).AsAsyncEnumerable(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<AuthToken> FindByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            // Note: due to a bug in Entity Framework Core's query visitor, the tokens can't be
            // filtered using token.Authorization.Id.Equals(key). To work around this issue,
            // this method is overriden to use an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            var key = ConvertIdentifierFromString<Guid>(identifier);

            return (from token in Tokens.Include(token => token.Application).Include(token => token.Authorization).AsTracking()
                    join authorization in Authorizations.AsTracking() on token.Authorization!.Id equals authorization.Id
                    where authorization.Id!.Equals(key)
                    select token).AsAsyncEnumerable(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async ValueTask<AuthToken?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            var key = ConvertIdentifierFromString<Guid>(identifier);

            return await (from token in Tokens.Include(token => token.Application).Include(token => token.Authorization).AsTracking()
                          where token.Id!.Equals(key)
                          select token).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async ValueTask<AuthToken?> FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return await (from token in Tokens.Include(token => token.Application).Include(token => token.Authorization).AsTracking()
                          where token.ReferenceId == identifier
                          select token).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<AuthToken> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            return (from token in Tokens.Include(token => token.Application).Include(token => token.Authorization).AsTracking()
                    where token.Subject == subject
                    select token).AsAsyncEnumerable(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async ValueTask<string?> GetApplicationIdAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            // If the application is not attached to the token, try to load it manually.
            if (token.Application is null)
            {
                var reference = Context.Entry(token).Reference(entry => entry.Application);
                if (reference.EntityEntry.State == EntityState.Detached)
                {
                    return null;
                }

                await reference.LoadAsync(cancellationToken);
            }

            if (token.Application is null)
            {
                return null;
            }

            return ConvertIdentifierToString(token.Application.Id);
        }

        /// <inheritdoc/>
        public virtual async ValueTask<TResult?> GetAsync<TState, TResult>(
            Func<IQueryable<AuthToken>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return await query(Tokens.Include(token => token.Application)
                .Include(token => token.Authorization)
                .AsTracking(), state).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async ValueTask<string?> GetAuthorizationIdAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            // If the authorization is not attached to the token, try to load it manually.
            if (token.Authorization is null)
            {
                var reference = Context.Entry(token).Reference(entry => entry.Authorization);
                if (reference.EntityEntry.State == EntityState.Detached)
                {
                    return null;
                }

                await reference.LoadAsync(cancellationToken);
            }

            if (token.Authorization is null)
            {
                return null;
            }

            return ConvertIdentifierToString(token.Authorization.Id);
        }

        /// <inheritdoc/>
        public virtual ValueTask<DateTimeOffset?> GetCreationDateAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (token.CreationDate is null)
            {
                return new ValueTask<DateTimeOffset?>(result: null);
            }

            return new ValueTask<DateTimeOffset?>(DateTime.SpecifyKind(token.CreationDate.Value, DateTimeKind.Utc));
        }

        /// <inheritdoc/>
        public virtual ValueTask<DateTimeOffset?> GetExpirationDateAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (token.ExpirationDate is null)
            {
                return new ValueTask<DateTimeOffset?>(result: null);
            }

            return new ValueTask<DateTimeOffset?>(DateTime.SpecifyKind(token.ExpirationDate.Value, DateTimeKind.Utc));
        }

        /// <inheritdoc/>
        public virtual ValueTask<string?> GetIdAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return new ValueTask<string?>(ConvertIdentifierToString(token.Id));
        }

        /// <inheritdoc/>
        public virtual ValueTask<string?> GetPayloadAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return new ValueTask<string?>(token.Payload);
        }

        /// <inheritdoc/>
        public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(token.Properties))
            {
                return new ValueTask<ImmutableDictionary<string, JsonElement>>(ImmutableDictionary.Create<string, JsonElement>());
            }

            // Note: parsing the stringified properties is an expensive operation.
            // To mitigate that, the resulting object is stored in the memory cache.
            var key = string.Concat("d0509397-1bbf-40e7-97e1-5e6d7bc2536c", "\x1e", token.Properties);
            var properties = Cache.GetOrCreate(key, entry =>
            {
                entry.SetPriority(CacheItemPriority.High)
                     .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                using var document = JsonDocument.Parse(token.Properties);
                var builder = ImmutableDictionary.CreateBuilder<string, JsonElement>();

                foreach (var property in document.RootElement.EnumerateObject())
                {
                    builder[property.Name] = property.Value.Clone();
                }

                return builder.ToImmutable();
            });

            return new ValueTask<ImmutableDictionary<string, JsonElement>>(properties);
        }

        /// <inheritdoc/>
        public virtual ValueTask<DateTimeOffset?> GetRedemptionDateAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (token.RedemptionDate is null)
            {
                return new ValueTask<DateTimeOffset?>(result: null);
            }

            return new ValueTask<DateTimeOffset?>(DateTime.SpecifyKind(token.RedemptionDate.Value, DateTimeKind.Utc));
        }

        /// <inheritdoc/>
        public virtual ValueTask<string?> GetReferenceIdAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return new ValueTask<string?>(token.ReferenceId);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string?> GetStatusAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return new ValueTask<string?>(token.Status);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string?> GetSubjectAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return new ValueTask<string?>(token.Subject);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string?> GetTypeAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return new ValueTask<string?>(token.Type);
        }

        /// <inheritdoc/>
        public virtual ValueTask<AuthToken> InstantiateAsync(CancellationToken cancellationToken)
        {
            try
            {
                return new ValueTask<AuthToken>(Activator.CreateInstance<AuthToken>());
            }

            catch (MemberAccessException exception)
            {
                return new ValueTask<AuthToken>(Task.FromException<AuthToken>(
                    new InvalidOperationException("An error occurred while trying to create a new token instance. Make sure that the token entity is not abstract and has a public parameterless constructor or create a custom token store that overrides 'InstantiateAsync()' to use a custom factory.", exception)));
            }
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<AuthToken> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            var query = Tokens.Include(token => token.Application)
                              .Include(token => token.Authorization)
                              .OrderBy(token => token.Id!)
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

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
            Func<IQueryable<AuthToken>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            return query(
                Tokens.Include(token => token.Application)
                      .Include(token => token.Authorization)
                      .AsTracking(), state).AsAsyncEnumerable(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async ValueTask PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
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
                var manager = Context.Database.GetService<IDbContextTransactionManager>();
                if (manager is IRelationalTransactionManager)
                {
                    // Note: relational providers like Sqlite are known to lack proper support
                    // for repeatable read transactions. To ensure this method can be safely used
                    // with such providers, the database transaction is created in a try/catch block.
                    try
                    {
                        return await Context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
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

                var tokens = await
                    (from token in Tokens.AsTracking()
                     where token.CreationDate < date
                     where (token.Status != OpenIddictConstants.Statuses.Inactive && token.Status != OpenIddictConstants.Statuses.Valid) ||
                           (token.Authorization != null && token.Authorization.Status != OpenIddictConstants.Statuses.Valid) ||
                            token.ExpirationDate < DateTime.UtcNow
                     orderby token.Id
                     select token).Take(1_000).ToListAsync(cancellationToken);

                if (tokens.Count == 0)
                {
                    break;
                }

                Context.RemoveRange(tokens);

                try
                {
                    await Context.SaveChangesAsync(cancellationToken);
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
                throw new AggregateException("An error occurred while pruning tokens.", exceptions);
            }
        }

        /// <inheritdoc/>
        public virtual async ValueTask SetApplicationIdAsync(AuthToken token, string? identifier, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
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
                    throw new InvalidOperationException("The application associated with the token cannot be found.");
                }

                token.Application = application;
            }

            else
            {
                // If the application is not attached to the token, try to load it manually.
                if (token.Application is null)
                {
                    var reference = Context.Entry(token).Reference(entry => entry.Application);
                    if (reference.EntityEntry.State == EntityState.Detached)
                    {
                        return;
                    }

                    await reference.LoadAsync(cancellationToken);
                }

                token.Application = null;
            }
        }

        /// <inheritdoc/>
        public virtual async ValueTask SetAuthorizationIdAsync(AuthToken token, string? identifier, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (!string.IsNullOrEmpty(identifier))
            {
                var key = ConvertIdentifierFromString<Guid>(identifier);

                // Warning: FindAsync() is deliberately not used to work around a breaking change introduced
                // in Entity Framework Core 3.x (where a ValueTask instead of a Task is now returned).
                var authorization =
                    await Authorizations.AsQueryable()
                                        .AsTracking()
                                        .FirstOrDefaultAsync(authorization => authorization.Id!.Equals(key), cancellationToken);

                if (authorization is null)
                {
                    throw new InvalidOperationException("The authorization associated with the token cannot be found.");
                }

                token.Authorization = authorization;
            }

            else
            {
                // If the authorization is not attached to the token, try to load it manually.
                if (token.Authorization is null)
                {
                    var reference = Context.Entry(token).Reference(entry => entry.Authorization);
                    if (reference.EntityEntry.State == EntityState.Detached)
                    {
                        return;
                    }

                    await reference.LoadAsync(cancellationToken);
                }

                token.Authorization = null;
            }
        }

        /// <inheritdoc/>
        public virtual ValueTask SetCreationDateAsync(AuthToken token, DateTimeOffset? date, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.CreationDate = date?.UtcDateTime;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetExpirationDateAsync(AuthToken token, DateTimeOffset? date, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.ExpirationDate = date?.UtcDateTime;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetPayloadAsync(AuthToken token, string? payload, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.Payload = payload;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetPropertiesAsync(AuthToken token,
            ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (properties is null || properties.IsEmpty)
            {
                token.Properties = null;

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

            token.Properties = Encoding.UTF8.GetString(stream.ToArray());

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetRedemptionDateAsync(AuthToken token, DateTimeOffset? date, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.RedemptionDate = date?.UtcDateTime;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetReferenceIdAsync(AuthToken token, string? identifier, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.ReferenceId = identifier;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetStatusAsync(AuthToken token, string? status, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.Status = status;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetSubjectAsync(AuthToken token, string? subject, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.Subject = subject;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetTypeAsync(AuthToken token, string? type, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.Type = type;

            return default;
        }

        /// <inheritdoc/>
        public virtual async ValueTask UpdateAsync(AuthToken token, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            Context.Attach(token);

            // Generate a new concurrency token and attach it
            // to the token before persisting the changes.
            token.ConcurrencyToken = Guid.NewGuid().ToString();

            Context.Update(token);

            try
            {
                await Context.SaveChangesAsync(cancellationToken);
            }

            catch (DbUpdateConcurrencyException exception)
            {
                // Reset the state of the entity to prevents future calls to SaveChangesAsync() from failing.
                Context.Entry(token).State = EntityState.Unchanged;

                throw new OpenIddictExceptions.ConcurrencyException("The token was concurrently updated and cannot be persisted in its current state. Reload the token from the database and retry the operation.", exception);
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

            return (TKey) TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(identifier);
        }

        /// <summary>
        /// Converts the provided identifier to its string representation.
        /// </summary>
        /// <param name="identifier">The identifier to convert.</param>
        /// <returns>A <see cref="string"/> representation of the provided identifier.</returns>
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
