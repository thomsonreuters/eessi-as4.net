using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Eu.EDelivery.AS4.Fe.Pmodes;
using NSubstitute;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.Tests
{
    public class As4PmodeServiceTests
    {
        protected IAs4PmodeService Service { get; private set; }
        protected IAs4PmodeSource Source { get; private set; }
        protected IEnumerable<SendingPmode> SendingPmodes { get; private set; }
        protected IEnumerable<ReceivingPmode> ReceivingPmodes { get; private set; }
        protected ReceivingPmode ReceivingPmode { get; private set; }
        protected SendingPmode SendingPmode { get; private set; }

        protected As4PmodeServiceTests Setup()
        {
            Source = Substitute.For<IAs4PmodeSource>();
            Service = new As4PmodeService(Source);
            SetupPmodes();
            return this;
        }

        private void SetupPmodes()
        {
            ReceivingPmode = new ReceivingPmode()
            {
                Name = "test1",
                Type = PmodeType.Receiving
            };
            SendingPmode = new SendingPmode()
            {
                Name = "test2",
                Type = PmodeType.Sending
            };
            ReceivingPmodes = new List<ReceivingPmode>() { ReceivingPmode };
            SendingPmodes = new List<SendingPmode> { SendingPmode };

            Source.GetReceivingNames().Returns(ReceivingPmodes.Select(pmode => pmode.Name));
            Source.GetReceivingByName(Arg.Is(ReceivingPmode.Name)).Returns(ReceivingPmode);

            Source.GetSendingNames().Returns(SendingPmodes.Select(pmode => pmode.Name));
            Source.GetSendingByName(Arg.Is(SendingPmode.Name)).Returns(SendingPmode);
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
                test.Source.Received().GetReceivingNames();
                Assert.True(result.First() == ReceivingPmode.Name);
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

            [Fact]
            public async Task Throws_Exception_When_Pmode_Doesnt_Exist()
            {
                // Setup
                var test = Setup();

                // Act & Assert
                await Assert.ThrowsAsync(typeof(Exception), () => test.Service.GetReceivingByName("IDONTEXIST"));
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
                Assert.True(result.First() == SendingPmode.Name);
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
                var result = await test.Service.GetSendingByName(SendingPmode.Name);

                // Assert
                await test.Source.Received().GetSendingByName(Arg.Is(SendingPmode.Name));
                Assert.True(result == SendingPmode);
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
                var pmode = new ReceivingPmode()
                {
                    Name = ReceivingPmode.Name
                };

                await Assert.ThrowsAsync(typeof(Exception), () => test.Service.CreateReceiving(pmode));
            }

            [Fact]
            public async Task Calls_Source_SaveReceiving()
            {
                // Setup
                var test = Setup();
                var pmode = new ReceivingPmode()
                {
                    Name = "newPmode"
                };

                // Act
                await test.Service.CreateReceiving(pmode);

                // Assert
                await test.Source.Received().CreateReceiving(Arg.Is<ReceivingPmode>(x => x.Name == "newPmode"));
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
                await Assert.ThrowsAsync(typeof(Exception), () => test.Service.DeleteReceiving("new"));
            }

            [Fact]
            public async Task Deletes_Pmode()
            {
                // Setup
                var test = Setup();
                
                // Act
                await test.Service.DeleteReceiving(ReceivingPmode.Name);

                // Assert
                await test.Source.Received().DeleteReceiving(ReceivingPmode.Name);
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
                await Assert.ThrowsAsync(typeof(Exception), () => test.Service.CreateSending(SendingPmode));
            }

            [Fact]
            public async Task Creates_The_Pmode()
            {
                // Setup
                var test = Setup();
                var pmode = new SendingPmode()
                {
                    Name = "newPmode"
                };

                // Act
                await test.Service.CreateSending(pmode);

                // Assert
                await test.Source.Received().CreateSending(Arg.Is<SendingPmode>(x => x.Name == pmode.Name));
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
                await Assert.ThrowsAsync(typeof(Exception), () => test.Service.DeleteSending("sendingPmode"));
            }

            [Fact]
            public async Task Deletes_Pmode()
            {
                // Setup
                var test = Setup();

                // Act
                await test.Service.DeleteSending(SendingPmode.Name);

                // Assert
                await test.Source.Received().DeleteSending(Arg.Is(SendingPmode.Name));
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
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => test.Service.UpdateSending(SendingPmode, null));
            }

            [Fact]
            public async Task Throws_Exception_When_A_Pmode_With_The_New_Name_Already_Exists()
            {
                // Setup
                var test = Setup();
                var newPmode = new SendingPmode()
                {
                    Name = SendingPmode.Name
                };

                // Act
                await Assert.ThrowsAsync(typeof(Exception), () => test.Service.UpdateSending(newPmode, "NEW"));
            }

            [Fact]
            public async Task Updates_Existing()
            {
                // Setup
                var test = Setup();
                var newPmode = new SendingPmode()
                {
                    Name = "NEW"
                };

                // Act
                await test.Service.UpdateSending(newPmode, SendingPmode.Name);

                // Assert
                await test.Source.UpdateSending(Arg.Is<SendingPmode>(x => x.Name == "NEW"), Arg.Is(SendingPmode.Name));
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
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => test.Service.UpdateReceiving(ReceivingPmode, null));
            }

            [Fact]
            public async Task Throws_Exception_When_A_Pmode_With_The_New_Name_Already_Exists()
            {
                // Setup
                var test = Setup();
                var newPmode = new ReceivingPmode()
                {
                    Name = ReceivingPmode.Name
                };

                // Act
                await Assert.ThrowsAsync(typeof(Exception), () => test.Service.UpdateReceiving(newPmode, "NEW"));
            }

            [Fact]
            public async Task Updates_Existing()
            {
                // Setup
                var test = Setup();
                var newPmode = new ReceivingPmode()
                {
                    Name = "NEW"
                };

                // Act
                await test.Service.UpdateReceiving(newPmode, ReceivingPmode.Name);

                // Assert
                await test.Source.UpdateReceiving(Arg.Is<ReceivingPmode>(x => x.Name == "NEW"), Arg.Is(ReceivingPmode.Name));
            }
        }
    }
}
