using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Eu.EDelivery.AS4.Common;

namespace Eu.EDelivery.AS4.Utilities
{
    /// <summary>
    /// Static Generator of ID's
    /// </summary>
    public static class IdGenerator
    {
        private static readonly Regex MacroMatchRegex = new Regex(@"\{([^\}]+)\}");
        private static readonly Dictionary<string, Func<string>> Macros = new Dictionary<string, Func<string>>
        {
            {"GUID", () => Guid.NewGuid().ToString()},
            {"MACHINENAME", Dns.GetHostName},
            {
                "IPADDRESS", () => Dns.GetHostEntry(Dns.GetHostName()).AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString()
            }
        };

        private static IConfig _config;

        /// <summary>
        /// Generate ID with default format
        /// </summary>
        /// <returns></returns>
        public static string Generate()
        {
            _config = _config ?? Config.Instance;

            if (!_config.IsInitialized)
                _config.Initialize();

            string defaultFormat = _config.GetSetting("idformat");
            if (string.IsNullOrEmpty(defaultFormat))
                defaultFormat = "{GUID}@{IPADDRESS}";

            return Generate(defaultFormat);
        }

        /// <summary>
        /// Set a given <see cref="IConfig"/>
        /// to use when generating the Id
        /// </summary>
        /// <param name="config"></param>
        public static void SetContext(IConfig config)
        {
            IdGenerator._config = config;
        }

        /// <summary>
        /// Generate ID with given format
        /// </summary>
        /// <param name="idFormat"></param>
        /// <returns></returns>
        public static string Generate(string idFormat)
        {
            if (idFormat == null)
                throw new ArgumentNullException(nameof(idFormat));

            if (idFormat.Length == 0)
                throw new ArgumentException("idFormat is invalid.", nameof(idFormat));

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
                idBuilder = idBuilder.Replace(valueToReplace, Macros[macroName]());
            return idBuilder;
        }
    }
}