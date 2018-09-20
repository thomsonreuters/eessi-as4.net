using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Factories
{
    internal static class PayloadFileNameFactory
    {
        private static readonly Regex MacroMatchRegex = new Regex(@"\{([^\}]+)\}");

        private static readonly Dictionary<string, Func<Attachment, UserMessage, string>> NamingMacros
            = new Dictionary<string, Func<Attachment, UserMessage, string>>(StringComparer.OrdinalIgnoreCase)
            {
                {"MessageId", (attachment, userMessage) => userMessage.MessageId},
                {"AttachmentId", (attachment, userMessage) => attachment.Id}
            };

        public const string PatternDocumentation =
                   "The Payload naming pattern lets you define how the filename of the delivered payloads should look like. \n\r" +
                   "There are a few macro's that can be used to define the pattern:\n\r" +
                   "{MessageId}: inserts the ebMS MessageId in the filename.\n\r" +
                   "{AttachmentId}: inserts the Id of the attachment in the filename.\n\r" +
                   "The macro's can be combined which means that it is possible to use {MessageId}_{AttachmentId} for instance.\n\r" +
                   "If no pattern is defined, {AttachmentId} will be used by default";

        public static string CreateFileName(string pattern, Attachment attachment, UserMessage userMessage)
        {
            if (String.IsNullOrEmpty(pattern))
            {
                pattern = "{AttachmentId}";
            }
            else
            {
                if (NamingMacros.Keys.Any(p => pattern.IndexOf(p, StringComparison.OrdinalIgnoreCase) > -1) == false)
                {
                    pattern = pattern.TrimEnd('_') + "_{AttachmentId}";
                }
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