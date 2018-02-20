using System;

namespace Eu.EDelivery.AS4.Exceptions
{
    public class ErrorResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResult" /> class.
        /// </summary>
        public ErrorResult(string description, ErrorAlias alias)
        {
            Description = description;
            Code = ErrorCodeUtils.GetErrorCode(alias);
            Alias = alias;
        }

        public string Description { get; }

        public ErrorCode Code { get; }

        public ErrorAlias Alias { get; }

        public string GetAliasDescription()
        {
            if (Alias == ErrorAlias.NonApplicable)
            {
                return string.Empty;
            }

            return Alias.ToString();
        }
    }
}
