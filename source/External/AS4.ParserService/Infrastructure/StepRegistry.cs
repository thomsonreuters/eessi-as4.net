using AS4.ParserService.CustomSteps;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Send;

namespace AS4.ParserService.Infrastructure
{
    internal static class StepRegistry
    {
        private static readonly StepConfiguration OutBoundProcessingConfig;

        private static readonly StepConfiguration InBoundProcessingConfig;

        private static readonly StepConfiguration ReceiptCreationConfig;

        static StepRegistry()
        {
            OutBoundProcessingConfig =
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

            InBoundProcessingConfig = new StepConfiguration()
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

            ReceiptCreationConfig = new StepConfiguration()
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
            return OutBoundProcessingConfig;
        }

        public static StepConfiguration GetInboundProcessingConfiguration()
        {
            return InBoundProcessingConfig;
        }

        public static StepConfiguration GetReceiptCreationConfiguration()
        {
            return ReceiptCreationConfig;
        }
    }
}