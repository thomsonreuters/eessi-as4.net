using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Factories
{
    public static class PayloadFileNameFactory
    {
        private static readonly Regex MacroMatchRegex = new Regex(@"\{([^\}]+)\}");

        private static readonly Dictionary<string, Func<Attachment, UserMessage, string>> NamingMacros
            = new Dictionary<string, Func<Attachment, UserMessage, string>>(StringComparer.OrdinalIgnoreCase)
            {
                {"MessageId", (attachment, userMessage) => userMessage.MessageId},
                {"AttachmentId", (attachment, userMessage) => attachment.Id}
            };

        public static string CreateFileName(string pattern, Attachment attachment, UserMessage userMessage)
        {
            if (String.IsNullOrEmpty(pattern))
            {
                pattern = "{AttachmentId}";
            }

            var idBuilder = new StringBuilder(pattern);

            var matches = MacroMatchRegex.Matches(pattern);

            foreach (Match match in matches)
            {
                idBuilder = ReplaceValueWithMacro(idBuilder, match, attachment, userMessage);
            }

            return idBuilder.ToString();
        }

        private static StringBuilder ReplaceValueWithMacro(StringBuilder idBuilder, Match match, Attachment attachment, UserMessage userMessage)
        {
            string valueToReplace = match.Groups[0].Value;
            string macroName = match.Groups[1].Value;

            if (NamingMacros.ContainsKey(macroName))
            {
                idBuilder = idBuilder.Replace(valueToReplace, NamingMacros[macroName](attachment, userMessage));
            }

            return idBuilder;
        }
    }
}