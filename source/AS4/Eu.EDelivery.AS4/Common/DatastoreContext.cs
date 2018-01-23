using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Strategies.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly IServiceProvider _serviceProvider =
            new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

        private readonly IConfig _config;
        private readonly IDictionary<string, Func<string, DbContextOptionsBuilder>> _providers =
            new Dictionary<string, Func<string, DbContextOptionsBuilder>>(StringComparer.InvariantCulture);

        private readonly IDictionary<string, Func<DatastoreContext, IAS4DbCommand>> _retrieveCommands =
            new Dictionary<string, Func<DatastoreContext, IAS4DbCommand>>
            {
                {"SqlServer", ctx => new SqlServerDbCommand(ctx)},
                {"Sqlite", ctx => new SqliteDbCommand(ctx)},
                {"InMemory", ctx => new InMemoryDbCommand(ctx)}
            };

        private RetryPolicy _policy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreContext"/> class.
        /// </summary>
        public DatastoreContext(DbContextOptions<DatastoreContext> options, IConfig config) : base(options)
        {
            _config = config;
            InitializeFields();
        }

        /*

        //   The code below is required when creating a new Db-Migration.
        //  The Add-Migration command requires a default constructor on DatastoreContext

        public DatastoreContext() : this(GetDbContextOptions())
        {
        }

        private static DbContextOptions<DatastoreContext> GetDbContextOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatastoreContext>();

            optionsBuilder.UseSqlServer("Server=.;database=as4test;integrated security=sspi");

            return optionsBuilder.Options;
        }

        */

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreContext"/> class. 
        /// Create a new Data Store Context with given a Configuration Dependency
        /// </summary>
        /// <param name="config">
        /// </param>
        public DatastoreContext(IConfig config)
        {
            _config = config;
            InitializeFields();
        }

        private void InitializeFields()
        {
            _policy = Policy
                .Handle<DbUpdateException>()
                .RetryAsync();

            if (_config == null) { return; }

            string providerKey = _config.GetSetting("Provider");
            if (!_retrieveCommands.ContainsKey(providerKey))
            {
                throw new KeyNotFoundException(
                    $"No Native Command implementation found for DBMS-type: '{providerKey}'");
            }

            NativeCommands = _retrieveCommands[providerKey](this);
        }

        public DbSet<InMessage> InMessages { get; set; }

        public DbSet<OutMessage> OutMessages { get; set; }

        public DbSet<InException> InExceptions { get; set; }

        public DbSet<OutException> OutExceptions { get; set; }

        public DbSet<ReceptionAwareness> ReceptionAwareness { get; set; }

        public IAS4DbCommand NativeCommands { get; private set; }

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
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            string providerKey = _config.GetSetting("Provider");
            string connectionString = _config.GetSetting("connectionstring");

            ConfigureProviders(optionsBuilder);

            if (!_providers.ContainsKey(providerKey))
            {
                throw new KeyNotFoundException($"No Database provider found for key: {providerKey}");
            }

            _providers[providerKey](connectionString);

            // Make sure no InvalidOperation is thrown when an ambient transaction is detected.
            optionsBuilder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));

            var logger = new LoggerFactory();
            logger.AddProvider(new TraceLoggerProvider());

            optionsBuilder.UseLoggerFactory(logger);
        }

        private void ConfigureProviders(DbContextOptionsBuilder optionsBuilder)
        {
            _providers["Sqlite"] = c =>
            {
                string GetDirectoryFromConnectionString(string connectionString)
                {
                    string[] parts = connectionString.Split('=');

                    if (parts.Length != 2)
                    {
                        return string.Empty;
                    }

                    return Path.GetDirectoryName(parts[1]);
                }

                string databaseLocation = GetDirectoryFromConnectionString(c);

                if (!String.IsNullOrWhiteSpace(databaseLocation) && !Directory.Exists(databaseLocation))
                {
                    Directory.CreateDirectory(databaseLocation);
                }

                return optionsBuilder.UseSqlite(c);
            };

            _providers["SqlServer"] = c => optionsBuilder.UseSqlServer(c);

            // TODO: add other providers
            _providers["InMemory"] = _ => 
                optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString())
                              .UseInternalServiceProvider(_serviceProvider)
                              .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        }

        /// <summary>
        ///     Override this method to further configure the model that was discovered by convention from the entity types
        ///     exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        ///     and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <remarks>
        ///     If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        ///     then this method will not be run.
        /// </remarks>
        /// <param name="modelBuilder">
        ///     The builder being used to construct the model for this context. Databases (and other extensions) typically
        ///     define extension methods on this object that allow you to configure aspects of the model that are specific
        ///     to a given database.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // It is necessary to define the property-access mode for properties that are defined in a base
            // class and have a private setter.  Failing to do this results in Entity Framework not generating
            // a column in the table for that property

            modelBuilder.Entity<InMessage>().HasKey(im => im.Id);
            modelBuilder.Entity<InMessage>().Property(im => im.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<InMessage>().Property(im => im.MEP).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InMessage>().Property(im => im.EbmsMessageType).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InMessage>().HasIndex(im => new { im.EbmsMessageId, im.IsDuplicate });
            modelBuilder.Entity<InMessage>().HasIndex(im => new { im.Operation, im.InsertionTime });
            modelBuilder.Entity<InMessage>().HasIndex(im => im.EbmsRefToMessageId);
            modelBuilder.Entity<InMessage>().Property(im => im.MEP).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InMessage>().Property(im => im.EbmsMessageType).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InMessage>().Property(im => im.PMode).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InMessage>().Property(im => im.PModeId).UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<OutMessage>().HasKey(im => im.Id);
            modelBuilder.Entity<OutMessage>().Property(im => im.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<OutMessage>().HasAlternateKey(im => im.EbmsMessageId);
            modelBuilder.Entity<OutMessage>().HasIndex(im => new { im.Operation, im.MEP, im.Mpc, im.InsertionTime });
            modelBuilder.Entity<OutMessage>().HasIndex(im => im.EbmsRefToMessageId);
            modelBuilder.Entity<OutMessage>().HasIndex(im => im.InsertionTime);
            modelBuilder.Entity<OutMessage>().Property(im => im.MEP).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutMessage>().Property(im => im.EbmsMessageType).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutMessage>().Property(im => im.PMode).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutMessage>().Property(im => im.PModeId).UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<InException>().HasKey(ie => ie.Id);
            modelBuilder.Entity<InException>().Property(ie => ie.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<InException>().HasIndex(ie => ie.EbmsRefToMessageId);
            modelBuilder.Entity<InException>().HasIndex(ie => ie.Operation);
            modelBuilder.Entity<InException>().Property(ie => ie.Id).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InException>().Property(ie => ie.EbmsRefToMessageId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InException>().Property(ie => ie.MessageBody).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InException>().Property(ie => ie.Exception).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InException>().Property(ie => ie.PMode).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InException>().Property(ie => ie.PModeId).UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<OutException>().HasKey(oe => oe.Id);
            modelBuilder.Entity<OutException>().Property(oe => oe.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<OutException>().HasIndex(oe => oe.EbmsRefToMessageId);
            modelBuilder.Entity<OutException>().HasIndex(oe => oe.Operation);
            modelBuilder.Entity<OutException>().Property(oe => oe.Id).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutException>().Property(oe => oe.EbmsRefToMessageId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutException>().Property(oe => oe.MessageBody).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutException>().Property(oe => oe.Exception).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutException>().Property(oe => oe.PMode).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutException>().Property(oe => oe.PModeId).UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<ReceptionAwareness>().HasKey(r => r.Id);
            modelBuilder.Entity<ReceptionAwareness>().Property(r => r.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<ReceptionAwareness>().HasAlternateKey(r => r.InternalMessageId);
            modelBuilder.Entity<ReceptionAwareness>().HasIndex(r => new { r.Status, r.CurrentRetryCount });
        }

        /// <summary>
        /// Determines whether the DatastoreContext is already tracking the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns>True if the specified entity is already being tracked by the DatastoreContext.</returns>
        public bool IsEntityAttached<T>(T entity) where T : Entity
        {
            return ChangeTracker.Entries<T>().Any(e => e.Entity.Id == entity.Id);
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
            catch (DataException exception)
            {
                throw ThrowDatastoreUnavailableException(exception);
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
            catch (DataException exception)
            {
                throw ThrowDatastoreUnavailableException(exception);
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
            catch (DataException exception)
            {
                throw ThrowDatastoreUnavailableException(exception);
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
            PolicyResult<int> policyResult = await _policy.ExecuteAndCaptureAsync(()
                => base.SaveChangesAsync(cancellationToken), cancellationToken);

            if (policyResult.FinalException != null)
            {
                throw ThrowDatastoreUnavailableException(policyResult.FinalException);
            }

            return policyResult.Result;
        }

        private static DataException ThrowDatastoreUnavailableException(Exception innerException = null)
        {
            Exception mostInnerException = null;

            var logger = LogManager.GetCurrentClassLogger();

            while (innerException != null)
            {
                logger.Error(innerException.Message);
                logger.Trace(innerException.StackTrace);

                mostInnerException = innerException;
                innerException = innerException.InnerException;
            }

            return new DataException("Datastore unavailable", mostInnerException);
        }
    }
}