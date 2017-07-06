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
using Eu.EDelivery.AS4.Repositories;

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
        private readonly string pmodeString;
        private readonly string MessageBody1 = "TEST";
        private readonly string MessageBody2 = "TEST2";
        private DatastoreContext datastoreContext;
        private MonitorService monitorService;
        private DbContextOptions<DatastoreContext> options;
        protected IDatastoreRepository DatastoreRepository;

        public MonitorServiceTests()
        {
            pmodeString = File.ReadAllText(@"receivingpmode.xml");
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

            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new SettingsAutoMapper());
                cfg.AddProfile(new MonitorAutoMapper());
            });
            DatastoreRepository = Substitute.For<IDatastoreRepository>();
            monitorService = new MonitorService(datastoreContext, SetupPmodeSource(), DatastoreRepository);

            return this;
        }

        private static As4PmodeSource SetupPmodeSource()
        {
            var sourceOptions = Substitute.For<IOptions<PmodeSettings>>();
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
                datastoreContext.InMessages.Add(new InMessage
                {
                    EbmsMessageId = InEbmsMessageId1,
                    EbmsRefToMessageId = InEbmsRefToMessageId1,
                    PMode = pmodeString,
                    Status = InStatus.Created
                });
                datastoreContext.InMessages.Add(new InMessage
                {
                    EbmsMessageId = InEbmsMessageId2,
                    EbmsRefToMessageId = InEbmsRefToMessageId2,
                    PMode = pmodeString,
                    Status = InStatus.Received
                });
                datastoreContext.OutMessages.Add(new OutMessage
                {
                    EbmsMessageId = OutEbmsMessageId1,
                    EbmsRefToMessageId = OutEbmsRefToMessageId1,
                    PMode = pmodeString,
                    Status = OutStatus.Created
                });
                datastoreContext.OutMessages.Add(new OutMessage
                {
                    EbmsMessageId = OutEbmsMessageId2,
                    EbmsRefToMessageId = OutEbmsRefToMessageId2,
                    PMode = pmodeString,
                    Status = OutStatus.Created
                });
                datastoreContext.InExceptions.Add(new InException
                {
                    EbmsRefToMessageId = InEbmsRefToMessageId1,
                    PMode = pmodeString,                
                    MessageBody = Encoding.ASCII.GetBytes(MessageBody1)
                });
                datastoreContext.InExceptions.Add(new InException
                {
                    EbmsRefToMessageId = OutEbmsRefToMessageId1,
                    PMode = pmodeString,
                    MessageBody = Encoding.ASCII.GetBytes(MessageBody1)
                });
                datastoreContext.OutExceptions.Add(new OutException
                {
                    EbmsRefToMessageId = OutEbmsRefToMessageId1,
                    PMode = pmodeString,
                    MessageBody = Encoding.ASCII.GetBytes(MessageBody1)
                });
                datastoreContext.OutExceptions.Add(new OutException
                {
                    EbmsRefToMessageId = InEbmsRefToMessageId1,
                    PMode = pmodeString,
                    MessageBody = Encoding.ASCII.GetBytes(MessageBody1)
                });
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

                Assert.True(result.Messages.Count() == 4, "Cound should be 4");
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

                Assert.True(result.Messages.FirstOrDefault(x => x.EbmsRefToMessageId == InEbmsRefToMessageId1).PMode == "8.1.2-basePmode");

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
                        EbmsRefToMessageId = InEbmsRefToMessageId1,
                        Direction = new[] { Direction.Inbound }
                    };
                    var result = await monitorService.GetExceptions(filter);

                    Assert.True(result.Messages.Count() == 1);
                    Assert.True(result.Messages.First().EbmsRefToMessageId == InEbmsRefToMessageId1);
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

                protected override void SetupDataStore()
                {
                    using (datastoreContext = new DatastoreContext(options))
                    {
                        datastoreContext.InMessages.Add(new InMessage
                        {
                            EbmsMessageId = InEbmsMessageId1,
                            EbmsRefToMessageId = InEbmsRefToMessageId1,
                            PMode = pmodeString
                        });
                        datastoreContext.InMessages.Add(new InMessage
                        {
                            EbmsMessageId = InEbmsRefToMessageId1,
                            PMode = pmodeString
                        });
                        datastoreContext.OutMessages.Add(new OutMessage
                        {                            
                            EbmsMessageId = InEbmsRefToMessageId1,
                            PMode = pmodeString
                        });
                        datastoreContext.InMessages.Add(new InMessage
                        {
                            EbmsMessageId = "RANDOM",
                            EbmsRefToMessageId = InEbmsMessageId1,
                            PMode = pmodeString
                        });
                        datastoreContext.InMessages.Add(new InMessage
                        {
                            EbmsMessageId = InEbmsMessageId2,
                            PMode = pmodeString
                        });

                        datastoreContext.OutMessages.Add(new OutMessage
                        {
                            EbmsMessageId = OutEbmsMessageId1,
                            EbmsRefToMessageId = OutEbmsRefToMessageId1
                        });
                        datastoreContext.OutMessages.Add(new OutMessage
                        {
                            EbmsMessageId = OutEbmsMessageId2,
                            EbmsRefToMessageId = OutEbmsMessageId1
                        });
                        datastoreContext.InMessages.Add(new InMessage
                        {
                            EbmsMessageId = Guid.NewGuid().ToString(),
                            EbmsRefToMessageId = OutEbmsMessageId1
                        });
                        datastoreContext.InMessages.Add(new InMessage
                        {
                            EbmsMessageId = OutEbmsRefToMessageId1,
                            EbmsRefToMessageId = Guid.NewGuid().ToString()
                        });

                        datastoreContext.OutMessages.Add(new OutMessage
                        {
                            EbmsMessageId = _outEbmsMessage3
                        });
                        datastoreContext.OutMessages.Add(new OutMessage
                        {
                            EbmsMessageId = Guid.NewGuid().ToString(),
                            EbmsRefToMessageId = _outEbmsMessage3
                        });
                        datastoreContext.InMessages.Add(new InMessage
                        {
                            EbmsMessageId = Guid.NewGuid().ToString(),
                            EbmsRefToMessageId = _outEbmsMessage3
                        });

                        datastoreContext.InMessages.Add(new InMessage
                        {
                            EbmsMessageId = Guid.NewGuid().ToString()
                        });
                        datastoreContext.OutMessages.Add(new OutMessage
                        {
                            EbmsMessageId = Guid.NewGuid().ToString()
                        });

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
                    await ExpectExceptionAsync(() => monitorService.DownloadExceptionBody(Direction.Inbound, null), typeof(ArgumentNullException));
                }

                [Theory]
                [InlineData(Direction.Inbound, "ebmsRefToMessageId1")]
                [InlineData(Direction.Outbound, "OutEbmsRefToMessageId1")]
                public async Task Gets_The_MesageBody(Direction direction, string ebmsMessageId)
                {
                    var testBody = MessageBody1;
                    var result = await Setup().monitorService.DownloadExceptionBody(direction, ebmsMessageId);
                    Assert.True(testBody == Encoding.ASCII.GetString(result));
                }
            }
        }
    }
}