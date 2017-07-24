using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Pmodes;
using Eu.EDelivery.AS4.Fe.Pmodes.Model;
using NSubstitute;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class As4PmodeServiceTests
    {
        protected IPmodeService Service { get; private set; }
        protected IAs4PmodeSource Source { get; private set; }
        protected IEnumerable<SendingBasePmode> SendingPmodes { get; private set; }
        protected IEnumerable<ReceivingBasePmode> ReceivingPmodes { get; private set; }
        protected ReceivingBasePmode ReceivingBasePmode { get; private set; }
        protected SendingBasePmode SendingBasePmode { get; private set; }

        protected As4PmodeServiceTests Setup()
        {
            Source = Substitute.For<IAs4PmodeSource>();
            Service = new PmodeService(Source, true);
            SetupPmodes();
            return this;
        }

        private void SetupPmodes()
        {
            ReceivingBasePmode = new ReceivingBasePmode()
            {
                Name = "test1",
                Type = PmodeType.Receiving
            };
            SendingBasePmode = new SendingBasePmode()
            {
                Name = "test2",
                Type = PmodeType.Sending
            };
            ReceivingPmodes = new List<ReceivingBasePmode> { ReceivingBasePmode };
            SendingPmodes = new List<SendingBasePmode> { SendingBasePmode };

            Source.GetReceivingNames().Returns(ReceivingPmodes.Select(pmode => pmode.Name));
            Source.GetReceivingByName(Arg.Is(ReceivingBasePmode.Name)).Returns(ReceivingBasePmode);

            Source.GetSendingNames().Returns(SendingPmodes.Select(pmode => pmode.Name));
            Source.GetSendingByName(Arg.Is(SendingBasePmode.Name)).Returns(SendingBasePmode);
        }

        public class GetReceivingNames : As4PmodeServiceTests
        {
            [Fact]
            public async Task Calls_Source_And_Returns_Names()
            {
                // Setup
                var test = Setup();

                // Act
                var result = await test.Service.GetReceivingNames();

                // Assert
                await test.Source.Received().GetReceivingNames();
                Assert.True(result.First() == ReceivingBasePmode.Name);
            }

            [Fact]
            public async Task Returns_Empty_List_When_No_Modes_Exist()
            {
                // Setup
                var test = Setup();
                Source.GetReceivingNames().Returns(Enumerable.Empty<string>());

                // Act
                var result = await test.Service.GetReceivingNames();

                // Assert
                Assert.True(!result.Any());
            }
        }

        public class GetReceivingByName : As4PmodeServiceTests
        {
            [Fact]
            public async Task Throws_Exception_When_Parameters_Are_Null()
            {
                // Setup
                var test = Setup();

                // Act
                await Assert.ThrowsAsync(typeof(ArgumentException), () => test.Service.GetReceivingByName(string.Empty));
            }
        }

        public class GetSendingNames : As4PmodeServiceTests
        {
            [Fact]
            public async Task Calls_Source_And_Returns_Pmode_When_It_Exists()
            {
                // Setup
                var test = Setup();

                // Act
                var result = await test.Service.GetSendingNames();

                // Assert
                Assert.NotNull(result);
                Assert.True(result.First() == SendingBasePmode.Name);
                await test.Source.Received().GetSendingNames();
            }

            [Fact]
            public async Task Returns_Empty_List_When_No_Modes_Exist()
            {
                // Setup
                var test = Setup();
                Source.GetReceivingNames().Returns(Enumerable.Empty<string>());

                // Act
                var result = await test.Service.GetReceivingNames();

                // Assert
                await test.Source.Received().GetReceivingNames();
                Assert.True(!result.Any());
            }
        }

        public class GetSendingByName : As4PmodeServiceTests
        {
            [Fact]
            public async Task Calls_Source_And_Returns_Pmode()
            {
                // Setup
                var test = Setup();

                // Act
                var result = await test.Service.GetSendingByName(SendingBasePmode.Name);

                // Assert
                await test.Source.Received().GetSendingByName(Arg.Is(SendingBasePmode.Name));
                Assert.True(result == SendingBasePmode);
            }

            [Fact]
            public async Task Throws_Exception_When_Parameters_Are_Null()
            {
                // Setup
                var test = Setup();

                // Act
                await Assert.ThrowsAsync(typeof(ArgumentException), () => test.Service.GetSendingByName(string.Empty));
            }
        }

        public class CreateReceivingPmode : As4PmodeServiceTests
        {
            [Fact]
            public async Task Throws_Exception_When_Parameters_Are_Null()
            {
                // Setup
                var test = Setup();

                // Act & Assert
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => test.Service.CreateReceiving(null));
            }

            [Fact]
            public async Task Throws_Exception_When_Pmode_Already_Exists()
            {
                // Setup
                var test = Setup();
                var pmode = new ReceivingBasePmode()
                {
                    Name = ReceivingBasePmode.Name
                };

                await Assert.ThrowsAsync(typeof(AlreadyExistsException), () => test.Service.CreateReceiving(pmode));
            }

            [Fact]
            public async Task Calls_Source_SaveReceiving()
            {
                // Setup
                var test = Setup();
                var pmode = new ReceivingBasePmode()
                {
                    Name = "newPmode"
                };

                // Act
                await test.Service.CreateReceiving(pmode);

                // Assert
                await test.Source.Received().CreateReceiving(Arg.Is<ReceivingBasePmode>(x => x.Name == "newPmode"));
            }
        }

        public class DeleteReceiving : As4PmodeServiceTests
        {
            [Fact]
            public async Task Throws_Exception_When_Parameters_Are_Null()
            {
                // Setup
                var test = Setup();

                // Act & Assert
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => test.Service.DeleteReceiving(null));
            }

            [Fact]
            public async Task Throws_Exception_When_Pmode_Doesnt_Exist()
            {
                // Setup
                var test = Setup();

                // Act & Assert
                await Assert.ThrowsAsync(typeof(NotFoundException), () => test.Service.DeleteReceiving("new"));
            }

            [Fact]
            public async Task Deletes_Pmode()
            {
                // Setup
                var test = Setup();

                // Act
                await test.Service.DeleteReceiving(ReceivingBasePmode.Name);

                // Assert
                await test.Source.Received().DeleteReceiving(ReceivingBasePmode.Name);
            }
        }

        public class CreateSending : As4PmodeServiceTests
        {
            [Fact]
            public async Task Throws_Exception_When_Parameters_Are_Null()
            {
                // Setup
                var test = Setup();

                // Act & Assert
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => test.Service.CreateReceiving(null));
            }

            [Fact]
            public async Task Throws_Exception_When_Pmode_Already_Exists()
            {
                // Setup
                var test = Setup();

                // Act & Assert
                await Assert.ThrowsAsync(typeof(AlreadyExistsException), () => test.Service.CreateSending(SendingBasePmode));
            }

            [Fact]
            public async Task Creates_The_Pmode()
            {
                // Setup
                var test = Setup();
                var pmode = new SendingBasePmode()
                {
                    Name = "newPmode"
                };

                // Act
                await test.Service.CreateSending(pmode);

                // Assert
                await test.Source.Received().CreateSending(Arg.Is<SendingBasePmode>(x => x.Name == pmode.Name));
            }
        }

        public class DeleteSending : As4PmodeServiceTests
        {
            [Fact]
            public async Task Throws_Exception_When_Parameters_Are_Null()
            {
                // Setup
                var test = Setup();

                // Act & Assert
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => test.Service.DeleteSending(null));
            }

            [Fact]
            public async Task Throws_Exception_When_Pmode_Doesnt_Exist()
            {
                // Setup
                var test = Setup();

                // Act & Assert
                await Assert.ThrowsAsync(typeof(NotFoundException), () => test.Service.DeleteSending("sendingPmode"));
            }

            [Fact]
            public async Task Deletes_Pmode()
            {
                // Setup
                var test = Setup();

                // Act
                await test.Service.DeleteSending(SendingBasePmode.Name);

                // Assert
                await test.Source.Received().DeleteSending(Arg.Is(SendingBasePmode.Name));
            }
        }

        public class UpdateSending : As4PmodeServiceTests
        {
            [Fact]
            public async Task Throws_Exception_When_Parameters_Are_Null()
            {
                // Setup
                var test = Setup();

                // Act & Assert
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => test.Service.UpdateSending(null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => test.Service.UpdateSending(SendingBasePmode, null));
            }

            [Fact]
            public async Task Throws_Exception_When_A_Pmode_With_The_New_Name_Already_Exists()
            {
                // Setup
                var test = Setup();
                var newPmode = new SendingBasePmode()
                {
                    Name = SendingBasePmode.Name
                };

                // Act
                await Assert.ThrowsAsync(typeof(AlreadyExistsException), () => test.Service.UpdateSending(newPmode, "NEW"));
            }

            [Fact]
            public async Task Updates_Existing()
            {
                // Setup
                var test = Setup();
                var newPmode = new SendingBasePmode()
                {
                    Name = "NEW"
                };

                // Act
                await test.Service.UpdateSending(newPmode, SendingBasePmode.Name);

                // Assert
                await test.Source.UpdateSending(Arg.Is<SendingBasePmode>(x => x.Name == "NEW"), Arg.Is(SendingBasePmode.Name));
            }

            [Fact]
            public async Task Update_Existing_When_Name_IsNot_Changed()
            {
                // Setup
                var test = Setup();

                // Act
                await test.Service.UpdateSending(SendingBasePmode, SendingBasePmode.Name);

                // Assert
                await test.Source.UpdateSending(Arg.Is<SendingBasePmode>(x => x.Name == "NEW"), Arg.Is(SendingBasePmode.Name));
            }
        }

        public class UpdateReceiving : As4PmodeServiceTests
        {
            [Fact]
            public async Task Throws_Exception_When_Parameters_Are_Null()
            {
                // Setup
                var test = Setup();

                // Act & Assert
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => test.Service.UpdateReceiving(null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => test.Service.UpdateReceiving(ReceivingBasePmode, null));
            }

            [Fact]
            public async Task Throws_Exception_When_A_Pmode_With_The_New_Name_Already_Exists()
            {
                // Setup
                var test = Setup();
                var newPmode = new ReceivingBasePmode()
                {
                    Name = ReceivingBasePmode.Name
                };

                // Act
                await Assert.ThrowsAsync(typeof(AlreadyExistsException), () => test.Service.UpdateReceiving(newPmode, "NEW"));
            }

            [Fact]
            public async Task Updates_Existing()
            {
                // Setup
                var test = Setup();
                var newPmode = new ReceivingBasePmode()
                {
                    Name = "NEW"
                };

                // Act
                await test.Service.UpdateReceiving(newPmode, ReceivingBasePmode.Name);

                // Assert
                await test.Source.UpdateReceiving(Arg.Is<ReceivingBasePmode>(x => x.Name == "NEW"), Arg.Is(ReceivingBasePmode.Name));
            }

            [Fact]
            public async Task Update_Existing_When_Name_IsNot_Changed()
            {
                // Setup
                var test = Setup();

                // Act
                await test.Service.UpdateReceiving(ReceivingBasePmode, ReceivingBasePmode.Name);

                // Assert
                await test.Source.UpdateReceiving(Arg.Is<ReceivingBasePmode>(x => x.Name == "NEW"), Arg.Is(ReceivingBasePmode.Name));
            }
        }
    }
}
