using System;
using System.Diagnostics;
using System.Management;

namespace Eu.EDelivery.AS4.IntegrationTests.Fixture
{
    /// <summary>
    /// <see cref="Process"/> implementation to manipulate Child Processes as a Parent.
    /// </summary>
    public class ParentProcess : Process
    {
        private readonly Process[] _childProcesses;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParentProcess"/> class.
        /// </summary>
        /// <param name="childProcesses">The child Processes.</param>
        public ParentProcess(params Process[] childProcesses)
        {
            _childProcesses = childProcesses;
        }

        /// <summary>
        /// Immediately stops the associated process and child processes.
        /// </summary>
        public void KillMeAndChildren()
        {            
            foreach (Process process in _childProcesses)
            {
                KillProcessAndChildren(process.Id);
            }
        }

        private static void KillProcessAndChildren(int pid)
        {
            Console.WriteLine($"Killing process {pid}");

            var processSearcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection processCollection = processSearcher.Get();

            foreach (ManagementBaseObject childProcess in processCollection)
            {
                var childObject = (ManagementObject)childProcess;
                KillProcessAndChildren(Convert.ToInt32(childObject["ProcessID"]));
            }

            try
            {
                Process process = GetProcessById(pid);
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch (ArgumentException)
            {
                // ignored
            }
        }
    }
}
