using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Settings;
using NSubstitute;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.Tests
{
    public class As4SettingsServiceTests
    {
        private const string SubmitAgentName = "submitAgentName";
        private const string ReceiveAgentName = "receiveAgentName";
        private As4SettingsService settingsService;
        private ISettingsSource settingsSource;

        private As4SettingsServiceTests Setup()
        {
            var settings = new AS4Model.Settings
            {
                Agents = new SettingsAgents
                {
                    SubmitAgents = new List<SettingsAgent>
                    {
                        new SettingsAgent
                        {
                            Name = SubmitAgentName
                        }
                    }.ToArray(),
                    ReceiveAgents = new List<SettingsAgent>
                    {
                        new SettingsAgent
                        {
                            Name = ReceiveAgentName
                        }
                    }.ToArray()
                }
            };
            var mapper = Substitute.For<IMapper>();
            settingsSource = Substitute.For<ISettingsSource>();
            settingsSource.Get().Returns(settings);
            settingsService = new As4SettingsService(mapper, settingsSource);
            return this;
        }

        public class UpdateOrCreateAgent : As4SettingsServiceTests
        {
            [Fact]
            public async Task Throws_Exception_When_Arguments_Are_Null()
            {
                // Act & Assert
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.UpdateOrCreateAgent(null, null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.UpdateOrCreateAgent(new SettingsAgent(), null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.UpdateOrCreateAgent(new SettingsAgent(), agents => agents.SubmitAgents, null));
            }

            [Fact]
            public async Task Creates_Agent_When_It_Doesnt_Exist_Yet()
            {
                // Setup
                var newAgentName = "newAgent";
                var newAgent = new SettingsAgent()
                {
                    Name = newAgentName
                };

                var service = Setup().settingsService;

                // Act & Assert
                await service.UpdateOrCreateAgent(newAgent, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
                await settingsSource.Received().Save(Arg.Is<AS4Model.Settings>(settings => settings.Agents.SubmitAgents.Any(agent => agent.Name == newAgentName)));

                await service.UpdateOrCreateAgent(newAgent, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
                await settingsSource.Received().Save(Arg.Is<AS4Model.Settings>(settings => settings.Agents.ReceiveAgents.Any(agent => agent.Name == newAgentName)));
            }
        }

        public class DeleteAgent : As4SettingsServiceTests
        {
            [Fact]
            public async Task Deletes_Agent()
            {
                // Act & Assert
                await Setup().settingsService.DeleteAgent(SubmitAgentName, agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents);
                await settingsSource.Received().Save(Arg.Is<AS4Model.Settings>(x => !x.Agents.SubmitAgents.Any()));
                await Setup().settingsService.DeleteAgent(ReceiveAgentName, agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents);
                await settingsSource.Received().Save(Arg.Is<AS4Model.Settings>(x => !x.Agents.ReceiveAgents.Any()));

                await settingsSource.Received().Get();
            }

            [Fact]
            public async Task Throws_Exception_when_Agent_Not_Exists()
            {
                // Setup
                Setup();

                // Act & Assert
                await Assert.ThrowsAsync(typeof(Exception), () => settingsService.DeleteAgent("IDONTEXISTAGENT", agents => agents.SubmitAgents, (settings, agents) => settings.SubmitAgents = agents));
                await Assert.ThrowsAsync(typeof(Exception), () => settingsService.DeleteAgent("IDONTEXISTAGENT", agents => agents.ReceiveAgents, (settings, agents) => settings.ReceiveAgents = agents));
                await settingsSource.DidNotReceive().Save(Arg.Any<AS4Model.Settings>());
            }

            [Fact]
            public async Task Throws_Exception_When_Parameters_Are_Null()
            {
                // Act & Assert
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.DeleteAgent(null, null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.DeleteAgent("TEST", null, null));
                await Assert.ThrowsAsync(typeof(ArgumentNullException), () => Setup().settingsService.DeleteAgent("TEST", agents => agents.SubmitAgents, null));                
            }
        }
    }
}