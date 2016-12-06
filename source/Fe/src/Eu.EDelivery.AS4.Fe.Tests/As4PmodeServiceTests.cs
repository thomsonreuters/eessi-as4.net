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
            public async Task Calls_Source_And_Returns_Pmode_When_It_Exists()
            {
                // Setup
                var test = Setup();

                // Act
                var result = await test.Service.GetReceivingByName(ReceivingPmode.Name);

                // Assert
                Assert.NotNull(result);
                Assert.True(result == ReceivingPmode);
                await test.Source.GetReceivingByName(Arg.Is(ReceivingPmode.Name));
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

            [Fact]
            public async Task Throws_Exception_When_Pmode_Doesnt_Exist()
            {
                // Setup
                var test = Setup();

                // Act
                await Assert.ThrowsAsync(typeof(Exception), () => test.Service.GetReceivingByName("IDONTEXIST"));
            }
        }
    }
}