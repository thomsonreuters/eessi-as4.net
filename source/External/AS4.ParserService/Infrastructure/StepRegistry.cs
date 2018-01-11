using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AS4.ParserService.CustomSteps;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Send;

namespace AS4.ParserService.Infrastructure
{
    internal static class StepRegistry
    {
        private static readonly StepConfiguration _outBoundProcessingConfig;

        private static readonly StepConfiguration _inBoundProcessingConfig;

        private static readonly StepConfiguration _receiptCreationConfig;

        static StepRegistry()
        {
            _outBoundProcessingConfig =
                new StepConfiguration
                {
                    NormalPipeline = new[]
                    {
                        new Step {Type = typeof(ValidateSendingPModeStep).AssemblyQualifiedName},
                        new Step {Type = typeof(CompressAttachmentsStep).AssemblyQualifiedName},
                        new Step {Type = typeof(SignAS4MessageStep).AssemblyQualifiedName},
                        new Step {Type = typeof(EncryptAS4MessageStep).AssemblyQualifiedName}
                    }
                };

            _inBoundProcessingConfig = new StepConfiguration()
            {
                NormalPipeline = new[]
                {
                    new Step {Type = typeof(ValidateAS4MessageStep).AssemblyQualifiedName},
                    new Step {Type = typeof(DecryptAS4MessageStep).AssemblyQualifiedName},
                    new Step {Type = typeof(VerifySignatureAS4MessageStep).AssemblyQualifiedName},
                    new Step {Type = typeof(DecompressAttachmentsStep).AssemblyQualifiedName},
                },
                ErrorPipeline = new[]
                {
                    new Step {Type = typeof(CreateAS4ErrorStep).AssemblyQualifiedName},
                    new Step {Type = typeof(SignAS4MessageStep).AssemblyQualifiedName}
                }
            };

            _receiptCreationConfig = new StepConfiguration()
            {
                NormalPipeline = new[]
                {
                    new Step {Type = typeof(CreateAS4ReceiptStep).AssemblyQualifiedName},
                    new Step {Type = typeof(SignAS4MessageStep).AssemblyQualifiedName}
                }
            };
        }


        public static StepConfiguration GetOutboundProcessingStepConfiguration()
        {
            return _outBoundProcessingConfig;
        }

        public static StepConfiguration GetInboundProcessingConfiguration()
        {
            return _inBoundProcessingConfig;
        }

        public static StepConfiguration GetReceiptCreationConfiguration()
        {
            return _receiptCreationConfig;
        }
    }
}