using System;
using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using SignalMessage = Eu.EDelivery.AS4.Xml.SignalMessage;
using XmlError = Eu.EDelivery.AS4.Xml.Error;
using CoreError = Eu.EDelivery.AS4.Model.Core.Error;
using Property = FsCheck.Property;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    public class GivenErrorMapFacts
    {
        public class GivenValidArguments : GivenErrorMapFacts
        {
            [Property]
            public Property Error_Origin_Is_Correctly_Mapped(string origin)
            {
                // Arrange
                XmlError error = CreateBasicError();
                error.origin = origin;

                // Act
                CoreError actual = MapErrorXmlToModel(error);
                
                // Assert
                Maybe<string> originM = actual.ErrorLines.First().Origin;
                return (originM == Maybe<string>.Nothing)
                    .Label("mapped origin is nothing")
                    .And(origin == null)
                    .Label("incoming origin is 'null'")
                    .Or(() => origin == originM.UnsafeGet)
                    .Label("incoming origin = mapped origin");
            }

            [Property]
            public Property Error_Category_Is_Correctly_Mapped(string category)
            {
                // Arrange
                XmlError error = CreateBasicError();
                error.category = category;

                // Act
                CoreError actual = MapErrorXmlToModel(error);

                // Assert
                Maybe<string> catM = actual.ErrorLines.First().Category;
                return (catM == Maybe<string>.Nothing)
                    .Label("mapped category is nothing")
                    .And(category == null)
                    .Label("incoming category is 'null'")
                    .Or(() => category == catM.UnsafeGet)
                    .Label("incoming category = mapped category");
            }

            [Property]
            public Property Error_Detail_Is_Correctly_Mapped(string detail)
            {
                // Arrange
                XmlError error = CreateBasicError();
                error.ErrorDetail = detail;

                // Act
                CoreError actual = MapErrorXmlToModel(error);

                // Assert
                Maybe<string> detailM = actual.ErrorLines.First().Detail;
                return (detailM == Maybe<string>.Nothing)
                    .Label("mapped detail is nothing")
                    .And(detail == null)
                    .Label("incoming detail is 'null'")
                    .Or(() => detail == detailM.UnsafeGet);
            }

            [CustomProperty]
            public Property Error_Description_Is_Correctly_Mapped(
                Maybe<Tuple<NonNull<string>, NonNull<string>>> desc)
            {
                // Arrange
                XmlError error = CreateBasicError();
                desc.Do(t => error.Description = new Description { lang = t.Item1.Get, Value = t.Item2.Get });

                // Act
                CoreError actual = MapErrorXmlToModel(error);

                // Assert
                Maybe<ErrorDescription> descM = actual.ErrorLines.First().Description;
                return (descM == Maybe<ErrorDescription>.Nothing)
                    .Label("mapped description is nothing")
                    .And(desc == Maybe<Tuple<NonNull<string>, NonNull<string>>>.Nothing)
                    .Label("incoming description is empty")
                    .Or(() => desc.UnsafeGet.Item1.Get == descM.UnsafeGet.Language
                              && desc.UnsafeGet.Item2.Get == descM.UnsafeGet.Value)
                    .Label("incoming description = mapped description");
            }

            [Property]
            public Property Error_RefToMessageInError_Is_Correctly_Mapped(string refTo)
            {
                // Arrange
                XmlError error = CreateBasicError();
                error.refToMessageInError = refTo;

                // Act
                CoreError actual = MapErrorXmlToModel(error);

                // Assert
                Maybe<string> refToM = actual.ErrorLines.First().RefToMessageInError;
                return (refToM == Maybe<string>.Nothing)
                    .Label("mapped reference is nothing")
                    .And(refTo == null)
                    .Label("incoming reference is 'nul'")
                    .Or(() => refToM.UnsafeGet == refTo)
                    .Label("mapped reference = incoming reference");
            }

            private static XmlError CreateBasicError()
            {
                return new XmlError
                {
                    errorCode = Guid.NewGuid().ToString(),
                    shortDescription = Guid.NewGuid().ToString(),
                    severity = Guid.NewGuid().ToString(),
                    origin = Guid.NewGuid().ToString()
                };
            }

            private static CoreError MapErrorXmlToModel(XmlError error)
            {
                return AS4Mapper.Map<CoreError>(
                    new SignalMessage
                    {
                        MessageInfo = new AS4.Xml.MessageInfo
                        {
                            MessageId = $"error-{Guid.NewGuid()}",
                            RefToMessageId = $"reftoid-{Guid.NewGuid().ToString()}",
                            Timestamp = DateTime.UtcNow
                        },
                        Error = new[] { error }
                    });
            }

            [Fact]
            public void ThenErrorDescriptionIsCorreclyMapped()
            {
                // Arrange
                string expectedValue = Guid.NewGuid().ToString();
                string expectedLanguage = Guid.NewGuid().ToString();

                SignalMessage signalMessage = GetPopulatedXmlError();
                signalMessage.Error[0].Description.Value = expectedValue;
                signalMessage.Error[0].Description.lang = expectedLanguage;

                // Act
                var error = AS4Mapper.Map<CoreError>(signalMessage);

                // Assert
                ErrorDescription actual = error.ErrorLines.First().Description.UnsafeGet;
                Assert.Equal(expectedValue, actual.Value);
                Assert.Equal(expectedLanguage, actual.Language);
            }

            [Fact]
            public void ThenMessageIdIsCorrectlyMapped()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                SignalMessage signalMessage = GetPopulatedXmlError();
                signalMessage.MessageInfo.MessageId = messageId;

                // Act
                var error = AS4Mapper.Map<CoreError>(signalMessage);

                // Assert
                Assert.Equal(messageId, error.MessageId);
            }
        }

        protected SignalMessage GetPopulatedXmlError()
        {
            return new SignalMessage
            {
                MessageInfo = new MessageInfo
                {
                    MessageId = $"error-{Guid.NewGuid()}",
                    RefToMessageId = $"user-{Guid.NewGuid()}",
                    Timestamp = DateTime.UtcNow
                },
                Error = new[]
                {
                    new XmlError
                    {
                        category = "myCategory",
                        Description = new Description
                        {
                            lang = "en",
                            Value = "this is a long description"
                        },
                        errorCode = "errorCode",
                        ErrorDetail = "errorDetail",
                        origin = "origin",
                        refToMessageInError = "refToMessageInError",
                        severity = "severity",
                        shortDescription = "shortDescription"
                    }
                }
            };
        }
    }
}