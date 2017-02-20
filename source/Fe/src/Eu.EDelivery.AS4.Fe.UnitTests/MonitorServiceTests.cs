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
            datastoreContext?.Dispose();

            options = new DbContextOptionsBuilder<DatastoreContext>()
                .UseInMemoryDatabase()
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

        private void SetupDataStore()
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
                    EbmsRefToMessageId = InEbmsRefToMessageId2
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
                    EbmsRefToMessageId = OutEbmsRefToMessageId2
                });
                datastoreContext.InExceptions.Add(new InException
                {
                    EbmsRefToMessageId = InEbmsRefToMessageId1,
                    Id = 12
                });
                datastoreContext.OutExceptions.Add(new OutException
                {
                    EbmsRefToMessageId = OutEbmsRefToMessageId1,
                    Id = 13
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

                Assert.True(result.Messages.FirstOrDefault(x => x.EbmsRefToMessageId == InEbmsRefToMessageId1).PMode == "8.1.2-pmodeWrapper");
                Assert.True(string.IsNullOrEmpty(result.Messages.Last().PMode));

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
                Assert.True(result.Messages.FirstOrDefault(x => x.EbmsRefToMessageId == OutEbmsRefToMessageId1).PMode == "8.1.2-pmodeWrapper");
                Assert.True(string.IsNullOrEmpty(result.Messages.Last().PMode));

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
        }

        public class GetPmodeNumber : MonitorServiceTests
        {
            [Fact]
            public void Returns_Pmode_Number_From_Pmode_String()
            {
                var pmode = File.ReadAllText(@"receivingpmode.xml");
                var result = Setup().GetPmodeNumber(pmode);
                Assert.True(result == "8.1.2-pmodeWrapper");

                Cleanup();
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
    }
}