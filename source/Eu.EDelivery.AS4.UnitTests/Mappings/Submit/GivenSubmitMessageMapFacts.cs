using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Serialization;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.PMode.CollaborationInfo;
using MessageProperty = Eu.EDelivery.AS4.Model.PMode.MessageProperty;
using Party = Eu.EDelivery.AS4.Model.PMode.Party;
using PartyId = Eu.EDelivery.AS4.Model.PMode.PartyId;
using PartyInfo = Eu.EDelivery.AS4.Model.PMode.PartyInfo;
using Service = Eu.EDelivery.AS4.Model.Core.Service;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    public class GivenSubmitMessageMapFacts
    {
        [Fact]
        public void Create_UserMessage_From_SubmitMessage()
        {
            // Arrange
            const string submitXml =
                @"<?xml version=""1.0""?>
                <SubmitMessage xmlns=""urn:cef:edelivery:eu:as4:messages"">
                  <MessageInfo>
                    <MessageId>F4840B69-8057-40C9-8530-EC91F946C3BF</MessageId>
                  </MessageInfo>
                  <Collaboration>
                    <AgreementRef>
                      <Value>eu.europe.agreements</Value>
                      <PModeId>sample-pmode</PModeId>
                    </AgreementRef>
                  </Collaboration>
                  <MessageProperties>
                    <MessageProperty>
                      <Name>Payloads</Name>
                      <Type>Metadata</Type>
                      <Value>2</Value>
                    </MessageProperty>
                  </MessageProperties>
                  <Payloads>
                    <Payload>
                      <Id>earth</Id>
                      <MimeType>image/jpeg</MimeType>
                      <Location>file:///messages\attachments\earth.jpg</Location>
                      <PayloadProperties/>
                    </Payload>
                    <Payload>
                      <Id>xml-sample</Id>
                      <MimeType>application/xml</MimeType>
                      <Location>file:///messages\attachments\sample.xml</Location>
                      <PayloadProperties>
                        <PayloadProperty>
                          <Name>Important</Name>
                          <Value>Yes</Value>
                        </PayloadProperty>
                      </PayloadProperties>
                    </Payload>
                  </Payloads>
                </SubmitMessage>";

            var submit = AS4XmlSerializer.FromString<SubmitMessage>(submitXml);
            var sendingPMode = new SendingProcessingMode();

            // Act
            UserMessage result = SubmitMessageMap.CreateUserMessage(submit, sendingPMode);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Maybe.Nothing<AgreementReference>(), result.CollaborationInfo.AgreementReference);
            Assert.Single(result.MessageProperties);
            Assert.True(result.PayloadInfo.Count() == 2, "expected 2 part infos");
        }

        [Property]
        public Property Creates_CollaborationInfo_From_Submit_Collaboration(
            NonEmptyString action,
            string pmodeId,
            NonEmptyString agreementValue,
            string agreementType,
            NonEmptyString serviceValue,
            string serviceType,
            NonEmptyString conversationId)
        {
            // Arrange
            var submit = new SubmitMessage
            {
                Collaboration =
                {
                    Action = action.Get,
                    AgreementRef =
                    {
                        Value = agreementValue.Get,
                        RefType = agreementType,
                        PModeId = pmodeId
                    },
                    Service =
                    {
                        Value = serviceValue.Get,
                        Type = serviceType
                    },
                    ConversationId = conversationId.Get
                }
            };
            var sendingPMode = new SendingProcessingMode();

            // Act
            UserMessage result = SubmitMessageMap.CreateUserMessage(submit, sendingPMode);

            // Assert
            var actual = result.CollaborationInfo;

            return actual.Action.Equals(action.Get).Label("equal action")
                .And(actual.Service.Value.Equals(serviceValue.Get).Label("equal service value"))
                .And(actual.AgreementReference.UnsafeGet.Value.Equals(agreementValue.Get).Label("equal agreement value"))
                .And(actual.ConversationId.Equals(conversationId.Get).Label("equal conversation id"));
        }

        [Property]
        public void Use_Test_Defaults_When_Submit_Collaboration_Is_Incomplete(string serviceType)
        {
            // Arrange
            var submit = new SubmitMessage
            {
                Collaboration =
                {
                    Action = null,
                    AgreementRef = null,
                    Service = { Value = null, Type = serviceType }
                }
            };
            var sendingPMode = new SendingProcessingMode
            {
                MessagePackaging = { CollaborationInfo = null }
            };

            // Act
            UserMessage result = SubmitMessageMap.CreateUserMessage(submit, sendingPMode);

            // Assert
            Assert.True(result.IsTest);
            Assert.Equal(AS4.Model.Core.CollaborationInfo.DefaultTest, result.CollaborationInfo);
        }

        [Fact]
        public void Fails_When_Submit_Tries_To_Override_Action()
        {
            // Arrange
            var submit = new SubmitMessage
            {
                Collaboration = { Action = Guid.NewGuid().ToString() }
            };
            var sendingPMode = new SendingProcessingMode
            {
                AllowOverride = false,
                MessagePackaging =
                {
                    CollaborationInfo = new CollaborationInfo
                    {
                        Action = Guid.NewGuid().ToString()
                    }
                }
            };

            // Act / Assert
            Assert.Throws<NotSupportedException>(
                () => SubmitMessageMap.CreateUserMessage(submit, sendingPMode));
        }

        public enum Mapped { Submit, PMode, Default }

        public static IEnumerable<object[]> SubmitMappingFixtures = new[]
        {
            new object[] { false, null, null, Mapped.Default }, 
            new object[] { true, null, null, Mapped.Default },
            new object[] { false, null, Guid.NewGuid().ToString(), Mapped.PMode },
            new object[] { true, null, Guid.NewGuid().ToString(), Mapped.PMode },
            new object[] { false, Guid.NewGuid().ToString(), null, Mapped.Submit },
            new object[] { true, Guid.NewGuid().ToString(), null, Mapped.Submit },
            new object[] { true, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Mapped.Submit }
        };

        [Theory]
        [MemberData(nameof(SubmitMappingFixtures))]
        public void Creates_AgreementReference_From_Either_Submit_Or_SendingPMode(
            bool allowOverride,
            string submitAgreement,
            string pmodeAgreement,
            Mapped expected)
        {
            // Arrange
            var submit = new SubmitMessage
            {
                Collaboration = { AgreementRef = { Value = submitAgreement } }
            };
            var sendingPMode = new SendingProcessingMode
            {
                AllowOverride = allowOverride,
                MessagePackaging =
                {
                    CollaborationInfo = new CollaborationInfo
                    {
                        AgreementReference = { Value = pmodeAgreement }
                    }
                }
            };

            // Act
            UserMessage result = SubmitMessageMap.CreateUserMessage(submit, sendingPMode);

            // Assert
            Mapped actual =
                result.CollaborationInfo
                      .AgreementReference
                      .Select(a => a.Value == pmodeAgreement 
                        ? Mapped.PMode :
                        a.Value == submitAgreement 
                            ? Mapped.Submit 
                            : Mapped.Default)
                      .GetOrElse(Mapped.Default);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Fails_When_Submit_Tries_To_Override_AgreementReference()
        {
            // Arrange
            var submit = new SubmitMessage
            {
                Collaboration = { AgreementRef = { Value = Guid.NewGuid().ToString() } }
            };
            var sendingPMode = new SendingProcessingMode
            {
                AllowOverride = false,
                MessagePackaging =
                {
                    CollaborationInfo = new CollaborationInfo
                    {
                        AgreementReference = { Value = Guid.NewGuid().ToString() }
                    }
                }
            };

            // Act / Assert
            Assert.Throws<NotSupportedException>(
                () => SubmitMessageMap.CreateUserMessage(submit, sendingPMode));
        }

        [Theory]
        [MemberData(nameof(SubmitMappingFixtures))]
        public void Creates_Service_From_Either_Submit_Or_SendingPMode(
            bool allowOverride,
            string submitService,
            string pmodeService,
            Mapped expected)
        {
            // Arrange
            var submit = new SubmitMessage
            {
                Collaboration = { Service = { Value = submitService } }
            };
            var sendingPMode = new SendingProcessingMode
            {
                AllowOverride = allowOverride,
                MessagePackaging =
                {
                    CollaborationInfo = new CollaborationInfo
                    {
                        Service = { Value = pmodeService }
                    }
                }
            };

            // Act
            UserMessage result = SubmitMessageMap.CreateUserMessage(submit, sendingPMode);

            // Assert
            string userService = result.CollaborationInfo.Service.Value;
            Mapped actual =
                userService == pmodeService
                    ? Mapped.PMode
                    : userService == submitService
                        ? Mapped.Submit
                        : Mapped.Default;

            Assert.Equal(expected, actual);
            Assert.True(
                (actual == Mapped.Default) == (result.CollaborationInfo.Service.Equals(Service.TestService)),
                "fallback on test Service when none in Submit and SendingPMode is defined");
        }

        [Fact]
        public void Fails_When_Submit_Tries_To_Override_Service()
        {
            // Arrange
            var submit = new SubmitMessage
            {
                Collaboration = { Service = { Value = Guid.NewGuid().ToString() } }
            };
            var sendingPMode = new SendingProcessingMode
            {
                MessagePackaging =
                {
                    CollaborationInfo = new CollaborationInfo
                    {
                        Service = { Value = Guid.NewGuid().ToString() }
                    }
                }
            };

            // Act / Assert
            Assert.Throws<NotSupportedException>(
                () => SubmitMessageMap.CreateUserMessage(submit, sendingPMode));
        }

        [Theory]
        [MemberData(nameof(SubmitMappingFixtures))]
        public void Creates_FromParty_From_Either_Submit_Or_SendingPMode(
            bool allowOverride,
            string submitFromParty,
            string pmodeFromParty,
            Mapped expected)
        {
            // Arrange
            var submit = new SubmitMessage
            {
                PartyInfo =
                {
                    FromParty = submitFromParty != null
                        ? new AS4.Model.Common.Party
                        {
                            Role = Guid.NewGuid().ToString(),
                            PartyIds = new[] { new AS4.Model.Common.PartyId { Id = submitFromParty } }
                        }
                        : null
                }
            };
            var sendingPMode = new SendingProcessingMode
            {
                AllowOverride = allowOverride,
                MessagePackaging =
                {
                    PartyInfo = new PartyInfo
                    {
                        FromParty = pmodeFromParty != null
                            ? new Party(Guid.NewGuid().ToString(), new PartyId(pmodeFromParty))
                            : null
                    }
                }
            };

            // Act
            UserMessage result = SubmitMessageMap.CreateUserMessage(submit, sendingPMode);

            // Assert
            Mapped actual =
                result.Sender.PartyIds.First().Id == pmodeFromParty
                    ? Mapped.PMode
                    : result.Sender.PartyIds.First().Id == submitFromParty
                        ? Mapped.Submit
                        : Mapped.Default;

            Assert.Equal(expected, actual);
            Assert.True(
                (actual == Mapped.Default) == (result.Sender.Equals(AS4.Model.Core.Party.DefaultFrom)),
                "fallback on default FromParty when none in Submit and SendingPMode is defined");
        }

        [Fact]
        public void Fails_When_Submit_Tries_To_Override_FromParty()
        {
            // Arrange
            var submit = new SubmitMessage
            {
                PartyInfo =
                {
                    FromParty = new AS4.Model.Common.Party
                    {
                        Role = Guid.NewGuid().ToString(),
                        PartyIds = new[] { new AS4.Model.Common.PartyId { Id = Guid.NewGuid().ToString() } }
                    }

                }
            };
            var sendingPMode = new SendingProcessingMode
            {
                AllowOverride = false,
                MessagePackaging =
                {
                    PartyInfo = new PartyInfo
                    {
                        FromParty = new Party(Guid.NewGuid().ToString(), new PartyId(Guid.NewGuid().ToString()))
                    }
                }
            };

            // Act / Assert
            Assert.Throws<NotSupportedException>(
                () => SubmitMessageMap.CreateUserMessage(submit, sendingPMode));
        }

        [Theory]
        [MemberData(nameof(SubmitMappingFixtures))]
        public void Creates_ToParty_From_Either_Submit_Or_SendingPMode(
            bool allowOverride,
            string submitToParty,
            string pmodeToParty,
            Mapped expected)
        {
            // Arrange
            var submit = new SubmitMessage
            {
                PartyInfo =
                {
                    ToParty = submitToParty != null
                        ? new AS4.Model.Common.Party
                        {
                            Role = Guid.NewGuid().ToString(),
                            PartyIds = new[] { new AS4.Model.Common.PartyId { Id = submitToParty } }
                        }
                        : null
                }
            };
            var sendingPMode = new SendingProcessingMode
            {
                AllowOverride = allowOverride,
                MessagePackaging =
                {
                    PartyInfo = new PartyInfo
                    {
                        ToParty = pmodeToParty != null
                            ? new Party(Guid.NewGuid().ToString(), new PartyId(pmodeToParty))
                            : null
                    }
                }
            };

            // Act
            UserMessage result = SubmitMessageMap.CreateUserMessage(submit, sendingPMode);

            // Assert
            Mapped actual =
                result.Receiver.PartyIds.First().Id == pmodeToParty
                    ? Mapped.PMode
                    : result.Receiver.PartyIds.First().Id == submitToParty
                        ? Mapped.Submit
                        : Mapped.Default;

            Assert.Equal(expected, actual);
            Assert.True(
                (actual == Mapped.Default) == (result.Receiver.Equals(AS4.Model.Core.Party.DefaultTo)),
                "fallback on default ToParty when none in Submit and SendingPMode is defined");
        }

        [Fact]
        public void Fails_When_Submit_Tries_To_Override_ToParty()
        {
            // Arrange
            var submit = new SubmitMessage
            {
                PartyInfo =
                {
                    ToParty = new AS4.Model.Common.Party
                    {
                        Role = Guid.NewGuid().ToString(),
                        PartyIds = new[] { new AS4.Model.Common.PartyId { Id = Guid.NewGuid().ToString() } }
                    }
                }
            };
            var sendingPMode = new SendingProcessingMode
            {
                AllowOverride = false,
                MessagePackaging =
                {
                    PartyInfo = new PartyInfo
                    {
                        ToParty = new Party(Guid.NewGuid().ToString(), new PartyId(Guid.NewGuid().ToString()))
                    }
                }
            };

            // Act / Assert
            Assert.Throws<NotSupportedException>(
                () => SubmitMessageMap.CreateUserMessage(submit, sendingPMode));
        }

        [Theory]
        [MemberData(nameof(SubmitMappingFixtures))]
        public void Resolves_Mpc_From_Either_Submit_Or_SendingPMode(
            bool allowOverride,
            string submitMpc,
            string pmodeMpc,
            Mapped expected)
        {
            // Arrange
            var submit = new SubmitMessage { MessageInfo = { Mpc = submitMpc } };
            var sendingPMode = new SendingProcessingMode
            {
                AllowOverride = allowOverride,
                MessagePackaging = { Mpc = pmodeMpc }
            };

            // Act
            UserMessage result = SubmitMessageMap.CreateUserMessage(submit, sendingPMode);

            // Assert
            Mapped actual = 
                result.Mpc == pmodeMpc 
                    ? Mapped.PMode 
                    : result.Mpc == submitMpc 
                        ? Mapped.Submit 
                        : Mapped.Default;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Fails_When_Submit_Tries_To_Override_Mpc()
        {
            // Arrange
            var submit = new SubmitMessage { MessageInfo = { Mpc = Guid.NewGuid().ToString() } };
            var sendingPMode = new SendingProcessingMode
            {
                AllowOverride = false,
                MessagePackaging = { Mpc = Guid.NewGuid().ToString() }
            };

            // Act / Assert
            Assert.Throws<NotSupportedException>(
                () => SubmitMessageMap.CreateUserMessage(submit, sendingPMode));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Assign_Compression_Properties_To_Payloads_When_UseCompression_In_SendingPMode_Is_On(
            bool useCompression)
        {
            // Arrange
            var submit = new SubmitMessage
            {
                Payloads = new[]
                {
                    new Payload("xml-payload")
                    {
                        PayloadProperties = new []
                        {
                            new PayloadProperty("DocumentType", "Business Document")
                        }
                    },
                    new Payload("image-payload")
                }
            };
            var sendingPMode = new SendingProcessingMode
            {
                MessagePackaging =
                {
                    UseAS4Compression = useCompression
                }
            };

            // Act
            UserMessage result = SubmitMessageMap.CreateUserMessage(submit, sendingPMode);

            // Assert
            Assert.True(2 == result.PayloadInfo.Count(), "expect 2 part infos");
            Assert.All(result.PayloadInfo, p => Assert.StartsWith("cid:", p.Href));
            Assert.True(
                result.PayloadInfo.First().Properties.Count >= 1,
                "original payload property is present");
            Assert.True(
                useCompression == result.PayloadInfo.All(p => p.Properties.ContainsKey("CompressionType")), 
                "expect all part infos to have a 'CompressionType' property");
        }


        [Fact]
        public void Combine_Submit_And_SendingPMode_MessageProperties()
        {
            // Arrange
            var submit = new SubmitMessage
            {
                MessageProperties = new []
                {
                    new AS4.Model.Common.MessageProperty { Name = "originalSender", Type = "Important", Value = "Holodeck" }, 
                    new AS4.Model.Common.MessageProperty { Name = "finalRecipient", Value = "AS4.NET" }, 
                }
            };
            var sendingPMode = new SendingProcessingMode
            {
                MessagePackaging =
                {
                    MessageProperties = new List<MessageProperty>
                    {
                        new MessageProperty { Name = "capability", Type = "info", Value = "receiving" },
                        new MessageProperty { Name = "endpoint", Value = "international" },
                    }
                }
            };

            // Act
            UserMessage result = SubmitMessageMap.CreateUserMessage(submit, sendingPMode);

            // Assert
            Assert.Collection(
                result.MessageProperties,
                p => Assert.Equal(("originalSender", "Important", "Holodeck"), (p.Name, p.Type, p.Value)),
                p => Assert.Equal(("finalRecipient", "AS4.NET"), (p.Name, p.Value)),
                p => Assert.Equal(("capability", "info", "receiving"), (p.Name, p.Type, p.Value)),
                p => Assert.Equal(("endpoint", "international"), (p.Name, p.Value)));
        }

        [Fact]
        public void Use_Submit_MessageProperties_When_SendingPMode_MessageProperties_Are_Empty()
        {
            // Arrange
            var submit = new SubmitMessage
            {
                MessageProperties = new[]
                {
                    new AS4.Model.Common.MessageProperty { Name = "originalSender", Type = "Important", Value = "Holodeck" },
                    new AS4.Model.Common.MessageProperty { Name = "finalRecipient", Value = "AS4.NET" },
                }
            };
            var sendingPMode = new SendingProcessingMode
            {
                MessagePackaging =
                {
                    MessageProperties = null
                }
            };

            // Act
            UserMessage result = SubmitMessageMap.CreateUserMessage(submit, sendingPMode);

            // Assert
            Assert.Collection(
                result.MessageProperties,
                p => Assert.Equal(("originalSender", "Important", "Holodeck"), (p.Name, p.Type, p.Value)),
                p => Assert.Equal(("finalRecipient", "AS4.NET"), (p.Name, p.Value)));

        }

        [Fact]
        public void Use_SendingPMode_MessageProperties_When_Submit_MessageProperties_Are_Empty()
        {
            // Arrange
            var submit = new SubmitMessage
            {
                MessageProperties = null
            };
            var sendingPMode = new SendingProcessingMode
            {
                MessagePackaging =
                {
                    MessageProperties = new List<MessageProperty>
                    {
                        new MessageProperty { Name = "capability", Type = "info", Value = "receiving" },
                        new MessageProperty { Name = "endpoint", Value = "international" },
                    }
                }
            };

            // Act
            UserMessage result = SubmitMessageMap.CreateUserMessage(submit, sendingPMode);

            // Assert
            Assert.Collection(
                result.MessageProperties,
                p => Assert.Equal(("capability", "info", "receiving"), (p.Name, p.Type, p.Value)),
                p => Assert.Equal(("endpoint", "international"), (p.Name, p.Value)));
        }
    }
}
