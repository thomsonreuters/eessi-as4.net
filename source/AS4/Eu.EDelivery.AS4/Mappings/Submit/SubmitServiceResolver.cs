using System;
using AutoMapper;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using CoreService = Eu.EDelivery.AS4.Model.Core.Service;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve the <see cref="CoreService"/>
    /// </summary>
    public class SubmitServiceResolver : ISubmitResolver<CoreService>
    {
        private readonly IPModeResolver<CoreService> _pmodeResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitServiceResolver"/> class
        /// </summary>
        public SubmitServiceResolver()
        {
            this._pmodeResolver = new PModeServiceResolver();
        }

        /// <summary>
        /// 1. SubmitMessage / CollaborationInfo / Service / Value
        /// 2. PMode / Message Packaging / CollaborationInfo / Service / Value
        /// 3. Default
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoreService Resolve(SubmitMessage message)
        {
            if (message.PMode.AllowOverride == false && DoesSubmitMessageTriesToOverridePModeValues(message))
            {
                throw new AS4Exception($"Submit message is not allowed by PMode {message.PMode.Id} to override Service");
            }

            if (message.Collaboration.Service?.Value != null)
                return Mapper.Map<CoreService>(message.Collaboration.Service);

            return this._pmodeResolver.Resolve(message.PMode);
        }

        private static bool DoesSubmitMessageTriesToOverridePModeValues(SubmitMessage message)
        {            
            return

                message.Collaboration.Service?.Value != null &&
                message.PMode.MessagePackaging.CollaborationInfo?.Service?.Value != null &&
                StringComparer.OrdinalIgnoreCase.Equals(message.Collaboration.Service?.Value,
                    message.PMode.MessagePackaging.CollaborationInfo?.Service?.Value) == false;
        }
    }
}
