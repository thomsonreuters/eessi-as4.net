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
        private static readonly Regex MacroMatchRegex;
        private static readonly Dictionary<string, Func<string>> Macros;

        private static IConfig _config;
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
            IPAddress[] addressList = Dns.GetHostEntry(hostName).AddressList;
            Func<IPAddress, bool> whereIpIsInterNetwork = ip => ip.AddressFamily == AddressFamily.InterNetwork;

            return addressList.FirstOrDefault(whereIpIsInterNetwork)?.ToString();
        }

        /// <summary>
        /// Generate ID with default format
        /// </summary>
        /// <returns></returns>
        public string Create()
        {
            _config = _config ?? Config.Instance;

            if (!_config.IsInitialized)
            {
                _config.Initialize();
            }

            string defaultFormat = _config.GetSetting("idformat");
            if (string.IsNullOrEmpty(defaultFormat))
            {
                defaultFormat = "{GUID}@{IPADDRESS}";
            }

            return Create(defaultFormat);
        }

        /// <summary>
        /// Set a given <see cref="IConfig"/>
        /// to use when generating the Id
        /// </summary>
        /// <param name="config"></param>
        public void SetContext(IConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Generate ID with given format
        /// </summary>
        /// <param name="idFormat"></param>
        /// <returns></returns>
        internal string Create(string idFormat)
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

        private StringBuilder ReplaceValueWithMacro(StringBuilder idBuilder, Match match)
        {
            string valueToReplace = match.Groups[0].Value;
            string macroName = match.Groups[1].Value;

            if (Macros.ContainsKey(macroName))
                idBuilder = idBuilder.Replace(valueToReplace, Macros[macroName]());

            return idBuilder;
        }
    }
}