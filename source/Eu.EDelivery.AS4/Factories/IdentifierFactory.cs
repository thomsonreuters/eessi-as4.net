using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Eu.EDelivery.AS4.Common;

namespace Eu.EDelivery.AS4.Factories
{
    /// <summary>
    /// Factory to create Entity ID's
    /// </summary>
    public class IdentifierFactory
    {
        private const string DefaultIdFormat = "{GUID}@{IPADDRESS}";

        private static readonly Regex MacroMatchRegex;
        private static readonly Dictionary<string, Func<string>> Macros;

        public static readonly IdentifierFactory Instance = new IdentifierFactory();

        static IdentifierFactory()
        {
            MacroMatchRegex = new Regex(@"\{([^\}]+)\}");
            Macros = new Dictionary<string, Func<string>>
            {
                {"GUID", () => Guid.NewGuid().ToString()},
                {"MACHINENAME", Dns.GetHostName},
                {"IPADDRESS", GetHostIpAddress}
            };
        }

        private static string GetHostIpAddress()
        {
            string hostName = Dns.GetHostName();

            return Dns.GetHostEntry(hostName).AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?
                .ToString();
        }

        /// <summary>
        /// Generate ID with default format
        /// </summary>
        /// <returns></returns>
        public string Create()
        {
            if (!Config.Instance.IsInitialized) 
            {
                return Create(DefaultIdFormat);
            }

            string defaultFormat = Config.Instance.EbmsMessageIdFormat;
            if (!string.IsNullOrEmpty(defaultFormat))
            {
                return Create(defaultFormat);
            }

            return Create(DefaultIdFormat);
        }

        /// <summary>
        /// Generate ID with given format
        /// </summary>
        /// <param name="idFormat"></param>
        /// <returns></returns>
        public string Create(string idFormat)
        {
            if (idFormat == null)
            {
                throw new ArgumentNullException(nameof(idFormat));
            }

            if (idFormat.Length == 0)
            {
                throw new ArgumentException(@"idFormat is invalid.", nameof(idFormat));
            }

            var idBuilder = new StringBuilder(idFormat);

            idBuilder = MacroMatchRegex.Matches(idFormat).Cast<Match>()
                .Aggregate(idBuilder, ReplaceValueWithMacro);

            return idBuilder.ToString();
        }

        private static StringBuilder ReplaceValueWithMacro(StringBuilder idBuilder, Match match)
        {
            string valueToReplace = match.Groups[0].Value;
            string macroName = match.Groups[1].Value;

            if (Macros.ContainsKey(macroName))
            {
                idBuilder = idBuilder.Replace(valueToReplace, Macros[macroName]());
            }

            return idBuilder;
        }
    }
}