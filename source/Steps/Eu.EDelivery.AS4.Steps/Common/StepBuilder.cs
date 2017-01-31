using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Receive.Participant;
using Eu.EDelivery.AS4.Steps.ReceptionAwareness;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.Steps.Services;
using Eu.EDelivery.AS4.Steps.Submit;

namespace Eu.EDelivery.AS4.Steps.Common
{
    /// <summary>
    /// Builder for the <see cref="IStep" /> implementations
    /// </summary>
    [Obsolete("This class is never used.")]
    public class StepBuilder
    {
        private readonly IInMessageService _inMessageService;
        private readonly IOutMessageService _outMessageService;
        private readonly IInExceptionService _inExceptionService;
        private readonly IDatastoreRepository _dataRepository;

        private StepOptions _options;
        private IRegistry _registry;
        private IConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepBuilder"/> class. 
        /// Create a Step Builder with default Build Step Options
        /// </summary>
        public StepBuilder()
        {
            this._options = StepOptions.UseDefaults | StepOptions.UseDatastore;
            this._registry = Registry.Instance;

            Func<DatastoreContext> datastore = () => new DatastoreContext(this._config);
            this._dataRepository = new DatastoreRepository(datastore);
            this._inMessageService = new InMessageService(this._dataRepository);
            this._outMessageService = new OutMessageService(this._dataRepository);
            this._inExceptionService = new InExceptionService(this._dataRepository);
        }

        /// <summary>
        /// Add <see cref="IConfig" /> to the Builder
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public StepBuilder WithConfig(IConfig config)
        {
            this._config = config;
            if (!this._config.IsInitialized)
                this._config.Initialize();

            return this;
        }

        /// <summary>
        /// Add <see cref="IRegistry"/> to the Builder
        /// </summary>
        /// <param name="registry"></param>
        /// <returns></returns>
        public StepBuilder WithRegistry(IRegistry registry)
        {
            this._registry = registry;
            return this;
        }

        /// <summary>
        /// Add <see cref="StepOptions" /> to Builder
        /// </summary>
        /// <param name="stepOptions"></param>
        /// <returns></returns>
        public StepBuilder WithOptions(StepOptions stepOptions)
        {
            this._options = stepOptions;
            return this;
        }

        /// <summary>
        /// Build the Step for AS4 Send
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <returns></returns>
        public IStep BuildSendStep(InternalMessage internalMessage = null)
        {
            IEnumerable<StepEntry> stepEntries = GetSendStepEntries();
            IEnumerable<IStep> steps = FilterSteps(stepEntries);

            return new OutExceptionStepDecorator(CompositeSteps(steps), this._dataRepository);
        }

        private IEnumerable<StepEntry> GetSendStepEntries()
        {
            return new List<StepEntry>
            {
                StepEntry.Create(StepOptions.UseDatastore, new SetReceptionAwarenessStep(this._dataRepository)),
                StepEntry.Create(StepOptions.UseDefaults, new CompressAttachmentsStep()),
                StepEntry.Create(StepOptions.UseDefaults, new SignAS4MessageStep(this._registry.CertificateRepository)),
                StepEntry.Create(StepOptions.UseDefaults, new SendAS4MessageStep(this._registry.SerializerProvider)),
                StepEntry.Create(StepOptions.UseDatastore, new SendUpdateDataStoreStep(this._inMessageService))
            };
        }

        /// <summary>
        /// Build the Step for the AS4 Submit
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <returns></returns>
        public IStep BuildSubmitStep(InternalMessage internalMessage = null)
        {
            IEnumerable<StepEntry> stepEntries = GetSubmitStepEntries();
            IEnumerable<IStep> steps = FilterSteps(stepEntries);

            return new OutExceptionStepDecorator(CompositeSteps(steps), this._dataRepository);
        }

        private IEnumerable<StepEntry> GetSubmitStepEntries()
        {
            return new List<StepEntry>
            {
                StepEntry.Create(StepOptions.UseDefaults, new RetrieveSendingPModeStep(this._config)),
                StepEntry.Create(StepOptions.UseDefaults, new CreateAS4MessageStep()),
                StepEntry.Create(StepOptions.UseDefaults,new RetrievePayloadsStep(this._registry.PayloadRetrieverProvider)),
                StepEntry.Create(StepOptions.UseDatastore, new StoreAS4MessageStep(this._dataRepository))
            };
        }

        /// <summary>
        /// Build the Step for the AS4 Receive
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <returns></returns>
        public IStep BuildReceiveStep(InternalMessage internalMessage = null)
        {
            IEnumerable<IStep> steps = GetDecoratedReceiveSteps();
            var decoratorStep = new ReceiveExceptionStepDecorator(
                CompositeSteps(steps), this._outMessageService, this._inExceptionService);

            IList<IStep> undecoratedSteps = GetUnDecoratedReceiveSteps();
            if (undecoratedSteps == null)
                return decoratorStep;

            undecoratedSteps.Insert(0, decoratorStep);
            return CompositeSteps(undecoratedSteps);
        }

        private IEnumerable<IStep> GetDecoratedReceiveSteps()
        {
            IEnumerable<StepEntry> stepEntries = GetReceiveStepEntries();
            return FilterSteps(stepEntries);
        }

        private IList<IStep> GetUnDecoratedReceiveSteps()
        {
            IEnumerable<StepEntry> undecoratedEntries = GetUndecoratedStepEntries();
           return FilterSteps(undecoratedEntries) as IList<IStep>;
        }

        private IEnumerable<StepEntry> GetReceiveStepEntries()
        {
            return new List<StepEntry>
            {
                StepEntry.Create(StepOptions.UseDefaults, new DeterminePModesStep(this._dataRepository, this._config, new PModeRuleVisitor())),
                StepEntry.Create(StepOptions.UseDefaults, new VerifySignatureAS4MessageStep(this._registry.CertificateRepository)),
                StepEntry.Create(StepOptions.UseDefaults, new DecompressAttachmentsStep()),
                StepEntry.Create(StepOptions.UseDatastore, new ReceiveUpdateDatastoreStep(this._inMessageService)),
                StepEntry.Create(StepOptions.UseDefaults, new CreateAS4ReceiptStep()),
                StepEntry.Create(StepOptions.UseDatastore, new StoreAS4ReiptStep(this._outMessageService)),
                StepEntry.Create(StepOptions.UseDefaults, new SignAS4MessageStep(this._registry.CertificateRepository)),
                StepEntry.Create(StepOptions.UseDefaults, new SendAS4ReceiptStep())
            };
        }

        private IEnumerable<StepEntry> GetUndecoratedStepEntries()
        {
            return new List<StepEntry>()
            {
                StepEntry.Create(StepOptions.UseDefaults, new CreateAS4ErrorStep()),
                StepEntry.Create(StepOptions.UseDefaults, new SignAS4MessageStep(this._registry.CertificateRepository))
            };
        }

        /// <summary>
        /// Build the Step for the AS4 Deliver
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <returns></returns>
        public IStep BuildDeliverStep(InternalMessage internalMessage = null)
        {
            IEnumerable<StepEntry> stepEntries = GetDeliverStepEntries();
            IEnumerable<IStep> steps = FilterSteps(stepEntries);

            return new InExceptionStepDecorator(CompositeSteps(steps), this._dataRepository);
        }

        private IEnumerable<StepEntry> GetDeliverStepEntries()
        {
            return new List<StepEntry>
            {
                StepEntry.Create(StepOptions.UseDefaults, new UploadAttachmentsStep(this._registry.AttachmentUploader)),
                StepEntry.Create(StepOptions.UseDefaults, new CreateDeliverMessageStep()),
                StepEntry.Create(StepOptions.UseDefaults,new SendDeliverMessageStep(this._registry.DeliverSenderProvider)),
                StepEntry.Create(StepOptions.UseDatastore, new DeliverUpdateDatastoreStep(this._dataRepository))
            };
        }

        /// <summary>
        /// Build the Step for the AS4 Notify (In)
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <returns></returns>
        public IStep BuildInNotifyStep(InternalMessage internalMessage = null)
        {
            IEnumerable<StepEntry> stepEntries = GetCommonNotifyStepEntries();
            IList<IStep> steps = FilterSteps(stepEntries).ToList();

            steps.Add(new NotifyUpdateInMessageDatastoreStep(this._dataRepository));
            return new InExceptionStepDecorator(CompositeSteps(steps), this._dataRepository);
        }

        /// <summary>
        /// Build the Step for the AS4 (Out)
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <returns></returns>
        public IStep BuildOutNotifyStep(InternalMessage internalMessage = null)
        {
            IEnumerable<StepEntry> stepEntries = GetCommonNotifyStepEntries();
            IList<IStep> steps = FilterSteps(stepEntries).ToList();

            steps.Add(new NotifyUpdateOutMessageDatastoreStep(this._dataRepository));
            return new OutExceptionStepDecorator(CompositeSteps(steps), this._dataRepository);
        }

        private IEnumerable<StepEntry> GetCommonNotifyStepEntries()
        {
            return new List<StepEntry>
            {
                StepEntry.Create(StepOptions.UseDefaults, new CreateNotifyMessageStep()),
                StepEntry.Create(StepOptions.UseDefaults, new SendNotifyMessageStep(this._registry.NotifySenderProvider)),
            };
        }

        /// <summary>
        /// Build the Step for the AS4 Reception Awareness
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <returns></returns>
        public IStep BuildReceptionAwarenessStep(InternalMessage internalMessage = null)
        {
            return new ReceptionAwarenessUpdateDatastoreStep(this._dataRepository, this._inMessageService);
        }

        private IStep CompositeSteps(IEnumerable<IStep> steps)
        {
            return new CompositeStep(steps.ToArray());
        }

        private IEnumerable<IStep> FilterSteps(IEnumerable<StepEntry> stepEntries)
        {
            return stepEntries
                .Where(s => (s.Options & this._options) == s.Options)
                .Select(s => s.Step);
        }
    }

    [Flags]
    public enum StepOptions
    {
        UseDatastore = 2,
        UseDefaults = 1
    }

    public class StepEntry
    {
        public IStep Step { get; set; }
        public StepOptions Options { get; set; }

        private StepEntry(IStep step, StepOptions options)
        {
            this.Step = step;
            this.Options = options;
        }

        public static StepEntry Create(StepOptions options, IStep step)
        {
            return new StepEntry(step, options);
        }
    }
}