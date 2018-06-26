using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Strategies.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using NLog;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Common
{
    /// <summary>
    /// Abstraction layer for the Data Store
    /// </summary>
    public class DatastoreContext : DbContext
    {
        private static readonly IDictionary<string, Func<string, DbContextOptionsBuilder, DbContextOptionsBuilder>> DbProviders =
            InitializeDbProviders();

        private static readonly IDictionary<string, Func<DatastoreContext, IAS4DbCommand>> NativeCommandsProvider =
            new Dictionary<string, Func<DatastoreContext, IAS4DbCommand>>
            {
                { "SqlServer", ctx => new SqlServerDbCommand(ctx) },
                { "Sqlite", ctx => new SqliteDbCommand(ctx) },
                { "InMemory", ctx => new InMemoryDbCommand(ctx) }
            };

        private readonly IConfig _config;

        private RetryPolicy _policy;

        // TODO: FE needs this in the Monitoring?
        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreContext"/> class.
        /// </summary>
        public DatastoreContext(DbContextOptions<DatastoreContext> options) : this(options, Config.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreContext"/> class. 
        /// Create a new Data Store Context with given a Configuration Dependency
        /// </summary>
        /// <param name="config">
        /// </param>
        public DatastoreContext(IConfig config) : this(new DbContextOptions<DatastoreContext>(), config)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreContext"/> class.
        /// </summary>
        public DatastoreContext(DbContextOptions<DatastoreContext> options, IConfig config) : base(options)
        {
            _config = config;
            InitializeFields();
        }

        // The code below is required when creating a new Db-Migration.
        // The Add-Migration command requires a default constructor on DatastoreContext
        // Also use a hard-coded 'NativeCommand' in the 'InitializeFields()' call.

        //public DatastoreContext() : this(GetDbContextOptions(), null)
        //{
        //}

        //private static DbContextOptions<DatastoreContext> GetDbContextOptions()
        //{
        //    var optionsBuilder = new DbContextOptionsBuilder<DatastoreContext>();

        //    //optionsBuilder.UseSqlServer("Server=.;database=journaltable;integrated security=sspi");
        //    optionsBuilder.UseSqlite(@"Filename=database\messages.db");

        //    return optionsBuilder.Options;
        //}

        private static IDictionary<string, Func<string, DbContextOptionsBuilder, DbContextOptionsBuilder>> InitializeDbProviders()
        {
            return new Dictionary<string, Func<string, DbContextOptionsBuilder, DbContextOptionsBuilder>>(StringComparer.InvariantCulture)
            {
                {
                    "Sqlite", (c, b) =>
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

                        return b.UseSqlite(c);
                    }
                },
                {
                    "SqlServer", (c, b) => b.UseSqlServer(c)
                },
                {
                    "InMemory", (c, b) => b.UseInMemoryDatabase(Guid.NewGuid().ToString())
                                           .ConfigureWarnings(w => w.Ignore(RelationalEventId.AmbientTransactionWarning))
                }
            };
        }

        private void InitializeFields()
        {
            _policy = Policy
                .Handle<DbUpdateException>()
                .RetryAsync();

            if (_config == null)
            {
                return;
            }

            string providerKey = _config.GetSetting("Provider");
            if (!NativeCommandsProvider.ContainsKey(providerKey))
            {
                throw new KeyNotFoundException(
                    $"No Native Command implementation found for DBMS-type: '{providerKey}'");
            }

            NativeCommands = NativeCommandsProvider[providerKey](this);
        }

        public DbSet<InMessage> InMessages { get; set; }

        public DbSet<OutMessage> OutMessages { get; set; }

        public DbSet<InException> InExceptions { get; set; }

        public DbSet<OutException> OutExceptions { get; set; }

        public DbSet<ReceptionAwareness> ReceptionAwareness { get; set; }

        public DbSet<SmpConfiguration> SmpConfigurations { get; set; }

        public DbSet<RetryReliability> RetryReliability { get; set; }

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

            if (!DbProviders.ContainsKey(providerKey))
            {
                throw new KeyNotFoundException($"No Database provider found for key: {providerKey}");
            }

            var databaseInitializer = DbProviders[providerKey];

            databaseInitializer(connectionString, optionsBuilder);

            // Make sure no InvalidOperation is thrown when an ambient transaction is detected.
            optionsBuilder.ConfigureWarnings(x => x.Ignore(CoreEventId.IncludeIgnoredWarning));
            optionsBuilder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
            optionsBuilder.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));


            optionsBuilder.UseLoggerFactory(Logger);
        }

        private static readonly LoggerFactory Logger = new LoggerFactory(new[] { new TraceLoggerProvider() });

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

            modelBuilder.Entity<InMessage>().HasKey(im => im.Id).HasName("PK_InMessages");
            modelBuilder.Entity<InMessage>().Property(im => im.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<InMessage>().Property(im => im.MEP)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InMessage>().Property(im => im.EbmsMessageType)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InMessage>().Property(im => im.Operation)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InMessage>().Property(im => im.MEP)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InMessage>().Property(im => im.EbmsMessageType)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InMessage>().Property(im => im.PMode).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InMessage>().Property(im => im.PModeId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InMessage>().HasIndex(im => new { im.EbmsMessageId, im.IsDuplicate }).HasName("IX_InMessages_EbmsMessageId_IsDuplicate");
            modelBuilder.Entity<InMessage>().HasIndex(im => new { im.Operation, im.InsertionTime }).HasName("IX_InMessages_Operation_InsertionTime");
            modelBuilder.Entity<InMessage>().HasIndex(im => im.EbmsRefToMessageId).HasName("IX_InMessages_EbmsRefToMessageId");

            modelBuilder.Entity<OutMessage>().HasKey(im => im.Id).HasName("PK_OutMessages");
            modelBuilder.Entity<OutMessage>().Property(im => im.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<OutMessage>().Property(im => im.Operation)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutMessage>().Property(im => im.MEP)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutMessage>().Property(im => im.EbmsMessageType)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutMessage>().Property(im => im.PMode).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutMessage>().Property(im => im.PModeId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutMessage>().HasIndex(im => im.EbmsMessageId).HasName("IX_OutMessages_EbmsMessageId");
            modelBuilder.Entity<OutMessage>().HasIndex(im => new { im.Operation, im.MEP, im.Mpc, im.InsertionTime }).HasName("IX_OutMessages_Operation_MEP_MPC_InsertionTime");
            modelBuilder.Entity<OutMessage>().HasIndex(im => im.EbmsRefToMessageId).HasName("IX_OutMessages_EbmsRefToMessageId");
            modelBuilder.Entity<OutMessage>().HasIndex(im => im.InsertionTime).HasName("IX_OutMessages_InsertionTime");

            modelBuilder.Entity<InException>().HasKey(ie => ie.Id).HasName("PK_InExceptions");
            modelBuilder.Entity<InException>().Property(ie => ie.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<InException>().Property(oe => oe.Operation)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InException>().HasIndex(ie => ie.EbmsRefToMessageId).HasName("IX_InExceptions_EbmsRefToMessageId");
            modelBuilder.Entity<InException>().HasIndex(ie => ie.Operation).HasName("IX_InExceptions_Operation");
            modelBuilder.Entity<InException>().Property(ie => ie.Id).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InException>().Property(ie => ie.EbmsRefToMessageId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InException>().Property(ie => ie.MessageBody).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InException>().Property(ie => ie.Exception).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InException>().Property(ie => ie.PMode).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<InException>().Property(ie => ie.PModeId).UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<OutException>().HasKey(oe => oe.Id).HasName("PK_OutExceptions");
            modelBuilder.Entity<OutException>().Property(oe => oe.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<OutException>().Property(oe => oe.Operation)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutException>().HasIndex(oe => oe.EbmsRefToMessageId).HasName("IX_OutExceptions_EbmsRefToMessageId");
            modelBuilder.Entity<OutException>().HasIndex(oe => oe.Operation).HasName("IX_OutExceptions_Operation");
            modelBuilder.Entity<OutException>().Property(oe => oe.Id).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutException>().Property(oe => oe.EbmsRefToMessageId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutException>().Property(oe => oe.MessageBody).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutException>().Property(oe => oe.Exception).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutException>().Property(oe => oe.PMode).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<OutException>().Property(oe => oe.PModeId).UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<ReceptionAwareness>().HasKey(r => r.Id).HasName("PK_ReceptionAwareness");
            modelBuilder.Entity<ReceptionAwareness>().Property(r => r.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<ReceptionAwareness>().Property(r => r.Status)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<ReceptionAwareness>().HasAlternateKey(r => r.RefToOutMessageId).HasName("AK_ReceptionAwareness_RefToOutMessageId");
            modelBuilder.Entity<ReceptionAwareness>().HasIndex(r => new { r.Status, r.CurrentRetryCount }).HasName("IX_ReceptionAwareness_Status_CurrentRetryCount");

            modelBuilder.Entity<SmpConfiguration>().HasKey(sc => sc.Id).HasName("PK_SmpConfigurations");
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<SmpConfiguration>().HasIndex(sc => new { sc.ToPartyId, sc.PartyRole, sc.PartyType }).IsUnique().HasName("IX_SmpConfigurations_ToPartyId_PartyRole_PartyType");
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.Id).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.PartyRole).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.PartyType).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.ToPartyId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.Action).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.ServiceType).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.ServiceValue).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.FinalRecipient).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.TlsEnabled).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.Url).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.EncryptionEnabled).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.EncryptAlgorithm).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.EncryptAlgorithmKeySize).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.EncryptPublicKeyCertificate).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.EncryptPublicKeyCertificateName).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.EncryptKeyDigestAlgorithm).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.EncryptKeyMgfAlorithm).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<SmpConfiguration>().Property(sc => sc.EncryptKeyTransportAlgorithm).UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<RetryReliability>().HasKey(rr => rr.Id).HasName("PK_RetryReliability");
            modelBuilder.Entity<RetryReliability>().Property(rr => rr.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<RetryReliability>().Property(rr => rr.RefToInMessageId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<RetryReliability>().Property(rr => rr.RefToOutMessageId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<RetryReliability>().Property(rr => rr.RefToInExceptionId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<RetryReliability>().Property(rr => rr.RefToOutExceptionId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<RetryReliability>().Property(rr => rr.MaxRetryCount).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<RetryReliability>().Property(rr => rr.RetryInterval)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<RetryReliability>().Property(rr => rr.RetryType)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<RetryReliability>().Property(rr => rr.Status)
                        .HasConversion<string>()
                        .UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<Journal>().HasKey(j => j.Id).HasName("PK_Journal");
            modelBuilder.Entity<Journal>().Property(j => j.Id).UseSqlServerIdentityColumn();
            modelBuilder.Entity<Journal>().Property(j => j.Id).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<Journal>().Property(j => j.RefToOutMessageId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<Journal>().Property(j => j.RefToInMessageId).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<Journal>().Property(j => j.MessageStatus).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<Journal>().Property(j => j.MessageOperation).UsePropertyAccessMode(PropertyAccessMode.Field);
            modelBuilder.Entity<Journal>().Property(j => j.AgentType).UsePropertyAccessMode(PropertyAccessMode.Field);
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
                => base.SaveChangesAsync(cancellationToken));

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