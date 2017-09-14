using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    public class GivenExceptionEntityPersitenceFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task ExceptionOperationIsCorrectlyPersisted()
        {
            long savedId;

            using (var db = this.GetDataStoreContext())
            {
                var inException = new InException("message-id", "some-error-happened");
                inException.SetOperation(Operation.Sent);

                db.InExceptions.Add(inException);

                await db.SaveChangesAsync();

                savedId = inException.Id;

                Assert.NotEqual(default(long), savedId);
            }

            using (var db = this.GetDataStoreContext())
            {
                var inMessage = db.InExceptions.FirstOrDefault(i => i.Id == savedId);

                Assert.NotNull(inMessage);
                Assert.Equal(Operation.Sent, OperationUtils.Parse(inMessage.Operation));
            }
        }

        [Fact]
        public async Task OutExceptionPModeInformationIsCorrectlyPersisted()
        {
            long savedId;

            const string pmodeId = "pmode-id1";
            const string pmodeContent = "<pmode></pmode>";

            using (var db = this.GetDataStoreContext())
            {
                var outException = new OutException("message-id", "some-error-happened");
                outException.SetPModeInformation(pmodeId, pmodeContent);

                db.OutExceptions.Add(outException);

                await db.SaveChangesAsync();

                savedId = outException.Id;

                Assert.NotEqual(default(long), savedId);
            }

            using (var db = this.GetDataStoreContext())
            {
                var outException = db.OutExceptions.FirstOrDefault(i => i.Id == savedId);

                Assert.NotNull(outException);
                Assert.Equal(pmodeId, outException.PModeId);
                Assert.Equal(pmodeContent, outException.PMode);
            }
        }

        [Fact]
        public async Task InExceptionPModeInformationIsCorrectlyPersisted()
        {
            long savedId;

            const string pmodeId = "pmode-id1";
            const string pmodeContent = "<pmode></pmode>";

            using (var db = this.GetDataStoreContext())
            {
                var inException = new InException("message-id", "some-error-happened");
                inException.SetPModeInformation(pmodeId, pmodeContent);

                db.InExceptions.Add(inException);

                await db.SaveChangesAsync();

                savedId = inException.Id;

                Assert.NotEqual(default(long), savedId);
            }

            using (var db = this.GetDataStoreContext())
            {
                var inException = db.InExceptions.FirstOrDefault(i => i.Id == savedId);

                Assert.NotNull(inException);
                Assert.Equal(pmodeId, inException.PModeId);
                Assert.Equal(pmodeContent, inException.PMode);
            }
        }
    }
}
