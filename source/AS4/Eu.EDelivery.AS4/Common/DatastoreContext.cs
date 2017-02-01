using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NLog;
using Polly;
using Polly.Retry;

namespace Eu.EDelivery.AS4.Common
{
    /// <summary>
    /// Abstraction layer for the Data Store
    /// </summary>
    public class DatastoreContext : DbContext
    {
        private readonly IConfig _config;        
        private RetryPolicy _policy;
        private readonly IDictionary<string, Func<string, DbContextOptionsBuilder>> _providers =
            new Dictionary<string, Func<string, DbContextOptionsBuilder>>(StringComparer.InvariantCulture);

        public DbSet<InMessage> InMessages { get; set; }
        public DbSet<OutMessage> OutMessages { get; set; }
        public DbSet<InException> InExceptions { get; set; }
        public DbSet<OutException> OutExceptions { get; set; }
        public DbSet<ReceptionAwareness> ReceptionAwareness { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreContext"/> class. 
        /// Create a new DataStore Context with given Context Options
        /// </summary>
        /// <param name="options">
        /// </param>
        public DatastoreContext(DbContextOptions<DatastoreContext> options) : base(options)
        {
            InitializeFields();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreContext"/> class. 
        /// Create a new Data Store Context with given a Configuration Dependency
        /// </summary>
        /// <param name="config">
        /// </param>
        public DatastoreContext(IConfig config)
        {
            this._config = config;
            InitializeFields();
        }

        private void InitializeFields()
        {
            this._policy = Policy
                .Handle<DbUpdateException>()
                .RetryAsync();


            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        ///     <para>
        ///         Override this method to configure the database (and other options) to be used for this context.
        ///         This method is called for each instance of the context that is created.
        ///     </para>
        ///     <para>
        ///         In situations where an instance of <see cref="T:Microsoft.EntityFrameworkCore.DbContextOptions" /> may or may not have been passed
        ///         to the constructor, you can use <see cref="P:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.IsConfigured" /> to determine if
        ///         the options have already been set, and skip some or all of the logic in
        ///         <see cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)" />.
        ///     </para>
        /// </summary>
        /// <param name="optionsBuilder">
        ///     A builder used to create or modify options for this context. Databases (and other extensions)
        ///     typically define extension methods on this object that allow you to configure the context.
        /// </param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;

            string connectionString = this._config.GetSetting("connectionstring");
            ConfigureProviders(optionsBuilder);

            string providerKey = this._config.GetSetting("Provider");
            if (!this._providers.ContainsKey(providerKey)) throw new AS4Exception($"No Database provider found for key: {providerKey}");
            this._providers[providerKey](connectionString);

            // Make sure no InvalidOperation is thrown when an ambient transaction is detected.
            optionsBuilder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
        }

        private void ConfigureProviders(DbContextOptionsBuilder optionsBuilder)
        {
            this._providers["Sqlite"] = c => optionsBuilder.UseSqlite(c);
            this._providers["SqlServer"] = c => optionsBuilder.UseSqlServer(c);
            this._providers["InMemory"] = c => optionsBuilder.UseInMemoryDatabase(c);

            // TODO: add other providers
        }

        /// <summary>
        ///     Saves all changes made in this context to the database.
        /// </summary>
        /// <remarks>
        ///     This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
        ///     changes to entity instances before saving to the underlying database. This can be disabled via
        ///     <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
        /// </remarks>
        /// <returns>
        ///     The number of state entries written to the database.
        /// </returns>
        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DataException)
            {
                throw ThrowDatastoreUnavailableException();
            }
        }

        /// <summary>
        ///     Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">
        ///     Indicates whether <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after the changes have
        ///     been sent successfully to the database.
        /// </param>
        /// <remarks>
        ///     This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
        ///     changes to entity instances before saving to the underlying database. This can be disabled via
        ///     <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
        /// </remarks>
        /// <returns>
        ///     The number of state entries written to the database.
        /// </returns>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            try
            {
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }
            catch (DataException)
            {
                throw ThrowDatastoreUnavailableException();
            }
        }

        /// <summary>
        ///     Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess">
        ///     Indicates whether <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AcceptAllChanges" /> is called after the changes have
        ///     been sent successfully to the database.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
        ///         changes to entity instances before saving to the underlying database. This can be disabled via
        ///         <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
        ///     </para>
        ///     <para>
        ///         Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///         that any asynchronous operations have completed before calling another method on this context.
        ///     </para>
        /// </remarks>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     number of state entries written to the database.
        /// </returns>
        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            catch (DataException)
            {
                throw ThrowDatastoreUnavailableException();
            }
        }

        /// <summary>
        ///     Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
        ///         changes to entity instances before saving to the underlying database. This can be disabled via
        ///         <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
        ///     </para>
        ///     <para>
        ///         Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        ///         that any asynchronous operations have completed before calling another method on this context.
        ///     </para>
        /// </remarks>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     number of state entries written to the database.
        /// </returns>
        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            PolicyResult<int> policyResult = await this._policy.ExecuteAndCaptureAsync(()
                => base.SaveChangesAsync(cancellationToken), cancellationToken);

            if (policyResult.FinalException != null)
                throw ThrowDatastoreUnavailableException(policyResult.FinalException);

            return policyResult.Result;
        }

        private AS4Exception ThrowDatastoreUnavailableException(Exception innerException = null)
        {
            return new AS4ExceptionBuilder()
                .WithInnerException(innerException)
                .WithDescription("Datastore unavailable")
                .WithErrorCode(ErrorCode.Ebms0004)
                .Build();
        }
    }
}