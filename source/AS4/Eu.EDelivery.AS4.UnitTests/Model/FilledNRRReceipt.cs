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
                new NonRepudiationInformation(new []
                {
                    new Reference(
                        "ignored URI",
                        new List<ReferenceTransform> { new ReferenceTransform("ignored algorithm") },
                        new ReferenceDigestMethod("ignored algorithm"),
                        digestValue: new byte[0])
                })) { }
    }
}
