using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.WindowsService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            CreateEventSourceIfNeeded();
        }

        private const string LogName = "Application",
                             Source = "AS4.NET Component";

        private void CreateEventSourceIfNeeded()
        {
            if (!EventLog.SourceExists(Source))
            {
                EventLog.CreateEventSource(Source, LogName);
            }
        }
    }
}
