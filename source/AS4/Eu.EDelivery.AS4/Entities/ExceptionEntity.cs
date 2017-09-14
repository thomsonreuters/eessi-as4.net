using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// Entity for Exceptions
    /// </summary>
    public class ExceptionEntity : Entity
    {
        public string EbmsRefToMessageId { get; set; }

        public string Exception { get; set; }

        public string PMode { get; protected set; }
        public string PModeId { get; protected set; }

        /// <summary>
        /// Set the Id & string represenation of the PMode that is used to process the message.
        /// </summary>
        /// <param name="pmodeId"></param>
        /// <param name="pmodeContent"></param>
        public void SetPModeInformation(string pmodeId, string pmodeContent)
        {
            PModeId = pmodeId;
            PMode = pmodeContent;
        }

        /// <summary>
        /// Set the PMode that is used to process the message.
        /// </summary>
        /// <param name="pmode"></param>
        public async Task SetPModeInformationAsync(IPMode pmode)
        {
            if (pmode != null)
            {
                PModeId = pmode.Id;

                // The Xml Serializer is not able to serialize an interface, therefore
                // the argument must first be cast to a correct implementation.

                if (pmode is SendingProcessingMode sp)
                {
                    PMode = await AS4XmlSerializer.ToStringAsync(sp);
                }
                else if (pmode is ReceivingProcessingMode rp)
                {
                    PMode = await AS4XmlSerializer.ToStringAsync(rp);
                }
                else
                {
                    throw new NotImplementedException("Unable to serialize the the specified IPMode");
                }
            }
        }

        public byte[] MessageBody { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEntity"/> class.
        /// </summary>
        public ExceptionEntity()
        {
            SetOperation(default(Operation));
        }

        public void SetOperation(Operation operation)
        {
            Operation = operation.ToString();
        }

        [Column("Operation")]
        [MaxLength(50)]
        public string Operation { get; private set; }

        /// <summary>
        /// Update the <see cref="Entity" /> to lock it with a given <paramref name="value" />.
        /// </summary>
        /// <param name="value">Value indicating the <see cref="Entity" /> is locked.</param>
        public override void Lock(string value)
        {
            var updatedOperation = OperationUtils.Parse(value);

            if (updatedOperation != AS4.Entities.Operation.NotApplicable)
            {
                SetOperation(updatedOperation);
            }
        }
    }
}