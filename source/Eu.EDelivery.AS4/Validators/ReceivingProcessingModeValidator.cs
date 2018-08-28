using System;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.PMode;
using FluentValidation;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validator to check the <see cref="ReceivingProcessingMode" />
    /// </summary>
    public class ReceivingProcessingModeValidator : AbstractValidator<ReceivingProcessingMode>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivingProcessingModeValidator" /> class.
        /// </summary>
        private ReceivingProcessingModeValidator()
        {
            RuleFor(pmode => pmode.Id)
                .NotEmpty()
                .WithMessage("Id element must not be empty");

            RulesForReplyHandling();
            RulesForExceptionHandling();
            RulesForMessageHandling();
            RulesForForwarding();
            RulesForDeliver();
            RulesForDecryption();
        }

        public static readonly ReceivingProcessingModeValidator Instance = new ReceivingProcessingModeValidator();

        private void RulesForReplyHandling()
        {
            bool IsNotForwarding(ReceivingProcessingMode pmode)
            {
                return pmode.MessageHandling?.ForwardInformation == null;
            }

            RuleFor(pmode => pmode.ReplyHandling)
                .NotNull()
                .WithMessage("ReplyHandling element must be specified when there isn't a MessageHandling.Forward element")
                .When(IsNotForwarding);

            When(pmode => pmode.ReplyHandling != null, () =>
            {
                RuleFor(pmode => pmode.ReplyHandling.SendingPMode)
                    .NotEmpty()
                    .WithMessage(
                        "ReplyHandling.SendingPMode must be specified when there isn't a MessageHandling.Forward element")
                     .When(IsNotForwarding);

                RuleFor(pmode => pmode.ReplyHandling.ReceiptHandling)
                    .NotNull()
                    .WithMessage("ReplyHandling.ReceiptHandling element must be specified");

                RuleFor(pmode => pmode.ReplyHandling.ErrorHandling)
                    .NotNull()
                    .WithMessage("ReplyHandling.ErrorHandling element must be specified");
            });
        }

        private void RulesForExceptionHandling()
        {
            RulesForExceptionNotifyMethod();
            RulesForExceptionReliability();
        }

        private void RulesForExceptionReliability()
        {
            When(pmode => pmode.ExceptionHandling?.Reliability?.IsEnabled == true, () =>
            {
                RuleFor(pmode => pmode.ExceptionHandling.Reliability.RetryCount)
                    .Must(i => i > 0)
                    .WithMessage("ExceptionHandling.Reliability.RetryCount must be greater than 0 when Exceptionhandling.Reliability.IsEnabled = true");

                RuleFor(pmode => pmode.ExceptionHandling.Reliability.RetryInterval.AsTimeSpan())
                    .Must(t => t > default(TimeSpan))
                    .WithMessage(
                        $"Exceptionhandling.Reliability.RetryInterval must be greater than {default(TimeSpan)} when ExceptionHandling.Reliability.IsEnabled = true");
            });
        }

        private void RulesForExceptionNotifyMethod()
        {
            When(pmode => pmode.ExceptionHandling?.NotifyMessageConsumer == true, () =>
            {
                RuleFor(pmode => pmode.ExceptionHandling.NotifyMethod)
                    .NotNull()
                    .WithMessage(
                        "ExceptionHandling.NotifyMethod should be specified when the ExceptionHandling.NotifyMessageConsuer = true");

                When(pmode => pmode.ExceptionHandling.NotifyMethod != null, () =>
                {
                    RuleFor(pmode => pmode.ExceptionHandling.NotifyMethod.Type)
                        .NotEmpty()
                        .WithMessage(
                            "ExceptionHandling.NotifyMethod.Type shoud be specified when the ExceptionHandling.NotifyMessageConsumer = true");

                    RuleFor(pmode => pmode.ExceptionHandling.NotifyMethod.Parameters)
                        .NotNull()
                        .ForEach((Parameter p) => p != null)
                        .SetCollectionValidator(ParameterValidator.Instance)
                        .WithMessage(
                            "ExceptionHandling.NotifyMethod.Parameters should be specified with an empty tag or " +
                            "with non-empty Name and Value attributes when the ExceptionHandling.NotifyMessageProducer = true");
                });
            });
        }

        private void RulesForMessageHandling()
        {
            RuleFor(pmode => pmode.MessageHandling)
                .NotNull()
                .WithMessage("MessageHandling element must be specified")
                .DependentRules(
                    r => r.RuleFor(pmode => pmode.MessageHandling)
                          .Must(mh => mh.DeliverInformation != null
                                      || mh.ForwardInformation != null)
                          .WithMessage("MessageHandling element must contain either a <Deliver/> or a <Forward/> element"));
        }

        private void RulesForForwarding()
        {
            bool IsForwarding(ReceivingProcessingMode pmode)
            {
                return pmode.MessageHandling?.ForwardInformation != null;
            }

            RuleFor(pmode => pmode.MessageHandling.ForwardInformation.SendingPMode)
                .NotEmpty()
                .When(IsForwarding)
                .WithMessage("MessageHandling.Forward.SendingPMode must be specified with the ID of the Sending PMode that must be used for forwarding");
        }

        private void RulesForDeliver()
        {
            RulesForDeliverMethod();
            RulesForDeliverPayloadReferenceMethod();
            RulesForDeliverReliability();
        }

        private void RulesForDeliverMethod()
        {
            When(IsDeliverEnabled, () =>
            {
                RuleFor(pmode => pmode.MessageHandling.DeliverInformation.DeliverMethod)
                    .NotNull()
                    .WithMessage("MessageHandling.Deliver.DeliverMethod should be specified when MessageHandling.Deliver.IsEnabled = true");

                When(pmode => pmode.MessageHandling.DeliverInformation.DeliverMethod != null, () =>
                {
                    RuleFor(pmode => pmode.MessageHandling.DeliverInformation.DeliverMethod.Type)
                        .NotEmpty()
                        .WithMessage("MessageHandling.Deliver.DeliverMethod.Type should be specified when MessageHandling.Deliver.IsEnabled = true");

                    RuleFor(pmode => pmode.MessageHandling.DeliverInformation.DeliverMethod.Parameters)
                        .NotNull()
                        .SetCollectionValidator(ParameterValidator.Instance)
                        .ForEach((Parameter p) => p != null)
                        .WithMessage(
                            "MessageHandling.Deliver.DeliverMethod.Parameters should be specified as an empty tag or " +
                            "with non-empty Name and Value attributes when the MessageHandling.Deliver.IsEnabeld = true");
                });
            });
        }

        private void RulesForDeliverPayloadReferenceMethod()
        {
            When(IsDeliverEnabled, () =>
            {
                RuleFor(pmode => pmode.MessageHandling.DeliverInformation.PayloadReferenceMethod)
                    .NotNull()
                    .WithMessage(
                        "MessageHandling.Deliver.PayloadReferenceMethod should be specified when MessageHandling.Deliver.IsEnabled = true");

                When(pmode => pmode.MessageHandling?.DeliverInformation?.PayloadReferenceMethod != null, () =>
                {
                    RuleFor(pmode => pmode.MessageHandling.DeliverInformation.PayloadReferenceMethod.Type)
                        .NotEmpty()
                        .WithMessage(
                            "MessageHandling.Deliver.PayloadReferenceMethod.Type must be specified when MessageHandling.Deliver.IsEnabled = true");

                    RuleFor(pmode => pmode.MessageHandling.DeliverInformation.PayloadReferenceMethod.Parameters)
                        .NotNull()
                        .ForEach((Parameter p) => p != null)
                        .SetCollectionValidator(ParameterValidator.Instance)
                        .WithMessage(
                            "MessageHandling.Deliver.PayloadReferenceMethod.Parameters should be specified as an empty tag or " +
                            "with non-empty Name and Value attributes when the MessageHandling.Deliver.IsEnabled = true");
                });
            });
        }

        private void RulesForDeliverReliability()
        {
            When(pmode => pmode.MessageHandling?.DeliverInformation?.Reliability?.IsEnabled == true, () =>
            {
                RuleFor(pmode => pmode.MessageHandling.DeliverInformation.Reliability.RetryCount)
                    .Must(i => i > 0)
                    .WithMessage(
                        "MessageHandling.Deliver.Reliability.RetryCount must be greater than 0 when MessageHandling.Deliver.Reliability.IsEnabled = true");

                RuleFor(pmode => pmode.MessageHandling.DeliverInformation.Reliability.RetryInterval.AsTimeSpan())
                    .Must(t => t > default(TimeSpan))
                    .WithMessage(
                        $"MessageHandling.Deliver.Reliability.RetryInterval must be greater than {default(TimeSpan)} when MessageHandling.Deliver.Reliability.IsEnabled = true");
            });
        }

        private static bool IsDeliverEnabled(ReceivingProcessingMode pmode)
        {
            return pmode.MessageHandling?.DeliverInformation?.IsEnabled == true;
        }

        private void RulesForDecryption()
        {
            bool IsEncryptionAllowedOrRequired(ReceivingProcessingMode pmode)
            {
                Limit? encryption = pmode.Security?.Decryption?.Encryption;
                return encryption == Limit.Allowed || encryption == Limit.Required;
            }

            // TODO: should we validate on a non-specified decryption certificate when the encryption limit is set to NotAllowed?
            When(IsEncryptionAllowedOrRequired, () =>
            {
                RuleFor(pmode => pmode.Security.Decryption.DecryptCertificateInformation)
                    .Must(cert =>
                    {
                        if (cert is CertificateFindCriteria c)
                        {
                            return !String.IsNullOrWhiteSpace(c.CertificateFindValue);
                        }

                        if (cert is PrivateKeyCertificate k)
                        {
                            return !String.IsNullOrWhiteSpace(k.Certificate)
                                   && !String.IsNullOrWhiteSpace(k.Password);
                        }

                        return false;
                    })
                    .WithMessage(
                        "Security.Decryption.DecryptCertificate must be specified when the Security.Decryption.Encryption is set to either Allowed or Required");
            });
        }
    }
}