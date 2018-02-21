using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.ServiceHandler;
using Eu.EDelivery.AS4.ServiceHandler.Agents;

namespace Eu.EDelivery.AS4.WindowsService
{
    public partial class AS4Service : ServiceBase
    {
        private readonly EventLog _eventLog;

        private Kernel _kernel;
        private Task _rootTask, _feTask, _payloadServiceTask;
        private CancellationTokenSource _cancellation;

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4Service"/> class.
        /// </summary>
        public AS4Service()
        {
            InitializeComponent();
            AutoLog = false;

            _eventLog = new EventLog("Application", ".", "AS4.NET Component");
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command. </param>
        protected override void OnStart(string[] args)
        {
            _eventLog.WriteEntry("Starting AS4.NET Component Service");

            try
            {
                Config configuration = Config.Instance;
                Registry registration = Registry.Instance;
                configuration.Initialize("settings-service.xml");
            
                if (!configuration.IsInitialized)
                {
                    _eventLog.WriteEntry("AS4.NET Component cannot be initialized");

                    Stop();
                    return;
                }

                string certificateTypeRepository = configuration.GetSetting("CertificateRepository");

                registration.CertificateRepository =
                    !string.IsNullOrEmpty(certificateTypeRepository)
                        ? GenericTypeBuilder.FromType(certificateTypeRepository).Build<ICertificateRepository>()
                        : new CertificateRepository();
            

                registration.CreateDatastoreContext = () => new DatastoreContext(configuration);

                var provider = new AgentProvider(configuration);
                _kernel = new Kernel(provider.GetAgents());

                _cancellation = new CancellationTokenSource();
                _rootTask = _kernel.StartAsync(_cancellation.Token);

                _feTask = StartFeInProcess(_cancellation.Token);
                _payloadServiceTask = StartPayloadServiceInProcess(_cancellation.Token);

                _eventLog.WriteEntry("AS4.NET Component Service is started");
            }
            catch (Exception ex)
            {
                _eventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
        }

        private static Task StartFeInProcess(CancellationToken cancellation)
        {
            if (!Config.Instance.FeInProcess)
            {
                return Task.CompletedTask;
            }

            return Task.Factory.StartNew(() => Fe.Program.StartInProcess(cancellation), cancellation);
        }

        private static Task StartPayloadServiceInProcess(CancellationToken cancellation)
        {
            if (!Config.Instance.PayloadServiceInProcess)
            {
                return Task.CompletedTask;
            }

            return Task.Factory.StartNew(() => PayloadService.Program.Start(cancellation), cancellation);
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            _eventLog.WriteEntry("Stopping AS4.NET Component Service");

            try
            {
                _cancellation?.Cancel();

                StopTask(_rootTask);
                StopTask(_feTask);
                StopTask(_payloadServiceTask);

                _kernel?.Dispose();
                Config.Instance.Dispose();

                _eventLog.WriteEntry("AS4.NET Component Service is stopped");
            }
            catch (Exception ex)
            {
                _eventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
        }

        private static void StopTask(Task task)
        {
            if (task == null) { return; }

            try
            {
                task.GetAwaiter().GetResult();
            }
            catch (AggregateException exception)
            {
                exception.Handle(e => e is TaskCanceledException);
            }
            finally
            {
                task.Dispose();
            }
        }
    }
}
