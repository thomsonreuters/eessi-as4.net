using System.Management.Automation;

namespace Eu.EDelivery.AS4.TestUtils
{
    public static class Computer
    {
        public static void RunCommand(string command)
        {
            using (PowerShell instance = PowerShell.Create())
            {
                instance.AddScript(command);
                instance.Invoke();
            }
        }
    }
}
