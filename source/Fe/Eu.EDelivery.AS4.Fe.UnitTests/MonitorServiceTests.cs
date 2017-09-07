using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Fe.Monitor;
using Eu.EDelivery.AS4.Fe.Pmodes;
using Eu.EDelivery.AS4.Fe.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using System;
using System.Text;
using Eu.EDelivery.AS4.Fe.Monitor.Model;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class MonitorServiceTests : BaseTest
    {
        private readonly string InEbmsMessageId1 = "ebmsMessageId1";
        private readonly string InEbmsMessageId2 = "InEbmsMessageId2";
        private readonly string InEbmsRefToMessageId1 = "ebmsRefToMessageId1";
        private readonly string InEbmsRefToMessageId2 = "InEbmsRefToMessageId2";
        private readonly string OutEbmsMessageId1 = "OutEbmsMessageId1";
        private readonly string OutEbmsMessageId2 = "OutEbmsMessageId2";
        private readonly string OutEbmsRefToMessageId1 = "OutEbmsRefToMessageId1";
        private readonly string OutEbmsRefToMessageId2 = "OutEbmsRefToMessageId2";
        private readonly string InException = "THIS IS EXCEPTION 1";
        private readonly ReceivingProcessingMode pmode;
        private readonly string MessageBody1 = "TEST";
        private readonly string MessageBody2 = "TEST2";
        private DatastoreContext datastoreContext;
        private MonitorService monitorService;
        private DbContextOptions<DatastoreContext> options;
        protected IDatastoreRepository DatastoreRepository;
        private const string Exception = @"[9acd3265 - cd3a - 4903 - 9ec4 - 694fc4433c34@mindertestbed.org]Decryption failed
   at Eu.EDelivery.AS4.Steps.Receive.DecryptAS4MessageStep.TryDecryptAS4Message() in C:\Dev\codit.visualstudio.com\AS4.NET\source\Steps\Eu.EDelivery.AS4.Steps\Receive\DecryptAS4MessageStep.cs:line 109
   at Eu.EDelivery.AS4.Steps.Receive.DecryptAS4MessageStep.ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken) in C:\Dev\codit.visualstudio.com\AS4.NET\source\Steps\Eu.EDelivery.AS4.Steps\Receive\DecryptAS4MessageStep.cs:line 66
   at Eu.EDelivery.AS4.Steps.CompositeStep.<ExecuteAsync>d__2.MoveNext() in C:\Dev\codit.visualstudio.com\AS4.NET\source\AS4\Eu.EDelivery.AS4\Steps\CompositeStep.cs:line 43
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at Eu.EDelivery.AS4.Steps.Receive.ReceiveExceptionStepDecorator.<ExecuteAsync>d__4.MoveNext() in C:\Dev\codit.visualstudio.com\AS4.NET\source\Steps\Eu.EDelivery.AS4.Steps\Receive\ReceiveExceptionStepDecorator.cs:line 54
Failed to decrypt data element
   at Eu.EDelivery.AS4.Security.Strategies.EncryptionStrategy.TryDecryptEncryptedData(EncryptedData encryptedData) in C:\Dev\codit.visualstudio.com\AS4.NET\source\AS4\Eu.EDelivery.AS4\Security\Strategies\EncryptionStrategy.cs:line 288
   at Eu.EDelivery.AS4.Security.Strategies.EncryptionStrategy.DecryptMessage() in C:\Dev\codit.visualstudio.com\AS4.NET\source\AS4\Eu.EDelivery.AS4\Security\Strategies\EncryptionStrategy.cs:line 271
   at Eu.EDelivery.AS4.Model.Core.SecurityHeader.Decrypt(IEncryptionStrategy encryptionStrategy) in C:\Dev\codit.visualstudio.com\AS4.NET\source\AS4\Eu.EDelivery.AS4\Model\Core\SecurityHeader.cs:line 124
   at Eu.EDelivery.AS4.Steps.Receive.DecryptAS4MessageStep.TryDecryptAS4Message() in C:\Dev\codit.visualstudio.com\AS4.NET\source\Steps\Eu.EDelivery.AS4.Steps\Receive\DecryptAS4MessageStep.cs:line 104
";

        public MonitorServiceTests()
        {
            pmode = new ReceivingProcessingMode() { Id = "monitorServiceTestPModeId" };
        }

        private MonitorServiceTests Setup()
        {
            Cleanup();

            options = new DbContextOptionsBuilder<DatastoreContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using (var store = new DatastoreContext(options))
            {
                store.Database.EnsureCreated();
            }
            SetupDataStore();
            datastoreContext = new DatastoreContext(options);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new SettingsAutoMapper());
                cfg.AddProfile(new MonitorAutoMapper());
            });

            DatastoreRepository = Substitute.For<IDatastoreRepository>();
            monitorService = new MonitorService(datastoreContext, SetupPmodeSource(), DatastoreRepository, mapperConfig);

            return this;
        }

        private static As4PmodeSource SetupPmodeSource()
        {
            var sourceOptions = Substitute.For<IOptionsSnapshot<PmodeSettings>>();
            var pmodeSource = new As4PmodeSource(sourceOptions);
            return pmodeSource;
        }

        private void Cleanup()
        {
            datastoreContext?.Database?.EnsureDeleted();
            datastoreContext?.Dispose();
        }

        protected virtual void SetupDataStore()
        {
            using (datastoreContext = new DatastoreContext(options))
            {
                string pmodeString = AS4XmlSerializer.ToString(pmode);

                {
                    var message = new InMessage(ebmsMessageId: InEbmsMessageId1)
                    {
                        EbmsRefToMessageId = InEbmsRefToMessageId1,
                        InsertionTime = DateTime.UtcNow.AddMinutes(-1),
                    };
                    message.SetStatus(InStatus.Created);
                    message.SetPModeInformation(pmode);
                    datastoreContext.InMessages.Add(message);
                }

                {
                    var message = new InMessage(ebmsMessageId: InEbmsMessageId2)
                    {
                        EbmsRefToMessageId = InEbmsRefToMessageId2,
                        InsertionTime = DateTime.UtcNow.AddMinutes(-1)
                    };
                    message.SetStatus(InStatus.Received);
                    datastoreContext.InMessages.Add(message);
                }

                {
                    var message = new OutMessage(OutEbmsMessageId1)
                    {
                        EbmsRefToMessageId = OutEbmsRefToMessageId1,
                        InsertionTime = DateTime.UtcNow.AddMinutes(-1)
                    };
                    message.SetStatus(OutStatus.Created);

                    datastoreContext.OutMessages.Add(message);
                }

                {
                    var message = new OutMessage(OutEbmsMessageId2)
                    {
                        EbmsRefToMessageId = OutEbmsRefToMessageId2,

                        InsertionTime = DateTime.UtcNow.AddMinutes(-1)
                    };
                    message.SetStatus(OutStatus.Created);
                    datastoreContext.OutMessages.Add(message);
                }

                datastoreContext.InExceptions.Add(new InException
                {
                    EbmsRefToMessageId = InEbmsMessageId1,
                    PMode = pmodeString,
                    Exception = InException,
                    InsertionTime = DateTime.UtcNow.AddMinutes(-1),
                });
                datastoreContext.InExceptions.Add(new InException
                {
                    EbmsRefToMessageId = OutEbmsRefToMessageId1,
                    PMode = pmodeString,
                    MessageBody = Encoding.ASCII.GetBytes(MessageBody1),
                    InsertionTime = DateTime.UtcNow.AddMinutes(-1)
                });
                datastoreContext.OutExceptions.Add(new OutException
                {
                    EbmsRefToMessageId = OutEbmsRefToMessageId1,
                    PMode = pmodeString,
                    Exception = InException,
                    InsertionTime = DateTime.UtcNow.AddMinutes(-1)
                });
                datastoreContext.OutExceptions.Add(new OutException
                {
                    EbmsRefToMessageId = InEbmsRefToMessageId1,
                    PMode = pmodeString,
                    MessageBody = Encoding.ASCII.GetBytes(MessageBody1),
                    Exception = Exception,
                    InsertionTime = DateTime.UtcNow.AddMinutes(-1)
                });

                datastoreContext.SaveChanges();

                foreach (var inMessage in datastoreContext.InMessages)
                {
                    inMessage.SetPModeInformation(pmode);
                }

                foreach (var outMessage in datastoreContext.OutMessages)
                {
                    outMessage.SetPModeInformation(pmode);
                }

                datastoreContext.SaveChanges();
            }
        }

        public class GetMessages : MonitorServiceTests
        {
            [Fact]
            public async Task Throws_Business_Exception_When_No_Direction_Is_Specified()
            {
                await Setup()
                    .ExpectExceptionAsync(() => monitorService.GetMessages(new MessageFilter() { Direction = new Direction[] { } }), typeof(BusinessException));
            }

            [Fact]
            public async Task Throws_Exception_When_Direction_Property_Is_Null()
            {
                await Setup().ExpectExceptionAsync(() => monitorService.GetMessages(new MessageFilter { Direction = null }), typeof(ArgumentNullException));
            }

            [Fact]
            public async Task Throws_Exception_When_Parameter_Is_Null()
            {
                await Setup()
                    .ExpectExceptionAsync(() => monitorService.GetMessages(null), typeof(ArgumentNullException));
            }

            [Fact]
            public async Task Gets_All_In_And_Outbound_Messages()
            {
                var filter = new MessageFilter()
                {
                    Direction = new[] { Direction.Inbound, Direction.Outbound }
                };

                var result = await Setup().monitorService.GetMessages(filter);

                Assert.True(result.Messages.Count() == 4, "Count should be 4");
                Assert.True(result.Messages.Count(x => x.Direction == Direction.Inbound) == 2, "Expected 2 inbound messages");
                Assert.True(result.Messages.Count(x => x.Direction == Direction.Outbound) == 2, "Expected 2 outbound messages");
            }

            [Fact]
            public async Task Get_Only_Inboud_Messages()
            {
                var filter = new MessageFilter
                {
                    Direction = new[] { Direction.Inbound }
                };
                var result = await Setup().monitorService.GetMessages(filter);

                Assert.True(result.Messages.Count() == 2);
                Assert.True(result.Messages.All(message => message.Direction == Direction.Inbound));
            }

            [Fact]
            public async Task HasExceptions_Of_Message_Should_Be_True_When_Exceptions_Are_Available()
            {
                var result = await Setup().monitorService.GetMessages(new MessageFilter());

                Assert.True(result.Messages.FirstOrDefault(msg => msg.EbmsMessageId == InEbmsMessageId1).HasExceptions);
                Assert.False(result.Messages.FirstOrDefault(msg => msg.EbmsRefToMessageId == InEbmsRefToMessageId2).HasExceptions);

                Cleanup();
            }

            [Fact]
            public async Task No_Filter_Should_Return_All_Messages()
            {
                var filter = new MessageFilter();
                var result = await Setup().monitorService.GetMessages(filter);

                Assert.True(result.Page == 1);
                Assert.True(result.Total == 4);

                Cleanup();
            }

            [Fact]
            public async Task Pmode_Should_Only_Contain_Pmode_Number()
            {
                var result = await Setup().monitorService.GetMessages(new MessageFilter());

                var message = result.Messages.FirstOrDefault(x => x.EbmsRefToMessageId == InEbmsRefToMessageId1);

                Assert.NotNull(message);

                Assert.True(message.PModeId == pmode.Id);

                Cleanup();
            }

            [Fact]
            public async Task Should_Filter_Data_When_Existing_MessageId_Is_Supplied()
            {
                var filter = new MessageFilter
                {
                    EbmsRefToMessageId = InEbmsRefToMessageId1
                };

                var result = await Setup().monitorService.GetMessages(filter);

                Assert.True(result.Page == 1);
                Assert.True(result.Total == 1);

                Cleanup();
            }

            [Fact]
            public async Task Results_Should_Have_The_Inboud_Direction()
            {
                Setup();

                var result = await monitorService.GetMessages(new MessageFilter()
                {
                    Direction = new[] { Direction.Inbound }
                });

                Assert.True(result.Messages.All(message => message.Direction == Direction.Inbound));

                Cleanup();
            }

            [Fact]
            public async Task Results_Should_Have_The_Outbound_Direction()
            {
                Setup();

                var result = await monitorService.GetMessages(new MessageFilter()
                {
                    Direction = new[] { Direction.Outbound }
                });

                Assert.True(result.Messages.All(message => message.Direction == Direction.Outbound));

                Cleanup();
            }

            [Fact]
            public async Task Status_Should_Be_Mapped()
            {
                var result = await Setup().monitorService.GetMessages(new MessageFilter());
                Assert.True(result.Messages.All(msg => !string.IsNullOrEmpty(msg.Status)));
            }

            public class GetPmodeNumber : MonitorServiceTests
            {
                [Fact]
                public void Returns_Pmode_Number_From_Pmode_String()
                {
                    var pmode = File.ReadAllText(@"receivingpmode.xml");
                    var result = Setup().monitorService.GetPmodeNumber(pmode);
                    Assert.True(result == "8.1.2-basePmode");

                    Cleanup();
                }
            }

            public class GetExceptions : MonitorServiceTests
            {
                [Fact]
                public async Task Throws_Exception_When_Parameters_Is_Null()
                {
                    await Setup().ExpectExceptionAsync(() => monitorService.GetExceptions(null), typeof(ArgumentNullException));
                }

                [Fact]
                public async void Filter_Should_Filter_The_Data()
                {
                    Setup();

                    var filter = new ExceptionFilter()
                    {
                        EbmsRefToMessageId = InEbmsMessageId1,
                        Direction = new[] { Direction.Inbound }
                    };
                    var result = await monitorService.GetExceptions(filter);

                    Assert.True(result.Messages.Count() == 1, "Count should be 1");
                    Assert.True(result.Messages.First().EbmsRefToMessageId == InEbmsMessageId1, $"The first embsRefToMessagId should be {InEbmsRefToMessageId1}");
                }

                [Fact]
                public async void Filter_Should_Return_Nothing_When_No_Match()
                {
                    Setup();
                    var filter = new ExceptionFilter
                    {
                        EbmsRefToMessageId = "IDONTEXIST"
                    };
                    var result = await monitorService.GetExceptions(filter);

                    Assert.True(!result.Messages.Any());
                }

                [Fact]
                public async Task Return_All_Directions()
                {
                    var result = await Setup().monitorService.GetExceptions(new ExceptionFilter());

                    Assert.True(result.Messages.Count() == 4);
                }

                [Fact]
                public async Task Throws_Exception_When_No_Direction()
                {
                    var result = await Setup().ExpectExceptionAsync(() => monitorService.GetExceptions(new ExceptionFilter() { Direction = null }), typeof(ArgumentNullException));
                }

                [Fact]
                public async Task Exception_Short_Should_Not_Contain_The_Full_Exception()
                {
                    Setup();

                    var result = await monitorService.GetExceptions(new ExceptionFilter
                    {
                        EbmsRefToMessageId = InEbmsRefToMessageId1
                    });

                    Assert.True(result.Messages.First().ExceptionShort == "Decryption failed");
                }
            }

            public class Hash : MonitorServiceTests
            {
                [Fact]
                public async void Message_Should_Contain_Md5_Hash()
                {
                    Setup();

                    var inMessageResult = await monitorService.GetMessages(new MessageFilter { Direction = new[] { Direction.Inbound } });
                    var outMessageResult = await monitorService.GetMessages(new MessageFilter { Direction = new[] { Direction.Outbound } });

                    Assert.True(inMessageResult.Messages.All(msg => !string.IsNullOrEmpty(msg.Hash)));
                    Assert.True(outMessageResult.Messages.All(msg => !string.IsNullOrEmpty(msg.Hash)));
                }
            }

            public class GetRelatedMessages : MonitorServiceTests
            {
                private readonly string _outEbmsMessage3 = Guid.NewGuid().ToString();
                private readonly string ForwardedMessageId = "ForwardedMessage1";

                protected override void SetupDataStore()
                {
                    using (datastoreContext = new DatastoreContext(options))
                    {
                        datastoreContext.InMessages.Add(new InMessage(ebmsMessageId: InEbmsMessageId1)
                        {
                            EbmsRefToMessageId = InEbmsRefToMessageId1,
                        });
                        datastoreContext.InMessages.Add(new InMessage(ebmsMessageId: InEbmsRefToMessageId1));

                        datastoreContext.OutMessages.Add(new OutMessage(ebmsMessageId: InEbmsRefToMessageId1));

                        datastoreContext.InMessages.Add(new InMessage(ebmsMessageId: "RANDOM")
                        {
                            EbmsRefToMessageId = InEbmsMessageId1,
                        });
                        datastoreContext.InMessages.Add(new InMessage(ebmsMessageId: InEbmsMessageId2));

                        datastoreContext.OutMessages.Add(new OutMessage(ebmsMessageId: OutEbmsMessageId1)
                        {
                            EbmsRefToMessageId = OutEbmsRefToMessageId1
                        });
                        datastoreContext.OutMessages.Add(new OutMessage(ebmsMessageId: OutEbmsMessageId2)
                        {
                            EbmsRefToMessageId = OutEbmsMessageId1
                        });
                        datastoreContext.InMessages.Add(new InMessage(ebmsMessageId: Guid.NewGuid().ToString())
                        {
                            EbmsRefToMessageId = OutEbmsMessageId1
                        });
                        datastoreContext.InMessages.Add(new InMessage(ebmsMessageId: OutEbmsRefToMessageId1)
                        {
                            EbmsRefToMessageId = Guid.NewGuid().ToString()
                        });

                        datastoreContext.OutMessages.Add(new OutMessage(ebmsMessageId: _outEbmsMessage3));

                        datastoreContext.OutMessages.Add(new OutMessage(ebmsMessageId: Guid.NewGuid().ToString())
                        {
                            EbmsRefToMessageId = _outEbmsMessage3
                        });
                        datastoreContext.InMessages.Add(new InMessage(ebmsMessageId: Guid.NewGuid().ToString())
                        {
                            EbmsRefToMessageId = _outEbmsMessage3
                        });

                        datastoreContext.InMessages.Add(new InMessage(ebmsMessageId: Guid.NewGuid().ToString()));

                        datastoreContext.OutMessages.Add(new OutMessage(Guid.NewGuid().ToString()));

                        // Forwareded message
                        var newinMessage = new InMessage(ForwardedMessageId);
                        newinMessage.SetOperation(Operation.Forwarded);
                        datastoreContext.InMessages.Add(newinMessage);

                        var newOutMessage = new OutMessage(ForwardedMessageId);
                        newOutMessage.SetOperation(Operation.ToBeSent);
                        datastoreContext.OutMessages.Add(newOutMessage);

                        foreach (var inMessage in datastoreContext.InMessages)
                        {
                            inMessage.SetPModeInformation(pmode);
                        }
                        foreach (var outMessage in datastoreContext.OutMessages)
                        {
                            outMessage.SetPModeInformation(pmode);
                        }

                        datastoreContext.SaveChanges();
                    }
                }

                [Fact]
                public async void Returns_All_Related_Messages()
                {
                    Setup();

                    var result = await monitorService.GetRelatedMessages(Direction.Inbound, InEbmsMessageId1);

                    Assert.True(result.Messages.Count() == 3);
                }

                [Fact]
                public async Task OutMessages_Should_Return_All_Related_Messages()
                {
                    var result = await Setup().monitorService.GetRelatedMessages(Direction.Outbound, OutEbmsMessageId1);

                    Assert.True(result.Messages.Count() == 3);
                }

                [Fact]
                public async Task OutMessages_Without_RefTo_Message_Returns_Related_Messages()
                {
                    var result = await Setup().monitorService.GetRelatedMessages(Direction.Outbound, _outEbmsMessage3);
                    Assert.True(result.Messages.Count() == 2);
                }

                [Fact]
                public async Task Throws_Exception_When_Parames_Are_Null()
                {
                    await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().monitorService.GetRelatedMessages(Direction.Outbound, null));
                }

                [Fact]
                public async Task ForwardedMessage_ShouldBeReturned()
                {
                    var result = await Setup().monitorService.GetRelatedMessages(Direction.Inbound, ForwardedMessageId);

                    Assert.True(result.Messages.Count() == 1, "Expected 1 message");
                }
            }

            public class DownloadMessageBody : MonitorServiceTests
            {
                [Fact]
                public async Task Throws_Exception_When_Parameters_Are_Invalid()
                {
                    Setup();
                    await ExpectExceptionAsync(() => monitorService.DownloadMessageBody(Direction.Inbound, null), typeof(ArgumentNullException));
                }

                [Fact]
                public async Task Gets_The_MessageBody()
                {
                    var testBody = Encoding.ASCII.GetBytes(MessageBody1);
                    var testBody2 = Encoding.ASCII.GetBytes(MessageBody2);
                    var result = await Setup().monitorService.DownloadMessageBody(Direction.Inbound, InEbmsMessageId1);
                    var result2 = await Setup().monitorService.DownloadMessageBody(Direction.Outbound, OutEbmsMessageId1);
                    Assert.True(Encoding.ASCII.GetString(testBody) == MessageBody1);
                    Assert.True(Encoding.ASCII.GetString(testBody2) == MessageBody2);
                }
            }

            public class DownloadExceptionBody : MonitorServiceTests
            {
                [Fact]
                public async Task Throws_Exception_When_Parameters_Are_Invalid()
                {
                    Setup();
                    await ExpectExceptionAsync(() => monitorService.DownloadExceptionMessageBody(Direction.Inbound, 0), typeof(ArgumentOutOfRangeException));
                }

                [Theory]
                [InlineData(Direction.Inbound)]
                [InlineData(Direction.Outbound)]
                public async Task Gets_The_MesageBody(Direction direction)
                {
                    Setup();

                    long id = 0;
                    switch (direction)
                    {
                        case Direction.Inbound:
                            id = datastoreContext.InExceptions.Where(x => x.MessageBody != null).Select(x => x.Id).First();
                            break;
                        case Direction.Outbound:
                            id = datastoreContext.OutExceptions.Where(x => x.MessageBody != null).Select(x => x.Id).First();
                            break;
                    }

                    var result = await monitorService.DownloadExceptionMessageBody(direction, id);
                    Assert.True(result != null, $"Could not find body for {id}");
                }
            }
        }
    }
}