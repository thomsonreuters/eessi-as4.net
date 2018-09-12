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
        public static Gen<T> Or<T>(this Gen<T> g1, Gen<T> g2)
        {
            return Gen.OneOf(g1, g2);
        }

        public static Gen<T> OrNull<T>(this Gen<T> g1) where T : class
        {
            return Gen.OneOf(g1, Gen.Constant((T) null));
        }

        public static Gen<T3> Zip<T1, T2, T3>(this Gen<T1> g1, Gen<T2> g2, Func<T1, T2, T3> f)
        {
            return Gen.zip(g1, g2).Select(t => f(t.Item1, t.Item2));
        }

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
