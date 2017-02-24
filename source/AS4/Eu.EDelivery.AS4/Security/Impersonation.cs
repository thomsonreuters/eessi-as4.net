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

            [DllImport("advapi32.DLL")]
            public static extern bool ImpersonateLoggedOnUser(IntPtr hToken); //handle to token for logged-on user 

            [DllImport("advapi32.DLL")]
            public static extern bool RevertToSelf();
        }

        public static object Impersonate(string user, string password)
        {
            string domain = "";
            // ReSharper disable once StringIndexOfIsCultureSpecific.1
            if (user.IndexOf(@"\") > 0)
            {
                // ReSharper disable once StringIndexOfIsCultureSpecific.1
                domain = user.Substring(0, user.IndexOf(@"\"));
                // ReSharper disable once StringIndexOfIsCultureSpecific.1
                user = user.Substring(user.IndexOf(@"\") + 1);
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