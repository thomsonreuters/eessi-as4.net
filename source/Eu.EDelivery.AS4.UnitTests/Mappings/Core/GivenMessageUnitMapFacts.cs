using System.Linq;
using Eu.EDelivery.AS4.Mappings.Core;
using Eu.EDelivery.AS4.Model.Core;
using FsCheck;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    public class GivenMessageUnitMapFacts
    {
        [CustomProperty]
        public Property Mapping_UserMessage_Back_And_Forth_Stays_The_Same(UserMessage userMessage)
        {
            // Act
            var result = UserMessageMap.Convert(UserMessageMap.Convert(userMessage));

            // Assert
            return userMessage.CollaborationInfo.Equals(result.CollaborationInfo).Label("equal collaboration")
                   .And(userMessage.Sender.Equals(result.Sender).Label("equal sender"))
                   .And(userMessage.Receiver.Equals(result.Receiver).Label("equal receiver"))
                   .And(userMessage.PayloadInfo.SequenceEqual(result.PayloadInfo).Label("equal part infos"))
                   .And(userMessage.MessageProperties.SequenceEqual(result.MessageProperties).Label("equal message properties"));
        }

        [CustomProperty]
        public Property Mapping_Routing_UserMessage_Back_And_Forth_Reverse_Sender_Receiver_Party(UserMessage userMessage)
        {
            // Act
            var result = UserMessageMap.ConvertFromRouting(UserMessageMap.ConvertToRouting(userMessage));

            // Assert
            return userMessage.CollaborationInfo.Equals(result.CollaborationInfo).Label("equal collaboration")
                   .And(userMessage.Sender.Equals(result.Receiver).Label("equal reversed sender"))
                   .And(userMessage.Receiver.Equals(result.Sender).Label("equal reversed receiver"))
                   .And(userMessage.PayloadInfo.SequenceEqual(result.PayloadInfo).Label("equal part infos"))
                   .And(userMessage.MessageProperties.SequenceEqual(result.MessageProperties).Label("equal message properties"));
        }

        [CustomProperty]
        public Property Mapping_Receipt_Back_And_Forth_Stays_The_Same(Receipt receipt)
        {
            // Act
            var result = ReceiptMap.Convert(ReceiptMap.Convert(receipt), receipt.MultiHopRouting);

            // Assert
            return receipt.MessageId.Equals(result.MessageId).Label("equal message id")
                .And(receipt.RefToMessageId.Equals(result.RefToMessageId).Label("equal ref to message id"))
                .And(receipt.NonRepudiationInformation.Equals(result.NonRepudiationInformation).Label("equal non repudiation"))
                .And(receipt.MultiHopRouting.Equals(result.MultiHopRouting)).Label("equal routing usermessage");
        }

        [CustomProperty]
        public Property Mapping_Error_Back_And_Forth_Stays_The_Same(Error error)
        {
            // Act
            var result = ErrorMap.Convert(ErrorMap.Convert(error), error.MultiHopRouting);

            // Assert
            return error.MessageId.Equals(result.MessageId).Label("equal message id")
                .And(error.RefToMessageId.Equals(result.RefToMessageId).Label("equal ref to message id"))
                .And(error.ErrorLines.SequenceEqual(result.ErrorLines).Label("equal error lines"))
                .And(error.MultiHopRouting.Equals(result.MultiHopRouting).Label("equal routing usermessage"));
        }
    }
}
