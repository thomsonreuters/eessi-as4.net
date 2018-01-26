using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Services.DynamicDiscovery;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Submit;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Submit
{
    public class GivenDynamicDiscoveryStepFacts
    {
        [Fact]
        public async Task ThenExecuteStepResultInContactingSMPServer()
        {
            // Arrange
            SendingProcessingMode pmode = EnabledDynamicDiscoveryPMode(
                smpProfile: typeof(ChangeIdDiscoveryProfile).AssemblyQualifiedName);

            string beforeId = pmode.Id;

            // Act
            StepResult result = await ExerciseDynamicDiscovery(pmode);

            // Assert
            string afterId = result.MessagingContext.SendingPMode.Id;
            Assert.NotEqual(beforeId, afterId);
        }

        [Fact]
        public async Task ThenExecuteStepFailsWithMissingToPartyId()
        {
            // Arrange
            SendingProcessingMode pmode = EnabledDynamicDiscoveryPMode(
                smpProfile: typeof(ChangeIdDiscoveryProfile).AssemblyQualifiedName);

            pmode.MessagePackaging.PartyInfo.ToParty.PartyIds.Clear();

            // Act
            await Assert.ThrowsAsync<ConfigurationErrorsException>(
                () => ExerciseDynamicDiscovery(pmode));
        }

        private static SendingProcessingMode EnabledDynamicDiscoveryPMode(string smpProfile)
        {
            return new SendingProcessingMode
            {
                DynamicDiscovery = new DynamicDiscoveryConfiguration
                {
                    SmpProfile = smpProfile
                },
                MessagePackaging = new SendMessagePackaging
                {
                    PartyInfo = new PartyInfo
                    {
                        ToParty = new Party(new PartyId(Guid.NewGuid().ToString()))
                    }
                }
            };
        }

        private static async Task<StepResult> ExerciseDynamicDiscovery(SendingProcessingMode pmode)
        {
            var step = new DynamicDiscoveryStep();

            return await step.ExecuteAsync(
                new MessagingContext(new SubmitMessage())  {SendingPMode = pmode},
                CancellationToken.None);
        }

        public class ChangeIdDiscoveryProfile : IDynamicDiscoveryProfile
        {
            /// <summary>
            /// Retrieves the SMP meta data <see cref="XmlDocument"/> for a given <paramref name="partyId"/> using a given <paramref name="config"/>.
            /// </summary>
            /// <param name="partyId">The party identifier.</param>
            /// <param name="properties"></param>
            /// <returns></returns>
            public Task<XmlDocument> RetrieveSmpMetaData(string partyId, IDictionary<string, string> properties)
            {
                return Task.FromResult(new XmlDocument());
            }

            /// <summary>
            /// Complete the <paramref name="pmode"/> with the SMP metadata that is present in the <paramref name="smpMetaData"/> <see cref="XmlDocument"/>
            /// </summary>
            /// <param name="pmode">The <see cref="SendingProcessingMode"/> that must be decorated with the SMP metadata</param>
            /// <param name="smpMetaData">An XmlDocument that contains the SMP MetaData that has been received from an SMP server.</param>
            /// <returns>The completed <see cref="SendingProcessingMode"/></returns>
            public SendingProcessingMode DecoratePModeWithSmpMetaData(SendingProcessingMode pmode, XmlDocument smpMetaData)
            {
                pmode.Id = Guid.NewGuid().ToString();
                return pmode;
            }
        }
    }
}
