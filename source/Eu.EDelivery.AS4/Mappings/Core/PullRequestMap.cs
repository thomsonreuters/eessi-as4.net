using System;

namespace Eu.EDelivery.AS4.Mappings.Core
{
    internal class PullRequestMap
    {
        /// <summary>
        /// Maps from a XML representation to a domain model representation of an AS4 pull request.
        /// </summary>
        /// <param name="xml">The XML representation to convert.</param>
        internal static Model.Core.PullRequest Convert(Xml.SignalMessage xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            if (xml.PullRequest == null)
            {
                throw new ArgumentException(
                    @"Cannot create PullRequest domain model from a XML representation without a PullRequest element",
                    nameof(xml.PullRequest));
            }

            return new Model.Core.PullRequest(xml.PullRequest?.mpc);
        }

        /// <summary>
        /// Maps from a domain model representation to a XML representation of an AS4 pull request.
        /// </summary>
        /// <param name="model">The domain model to convert.</param>
        internal static Xml.SignalMessage Convert(Model.Core.PullRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return new Xml.SignalMessage
            {
                MessageInfo = new Xml.MessageInfo
                {
                    MessageId = model.MessageId,
                },
                PullRequest = new Xml.PullRequest
                {
                    mpc = model.Mpc
                }
            };
        }
    }
}