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

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class MonitorServiceTests
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
        private DatastoreContext datastoreContext;
        private MonitorService monitorService;
        private DbContextOptions<DatastoreContext> options;

        public MonitorServiceTests()
        {
            pmodeString = File.ReadAllText(@"receivingpmode.xml");
        }

        private MonitorService Setup()
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
            monitorService = new MonitorService(datastoreContext, SetupPmodeSource());

            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new SettingsAutoMapper());
                cfg.AddProfile(new MonitorAutoMapper());
            });

            return monitorService;
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
                    PMode = pmodeString
                });
                datastoreContext.InMessages.Add(new InMessage
                {
                    EbmsMessageId = InEbmsMessageId2,
                    EbmsRefToMessageId = InEbmsRefToMessageId2,
                    PMode = pmodeString
                });
                datastoreContext.OutMessages.Add(new OutMessage
                {
                    EbmsMessageId = OutEbmsMessageId1,
                    EbmsRefToMessageId = OutEbmsRefToMessageId1,
                    PMode = pmodeString
                });
                datastoreContext.OutMessages.Add(new OutMessage
                {
                    EbmsMessageId = OutEbmsMessageId2,
                    EbmsRefToMessageId = OutEbmsRefToMessageId2,
                    PMode = pmodeString
                });
                datastoreContext.InExceptions.Add(new InException
                {
                    EbmsRefToMessageId = InEbmsRefToMessageId1,
                    PMode = pmodeString,
                    Id = 12
                });
                datastoreContext.InExceptions.Add(new InException
                {
                    EbmsRefToMessageId = OutEbmsRefToMessageId1,
                    PMode = pmodeString,
                    Id = 13
                });
                datastoreContext.OutExceptions.Add(new OutException
                {
                    EbmsRefToMessageId = OutEbmsRefToMessageId1,
                    PMode = pmodeString,
                    Id = 14
                });
                datastoreContext.OutExceptions.Add(new OutException
                {
                    EbmsRefToMessageId = InEbmsRefToMessageId1,
                    PMode = pmodeString,
                    Id = 15
                });                
                datastoreContext.SaveChanges();
            }
        }

        public class GetInMessages : MonitorServiceTests
        {
            [Fact]
            public async Task HasExceptions_Of_Message_Should_Be_True_When_Exceptions_Are_Available()
            {
                var result = await Setup().GetInMessages(new InMessageFilter());

                Assert.True(result.Messages.FirstOrDefault(msg => msg.EbmsRefToMessageId == InEbmsRefToMessageId1).HasExceptions);
                Assert.False(result.Messages.FirstOrDefault(msg => msg.EbmsRefToMessageId == InEbmsRefToMessageId2).HasExceptions);

                Cleanup();
            }

            [Fact]
            public async Task No_Filter_Should_Return_All_Messages()
            {
                var filter = new InMessageFilter();
                var result = await Setup().GetInMessages(filter);

                Assert.True(result.Page == 1);
                Assert.True(result.Total == 2);

                Cleanup();
            }

            [Fact]
            public async Task Pmode_Should_Only_Contain_Pmode_Number()
            {
                var result = await Setup().GetInMessages(new InMessageFilter());

                Assert.True(result.Messages.FirstOrDefault(x => x.EbmsRefToMessageId == InEbmsRefToMessageId1).PMode == "8.1.2-basePmode");

                Cleanup();
            }

            [Fact]
            public async Task Should_Filter_Data_When_Existing_MessageId_Is_Supplied()
            {
                var filter = new InMessageFilter
                {
                    EbmsRefToMessageId = InEbmsRefToMessageId1
                };

                var result = await Setup().GetInMessages(filter);

                Assert.True(result.Page == 1);
                Assert.True(result.Total == 1);

                Cleanup();
            }

            [Fact]
            public async Task Results_Should_Have_The_Inboud_Direction()
            {
                Setup();

                var result = await monitorService.GetInMessages(new InMessageFilter());

                Assert.True(result.Messages.All(message => message.Direction == Direction.Inbound));

                Cleanup();
            }
        }

        public class GetOutMessages : MonitorServiceTests
        {
            [Fact]
            public async Task HasExceptions_Of_Message_Should_Be_True_When_Exceptions_Are_Available()
            {
                var result = await Setup().GetOutMessages(new OutMessageFilter());

                Assert.True(result.Messages.FirstOrDefault(msg => msg.EbmsRefToMessageId == OutEbmsRefToMessageId1).HasExceptions);
                Assert.False(result.Messages.FirstOrDefault(msg => msg.EbmsRefToMessageId == OutEbmsRefToMessageId2).HasExceptions);

                Cleanup();
            }

            [Fact]
            public async Task No_Filter_Should_Return_All_Messages()
            {
                var filter = new OutMessageFilter();
                var result = await Setup().GetOutMessages(filter);

                Assert.True(result.Page == 1);
                Assert.True(result.Total == 2);

                Cleanup();
            }

            [Fact]
            public async Task Pmode_Should_Only_Contain_Pmode_Number()
            {
                var result = await Setup().GetOutMessages(new OutMessageFilter());
                Assert.True(result.Messages.FirstOrDefault(x => x.EbmsRefToMessageId == OutEbmsRefToMessageId1).PMode == "8.1.2-basePmode");

                Cleanup();
            }

            [Fact]
            public async Task Should_Filter_Data_When_Existing_MessageId_Is_Supplied()
            {
                var filter = new OutMessageFilter
                {
                    EbmsRefToMessageId = OutEbmsRefToMessageId1
                };

                var result = await Setup().GetOutMessages(filter);

                Assert.True(result.Page == 1);
                Assert.True(result.Total == 1);

                Cleanup();
            }

            [Fact]
            public async Task Results_Should_Have_The_Outbound_Direction()
            {
                Setup();

                var result = await monitorService.GetOutMessages(new OutMessageFilter());

                Assert.True(result.Messages.All(message => message.Direction == Direction.Outbound));

                Cleanup();
            }
        }

        public class GetPmodeNumber : MonitorServiceTests
        {
            [Fact]
            public void Returns_Pmode_Number_From_Pmode_String()
            {
                var pmode = File.ReadAllText(@"receivingpmode.xml");
                var result = Setup().GetPmodeNumber(pmode);
                Assert.True(result == "8.1.2-basePmode");

                Cleanup();
            }
        }

        public class GetInExceptions : MonitorServiceTests
        {
            [Fact]
            public async void Filter_Should_Filter_The_Data()
            {
                Setup();

                var filter = new InExceptionFilter
                {
                    EbmsRefToMessageId = InEbmsRefToMessageId1
                };
                var result = await monitorService.GetInExceptions(filter);

                Assert.True(result.Messages.Count() == 1);
                Assert.True(result.Messages.First().EbmsRefToMessageId == InEbmsRefToMessageId1);

                Cleanup();
            }

            [Fact]
            public async void Filter_Should_Return_Nothing_When_No_Match()
            {
                Setup();
                var filter = new InExceptionFilter
                {
                    EbmsRefToMessageId = "IDONTEXIST"
                };
                var result = await monitorService.GetInExceptions(filter);

                Assert.True(!result.Messages.Any());

                Cleanup();
            }
        }

        public class GetOutExceptions : MonitorServiceTests
        {
            [Fact]
            public async void Filter_Should_Filter_The_Data()
            {
                Setup();

                var filter = new OutExceptionFilter
                {
                    EbmsRefToMessageId = OutEbmsRefToMessageId1
                };
                var result = await monitorService.GetOutExceptions(filter);

                Assert.True(result.Messages.Count() == 1);
                Assert.True(result.Messages.First().EbmsRefToMessageId == OutEbmsRefToMessageId1);
            }

            [Fact]
            public async void Filter_Should_Return_Nothing_When_No_Match()
            {
                Setup();
                var filter = new OutExceptionFilter
                {
                    EbmsRefToMessageId = "IDONTEXIST"
                };
                var result = await monitorService.GetOutExceptions(filter);

                Assert.True(!result.Messages.Any());
            }
        }

        public class Hash : MonitorServiceTests
        {
            [Fact]
            public async void Message_Should_Contain_Md5_Hash()
            {
                Setup();

                var inMessageResult = await monitorService.GetInMessages(new InMessageFilter());
                var outMessageResult = await monitorService.GetOutMessages(new OutMessageFilter());

                Assert.True(inMessageResult.Messages.All(msg => !string.IsNullOrEmpty(msg.Hash)));
                Assert.True(outMessageResult.Messages.All(msg => !string.IsNullOrEmpty(msg.Hash)));
            }
        }

        public class GetMessages : MonitorServiceTests
        {
            [Fact]
            public async void When_Inbound_And_Outbound_Messages_Are_Requested_Both_Should_Be_Returned()
            {

            }
        }

        public class GetRelatedMessages : MonitorServiceTests
        {
            private string OutEbmsMessage3 = Guid.NewGuid().ToString();

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
                        EbmsMessageId = OutEbmsMessage3
                    });
                    datastoreContext.OutMessages.Add(new OutMessage
                    {
                        EbmsMessageId = Guid.NewGuid().ToString(),
                        EbmsRefToMessageId = OutEbmsMessage3
                    });
                    datastoreContext.InMessages.Add(new InMessage
                    {
                        EbmsMessageId = Guid.NewGuid().ToString(),
                        EbmsRefToMessageId = OutEbmsMessage3
                    });

                    datastoreContext.InMessages.Add(new InMessage
                    {

                    });
                    datastoreContext.OutMessages.Add(new OutMessage
                    {

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
                var result = await Setup().GetRelatedMessages(Direction.Outbound, OutEbmsMessageId1);

                Assert.True(result.Messages.Count() == 3);
            }

            [Fact]
            public async Task OutMessages_Without_RefTo_Message_Returns_Related_Messages()
            {
                var result = await Setup().GetRelatedMessages(Direction.Outbound, OutEbmsMessage3);
                Assert.True(result.Messages.Count() == 2);
            }

            [Fact]
            public async Task Throws_Exception_When_Parames_Are_Null()
            {
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().GetRelatedMessages(Direction.Outbound, null));
            }
        }
    }
}