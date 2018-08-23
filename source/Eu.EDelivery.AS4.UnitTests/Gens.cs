using System;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.UnitTests.Model;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.FSharp.Core;

namespace Eu.EDelivery.AS4.UnitTests
{
    public class CustomProperty : PropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomProperty"/> class.
        /// </summary>
        public CustomProperty()
        {
            Arbitrary = new[] {typeof(Gens)};
        }
    }

    public static class Gens
    {
        public static Arbitrary<MessageUnit> MessageUnits()
        {
            return Gen.Elements<MessageUnit>(
                new UserMessage($"user-{Guid.NewGuid()}"),
                new Receipt(
                    $"receipt-{Guid.NewGuid()}", 
                    $"ref-to-user-{Guid.NewGuid()}",
                    DateTimeOffset.Now),
                new FilledNRReceipt(), 
                new Error(
                    $"error-{Guid.NewGuid()}", 
                    $"user-{Guid.NewGuid()}", 
                    ErrorLine.FromErrorResult(
                        new ErrorResult($"desc-{Guid.NewGuid()}", ErrorAlias.Other))))
                      .ToArbitrary();
        }

        public static Arbitrary<SignalMessage> SignalMessages()
        {
            return MessageUnits()
                   .Generator
                   .Where(u => u is SignalMessage)
                   .Select(u => u as SignalMessage)
                   .ToArbitrary();
        }

        public static Arbitrary<Maybe<T>> MaybeArbitrary<T>()
        {
            return Arb.Default
                .Option<T>()
                .Generator
                .Select(x => 
                    Equals(x, FSharpOption<T>.None) 
                        ? Maybe<T>.Nothing 
                        : Maybe.Just(x.Value))
                .ToArbitrary();
        }
    }
}
