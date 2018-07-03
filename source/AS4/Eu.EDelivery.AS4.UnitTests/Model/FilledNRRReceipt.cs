using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    public class FilledNRRReceipt : Receipt
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilledNRRReceipt"/> class.
        /// </summary>
        public FilledNRRReceipt()
            : base(
                "ignored-id",
                "ref-to-message-id",
                DateTimeOffset.Now,
                new NonRepudiationInformation
                {
                    MessagePartNRInformation =
                        new List<MessagePartNRInformation>
                        {
                            new MessagePartNRInformation
                            {
                                Reference =
                                    new Reference
                                    {
                                        DigestMethod = new ReferenceDigestMethod("ignored algorithm"),
                                        DigestValue = new byte[0],
                                        URI = "ignored URI",
                                        Transforms =
                                            new List<ReferenceTransform> { new ReferenceTransform("ignored algorithm") }
                                    }
                            }
                        }
                }) { }
    }
}
