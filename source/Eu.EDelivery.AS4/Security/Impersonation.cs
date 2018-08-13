using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Eu.EDelivery.AS4.Security
{
    /// <summary>
    /// Impersonation static class to handle the security about files in Windows
    /// </summary>
    public static class Impersonation
    {
        private static class NativeMethods
        {
            [DllImport("advapi32.DLL", SetLastError = true)]
            public static extern int LogonUser(
                string lpszUsername,
                string lpszDomain,
                string lpszPassword,
                int dwLogonType,
                int dwLogonProvider,
                out IntPtr phToken);
        }

        public static object Impersonate(string user, string password)
        {
            string domain = "";

            int domainSeparatorPosition = user.IndexOf(@"\", StringComparison.OrdinalIgnoreCase);

            if (domainSeparatorPosition > 0)
            {
                domain = user.Substring(0, domainSeparatorPosition);
                user = user.Substring(domainSeparatorPosition + 1);
            }

            IntPtr securityToken;

            NativeMethods.LogonUser(user, domain, password, 9, 0, out securityToken);
            if (securityToken == IntPtr.Zero)
                throw new InvalidOperationException(
                    "The username or password combination was invalid, please verify your settings");

            var newIdentity = new WindowsIdentity(securityToken);
            WindowsImpersonationContext impersonationContext = newIdentity.Impersonate();

            return impersonationContext;
        }

        public static void UndoImpersonation(object impersonationContext)
        {
            var context = impersonationContext as WindowsImpersonationContext;
            context?.Undo();
        }
    }
}